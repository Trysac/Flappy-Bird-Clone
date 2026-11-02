using System;
using System.Collections;
using System.IO;
using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>
    /// Opt-in capture of README screenshots from a built player:
    /// <c>FlappyDragon.exe -screenshot &lt;folder&gt; -scenario title,playing,gameover</c>.
    /// Without the -screenshot argument nothing is created, so normal runs and WebGL builds
    /// (which have no command line) are unaffected. Scenarios run in the order given.
    /// </summary>
    public sealed class ScreenshotScenarios : MonoBehaviour
    {
        const string DefaultScenarios = "title,playing,fireball,storm,gameover";

        string folder;
        string[] scenarios;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartWhenRequested()
        {
            string target = Argument("-screenshot");
            if (string.IsNullOrEmpty(target))
            {
                return;
            }
            var host = new GameObject(nameof(ScreenshotScenarios)).AddComponent<ScreenshotScenarios>();
            host.folder = target;
            host.scenarios = (Argument("-scenario") ?? DefaultScenarios).Split(',');
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            var director = FindAnyObjectByType<GameDirector>();
            yield return new WaitForSecondsRealtime(0.5f);
            foreach (string raw in scenarios)
            {
                string scenario = raw.Trim();
                yield return Stage(director, scenario);
                yield return new WaitForEndOfFrame();
                Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(folder, scenario + ".png"), shot.EncodeToPNG());
                Destroy(shot);
            }
            Application.Quit();
        }

        static IEnumerator Stage(GameDirector director, string scenario)
        {
            GameFlow flow = director.Flow;
            switch (scenario)
            {
                case "title":
                    GoToTitle(director);
                    yield return new WaitForSecondsRealtime(1f);
                    break;
                case "ready":
                    RestartRun(director);
                    yield return new WaitForSecondsRealtime(0.8f);
                    break;
                case "playing":
                    yield return FlyFor(director, 14f);
                    break;
                case "fireball":
                    yield return FlyFor(director, 9f);
                    director.Autopilot = false;
                    director.StageFireballAtBat();
                    // Keep flying level for the moment the fireball is in the air.
                    director.Player.Flap();
                    yield return new WaitForSecondsRealtime(0.18f);
                    break;
                case "storm":
                    yield return FlyFor(director, 25f);
                    break;
                case "paused":
                    yield return FlyFor(director, 6f);
                    flow.TogglePause();
                    yield return new WaitForSecondsRealtime(0.3f);
                    break;
                case "gameover":
                    yield return FlyFor(director, 22f);
                    director.Autopilot = false;
                    flow.Crash();
                    while (flow.Phase != GamePhase.GameOver)
                    {
                        yield return null;
                    }
                    yield return new WaitForSecondsRealtime(0.6f);
                    break;
                default:
                    Debug.LogWarning($"Unknown screenshot scenario '{scenario}'.");
                    break;
            }
        }

        static IEnumerator FlyFor(GameDirector director, float seconds)
        {
            RestartRun(director);
            director.Autopilot = true;
            yield return new WaitForSecondsRealtime(seconds);
        }

        static void RestartRun(GameDirector director)
        {
            director.Autopilot = false;
            GameFlow flow = director.Flow;
            if (flow.Phase == GamePhase.Playing)
            {
                flow.TogglePause();
            }
            if (flow.Phase == GamePhase.Crashed)
            {
                flow.ShowResults();
            }
            if (flow.Phase != GamePhase.Ready)
            {
                flow.StartRun();
            }
        }

        static void GoToTitle(GameDirector director)
        {
            director.Autopilot = false;
            GameFlow flow = director.Flow;
            if (flow.Phase == GamePhase.Ready)
            {
                flow.Launch();
            }
            if (flow.Phase == GamePhase.Playing)
            {
                flow.TogglePause();
            }
            if (flow.Phase == GamePhase.Crashed)
            {
                flow.ShowResults();
            }
            flow.ToTitle();
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
