// Night City around the playground: lit towers, neon signs, the skyline cylinder, glowing rails, rain.
using System.Collections.Generic;
using Sigf.Kit;
using UnityEngine;

public static class City
{
    public static readonly Color Pink = new Color(1f, 0.12f, 0.62f), Cyan = new Color(0.1f, 0.9f, 1f), Yellow = new Color(1f, 0.9f, 0.1f);
    public static Vector3 Center = new Vector3(3f, 0f, 25f);
    static Material railPink, railCyan;
    static readonly List<Material> flicker = new List<Material>();
    static readonly List<float> flickerPhase = new List<float>();
    static Transform rain;

    public static void Build()
    {
        Retheme();
        Skyline();
        Towers();
        Rails();
        Rain();
    }

    static void Retheme()
    {
        foreach (var l in Object.FindObjectsOfType<Light>())
            if (l.type == LightType.Directional) { l.color = new Color(0.85f, 0.45f, 1f); l.intensity = 0.85f; }
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.25f, 0.62f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.30f, 0.08f, 0.42f); RenderSettings.fogDensity = 0.004f;
        if (RenderSettings.skybox != null)
        {
            var m = new Material(RenderSettings.skybox);
            if (m.HasProperty("_Tint")) m.SetColor("_Tint", new Color(0.2f, 0.03f, 0.38f));
            if (m.HasProperty("_Exposure")) m.SetFloat("_Exposure", 0.75f);
            RenderSettings.skybox = m;
        }
    }

    static void Skyline()
    {
        var tex = Mix.Texture("skyline.png");
        tex.wrapMode = TextureWrapMode.Repeat;
        const int seg = 72; const float R = 330f, bottom = -30f;
        float circ = 2f * Mathf.PI * R, h = circ / 2f * 304f / 2048f * 2f; // texture repeats twice around
        var verts = new Vector3[(seg + 1) * 2]; var uv = new Vector2[verts.Length]; var tri = new int[seg * 6];
        for (int i = 0; i <= seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg; var d = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
            verts[i * 2] = Center + d * R + Vector3.up * bottom; verts[i * 2 + 1] = Center + d * R + Vector3.up * (bottom + h * 0.8f);
            uv[i * 2] = new Vector2(2f * i / seg, 0); uv[i * 2 + 1] = new Vector2(2f * i / seg, 1);
        }
        for (int i = 0; i < seg; i++) { int k = i * 6, v = i * 2; tri[k] = v; tri[k + 1] = v + 1; tri[k + 2] = v + 2; tri[k + 3] = v + 1; tri[k + 4] = v + 3; tri[k + 5] = v + 2; }
        var mesh = new Mesh { vertices = verts, uv = uv, triangles = tri };
        mesh.RecalculateBounds();
        var go = new GameObject("NightCitySkyline"); go.AddComponent<MeshFilter>().mesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = tex, color = new Color(1.2f, 0.9f, 1.3f, 1f) };
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static void Towers()
    {
        var rng = new System.Random(2077);
        string[] texs = { "tower_a.png", "tower_b.png", "tower_c.png" };
        string[] signs = { "sign_samurai.png", "sign_nightcity.png", "sign_chrome.png", "sign_kibble.png", "sign_seeds.png", "sign_ncpb.png", "sign_bd.png", "sign2_neon_kanji.png", "sign5_sushi.png", "sign4_open.png" };
        int n = 46, si = 0;
        for (int i = 0; i < n; i++)
        {
            float ang = (i + (float)rng.NextDouble() * 0.6f) * Mathf.PI * 2f / n;
            float rad = 185f + (float)rng.NextDouble() * 60f;
            float w = 24f + (float)rng.NextDouble() * 26f, dp = 24f + (float)rng.NextDouble() * 26f, ht = 120f + (float)rng.NextDouble() * 220f;
            var dir = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang));
            var pos = Center + dir * rad;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(go.GetComponent<Collider>());
            go.name = "NightCityTower";
            go.transform.position = pos + Vector3.up * (ht / 2f - 30f);
            go.transform.localScale = new Vector3(w, ht, dp);
            go.transform.rotation = Quaternion.LookRotation(-dir);
            var t = Mix.Texture(texs[rng.Next(texs.Length)]); t.wrapMode = TextureWrapMode.Repeat;
            Mix.Unlit(go, new Color(1.4f, 1.4f, 1.4f), t);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.material.mainTextureScale = new Vector2(w / 34f, ht / 44f);
            // a lit crown on top
            var crown = Mix.Shape(PrimitiveType.Cube, go.transform.position + Vector3.up * (ht / 2f + 1f), new Vector3(w + 1f, 2f, dp + 1f), Color.white, false);
            Mix.Unlit(crown, i % 2 == 0 ? Pink : Cyan);
            // a sign on the inner face of every other tower
            if (i % 2 == 0)
            {
                string sn = signs[si++ % signs.Length];
                var st = Mix.Texture(sn);
                float sw = Mathf.Min(w * 0.9f, 46f), sh = sw * st.height / st.width;
                if (sh > ht * 0.35f) { sh = ht * 0.35f; sw = sh * st.width / st.height; }
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Object.Destroy(q.GetComponent<Collider>());
                q.name = "NightCitySign";
                q.transform.position = pos - dir * (dp / 2f + 0.3f) + Vector3.up * (20f + (float)rng.NextDouble() * Mathf.Min(70f, ht - 50f));
                q.transform.rotation = Quaternion.LookRotation(dir);
                q.transform.localScale = new Vector3(sw, sh, 1f);
                Mix.Unlit(q, Color.white, st);
                foreach (var r in q.GetComponentsInChildren<Renderer>()) { flicker.Add(r.material); flickerPhase.Add((float)rng.NextDouble() * 20f); }
            }
        }
    }

    /// <summary>A glowing arch with the NIGHT CITY sign over the bird's path.</summary>
    public static void Gate(Vector3 at, Vector3 fwd)
    {
        RaycastHit h; float gy = at.y;
        if (Physics.Raycast(at + Vector3.up * 20f, Vector3.down, out h, 60f, ~0, QueryTriggerInteraction.Ignore)) gy = h.point.y;
        var right = Vector3.Cross(Vector3.up, fwd).normalized;
        var root = new GameObject("NightCityGate"); root.transform.position = new Vector3(at.x, gy, at.z);
        root.transform.rotation = Quaternion.LookRotation(fwd);
        const float half = 7.5f, hgt = 9f;
        for (int sgn = -1; sgn <= 1; sgn += 2)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.Destroy(p.GetComponent<Collider>());
            p.transform.SetParent(root.transform, false); p.transform.localPosition = new Vector3(sgn * half, hgt / 2f, 0); p.transform.localScale = new Vector3(0.7f, hgt, 0.7f);
            Mix.Unlit(p, sgn < 0 ? Pink : Cyan);
            var l = Mix.Glow(root.transform.position + right * sgn * half + Vector3.up * 2f, sgn < 0 ? Pink : Cyan, 12f, 3f, root.transform);
        }
        var bar = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.Destroy(bar.GetComponent<Collider>());
        bar.transform.SetParent(root.transform, false); bar.transform.localPosition = new Vector3(0, hgt + 0.2f, 0); bar.transform.localScale = new Vector3(half * 2f + 0.7f, 0.5f, 0.7f);
        Mix.Unlit(bar, Yellow);
        var tex = Mix.Texture("sign_nightcity.png");
        for (int side = 0; side < 2; side++)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Object.Destroy(q.GetComponent<Collider>());
            q.transform.SetParent(root.transform, false);
            q.transform.localPosition = new Vector3(0, hgt - 2.2f, side == 0 ? 0.4f : -0.4f);
            q.transform.localRotation = Quaternion.Euler(0, side == 0 ? 0f : 180f, 0);
            q.transform.localScale = new Vector3(12.5f, 12.5f * tex.height / tex.width, 1f);
            Mix.Unlit(q, Color.white, tex);
            foreach (var r in q.GetComponentsInChildren<Renderer>()) { flicker.Add(r.material); flickerPhase.Add(side * 3f); }
        }
    }

    static void Rails()
    {
        railPink = new Material(Shader.Find("Unlit/Color")) { color = Pink };
        railCyan = new Material(Shader.Find("Unlit/Color")) { color = Cyan };
        var seen = new HashSet<string>(); int k = 0;
        foreach (var rail in Object.FindObjectsOfType<Skatebirb.GrindableRail>())
        {
            var nx = rail.nextGrindable; if (nx == null) continue;
            Vector3 a = rail.transform.position, b = nx.transform.position;
            string key = a.ToString("F1") + b.ToString("F1"); if (!seen.Add(key)) continue;
            float len = Vector3.Distance(a, b); if (len < 0.2f) continue;
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(c.GetComponent<Collider>()); c.name = "NeonRail";
            c.transform.position = (a + b) / 2f + Vector3.up * 0.04f;
            c.transform.up = (b - a).normalized;
            c.transform.localScale = new Vector3(0.16f, len / 2f, 0.16f);
            var r = c.GetComponent<Renderer>(); r.sharedMaterial = (k++ % 2 == 0) ? railPink : railCyan;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Mix.Log("neon rails " + k);
    }

    static void Rain()
    {
        var go = new GameObject("NightCityRain"); rain = go.transform;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; main.loop = true; main.startLifetime = 0.9f; main.startSpeed = 0f; main.startSize = 0.025f;
        main.startColor = new Color(0.55f, 0.85f, 1f, 0.5f); main.maxParticles = 2500; main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission; em.rateOverTime = 1800f;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(28f, 0.5f, 28f);
        var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(-2f); vel.y = new ParticleSystem.MinMaxCurve(-28f); vel.z = new ParticleSystem.MinMaxCurve(0f);
        var pr = go.GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Stretch; pr.velocityScale = 0.03f; pr.lengthScale = 2f;
        pr.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive")) { mainTexture = Texture2D.whiteTexture };
        pr.material.SetColor("_TintColor", new Color(0.4f, 0.4f, 0.5f, 0.4f));
    }

    public static void Update()
    {
        float t = Time.unscaledTime;
        float pulse = 0.65f + 0.35f * Mathf.Sin(t * 5f);
        if (railPink != null) { railPink.color = Pink * (0.7f + 0.5f * pulse); railCyan.color = Cyan * (1.2f - 0.5f * pulse); }
        for (int i = 0; i < flicker.Count; i++)
        {
            if (flicker[i] == null) continue;
            float f = 1f; float ph = t * 2f + flickerPhase[i];
            if (Mathf.Sin(ph * 3.1f) * Mathf.Sin(ph * 7.3f) > 0.93f) f = 0.25f;
            flicker[i].color = new Color(f, f, f, 1f) * (1f + 0.15f * Mathf.Sin(ph));
        }
        if (rain != null && G.Board != null) rain.position = G.Pos + Vector3.up * 14f + (G.Cam != null ? G.Cam.transform.forward * 8f : Vector3.zero);
    }
}
