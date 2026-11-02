using System.Collections.Generic;
using NUnit.Framework;

namespace FlappyDragon.Core.Tests
{
    public class ScoreBoardTests
    {
        [Test]
        public void GatesAndCoinsAddPoints()
        {
            var board = new ScoreBoard(new MemoryBestScoreStore());
            var reported = new List<int>();
            board.ScoreChanged += reported.Add;
            board.BeginRun();
            board.PassGate();
            board.CollectCoin(CoinKind.Silver);
            board.CollectCoin(CoinKind.None);
            board.AddBonus(3);
            board.AddBonus(-10);

            Assert.That(board.Score, Is.EqualTo(7));
            Assert.That(board.GatesPassed, Is.EqualTo(1));
            Assert.That(board.CoinsCollected, Is.EqualTo(1));
            Assert.That(reported, Is.EqualTo(new[] { 0, 1, 4, 7 }));
        }

        [Test]
        public void ABetterRunIsSavedAsTheNewBest()
        {
            var store = new MemoryBestScoreStore(best: 4);
            var board = new ScoreBoard(store);
            Assert.That(board.Best, Is.EqualTo(4));

            board.BeginRun();
            for (int i = 0; i < 6; i++)
            {
                board.PassGate();
            }
            board.EndRun();

            Assert.That(board.IsNewBest, Is.True);
            Assert.That(board.Best, Is.EqualTo(6));
            Assert.That(store.LoadBest(), Is.EqualTo(6));
            Assert.That(store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void AWorseOrEqualRunKeepsTheBestAndWritesNothing()
        {
            var store = new MemoryBestScoreStore(best: 3);
            var board = new ScoreBoard(store);
            board.BeginRun();
            board.PassGate();
            board.PassGate();
            board.PassGate();
            board.EndRun();

            Assert.That(board.IsNewBest, Is.False);
            Assert.That(board.Best, Is.EqualTo(3));
            Assert.That(store.SaveCount, Is.EqualTo(0));
        }

        [Test]
        public void ANewRunStartsFromZeroButRemembersTheBest()
        {
            var board = new ScoreBoard(new MemoryBestScoreStore());
            board.BeginRun();
            board.PassGate();
            board.EndRun();
            board.BeginRun();

            Assert.That(board.Score, Is.EqualTo(0));
            Assert.That(board.GatesPassed, Is.EqualTo(0));
            Assert.That(board.IsNewBest, Is.False);
            Assert.That(board.Best, Is.EqualTo(1));
        }

        [Test]
        public void ACorruptNegativeBestIsTreatedAsZero()
        {
            Assert.That(new ScoreBoard(new MemoryBestScoreStore(best: -5)).Best, Is.EqualTo(0));
        }
    }

    public class MedalsTests
    {
        [TestCase(0, Medal.None)]
        [TestCase(9, Medal.None)]
        [TestCase(10, Medal.Bronze)]
        [TestCase(24, Medal.Bronze)]
        [TestCase(25, Medal.Silver)]
        [TestCase(50, Medal.Gold)]
        [TestCase(99, Medal.Gold)]
        [TestCase(100, Medal.Platinum)]
        [TestCase(1000, Medal.Platinum)]
        public void AwardsByThreshold(int score, Medal expected)
        {
            Assert.That(Medals.For(score), Is.EqualTo(expected));
        }

        [Test]
        public void NextThresholdPointsToTheNextMedal()
        {
            Assert.That(Medals.NextThreshold(0), Is.EqualTo(Medals.Bronze));
            Assert.That(Medals.NextThreshold(30), Is.EqualTo(Medals.Gold));
            Assert.That(Medals.NextThreshold(Medals.Platinum), Is.Null);
        }
    }

    public class CooldownTests
    {
        [Test]
        public void IsReadyAtFirstThenRechargesOverItsDuration()
        {
            var cooldown = new Cooldown(2f);
            Assert.That(cooldown.TryUse(), Is.True);
            Assert.That(cooldown.TryUse(), Is.False);
            Assert.That(cooldown.Charge, Is.EqualTo(0f));

            cooldown.Tick(1f);
            Assert.That(cooldown.IsReady, Is.False);
            Assert.That(cooldown.Charge, Is.EqualTo(0.5f).Within(1e-5));

            cooldown.Tick(1.5f);
            Assert.That(cooldown.IsReady, Is.True);
            Assert.That(cooldown.Charge, Is.EqualTo(1f));
            Assert.That(cooldown.TryUse(), Is.True);
        }

        [Test]
        public void ResetMakesItReadyImmediately()
        {
            var cooldown = new Cooldown(5f);
            cooldown.TryUse();
            cooldown.Reset();
            Assert.That(cooldown.IsReady, Is.True);
        }
    }

    public class GameFlowTests
    {
        [Test]
        public void AFullRunGoesThroughEveryScreen()
        {
            var flow = new GameFlow();
            var seen = new List<GamePhase>();
            flow.PhaseChanged += (_, next) => seen.Add(next);

            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Title));
            Assert.That(flow.StartRun(), Is.True);
            Assert.That(flow.Launch(), Is.True);
            Assert.That(flow.IsRunning, Is.True);
            Assert.That(flow.TogglePause(), Is.True);
            Assert.That(flow.IsRunning, Is.False);
            Assert.That(flow.TogglePause(), Is.True);
            Assert.That(flow.Crash(), Is.True);
            Assert.That(flow.ShowResults(), Is.True);
            Assert.That(flow.StartRun(), Is.True);

            Assert.That(seen, Is.EqualTo(new[]
            {
                GamePhase.Ready, GamePhase.Playing, GamePhase.Paused, GamePhase.Playing,
                GamePhase.Crashed, GamePhase.GameOver, GamePhase.Ready,
            }));
        }

        [Test]
        public void InvalidRequestsAreIgnored()
        {
            var flow = new GameFlow();
            int changes = 0;
            flow.PhaseChanged += (_, _) => changes++;

            Assert.That(flow.TogglePause(), Is.False, "No pause on the title screen.");
            Assert.That(flow.Crash(), Is.False);
            Assert.That(flow.Launch(), Is.False);
            Assert.That(flow.ShowResults(), Is.False);
            Assert.That(flow.ToTitle(), Is.False);

            flow.StartRun();
            flow.Launch();
            Assert.That(flow.StartRun(), Is.False, "No restart in the middle of a run.");
            Assert.That(flow.ToTitle(), Is.False);
            Assert.That(changes, Is.EqualTo(2));
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Playing));
        }

        [Test]
        public void ThePauseMenuCanRestartOrQuitToTheTitle()
        {
            var flow = new GameFlow();
            flow.StartRun();
            flow.Launch();
            flow.TogglePause();
            Assert.That(flow.StartRun(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Ready));

            flow.Launch();
            flow.TogglePause();
            Assert.That(flow.ToTitle(), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(GamePhase.Title));
        }
    }
}
