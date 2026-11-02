using System.Collections.Generic;
using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Runs the game: builds the world, moves it while playing, spawns gates from the
    /// <see cref="GatePlanner"/>, checks collisions on the play plane and drives the HUD
    /// through the <see cref="GameFlow"/> phases. Rules live in FlappyDragon.Core.
    /// </summary>
    public sealed class GameDirector : MonoBehaviour
    {
        public const float GroundTop = -4.65f;
        public const float DragonX = -3.6f;
        public const float SpawnX = 13.5f;
        const float DespawnX = -15f;
        const float HoverHeight = 0.6f;
        const float InputGraceSeconds = 0.2f;

        [SerializeField] GameArt art;
        [SerializeField] Hud hud;

        readonly List<Gate> gates = new List<Gate>();
        readonly List<Coin> coins = new List<Coin>();

        GameInput input;
        AudioDirector audioDirector;
        Scenery scenery;
        Transform course;
        GatePlanner planner;
        float distanceToNextGate;
        float speed;
        float phaseTime;
        int gateSeed;

        public GameFlow Flow { get; } = new GameFlow();

        public ScoreBoard Board { get; private set; }

        public DifficultyCurve Curve { get; } = DifficultyCurve.Default;

        public Dragon Player { get; private set; }

        public IReadOnlyList<Gate> Gates => gates;

        /// <summary>Flies by itself (used by the screenshot scenarios and attract mode).</summary>
        public bool Autopilot { get; set; }

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

            Board.ScoreChanged += hud.SetScore;
            Flow.PhaseChanged += OnPhaseChanged;
            hud.PauseClicked += () => Flow.TogglePause();
            hud.ResumeClicked += () => Flow.TogglePause();
            hud.RestartClicked += () => Flow.StartRun();
        }

        void Start() => Flow.StartRun();

        void OnPhaseChanged(GamePhase previous, GamePhase next)
        {
            phaseTime = 0f;
            if (next == GamePhase.Ready)
            {
                ResetRun();
            }
            if (next == GamePhase.Crashed)
            {
                audioDirector.Play(art.Hit, 1f, 0.02f);
                audioDirector.Play(art.Fall, 0.7f, 0.02f);
            }
            if (next == GamePhase.GameOver)
            {
                Board.EndRun();
                hud.ShowResults(Board);
                if (Board.IsNewBest)
                {
                    audioDirector.Play(art.Medal, 0.8f, 0f);
                }
            }
            if (next == GamePhase.Paused || previous == GamePhase.Paused)
            {
                audioDirector.Play(art.Click, 0.6f, 0f);
            }
            audioDirector.SetMusicDucked(next == GamePhase.Paused || next == GamePhase.GameOver);
            hud.ShowPhase(next);
        }

        void ResetRun()
        {
            foreach (Gate gate in gates)
            {
                Destroy(gate.gameObject);
            }
            gates.Clear();
            coins.Clear();
            planner.Reset();
            Board.BeginRun();
            speed = Curve.At(0).Speed;
            distanceToNextGate = 0f;
            Player.ResetTo(HoverHeight);
        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 1f / 20f);
            phaseTime += dt;
            bool acceptInput = phaseTime >= InputGraceSeconds;

            switch (Flow.Phase)
            {
                case GamePhase.Ready:
                    Player.Hover(dt, HoverHeight);
                    Scroll(dt * 0.5f);
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
                case GamePhase.GameOver:
                    if (acceptInput && phaseTime > 0.6f && input.Confirm())
                    {
                        Flow.StartRun();
                    }
                    break;
            }
        }

        void Flap()
        {
            Player.Flap();
            audioDirector.Play(art.Flap, 0.55f, 0.12f);
        }

        void Play(float dt)
        {
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
                    Destroy(coin.gameObject);
                    coins.RemoveAt(i);
                }
            }

            if (Player.Flight.IsGrounded)
            {
                Flow.Crash();
            }
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

        void Scroll(float distance) => scenery.Scroll(distance);

        void SpawnGate(Difficulty difficulty)
        {
            GatePlan plan = planner.Next(difficulty);
            Gate gate = Gate.Create(course, art, plan, SpawnX, GroundTop, ++gateSeed * 13);
            gates.Add(gate);
            if (plan.Coin != CoinKind.None)
            {
                // Off-centre, so taking the coin is a small risk.
                float offset = (gateSeed % 2 == 0 ? 1f : -1f) * plan.GapSize * 0.22f;
                coins.Add(Coin.Create(gate.transform, art, plan.Coin, new Vector3(SpawnX, plan.GapCenter + offset, 0f)));
            }
        }
    }
}
