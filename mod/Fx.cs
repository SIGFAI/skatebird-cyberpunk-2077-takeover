// Effects: glitch bursts, light flashes, fireworks, the neon board trail, the flatline screen.
using Sigf.Kit;
using UnityEngine;

public static class Fx
{
    public static float GlitchUntil, FlatlineUntil;
    static TrailRenderer trail;

    public static void Glitch(float seconds) { GlitchUntil = Mathf.Max(GlitchUntil, Time.unscaledTime + seconds); }

    public static void Flash(Vector3 pos, Color c, float range = 14f, float life = 0.35f)
    {
        var l = Mix.Glow(pos, c, range * 0.6f, 2.2f);
        Object.Destroy(l.gameObject, life);
    }

    public static void Fireworks(Vector3 pos)
    {
        Color[] cs = { City.Pink, City.Cyan, City.Yellow };
        for (int i = 0; i < 3; i++)
        {
            var p = pos + new Vector3(Random.Range(-4f, 4f), Random.Range(0f, 5f), Random.Range(-4f, 4f));
            Mix.After(i * 0.25f, () =>
            {
                Mix.Burst(p, cs[Random.Range(0, 3)], 40, 11f, 0.22f, 2f);
                Mix.Burst(p, Color.white, 14, 7f, 0.14f, 1.5f);
                Flash(p, cs[Random.Range(0, 3)], 30f, 0.5f);
            });
        }
    }

    public static void BoardTrail()
    {
        if (G.Body == null) return;
        var go = new GameObject("NeonBoardTrail");
        go.transform.SetParent(G.Body.transform, false);
        go.transform.localPosition = new Vector3(0f, 0.12f, -0.3f);
        trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.9f; trail.startWidth = 0.32f; trail.endWidth = 0f; trail.minVertexDistance = 0.1f;
        var m = new Material(Shader.Find("Legacy Shaders/Particles/Additive")) { mainTexture = Texture2D.whiteTexture };
        m.SetColor("_TintColor", new Color(0.9f, 0.9f, 0.9f, 0.9f));
        trail.material = m;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(City.Pink, 0f), new GradientColorKey(City.Cyan, 0.6f), new GradientColorKey(new Color(0.3f, 0.2f, 1f), 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.6f, 0.5f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = g;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    /// <summary>The finishing burst of a trick: neon sparks, a flash and a screen glitch.</summary>
    public static void Trick(float intensity)
    {
        var p = G.Pos + Vector3.up * 0.3f;
        Mix.Burst(p, City.Pink, (int)(16 * intensity), 4f, 0.07f, 1.3f);
        Mix.Burst(p, City.Cyan, (int)(16 * intensity), 4f, 0.07f, 1.3f);
        Mix.Burst(p, City.Yellow, (int)(8 * intensity), 5f, 0.06f, 1.1f);
        Flash(p + Vector3.up, Random.value < 0.5f ? City.Pink : City.Cyan, 12f, 0.25f);
        Glitch(0.22f);
        Mix.Play(Mix.Sound("glitch2.wav"), null, 0.8f, Random.Range(0.9f, 1.2f));
    }

    public static void Flatline()
    {
        FlatlineUntil = Time.unscaledTime + 2.2f;
        Glitch(1.0f);
        Mix.Play(Mix.Sound("glitch.wav"), null, 1f, 0.7f);
        Mix.Play(Mix.Sound("drop.wav"), null, 0.8f, 1.2f);
    }
}
