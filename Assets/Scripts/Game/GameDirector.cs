using System.Collections.Generic;
using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Runs the game: builds the world, moves it while playing, spawns gates, coins and bats from
    /// the <see cref="GatePlanner"/>, checks collisions on the play plane, and drives the HUD,
    /// audio, weather and effects through the <see cref="GameFlow"/> phases. Rules live in
    /// FlappyDragon.Core.
    /// </summary>
    public sealed class GameDirector : MonoBehaviour
    {
        public const float GroundTop = -4.65f;
        public const float DragonX = -3.6f;
        const float HoverHeight = 0.6f;
        const float InputGraceSeconds = 0.2f;
        const float FireCooldownSeconds = 2.2f;
        const int RainStartsAt = 20;

        [SerializeField] GameArt art;
        [SerializeField] Hud hud;
        [SerializeField] CameraRig cameraRig;
        [SerializeField] Light sun;

        readonly List<Gate> gates = new List<Gate>();
        readonly List<Coin> coins = new List<Coin>();
        readonly List<Bat> bats = new List<Bat>();
        readonly List<Fireball> fireballs = new List<Fireball>();
        readonly Cooldown fire = new Cooldown(FireCooldownSeconds);

        GameInput input;
        AudioDirector audioDirector;
        Scenery scenery;
        Transform course;
        GatePlanner planner;
        ParticleSystem rain;
        float distanceToNextGate;
        float speed;
        float phaseTime;
        float runTime;
        float storm;
        float sunIntensity;
        Color fogColor;
        Material sky;
        float skyExposure;
        int gateSeed;
        bool fireRequested;

        public GameFlow Flow { get; } = new GameFlow();

        public ScoreBoard Board { get; private set; }

        public DifficultyCurve Curve { get; } = DifficultyCurve.Default;

        public Dragon Player { get; private set; }

        public IReadOnlyList<Gate> Gates => gates;

        /// <summary>Flies by itself (used by the screenshot scenarios).</summary>
        public bool Autopilot { get; set; }

        /// <summary>Gates spawn just beyond the right edge of the view, whatever its width.</summary>
        float SpawnX => cameraRig.RightEdge + 2.5f;

        float DespawnX => cameraRig.LeftEdge - 3f;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Board = new ScoreBoard(new PlayerPrefsBestScoreStore());
            input = new GameInput(hud.IsOverButton);
            planner = new GatePlanner(System.Environment.TickCount, GroundTop + 1.05f, 4.9f, maxStep: 2.6f);

            audioDirector = gameObject.AddComponent<AudioDirector>();
            audioDirector.Init(art.Music);

            scenery = new GameObject("Scenery").AddComponent<Scenery>();
            scenery.Build(art, GroundTop);
            course = new GameObject("Course").transform;
            var settings = new FlightSettings { Floor = GroundTop + Dragon.Radius };
            Player = Dragon.Create(transform, art, settings, DragonX);
            rain = Effects.Rain(cameraRig.transform, art.ParticleAlpha, 60f);
            sunIntensity = sun.intensity;
            // A private copy of the sky, so darkening it for rain never touches the asset.
            RenderSettings.skybox = sky = new Material(RenderSettings.skybox);
            skyExposure = sky.GetFloat("_Exposure");
            fogColor = RenderSettings.fogColor;

            Board.ScoreChanged += OnScoreChanged;
            Flow.PhaseChanged += OnPhaseChanged;
            hud.PlayClicked += () => Flow.StartRun();
            hud.PauseClicked += () => Flow.TogglePause();
            hud.ResumeClicked += () => Flow.TogglePause();
            hud.RestartClicked += () => Flow.StartRun();
            hud.TitleClicked += () => Flow.ToTitle();
            hud.FireClicked += () => fireRequested = true;
        }

        void Start()
        {
            ResetRun();
            hud.SetBest(Board.Best);
            hud.ShowPhase(Flow.Phase, fireAvailable: true);
        }

        void OnScoreChanged(int score)
        {
            hud.SetScore(score);
        }

        void OnPhaseChanged(GamePhase previous, GamePhase next)
        {
            phaseTime = 0f;
            switch (next)
            {
                case GamePhase.Title:
                    ResetRun();
                    break;
                case GamePhase.Ready:
                    ResetRun();
                    break;
                case GamePhase.Crashed:
                    audioDirector.Play(art.Hit, 1f, 0.02f);
                    audioDirector.Play(art.Fall, 0.7f, 0.02f);
                    cameraRig.Shake(0.9f);
                    hud.Flash();
                    Effects.Burst(Player.transform.position, art.ParticleAlpha, new Color(1f, 0.45f, 0.2f), new Color(1f, 0.85f, 0.4f), 26, 6f, 0.45f, 0.8f, gravity: 1f);
                    break;
                case GamePhase.GameOver:
                    Board.EndRun();
                    hud.ShowResults(Board);
                    if (Board.IsNewBest || Medals.For(Board.Score) != Medal.None)
                    {
                        audioDirector.Play(art.Medal, 0.8f, 0f);
                    }
                    break;
            }
            if (next == GamePhase.Paused || previous == GamePhase.Paused || next == GamePhase.Title || next == GamePhase.Ready && previous != GamePhase.Title)
            {
                audioDirector.Play(art.Click, 0.6f, 0f);
            }
            audioDirector.SetMusicDucked(next == GamePhase.Paused || next == GamePhase.GameOver);
            hud.ShowPhase(next, fireAvailable: true);
        }

        void ResetRun()
        {
            foreach (Gate gate in gates)
            {
                Destroy(gate.gameObject);
            }
            foreach (Fireball fireball in fireballs)
            {
                Destroy(fireball.gameObject);
            }
            gates.Clear();
            coins.Clear();
            bats.Clear();
            fireballs.Clear();
            planner.Reset();
            fire.Reset();
            Board.BeginRun();
            speed = Curve.At(0).Speed;
            distanceToNextGate = 0f;
            runTime = 0f;
            Player.ResetTo(HoverHeight);
            hud.SetFireCharge(1f);
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 1f / 20f);
            phaseTime += dt;
            bool acceptInput = phaseTime >= InputGraceSeconds;
            bool fireClicked = fireRequested;
            fireRequested = false;

            switch (Flow.Phase)
            {
                case GamePhase.Title:
                    Player.Hover(dt, HoverHeight + 0.8f);
                    Scroll(dt * 1.2f);
                    if (acceptInput && (input.Flap() || input.Confirm()))
                    {
                        Flow.StartRun();
                    }
                    break;
                case GamePhase.Ready:
                    Player.Hover(dt, HoverHeight);
                    Scroll(dt * 0.8f);
                    if (acceptInput && (Autopilot || input.Flap()))
                    {
                        Flow.Launch();
                        Flap();
                    }
                    break;
                case GamePhase.Playing:
                    if (input.Pause())
                    {
                        Flow.TogglePause();
                        break;
                    }
                    if (Autopilot ? AutopilotWantsToFlap() : input.Flap())
                    {
                        Flap();
                    }
                    if ((Autopilot ? AutopilotWantsToFire() : fireClicked || input.Fire()))
                    {
                        BreatheFire();
                    }
                    Play(dt);
                    break;
                case GamePhase.Paused:
                    if (acceptInput && input.Pause())
                    {
                        Flow.TogglePause();
                    }
                    break;
                case GamePhase.Crashed:
                    Player.Fly(dt, crashed: true);
                    if (Player.Flight.IsGrounded && phaseTime > 0.6f)
                    {
                        Flow.ShowResults();
                    }
                    break;
            }
            UpdateWeather(dt);
        }

        void Flap()
        {
            Player.Flap();
            audioDirector.Play(art.Flap, 0.55f, 0.12f);
            Effects.Burst(Player.transform.position + new Vector3(-0.2f, -0.2f, 0f), art.ParticleAlpha,
                new Color(1f, 1f, 1f, 0.7f), new Color(0.9f, 0.95f, 1f, 0.5f), 6, 2.2f, 0.35f, 0.35f);
        }

        void BreatheFire()
        {
            if (!fire.TryUse())
            {
                return;
            }
            Vector3 mouth = Player.transform.TransformPoint(new Vector3(1.35f, 0.25f, 0f));
            fireballs.Add(Fireball.Create(art, new Vector3(mouth.x, mouth.y, 0f)));
            audioDirector.Play(art.Fireball, 0.8f, 0.08f);
        }

        void Play(float dt)
        {
            runTime += dt;
            fire.Tick(dt);
            hud.SetFireCharge(fire.Charge);

            Difficulty difficulty = Curve.At(Board.GatesPassed);
            speed = Mathf.MoveTowards(speed, difficulty.Speed, dt * 0.5f);
            float distance = speed * dt;
            Scroll(distance);
            Player.Fly(dt, crashed: false);

            distanceToNextGate -= distance;
            if (distanceToNextGate <= 0f)
            {
                SpawnGate(difficulty);
                distanceToNextGate += difficulty.Spacing;
            }

            Vector2 dragon = Player.Centre;
            for (int i = gates.Count - 1; i >= 0; i--)
            {
                Gate gate = gates[i];
                gate.Move(distance);
                if (!gate.Scored && gate.X + Gate.HalfWidth < dragon.x - Dragon.Radius)
                {
                    gate.Scored = true;
                    Board.PassGate();
                    audioDirector.Play(art.Point, 0.6f, 0f);
                }
                if (gate.Hits(dragon, Dragon.Radius * 0.9f))
                {
                    Flow.Crash();
                    return;
                }
                if (gate.X < DespawnX)
                {
                    Destroy(gate.gameObject);
                    gates.RemoveAt(i);
                }
            }

            UpdateFireballs(dt);
            CollectCoins(dragon);
            if (HitByBat(dragon) || Player.Flight.IsGrounded)
            {
                Flow.Crash();
            }
        }

        void UpdateFireballs(float dt)
        {
            for (int i = fireballs.Count - 1; i >= 0; i--)
            {
                Fireball fireball = fireballs[i];
                fireball.Advance(dt);
                Vector2 at = fireball.Centre;
                bool spent = fireball.Expired || at.x > cameraRig.RightEdge + 1f;
                foreach (Bat bat in bats)
                {
                    if (bat != null && Shapes.CirclesOverlap(at.x, at.y, Fireball.Radius, bat.Centre.x, bat.Centre.y, Bat.Radius))
                    {
                        Board.AddBonus(Bat.Bonus);
                        audioDirector.Play(art.Burst, 0.9f, 0.05f);
                        Effects.Burst(bat.transform.position, art.ParticleAdditive, new Color(1f, 0.6f, 0.15f), new Color(0.7f, 0.3f, 1f), 30, 7f, 0.5f, 0.6f);
                        cameraRig.Shake(0.25f);
                        Destroy(bat.gameObject);
                        bats.Remove(bat);
                        spent = true;
                        break;
                    }
                }
                foreach (Gate gate in gates)
                {
                    if (!spent && gate.Hits(at, Fireball.Radius))
                    {
                        Effects.Burst(fireball.transform.position, art.ParticleAdditive, new Color(1f, 0.7f, 0.2f), new Color(1f, 0.35f, 0.1f), 12, 3f, 0.4f, 0.4f);
                        spent = true;
                    }
                }
                if (spent)
                {
                    Destroy(fireball.gameObject);
                    fireballs.RemoveAt(i);
                }
            }
        }

        void CollectCoins(Vector2 dragon)
        {
            for (int i = coins.Count - 1; i >= 0; i--)
            {
                Coin coin = coins[i];
                if (coin == null)
                {
                    coins.RemoveAt(i);
                }
                else if (Shapes.CirclesOverlap(dragon.x, dragon.y, Dragon.Radius, coin.Centre.x, coin.Centre.y, Coin.Radius))
                {
                    Board.CollectCoin(coin.Kind);
                    audioDirector.Play(art.Coin, 0.7f, 0.04f);
                    Effects.Burst(coin.transform.position, art.ParticleAdditive, new Color(1f, 0.9f, 0.4f), new Color(1f, 1f, 1f), 14, 3.5f, 0.3f, 0.45f);
                    Destroy(coin.gameObject);
                    coins.RemoveAt(i);
                }
            }
        }

        bool HitByBat(Vector2 dragon)
        {
            for (int i = bats.Count - 1; i >= 0; i--)
            {
                Bat bat = bats[i];
                if (bat == null)
                {
                    bats.RemoveAt(i);
                    continue;
                }
                bat.Animate(runTime);
                if (Shapes.CirclesOverlap(dragon.x, dragon.y, Dragon.Radius * 0.9f, bat.Centre.x, bat.Centre.y, Bat.Radius))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Rain sets in at 20 points and comes and goes every few gates after that.</summary>
        void UpdateWeather(float dt)
        {
            int score = Board.Score;
            bool raining = Flow.Phase != GamePhase.Title && score >= RainStartsAt && (score - RainStartsAt) % 35 < 18;
            if (raining && !rain.isEmitting)
            {
                rain.Play();
            }
            else if (!raining && rain.isEmitting)
            {
                rain.Stop();
            }
            storm = Mathf.MoveTowards(storm, raining ? 1f : 0f, dt * 0.5f);
            sun.intensity = Mathf.Lerp(sunIntensity, sunIntensity * 0.55f, storm);
            RenderSettings.fogColor = Color.Lerp(fogColor, new Color(0.55f, 0.6f, 0.68f), storm);
            sky.SetFloat("_Exposure", Mathf.Lerp(skyExposure, skyExposure * 0.45f, storm));
        }

        bool AutopilotWantsToFlap()
        {
            float target = HoverHeight;
            foreach (Gate gate in gates)
            {
                if (!gate.Scored)
                {
                    target = gate.Plan.GapCenter - gate.Plan.GapSize * 0.15f;
                    break;
                }
            }
            return Player.Flight.Height < target && Player.Flight.Velocity < 0f;
        }

        bool AutopilotWantsToFire()
        {
            foreach (Bat bat in bats)
            {
                if (bat != null && bat.Centre.x > Player.Centre.x && bat.Centre.x - Player.Centre.x < 9f
                    && Mathf.Abs(bat.Centre.y - Player.Centre.y) < 0.8f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Puts a bat ahead of the dragon and breathes a fireball at it (screenshots).</summary>
        public void StageFireballAtBat()
        {
            Vector2 at = Player.Centre;
            bats.Add(Bat.Create(course, art, new Vector3(at.x + 5.5f, at.y + 0.2f, 0f), 0f, 0f));
            fire.Reset();
            BreatheFire();
        }

        void Scroll(float distance) => scenery.Scroll(distance);

        void SpawnGate(Difficulty difficulty)
        {
            GatePlan plan = planner.Next(difficulty);
            float x = SpawnX;
            Gate gate = Gate.Create(course, art, plan, x, GroundTop, ++gateSeed * 13);
            gates.Add(gate);
            if (plan.Coin != CoinKind.None)
            {
                // Off-centre, so taking the coin is a small risk.
                float offset = (gateSeed % 2 == 0 ? 1f : -1f) * plan.GapSize * 0.22f;
                coins.Add(Coin.Create(gate.transform, art, plan.Coin, new Vector3(x, plan.GapCenter + offset, 0f)));
            }
            if (plan.HasBat)
            {
                // Guards the open air after the gate, bobbing across the opening's height.
                var at = new Vector3(x + difficulty.Spacing * 0.5f, plan.GapCenter, 0f);
                bats.Add(Bat.Create(gate.transform, art, at, plan.GapSize * 0.3f, gateSeed));
            }
        }
    }
}
