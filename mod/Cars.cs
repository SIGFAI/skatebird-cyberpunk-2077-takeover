// Flying cars of Night City: dark bodies, neon underglow, light trails, a whoosh as they pass; some are NCPB police cars.
using System.Collections.Generic;
using Sigf.Kit;
using UnityEngine;

public class FlyingCar : MonoBehaviour
{
    public Vector3 vel;
    public float life = 14f;
    public bool police;
    Renderer redLight, blueLight;
    float flash;

    void Update()
    {
        transform.position += vel * Time.deltaTime;
        life -= Time.deltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }
        if (police && redLight != null)
        {
            flash += Time.deltaTime;
            bool a = ((int)(flash * 6f)) % 2 == 0;
            redLight.enabled = a; blueLight.enabled = !a;
        }
    }

    public void Setup(Renderer red, Renderer blue) { redLight = red; blueLight = blue; }
}

public static class Cars
{
    static readonly Color[] glow = { City.Pink, City.Cyan, City.Yellow, new Color(1f, 0.4f, 0.1f), new Color(0.6f, 0.3f, 1f) };
    static int count;

    static GameObject Part(Transform parent, PrimitiveType t, Vector3 lp, Vector3 sc, Color c, bool unlit)
    {
        var g = GameObject.CreatePrimitive(t);
        Object.Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = lp; g.transform.localScale = sc;
        if (unlit) Mix.Unlit(g, c); else Mix.Paint(g, c);
        return g;
    }

    /// <summary>A car moving along dir at speed from pos. Scale 1 = about 4 units long.</summary>
    public static FlyingCar Spawn(Vector3 pos, Vector3 dir, float speed, float scale, bool police, bool withLight = true)
    {
        var root = new GameObject(police ? "NightCityPoliceCar" : "NightCityCar");
        root.transform.position = pos;
        var body = new GameObject("body"); body.transform.SetParent(root.transform, false);
        body.transform.localScale = Vector3.one * scale;
        var car = root.AddComponent<FlyingCar>();
        car.vel = dir.normalized * speed; car.police = police;
        Color gl = glow[count++ % glow.Length];
        Color hull = police ? new Color(0.9f, 0.9f, 0.95f) : Color.Lerp(new Color(0.14f, 0.1f, 0.26f), gl, 0.12f);
        // local +Z is the nose
        Part(body.transform, PrimitiveType.Cube, Vector3.zero, new Vector3(1.9f, 0.55f, 4.3f), hull, false);
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, 0.5f, -0.3f), new Vector3(1.5f, 0.55f, 2.1f), police ? new Color(0.15f, 0.2f, 0.4f) : new Color(0.1f, 0.5f, 0.7f), true);
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, 0.82f, -0.3f), new Vector3(1.2f, 0.06f, 1.8f), gl, true);                    // roof neon
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, 0.62f, -2.0f), new Vector3(1.9f, 0.08f, 0.5f), gl, true);                    // spoiler
        Part(body.transform, PrimitiveType.Cube, new Vector3(0.45f, 0.08f, 5.6f), new Vector3(0.45f, 0.3f, 7f), new Color(0.35f, 0.32f, 0.18f), false).GetComponent<Renderer>().enabled = false;
        for (int sgn = -1; sgn <= 1; sgn += 2)
        {
            var bm = Part(body.transform, PrimitiveType.Cube, new Vector3(0.55f * sgn, 0.08f, 5.7f), new Vector3(0.5f, 0.3f, 7.2f), new Color(0.3f, 0.28f, 0.14f), false);
            Mix.Unlit(bm, new Color(0.3f, 0.28f, 0.14f), null, true);
        }
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, -0.34f, 0), new Vector3(1.7f, 0.08f, 3.9f), gl, true);       // underglow
        Part(body.transform, PrimitiveType.Cube, new Vector3(0.55f, 0.08f, 2.16f), new Vector3(0.5f, 0.14f, 0.05f), Color.white, true);   // headlights
        Part(body.transform, PrimitiveType.Cube, new Vector3(-0.55f, 0.08f, 2.16f), new Vector3(0.5f, 0.14f, 0.05f), Color.white, true);
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, 0.1f, -2.16f), new Vector3(1.6f, 0.1f, 0.05f), new Color(1f, 0.1f, 0.1f), true); // tail bar
        Part(body.transform, PrimitiveType.Cube, new Vector3(0, 0.02f, 0), new Vector3(1.93f, 0.05f, 4.33f), gl, true);       // neon belt line
        Renderer red = null, blue = null;
        if (police)
        {
            red = Part(body.transform, PrimitiveType.Cube, new Vector3(0.4f, 0.85f, -0.3f), new Vector3(0.6f, 0.18f, 0.35f), new Color(1f, 0.05f, 0.05f), true).GetComponent<Renderer>();
            blue = Part(body.transform, PrimitiveType.Cube, new Vector3(-0.4f, 0.85f, -0.3f), new Vector3(0.6f, 0.18f, 0.35f), new Color(0.1f, 0.3f, 1f), true).GetComponent<Renderer>();
            car.Setup(red, blue);
        }
        body.transform.rotation = Quaternion.LookRotation(dir.normalized);
        // light trails behind each tail
        AddTrail(root, body.transform.TransformPoint(new Vector3(0.7f, 0.1f, -2.1f) * 1f), gl, 0.4f * scale, 2.2f);
        AddTrail(root, body.transform.TransformPoint(new Vector3(-0.7f, 0.1f, -2.1f) * 1f), gl, 0.4f * scale, 2.2f);
        if (withLight) Mix.Glow(pos, police ? Color.red : gl, 11f * scale, 1.3f, root.transform);
        var au = root.AddComponent<AudioSource>();
        au.clip = Mix.Sound("whoosh.wav"); au.loop = true; au.spatialBlend = 1f; au.minDistance = 12f; au.maxDistance = 140f;
        au.volume = 0.45f; au.pitch = Random.Range(0.8f, 1.3f); au.Play();
        return car;
    }

    static void AddTrail(GameObject root, Vector3 at, Color c, float width, float time)
    {
        var g = new GameObject("trail"); g.transform.SetParent(root.transform, false); g.transform.position = at;
        var tr = g.AddComponent<TrailRenderer>();
        tr.time = time; tr.startWidth = width; tr.endWidth = 0f; tr.minVertexDistance = 0.2f;
        var m = new Material(Shader.Find("Legacy Shaders/Particles/Additive")) { mainTexture = Texture2D.whiteTexture };
        m.SetColor("_TintColor", c * 0.5f);
        tr.material = m;
        tr.startColor = c; tr.endColor = new Color(c.r, c.g, c.b, 0f);
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static bool Seen(Camera cam, Vector3 p)
    {
        var vp = cam.WorldToViewportPoint(p);
        if (vp.z < 10f || vp.x < 0.04f || vp.x > 0.96f || vp.y < 0.3f || vp.y > 0.8f) return false;
        return !Physics.Linecast(cam.transform.position, p, ~0, QueryTriggerInteraction.Ignore);
    }

    /// <summary>A car that crosses the screen in view of the camera (clear line of sight, below the HUD).</summary>
    public static FlyingCar Crossing(bool leftToRight, bool police = false, float scale = 1.7f)
    {
        var cam = G.Cam; if (cam == null) return null;
        var f = cam.transform.forward; f.y = 0f; f.Normalize();
        var r = Vector3.Cross(Vector3.up, f);
        float dirSign = leftToRight ? 1f : -1f;
        float speed = 20f;
        var drift = G.Velocity; drift.y = 0f; drift *= 0.9f;
        for (int tries = 0; tries < 60; tries++)
        {
            float d = Random.Range(30f, 80f), h = Random.Range(2f, 36f), lat = d * Random.Range(0.4f, 0.6f);
            var mid = cam.transform.position + f * d + Vector3.up * (h - 2f);
            var a = mid - r * lat * dirSign; var b = mid + r * lat * dirSign;
            if (!Seen(cam, a) || !Seen(cam, mid) || !Seen(cam, b)) continue;
            var vv = r * dirSign * speed + drift;
            var c = Spawn(a, vv, vv.magnitude, Mathf.Clamp(d / 24f, 1.1f, 3f) * scale / 1.7f, police);
            c.life = 2f * lat / speed + 0.3f;
            return c;
        }
        return null;
    }

    static float nextAmbient = 4f;
    public static void Update()
    {
        nextAmbient -= Time.deltaTime;
        if (nextAmbient > 0f) return;
        nextAmbient = Random.Range(3.5f, 6f);
        if (Object.FindObjectsOfType<FlyingCar>().Length > 3) return;
        Crossing(Random.value < 0.5f, Random.value < 0.25f, Random.Range(1.5f, 2.6f));
    }
}
