// The NCPB drone: a pigeon-police drone that tails the bird, scans every stunt and fines you for bails and screaming.
using Sigf.Kit;
using UnityEngine;

public static class Drone
{
    static GameObject root, eye, red, blue, ring;
    static Light spot;
    static string tag = "NCPB DRONE // SCANNING";
    static float tagUntil, flash;
    static Vector3 vel;

    static GameObject Part(PrimitiveType t, Vector3 lp, Vector3 sc, Color c, bool unlit)
    {
        var g = GameObject.CreatePrimitive(t);
        Object.Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(root.transform, false);
        g.transform.localPosition = lp; g.transform.localScale = sc;
        if (unlit) Mix.Unlit(g, c); else Mix.Paint(g, c);
        return g;
    }

    public static void Build()
    {
        root = new GameObject("NcpbDrone");
        root.transform.position = G.Pos + Vector3.up * 3f;
        Part(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.8f, 0.58f, 0.8f), new Color(0.85f, 0.85f, 0.95f), false);
        ring = Part(PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.4f, 0.04f, 1.4f), City.Cyan, true);
        eye = Part(PrimitiveType.Sphere, new Vector3(0, 0.05f, 0.5f), new Vector3(0.38f, 0.38f, 0.2f), new Color(1f, 0.1f, 0.1f), true);
        red = Part(PrimitiveType.Cube, new Vector3(0.28f, 0.48f, 0f), new Vector3(0.3f, 0.16f, 0.3f), new Color(1f, 0.05f, 0.05f), true);
        blue = Part(PrimitiveType.Cube, new Vector3(-0.28f, 0.48f, 0f), new Vector3(0.3f, 0.16f, 0.3f), new Color(0.1f, 0.3f, 1f), true);
        Part(PrimitiveType.Cylinder, new Vector3(0, -0.45f, 0), new Vector3(0.12f, 0.15f, 0.12f), City.Pink, true);
        var lg = new GameObject("spot"); lg.transform.SetParent(root.transform, false);
        spot = lg.AddComponent<Light>();
        spot.type = LightType.Spot; spot.range = 22f; spot.spotAngle = 45f; spot.intensity = 3.5f; spot.color = new Color(0.8f, 0.95f, 1f);
        var au = root.AddComponent<AudioSource>();
        au.clip = Mix.Sound("buzz.wav"); au.loop = true; au.spatialBlend = 1f; au.minDistance = 4f; au.maxDistance = 50f; au.volume = 0.25f; au.pitch = 1.6f; au.Play();
    }

    public static void Fine(int amount, string why)
    {
        int taken = Mathf.Min(amount, Gigs.Eddies);
        Gigs.Eddies -= taken;
        Hud.Popup("-" + amount + " €$", why, new Color(1f, 0.2f, 0.25f));
        tag = "FINED: " + why; tagUntil = Time.unscaledTime + 3f;
        Mix.Play(Mix.Sound("beep.wav"), root != null ? root.transform.position : (Vector3?)null, 0.8f, 0.6f);
        Fx.Flash(root.transform.position, Color.red, 18f, 0.4f);
    }

    public static void Update()
    {
        if (root == null || G.Board == null) return;
        var f = G.Forward; var r = Vector3.Cross(Vector3.up, f);
        var target = G.Pos + f * 3.2f + r * 3.6f + Vector3.up * (2.3f + 0.3f * Mathf.Sin(Time.time * 2f));
        root.transform.position = Vector3.SmoothDamp(root.transform.position, target, ref vel, 0.45f);
        var to = G.Pos + Vector3.up * 0.5f - root.transform.position;
        root.transform.rotation = Quaternion.Slerp(root.transform.rotation, Quaternion.LookRotation(to), Time.deltaTime * 5f);
        flash += Time.deltaTime;
        bool a = ((int)(flash * 5f)) % 2 == 0;
        red.GetComponent<Renderer>().enabled = a; blue.GetComponent<Renderer>().enabled = !a;
        ring.transform.Rotate(0, 200 * Time.deltaTime, 0);
        if (Time.unscaledTime > tagUntil) tag = "NCPB DRONE // SCANNING";
    }

    public static void DrawTag(float k)
    {
        if (root == null || G.Cam == null) return;
        var sp = G.Cam.WorldToScreenPoint(root.transform.position + Vector3.up * 1.0f);
        if (sp.z < 0.5f) return;
        bool fined = Time.unscaledTime < tagUntil;
        var st = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(17 * k) };
        var rect = new Rect(sp.x - 220 * k, Mathf.Max(Screen.height - sp.y - 14 * k, 170 * k), 440 * k, 28 * k);
        var pc = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.7f); GUI.DrawTexture(new Rect(rect.center.x - 150 * k, rect.y, 300 * k, rect.height), Texture2D.whiteTexture);
        GUI.color = fined ? new Color(1f, 0.3f, 0.3f) : City.Cyan; GUI.Label(rect, tag, st);
        GUI.color = pc;
    }
}
