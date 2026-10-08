// The Night City HUD: V's panel, street cred, minimap radar, gig tracker, eddies, radio calls, popups, glitch and scanlines.
using System.Collections.Generic;
using Sigf.Kit;
using UnityEngine;

public static class Hud
{
    class Pop { public string a, b; public float t0; public Vector3 pos; public Color col; }
    static readonly List<Pop> pops = new List<Pop>();
    static string commWho = "", commText = ""; static float commT0 = -99f, commLen;
    static GUIStyle st; static Texture2D scan, white;
    static float k;
    public static bool Visible = true;

    public static void Comm(string who, string text, float seconds)
    {
        commWho = who; commText = text; commT0 = Time.unscaledTime; commLen = seconds;
        Mix.Play(Mix.Sound("beep.wav"), null, 0.5f, who.StartsWith("JOHNNY") ? 0.8f : 1.2f);
        Fx.Glitch(0.3f);
    }

    public static void Popup(string a, string b, Color? col = null) => pops.Add(new Pop { a = a, b = b, t0 = Time.unscaledTime, pos = G.Pos, col = col ?? City.Yellow });

    static void Box(Rect r, Color c) { var p = GUI.color; GUI.color = c; GUI.DrawTexture(r, white); GUI.color = p; }

    static void Frame(Rect r, Color c, float w = 2f)
    {
        w *= k;
        Box(new Rect(r.x, r.y, r.width, w), c); Box(new Rect(r.x, r.yMax - w, r.width, w), c);
        Box(new Rect(r.x, r.y, w, r.height), c); Box(new Rect(r.xMax - w, r.y, w, r.height), c);
    }

    static void Text(Rect r, string s, int size, Color c, TextAnchor a = TextAnchor.MiddleLeft)
    {
        st.fontSize = Mathf.Max(8, Mathf.RoundToInt(size * k)); st.alignment = a;
        var p = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.85f * c.a); GUI.Label(new Rect(r.x + 2 * k, r.y + 2 * k, r.width, r.height), s, st);
        GUI.color = c; GUI.Label(r, s, st); GUI.color = p;
    }

    static void Init()
    {
        if (st != null) return;
        st = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false, clipping = TextClipping.Overflow };
        white = Texture2D.whiteTexture;
        scan = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
        scan.SetPixels(new[] { new Color(0, 0, 0, 0.0f), new Color(0, 0, 0, 0.0f), new Color(0, 0, 0, 0.22f), new Color(0, 0, 0, 0.0f) });
        scan.Apply();
    }

    public static void Draw()
    {
        if (G.Board == null) return;
        Init();
        k = Screen.height / 1080f;
        float t = Time.unscaledTime;
        float W = Screen.width, H = Screen.height;
        var cy = City.Cyan; var pk = City.Pink; var yl = City.Yellow;

        if (Visible)
        {
            // V's panel (top left)
            var p = new Rect(30 * k, 28 * k, 420 * k, 110 * k);
            Box(p, new Color(0.02f, 0.02f, 0.08f, 0.62f)); Frame(p, cy, 2);
            Box(new Rect(p.x, p.y, 8 * k, p.height), pk);
            Text(new Rect(p.x + 22 * k, p.y + 6 * k, 380 * k, 34 * k), "V  //  BIRB", 30, Color.white);
            var hp = new Rect(p.x + 22 * k, p.y + 46 * k, 370 * k, 14 * k);
            Box(hp, new Color(0, 0, 0, 0.7f)); Box(new Rect(hp.x, hp.y, hp.width, hp.height), new Color(cy.r, cy.g, cy.b, 0.9f));
            for (int i = 1; i < 10; i++) Box(new Rect(hp.x + hp.width * i / 10f, hp.y, 2 * k, hp.height), new Color(0, 0, 0, 0.8f));
            Text(new Rect(p.x + 22 * k, p.y + 62 * k, 250 * k, 22 * k), "STREET CRED  LVL " + Gigs.StreetCred, 18, yl);
            var cr = new Rect(p.x + 22 * k, p.y + 90 * k, 370 * k, 8 * k);
            Box(cr, new Color(0, 0, 0, 0.7f)); Box(new Rect(cr.x, cr.y, cr.width * Gigs.CredProgress, cr.height), yl);

            // minimap radar
            var mr = new Rect(30 * k, 150 * k, 220 * k, 220 * k);
            Box(mr, new Color(0.02f, 0.02f, 0.1f, 0.6f)); Frame(mr, cy, 2);
            var c0 = new Vector2(mr.center.x, mr.center.y);
            for (int i = 1; i < 4; i++)
            {
                Box(new Rect(mr.x + mr.width * i / 4f, mr.y, 1 * k, mr.height), new Color(cy.r, cy.g, cy.b, 0.18f));
                Box(new Rect(mr.x, mr.y + mr.height * i / 4f, mr.width, 1 * k), new Color(cy.r, cy.g, cy.b, 0.18f));
            }
            float yaw = (G.Cam != null ? G.Cam.transform.eulerAngles.y : 0f) * Mathf.Deg2Rad;
            float range = 55f;
            System.Action<Vector3, Color, float> dot = (wp, col, sz) =>
            {
                var d = wp - G.Pos; float x = d.x, z = d.z;
                float rx = x * Mathf.Cos(yaw) - z * Mathf.Sin(yaw), rz = x * Mathf.Sin(yaw) + z * Mathf.Cos(yaw);
                var v = new Vector2(rx, rz) / range * (mr.width / 2f);
                if (v.magnitude > mr.width / 2f - 8 * k) v = v.normalized * (mr.width / 2f - 8 * k);
                Box(new Rect(c0.x + v.x - sz * k / 2f, c0.y - v.y - sz * k / 2f, sz * k, sz * k), col);
            };
            foreach (var s in Gigs.Shards) if (!s.taken) dot(s.pos, yl, 9);
            if (Gigs.Beacon != null) dot(Gigs.BeaconPos, pk, 15);
            Box(new Rect(c0.x - 6 * k, c0.y - 6 * k, 12 * k, 12 * k), Color.white);
            Box(new Rect(c0.x - 2 * k, c0.y - 18 * k, 4 * k, 12 * k), cy);
            Text(new Rect(mr.x + 8 * k, mr.yMax - 26 * k, 200 * k, 22 * k), "NIGHT CITY  //  SCAN", 14, new Color(cy.r, cy.g, cy.b, 0.9f));

            // gig tracker (top right)
            var g = new Rect(W - 560 * k, 28 * k, 530 * k, 128 * k);
            Box(g, new Color(0.02f, 0.02f, 0.08f, 0.62f)); Frame(g, yl, 2);
            Box(new Rect(g.xMax - 8 * k, g.y, 8 * k, g.height), yl);
            Text(new Rect(g.x + 20 * k, g.y + 6 * k, 480 * k, 26 * k), "ACTIVE GIG", 16, new Color(cy.r, cy.g, cy.b, 0.95f));
            Text(new Rect(g.x + 20 * k, g.y + 30 * k, 500 * k, 40 * k), Gigs.Title, 32, yl);
            Text(new Rect(g.x + 20 * k, g.y + 72 * k, 500 * k, 26 * k), Gigs.Goal, 20, Color.white);
            Text(new Rect(g.x + 20 * k, g.y + 96 * k, 500 * k, 28 * k), Gigs.Progress, 24, pk);

            // eddies (bottom right)
            var e = new Rect(W - 400 * k, H - 112 * k, 370 * k, 80 * k);
            Box(e, new Color(0.02f, 0.02f, 0.08f, 0.6f)); Frame(e, yl, 2);
            Text(new Rect(e.x + 14 * k, e.y + 4 * k, 150 * k, 24 * k), "EDDIES", 16, cy);
            Text(new Rect(e.x + 14 * k, e.y + 24 * k, 350 * k, 52 * k), "€$ " + Gigs.Eddies.ToString("N0"), 46, yl);
        }

        // radio call (left, under the minimap)
        float ct = t - commT0;
        if (ct < commLen)
        {
            float a = Mathf.Clamp01(Mathf.Min(ct * 4f, (commLen - ct) * 3f));
            var c = new Rect(30 * k, 390 * k, 560 * k, 170 * k);
            Box(c, new Color(0.02f, 0.02f, 0.1f, 0.78f * a)); Frame(c, new Color(cy.r, cy.g, cy.b, a), 2);
            var pr = new Rect(c.x + 10 * k, c.y + 10 * k, 150 * k, 150 * k);
            var pc = GUI.color;
            bool jo = commWho.StartsWith("JOHNNY");
            GUI.color = new Color(1, 1, 1, a); if (jo) GUI.DrawTexture(pr, Mix.Texture("johnny.png", false)); else { Box(pr, new Color(0.08f, 0.05f, 0.18f, a)); Text(pr, "?", 90, new Color(pk.r, pk.g, pk.b, a), TextAnchor.MiddleCenter); Frame(pr, new Color(pk.r, pk.g, pk.b, a), 3); }
            GUI.color = pc;
            Text(new Rect(c.x + 172 * k, c.y + 8 * k, 380 * k, 26 * k), commWho + "  [INCOMING CALL]", 16, new Color(pk.r, pk.g, pk.b, a));
            int n = Mathf.Min(commText.Length, (int)(ct * 45f));
            st.wordWrap = true;
            Text(new Rect(c.x + 172 * k, c.y + 38 * k, 375 * k, 125 * k), commText.Substring(0, n), 21, new Color(1, 1, 1, a), TextAnchor.UpperLeft);
            st.wordWrap = false;
        }

        Drone.DrawTag(k);

        // floating eddie popups over the bird
        for (int i = pops.Count - 1; i >= 0; i--)
        {
            var pp = pops[i]; float age = t - pp.t0;
            if (age > 1.8f) { pops.RemoveAt(i); continue; }
            var cam = G.Cam; if (cam == null) continue;
            var sp = cam.WorldToScreenPoint(G.Pos + Vector3.up * 1.6f);
            if (sp.z < 0) continue;
            float y = H - sp.y - age * 70f * k - 30 * k;
            float al = Mathf.Clamp01((1.8f - age) * 2f);
            Text(new Rect(sp.x - 200 * k + (i % 3 - 1) * 60 * k, y, 400 * k, 44 * k), pp.a, 40, new Color(pp.col.r, pp.col.g, pp.col.b, al), TextAnchor.MiddleCenter);
            Text(new Rect(sp.x - 200 * k + (i % 3 - 1) * 60 * k, y + 38 * k, 400 * k, 28 * k), pp.b, 20, new Color(cy.r, cy.g, cy.b, al), TextAnchor.MiddleCenter);
        }

        // flatline (the bird bailed)
        if (t < Fx.FlatlineUntil)
        {
            float a = Mathf.Clamp01((Fx.FlatlineUntil - t) * 1.2f);
            Box(new Rect(0, 0, W, H), new Color(0.7f, 0f, 0.05f, 0.35f * a));
            float flick = Random.value < 0.15f ? 0.4f : 1f;
            Text(new Rect(0, H * 0.38f, W, 120 * k), "FLATLINED", 110, new Color(1f, 0.15f, 0.15f, a * flick), TextAnchor.MiddleCenter);
            Text(new Rect(0, H * 0.38f + 110 * k, W, 40 * k), "BIOMONITOR OFFLINE  //  REBOOTING BIRB...", 26, new Color(1f, 0.8f, 0.8f, a), TextAnchor.MiddleCenter);
        }

        // glitch bars
        if (t < Fx.GlitchUntil)
        {
            float a = Mathf.Clamp01((Fx.GlitchUntil - t) * 4f);
            for (int i = 0; i < 9; i++)
            {
                float y = Random.value * H, h = Random.Range(4f, 38f) * k, x = Random.Range(-40f, 40f) * k;
                Box(new Rect(x, y, W, h), Random.value < 0.5f ? new Color(pk.r, pk.g, pk.b, 0.28f * a) : new Color(cy.r, cy.g, cy.b, 0.28f * a));
            }
        }

        // scanlines over everything + a soft vignette
        var pc2 = GUI.color; GUI.color = Color.white;
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, W, H), scan, new Rect(0, 0, 1, H / (4f * Mathf.Max(1f, k * 1.5f))));
        GUI.color = pc2;
        var vig = new Color(0.1f, 0f, 0.2f, 0.32f);
        Box(new Rect(0, 0, W, 14 * k), vig); Box(new Rect(0, H - 14 * k, W, 14 * k), vig);
        Box(new Rect(0, 0, 14 * k, H), vig); Box(new Rect(W - 14 * k, 0, 14 * k, H), vig);
    }
}
