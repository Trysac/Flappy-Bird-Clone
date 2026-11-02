using System;

namespace FlappyDragon.Core
{
    /// <summary>Where the best score survives between sessions.</summary>
    public interface IBestScoreStore
    {
        int LoadBest();

        void SaveBest(int score);
    }

    /// <summary>Keeps the best score in memory only (tests, and platforms without storage).</summary>
    public sealed class MemoryBestScoreStore : IBestScoreStore
    {
        int best;

        public MemoryBestScoreStore(int best = 0) => this.best = best;

        public int SaveCount { get; private set; }

        public int LoadBest() => best;

        public void SaveBest(int score)
        {
            best = score;
            SaveCount++;
        }
    }

    /// <summary>Score of the current run and the persisted best score.</summary>
    public sealed class ScoreBoard
    {
        readonly IBestScoreStore store;

        public ScoreBoard(IBestScoreStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            Best = Math.Max(store.LoadBest(), 0);
        }

        public int Score { get; private set; }

        /// <summary>Gates flown through in this run (drives the difficulty).</summary>
        public int GatesPassed { get; private set; }

        public int CoinsCollected { get; private set; }

        public int Best { get; private set; }

        /// <summary>True when the finished run beat the previous best.</summary>
        public bool IsNewBest { get; private set; }

        public event Action<int> ScoreChanged;

        public void BeginRun()
        {
            Score = 0;
            GatesPassed = 0;
            CoinsCollected = 0;
            IsNewBest = false;
            ScoreChanged?.Invoke(Score);
        }

        public void PassGate()
        {
            GatesPassed++;
            Add(1);
        }

        public void CollectCoin(CoinKind coin)
        {
            int value = GatePlanner.ValueOf(coin);
            if (value <= 0)
            {
                return;
            }
            CoinsCollected++;
            Add(value);
        }

        /// <summary>Adds bonus points (for example a bat hit by a fireball).</summary>
        public void AddBonus(int points)
        {
            if (points > 0)
            {
                Add(points);
            }
        }

        /// <summary>Ends the run; the best score is saved only when it improved.</summary>
        public void EndRun()
        {
            if (Score > Best)
            {
                Best = Score;
                IsNewBest = true;
                store.SaveBest(Best);
            }
        }

        void Add(int points)
        {
            Score += points;
            ScoreChanged?.Invoke(Score);
        }
    }
}
