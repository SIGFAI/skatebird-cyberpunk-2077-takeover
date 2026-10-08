// The gigs (a short story in four jobs), the data shards to ride through, eddies (money) and street cred (level).
using System.Collections.Generic;
using Sigf.Kit;
using UnityEngine;

public static class Gigs
{
    public class Shard { public GameObject go, beam; public Vector3 pos; public bool taken; public float phase; }

    public static readonly List<Shard> Shards = new List<Shard>();
    public static int Eddies;
    public static int Collected, Tricks, Stage;
    public static GameObject Beacon;
    public static Vector3 BeaconPos;
    public static bool Done;
    static float stageStart; static int lastChk;
    static readonly string[] Titles = { "WELCOME TO NIGHT CITY", "STYLE OVER SUBSTANCE", "FULL DUMP", "THE DELIVERY" };
    static readonly string[] Goals = {
        "Ride through 3 glowing data shards",
        "Land 5 tricks in the rain",
        "Grab every shard left in the city",
        "Ride into the pink beacon with the chip" };
    static readonly int[] Need = { 3, 5, 0, 1 };
    static readonly int[] Reward = { 500, 750, 1000, 3000 };

    public static int StreetCred => 1 + Eddies / 2500;
    public static float CredProgress => (Eddies % 2500) / 2500f;
    public static string Title => Done ? "NIGHT CITY LEGEND" : Titles[Stage];
    public static string Goal => Done ? "Keep skating. The city is yours, choom." : Goals[Stage];
    public static string Progress
    {
        get
        {
            if (Done) return "";
            switch (Stage)
            {
                case 0: return Mathf.Min(Collected, 3) + " / 3";
                case 1: return Mathf.Min(Tricks, 5) + " / 5";
                case 2: return Collected + " / " + Shards.Count;
                default: return "";
            }
        }
    }

    static bool Ground(float x, float z, out Vector3 p)
    {
        p = Vector3.zero;
        RaycastHit h;
        if (!Physics.Raycast(new Vector3(x, 45f, z), Vector3.down, out h, 80f, ~0, QueryTriggerInteraction.Ignore)) return false;
        if (h.normal.y < 0.9f || h.point.y > 3f || h.point.y < -1f) return false;
        p = h.point; return true;
    }

    public static bool Los(Vector3 a, Vector3 b)
    {
        var d = b - a; float len = d.magnitude; if (len < 3f) return true;
        var o = a + d / len * 1.5f + Vector3.up * 0.6f;
        return !Physics.Raycast(o, d / len, len - 1.5f, ~0, QueryTriggerInteraction.Ignore);
    }

    public static void Build()
    {
        var rng = new System.Random(77);
        var f = G.Forward;
        var r = Vector3.Cross(Vector3.up, f);
        var places = new List<Vector3>();
        var start = G.Pos;
        // a first line of shards ahead of the bird (the first seconds of play)
        for (int i = 0; i < 7; i++)
        {
            var q = G.Pos + f * (9f + i * 6.5f) + r * Mathf.Sin(i * 0.9f) * 2.5f;
            Vector3 p; if (Ground(q.x, q.z, out p) && Los(places.Count > 0 ? places[places.Count - 1] : start, p)) places.Add(p);
        }
        for (int tries = 0; tries < 900 && places.Count < 14; tries++)
        {
            float x = -62f + (float)rng.NextDouble() * 130f, z = -32f + (float)rng.NextDouble() * 115f;
            Vector3 p; if (!Ground(x, z, out p)) continue;
            bool ok = true; foreach (var o in places) if ((o - p).sqrMagnitude < 16f * 16f) ok = false;
            if (!ok) continue;
            bool los = Los(start, p); foreach (var o in places) { if (los) break; if ((o - p).magnitude < 45f && Los(o, p)) los = true; }
            if (los) places.Add(p);
        }
        foreach (var p in places) Shards.Add(MakeShard(p + Vector3.up * 1.0f));
        // the delivery beacon: the farthest reachable shard spot
        Vector3 best = places.Count > 0 ? places[places.Count - 1] : G.Pos; float bd = 0f;
        foreach (var p in places) { float d = (p - G.Pos).magnitude; if (d > bd) { bd = d; best = p; } }
        BeaconPos = best;
        Mix.Log("shards " + Shards.Count + " beacon " + BeaconPos);
        stageStart = Time.unscaledTime;
    }

    static Shard MakeShard(Vector3 pos)
    {
        var s = new Shard { pos = pos, phase = Random.value * 6f };
        s.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(s.go.GetComponent<Collider>());
        s.go.name = "DataShard";
        s.go.transform.position = pos; s.go.transform.localScale = Vector3.one * 0.55f;
        Mix.Unlit(s.go, City.Yellow);
        var halo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(halo.GetComponent<Collider>());
        halo.transform.SetParent(s.go.transform, false); halo.transform.localScale = Vector3.one * 1.7f;
        Mix.Unlit(halo, new Color(0.45f, 0.38f, 0f), null, true);
        s.beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.Destroy(s.beam.GetComponent<Collider>());
        s.beam.name = "ShardBeam";
        s.beam.transform.position = pos + Vector3.up * 8f; s.beam.transform.localScale = new Vector3(0.35f, 8f, 0.35f);
        Mix.Unlit(s.beam, new Color(0.35f, 0.3f, 0f), null, true);
        return s;
    }

    static void MakeBeacon()
    {
        Beacon = new GameObject("DeliveryBeacon");
        Beacon.transform.position = BeaconPos;
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.Destroy(ring.GetComponent<Collider>());
        ring.transform.SetParent(Beacon.transform, false); ring.transform.localPosition = Vector3.up * 0.1f; ring.transform.localScale = new Vector3(5f, 0.05f, 5f);
        Mix.Unlit(ring, new Color(0.8f, 0.1f, 0.5f), null, true);
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Object.Destroy(beam.GetComponent<Collider>());
        beam.transform.SetParent(Beacon.transform, false); beam.transform.localPosition = Vector3.up * 30f; beam.transform.localScale = new Vector3(2.2f, 30f, 2.2f);
        Mix.Unlit(beam, new Color(0.3f, 0.04f, 0.2f), null, true);
        Mix.Glow(BeaconPos + Vector3.up * 2f, City.Pink, 14f, 4f, Beacon.transform);
        Hud.Comm("DEX-BEAK", "Chip's loaded, kid. Bring it to the pink beacon. Don't scratch the paint.", 6f);
    }

    public static void Add(int amount, string why)
    {
        Eddies += amount;
        Hud.Popup("+" + amount + " €$", why);
    }

    public static void OnTrick(string name)
    {
        Tricks++;
        if (Stage == 1) Check();
    }

    static void Check()
    {
        if (Done) return;
        bool complete = false;
        switch (Stage)
        {
            case 0: complete = Collected >= 3; break;
            case 1: complete = Tricks >= 5; break;
            case 2: complete = Collected >= Shards.Count; break;
        }
        if (complete) Complete();
    }

    static void Complete()
    {
        int rw = Reward[Stage];
        Eddies += rw;
        Mix.Say("GIG COMPLETE", 2.5f, City.Yellow, 0.3f, 80);
        Mix.Say("+" + rw + " €$", 2.5f, Color.white, 0.4f, 56);
        Mix.Play(Mix.Sound("beep.wav"), null, 0.9f);
        Mix.Play(Mix.Sound("drop.wav"), null, 0.7f);
        Fx.Glitch(0.5f);
        Fx.Fireworks(G.Pos + Vector3.up * 2f);
        Stage++;
        stageStart = Time.unscaledTime;
        if (Stage == 1) Hud.Comm("DEX-BEAK", "Not bad. Now show the street some style: five tricks, no whining.", 5.5f);
        else if (Stage == 2) Hud.Comm("JOHNNY SILVERBEAK", "Wake up, Samurai. Those shards are still out there. All of them.", 5.5f);
        else if (Stage == 3) MakeBeacon();
        else
        {
            Done = true;
            Hud.Comm("JOHNNY SILVERBEAK", "You did it, choom. Night City belongs to the birds now.", 7f);
            Mix.After(1.2f, () => Mix.Say("NIGHT CITY LEGEND", 4f, City.Pink, 0.3f, 90));
            Mix.After(0.5f, () => Fx.Fireworks(BeaconPos + Vector3.up * 8f));
            Mix.After(1.5f, () => Fx.Fireworks(G.Pos + Vector3.up * 6f));
        }
    }

    public static void Update()
    {
        if (G.Board == null) return;
        float t = Time.unscaledTime;
        var pos = G.Pos;
        foreach (var s in Shards)
        {
            if (s.taken) continue;
            float ph = t * 2f + s.phase;
            s.go.transform.position = s.pos + Vector3.up * (0.25f * Mathf.Sin(ph));
            s.go.transform.rotation = Quaternion.Euler(35f, t * 120f + s.phase * 40f, 35f);
            var d = pos - s.pos;
            if (new Vector2(d.x, d.z).magnitude < 2.3f && Mathf.Abs(d.y) < 3.5f) Take(s);
        }
        if (!Done && Stage < 3 && t > stageStart + 3f && (int)(t * 2f) != lastChk) { lastChk = (int)(t * 2f); Check(); }
        if (Beacon != null && Stage == 3 && !Done)
        {
            var d = pos - BeaconPos;
            if (new Vector2(d.x, d.z).magnitude < 3.2f && Mathf.Abs(d.y) < 4f) { Object.Destroy(Beacon); Beacon = null; Check2(); }
        }
    }

    static void Check2() { Complete(); }

    static void Take(Shard s)
    {
        s.taken = true;
        Collected++;
        Mix.Play(Mix.Sound("eddie.wav"), s.pos, 1f, 1f + Mathf.Min(Collected, 12) * 0.03f);
        Mix.Burst(s.pos, City.Yellow, 22, 5f, 0.12f, 1.2f);
        Mix.Burst(s.pos, City.Cyan, 10, 4f, 0.1f, 1f);
        Object.Destroy(s.go); Object.Destroy(s.beam);
        Fx.Flash(s.pos, City.Yellow);
        Add(100, "DATA SHARD");
        if (Collected == 1) Hud.Comm("JOHNNY SILVERBEAK", "First shard. Cute. Wait till you see what's inside the chip.", 4.5f);
        if (Stage == 0 || Stage == 2) Check();
    }
}
