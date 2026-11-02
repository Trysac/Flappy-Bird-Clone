using System;

namespace FlappyDragon.Core
{
    public enum GamePhase
    {
        /// <summary>Title screen.</summary>
        Title,

        /// <summary>The dragon hovers in place until the first flap.</summary>
        Ready,

        Playing,
        Paused,

        /// <summary>The dragon crashed and is falling to the ground.</summary>
        Crashed,

        /// <summary>Results screen.</summary>
        GameOver,
    }

    /// <summary>
    /// The screens of the game and the only transitions allowed between them. Invalid requests
    /// (pausing on the title screen, restarting mid-run...) are ignored and return false.
    /// </summary>
    public sealed class GameFlow
    {
        public GamePhase Phase { get; private set; } = GamePhase.Title;

        /// <summary>Raised with the previous and the new phase.</summary>
        public event Action<GamePhase, GamePhase> PhaseChanged;

        /// <summary>True while the world moves and the dragon can collide.</summary>
        public bool IsRunning => Phase == GamePhase.Playing;

        /// <summary>From the title, pause or results screen to a fresh run waiting for its first flap.</summary>
        public bool StartRun() => Phase == GamePhase.Title || Phase == GamePhase.Paused || Phase == GamePhase.GameOver
            ? Go(GamePhase.Ready)
            : false;

        /// <summary>The first flap of a run starts the world.</summary>
        public bool Launch() => Phase == GamePhase.Ready && Go(GamePhase.Playing);

        public bool TogglePause()
        {
            switch (Phase)
            {
                case GamePhase.Playing: return Go(GamePhase.Paused);
                case GamePhase.Paused: return Go(GamePhase.Playing);
                default: return false;
            }
        }

        public bool Crash() => Phase == GamePhase.Playing && Go(GamePhase.Crashed);

        /// <summary>The crashed dragon reached the ground: show the results.</summary>
        public bool ShowResults() => Phase == GamePhase.Crashed && Go(GamePhase.GameOver);

        public bool ToTitle() => Phase == GamePhase.Paused || Phase == GamePhase.GameOver
            ? Go(GamePhase.Title)
            : false;

        bool Go(GamePhase next)
        {
            GamePhase previous = Phase;
            Phase = next;
            PhaseChanged?.Invoke(previous, next);
            return true;
        }
    }
}
