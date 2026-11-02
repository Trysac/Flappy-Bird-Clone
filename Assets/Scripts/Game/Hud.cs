using System;
using FlappyDragon.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlappyDragon.Game
{
    /// <summary>Binds the UI Toolkit HUD (Hud.uxml): shows the screen of each phase and forwards button presses.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class Hud : MonoBehaviour
    {
        static readonly string[] MedalClasses = { "medal--bronze", "medal--silver", "medal--gold", "medal--platinum" };

        VisualElement root;
        VisualElement flash;
        VisualElement topBar;
        Label score;
        Label bestChip;
        Button pauseButton;
        Button fireButton;
        VisualElement fireCharge;
        VisualElement title;
        VisualElement titleMedal;
        Label titleBest;
        VisualElement ready;
        VisualElement pause;
        VisualElement gameOver;
        VisualElement medal;
        Label medalName;
        Label nextMedal;
        Label finalScore;
        Label bestScore;
        Label newBest;

        public event Action PlayClicked;
        public event Action PauseClicked;
        public event Action ResumeClicked;
        public event Action RestartClicked;
        public event Action TitleClicked;
        public event Action FireClicked;

        void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            flash = root.Q("flash");
            topBar = root.Q("top-bar");
            score = root.Q<Label>("score");
            bestChip = root.Q<Label>("best-chip");
            pauseButton = root.Q<Button>("pause-button");
            fireButton = root.Q<Button>("fire-button");
            fireCharge = root.Q("fire-charge");
            title = root.Q("title");
            titleMedal = root.Q("title-medal");
            titleBest = root.Q<Label>("title-best");
            ready = root.Q("ready");
            pause = root.Q("pause");
            gameOver = root.Q("game-over");
            medal = root.Q("medal");
            medalName = root.Q<Label>("medal-name");
            nextMedal = root.Q<Label>("next-medal");
            finalScore = root.Q<Label>("final-score");
            bestScore = root.Q<Label>("best-score");
            newBest = root.Q<Label>("new-best");

            root.Q<Button>("play-button").clicked += () => PlayClicked?.Invoke();
            pauseButton.clicked += () => PauseClicked?.Invoke();
            fireButton.clicked += () => FireClicked?.Invoke();
            root.Q<Button>("resume-button").clicked += () => ResumeClicked?.Invoke();
            root.Q<Button>("restart-button").clicked += () => RestartClicked?.Invoke();
            root.Q<Button>("retry-button").clicked += () => RestartClicked?.Invoke();
            root.Q<Button>("pause-title-button").clicked += () => TitleClicked?.Invoke();
            root.Q<Button>("over-title-button").clicked += () => TitleClicked?.Invoke();
            // The fire button is for touch; keyboard and gamepad focus skip it.
            fireButton.focusable = false;
            pauseButton.focusable = false;
        }

        public void ShowPhase(GamePhase phase, bool fireAvailable)
        {
            bool flying = phase == GamePhase.Ready || phase == GamePhase.Playing || phase == GamePhase.Paused || phase == GamePhase.Crashed;
            SetVisible(title, phase == GamePhase.Title);
            SetVisible(ready, phase == GamePhase.Ready);
            SetVisible(pause, phase == GamePhase.Paused);
            SetVisible(gameOver, phase == GamePhase.GameOver);
            SetVisible(topBar, flying);
            SetVisible(pauseButton, phase == GamePhase.Playing);
            SetVisible(fireButton, phase == GamePhase.Playing && fireAvailable);
            string focusName = phase == GamePhase.Title ? "play-button"
                : phase == GamePhase.Paused ? "resume-button"
                : phase == GamePhase.GameOver ? "retry-button"
                : null;
            if (focusName != null)
            {
                root.Q<Button>(focusName).Focus();
            }
            else
            {
                root.focusController?.focusedElement?.Blur();
            }
        }

        public void SetScore(int value)
        {
            score.text = value.ToString();
            if (value > 0)
            {
                score.AddToClassList("score--pop");
                score.schedule.Execute(() => score.RemoveFromClassList("score--pop")).StartingIn(90);
            }
        }

        public void SetBest(int best)
        {
            bestChip.text = $"BEST {best}";
            titleBest.text = best > 0 ? $"Best {best}" : "No best score yet";
            SetMedal(titleMedal, Medals.For(best));
        }

        /// <summary>Shows the fireball's recharge (0..1).</summary>
        public void SetFireCharge(float charge)
        {
            fireCharge.style.height = Length.Percent(Mathf.Clamp01(charge) * 100f);
            fireButton.EnableInClassList("fire-button--ready", charge >= 1f);
        }

        public void Flash()
        {
            flash.AddToClassList("flash--on");
            flash.schedule.Execute(() => flash.RemoveFromClassList("flash--on")).StartingIn(60);
        }

        public void ShowResults(ScoreBoard board)
        {
            finalScore.text = board.Score.ToString();
            bestScore.text = board.Best.ToString();
            SetVisible(newBest, board.IsNewBest);
            Medal earned = Medals.For(board.Score);
            SetMedal(medal, earned);
            medalName.text = earned == Medal.None ? "None yet" : earned.ToString();
            int? next = Medals.NextThreshold(board.Score);
            nextMedal.text = next.HasValue ? $"Next medal at {next.Value} points" : "You earned every medal!";
            SetBest(board.Best);
        }

        /// <summary>Is a screen position (pixels, origin bottom-left) over a button of the HUD?</summary>
        public bool IsOverButton(Vector2 screenPosition)
        {
            if (root?.panel == null)
            {
                return false;
            }
            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel,
                new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            for (VisualElement picked = root.panel.Pick(panelPosition); picked != null; picked = picked.parent)
            {
                if (picked is Button)
                {
                    return true;
                }
            }
            return false;
        }

        static void SetMedal(VisualElement element, Medal value)
        {
            for (int i = 0; i < MedalClasses.Length; i++)
            {
                element.EnableInClassList(MedalClasses[i], (int)value == i + 1);
            }
        }

        static void SetVisible(VisualElement element, bool visible) => element.EnableInClassList("hidden", !visible);
    }
}
