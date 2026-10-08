// Cyberpunk 2077 Takeover: SkateBIRD at night in Night City. Neon towers and signs, flying cars, rain, a glowing HUD,
// radio calls from Johnny Silverbeak, data shards to ride through, four gigs, eddies for every trick, a flatline screen on a bail.
using System.Collections;
using Sigf.Kit;
using UnityEngine;

public class SigfMod : MixMod
{
    bool wasBailed, started;
    float nextQuip;
    string lastTrick = "TRICK";
    AudioSource music;

    static readonly string[] Quips = {
        "Nice flip, choom. Corpos would pay good eddies for that.",
        "Preem! Even the NCPB drones stopped to watch.",
        "Chrome feathers and a deck. That's the whole dream.",
        "Hold that combo, Samurai. The city is watching.",
        "I've seen cyberpsychos with worse balance, kid.",
        "Rain, neon, kickflips. This is what heaven looks like." };

    public override void OnLoad() => G.StartLevel = "playground";

    public override void OnReady()
    {
        if (started) return;
        started = true;
        City.Build();
        var d = G.Bird.OurDecorator;
        d.ChangeClothing("Wraparound Glasses", true);
        d.ChangeClothing("Mohawk", true);
        var b = G.Body.transform;
        Mix.Glow(G.Pos + b.right * 1.0f + Vector3.up * 0.7f, City.Pink, 5.5f, 2.2f, b);
        Mix.Glow(G.Pos - b.right * 1.0f + Vector3.up * 0.7f, City.Cyan, 5.5f, 2.2f, b);
        Mix.Glow(G.Pos + Vector3.up * 0.1f, City.Pink, 4f, 2.5f, b);
        Fx.BoardTrail();
        Gigs.Build();
        Drone.Build();
        City.Gate(G.Pos + G.Forward * 19f, G.Forward);

        music = Mix.Play(Mix.Sound("music.wav"), null, 0.28f);
        music.loop = true;
        Mix.Play(Mix.Sound("buzz.wav"), null, 0.8f);
        Intro();
                Mix.After(5.5f, () => Hud.Comm("JOHNNY SILVERBEAK", "Wake up, Samurai. We have a city to shred. Grab those glowing shards.", 6f));

        G.OnTrick(t =>
        {
            string n = t != null ? t.Name : "Trick";
            lastTrick = n.ToUpper();
            Gigs.OnTrick(n);
            Fx.Trick(n == "Screm" ? 2f : 1f);
            if (n == "Screm") Drone.Fine(50, "NOISE COMPLAINT");
            if (Time.unscaledTime > nextQuip && Random.value < 0.6f)
            {
                nextQuip = Time.unscaledTime + 11f;
                Hud.Comm("JOHNNY SILVERBEAK", n == "Screm" ? "SCREAM LOUDER, Samurai! Make the towers shake!" : Quips[Random.Range(0, Quips.Length)], 4.5f);
            }
        });
        G.OnScore(p =>
        {
            int eddies = Mathf.Max(10, (int)p);
            Gigs.Add(eddies, lastTrick);
        });
    }

    static void Intro()
    {
        Mix.Say("CYBERPUNK 2077", 4f, City.Yellow, 0.2f, 104);
        Mix.Say("T A K E O V E R", 4f, City.Cyan, 0.31f, 70);
        Mix.After(2.6f, () => Mix.Say("Wake up, birb. Night City is waiting.", 3f, Color.white, 0.44f, 40));
        Fx.Glitch(0.8f);
    }

    public override void OnUpdate()
    {
        City.Update();
        Cars.Update();
        Gigs.Update();
        Drone.Update();
        bool bailed = G.Bailed;
        if (bailed && !wasBailed)
        {
            Fx.Flatline();
            Drone.Fine(100, "PUBLIC DISTURBANCE");
            Hud.Comm("JOHNNY SILVERBEAK", "Flatlined. Classic gonk move. Get up, Samurai.", 4f);
        }
        wasBailed = bailed;
    }

    public override void OnGUI() { Hud.Draw(); }

    public override IEnumerator Demo() => RealDemo();

    float demoT0; Vector3 startPos; Quaternion startRot;

    IEnumerator RealDemo()
    {
        demoT0 = Time.unscaledTime; startPos = G.Pos; startRot = G.Body.rotation;
        Intro();
        Mix.After(3.6f, () => Cars.Crossing(true));
        Mix.After(7.0f, () => Cars.Crossing(false, true));
        // ride the line of shards
        for (int i = 0; i < 10; i++)
        {
            G.Boost(5f);
            Mix.Log("demo ride speed " + G.Speed.ToString("F1") + " pos " + G.Pos + " fwd " + G.Forward + " shards " + Gigs.Collected);
            yield return Mix.Wait(0.7f);
        }
        G.Launch(5f); yield return Mix.Wait(0.3f); G.Flip("Kickflip");
        yield return Mix.Wait(1.3f);
        for (int i = 0; i < 3; i++) { if (G.Speed < 9f) G.Boost(5f); yield return Mix.Wait(0.6f); }
        string[] flips = { "Heelflip", "Pop Shuvit", "Kickflip", "Heelflip" };
        foreach (var f in flips)
        {
            G.GetUp();
            G.Launch(5.5f); yield return Mix.Wait(0.3f); G.Flip(f); G.Grab("Monch");
            yield return Mix.Wait(1.2f);
            if (G.Speed < 9f) G.Boost(5f);
        }
        G.Launch(6f); G.Screm();
        yield return Mix.Wait(1.5f);
        G.Bail();
        yield return Mix.Wait(3f);
        G.GetUp();
        yield return Mix.Wait(1.5f);
        // keep hunting shards: face the nearest one, ride, throw a trick on the way
        while (Time.unscaledTime - demoT0 < 66f)
        {
            Gigs.Shard best = null; float bd = 1e9f;
            foreach (var sh in Gigs.Shards) { if (sh.taken) continue; float dd = (sh.pos - G.Pos).magnitude; if (dd < bd && Gigs.Los(G.Pos, sh.pos)) { bd = dd; best = sh; } }
            if (best == null && Gigs.Beacon != null) { yield return RideTo(Gigs.BeaconPos, 12f); continue; }
            if (best == null) { G.Teleport(startPos, startRot); yield return Mix.Wait(1.2f); continue; }
            yield return RideTo(best.pos, 9f);
        }
        yield return Mix.Wait(2f);
    }

    IEnumerator RideTo(Vector3 target, float timeout)
    {
        float t0 = Time.unscaledTime, nextTrick = t0 + 2.5f;
        while (Time.unscaledTime - t0 < timeout)
        {
            G.GetUp();
            var d = target - G.Pos; d.y = 0f;
            if (d.magnitude < 3f) yield break;
            if (Vector3.Angle(G.Forward, d) > 20f) { G.Teleport(G.Pos + Vector3.up * 0.1f, Quaternion.LookRotation(d.normalized)); yield return Mix.Wait(0.1f); }
            if (G.Speed < 9f) G.Boost(4.5f);
            if (Time.unscaledTime > nextTrick && d.magnitude > 14f)
            {
                nextTrick = Time.unscaledTime + 3f;
                G.Launch(5.5f); yield return Mix.Wait(0.3f); G.Flip(Random.value < 0.5f ? "Kickflip" : "Heelflip");
            }
            yield return Mix.Wait(0.35f);
        }
    }
}
