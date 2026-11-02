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
        VisualElement root;
        Label score;
        Button pauseButton;
        VisualElement ready;
        VisualElement pause;
        VisualElement gameOver;
        Label finalScore;
        Label bestScore;
        Label newBest;

        public event Action PauseClicked;
        public event Action ResumeClicked;
        public event Action RestartClicked;

        void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            score = root.Q<Label>("score");
            pauseButton = root.Q<Button>("pause-button");
            ready = root.Q("ready");
            pause = root.Q("pause");
            gameOver = root.Q("game-over");
            finalScore = root.Q<Label>("final-score");
            bestScore = root.Q<Label>("best-score");
            newBest = root.Q<Label>("new-best");

            pauseButton.clicked += () => PauseClicked?.Invoke();
            root.Q<Button>("resume-button").clicked += () => ResumeClicked?.Invoke();
            root.Q<Button>("restart-button").clicked += () => RestartClicked?.Invoke();
            root.Q<Button>("retry-button").clicked += () => RestartClicked?.Invoke();
        }

        public void ShowPhase(GamePhase phase)
        {
            SetVisible(ready, phase == GamePhase.Ready);
            SetVisible(pause, phase == GamePhase.Paused);
            SetVisible(gameOver, phase == GamePhase.GameOver);
            SetVisible(pauseButton, phase == GamePhase.Playing);
            SetVisible(score, phase != GamePhase.GameOver);
            Button focus = phase == GamePhase.Paused ? root.Q<Button>("resume-button")
                : phase == GamePhase.GameOver ? root.Q<Button>("retry-button")
                : null;
            focus?.Focus();
        }

        public void SetScore(int value) => score.text = value.ToString();

        public void ShowResults(ScoreBoard board)
        {
            finalScore.text = board.Score.ToString();
            bestScore.text = board.Best.ToString();
            SetVisible(newBest, board.IsNewBest);
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

        static void SetVisible(VisualElement element, bool visible) => element.EnableInClassList("hidden", !visible);
    }
}
