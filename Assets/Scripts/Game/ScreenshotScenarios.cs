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
    /// The same hook runs inside the editor (ProjectBuilder.CaptureScreenshots), which renders
    /// the camera and the HUD off screen and blends them, for machines that cannot run the player.
    /// Adding <c>-frames N [-fps F]</c> records N numbered frames per scenario at a fixed timestep
    /// (into &lt;folder&gt;/&lt;scenario&gt;/) instead of one still, e.g. <c>-scenario gameplay -frames 90 -fps 15</c>.
    /// </summary>
    public sealed class ScreenshotScenarios : MonoBehaviour
    {
        const string DefaultScenarios = "title,playing,fireball,storm,gameover";
        const int Width = 1280;
        const int Height = 720;

        string folder;
        string[] scenarios;
        int frames;
        int fps;
        RenderTexture sceneTarget;
        RenderTexture uiTarget;

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
            host.frames = int.TryParse(Argument("-frames"), out int count) ? count : 0;
            host.fps = int.TryParse(Argument("-fps"), out int rate) ? rate : 15;
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            var director = FindAnyObjectByType<GameDirector>();
            if (Application.isEditor)
            {
                RenderOffscreen();
            }
            yield return new WaitForSecondsRealtime(0.5f);
            foreach (string raw in scenarios)
            {
                string scenario = raw.Trim();
                if (frames > 0)
                {
                    yield return Record(director, scenario);
                    continue;
                }
                yield return Stage(director, scenario);
                Texture2D shot = null;
                yield return Capture(texture => shot = texture);
                File.WriteAllBytes(Path.Combine(folder, scenario + ".png"), shot.EncodeToPNG());
                Destroy(shot);
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Stages <paramref name="scenario"/>, then saves <see cref="frames"/> frames while game time
        /// advances exactly 1/<see cref="fps"/> s per frame, however long each capture takes.
        /// </summary>
        IEnumerator Record(GameDirector director, string scenario)
        {
            string target = Path.Combine(folder, scenario);
            Directory.CreateDirectory(target);
            yield return Stage(director, scenario);
            Time.captureFramerate = fps;
            for (int i = 0; i < frames; i++)
            {
                Texture2D shot = null;
                yield return Capture(texture => shot = texture);
                File.WriteAllBytes(Path.Combine(target, $"frame_{i:0000}.png"), shot.EncodeToPNG());
                Destroy(shot);
            }
            Time.captureFramerate = 0;
            Debug.Log($"Saved {frames} frames of {scenario} at {fps} fps.");
        }

        IEnumerator Capture(Action<Texture2D> done)
        {
            if (Application.isEditor)
            {
                yield return null;
                done(CaptureOffscreen());
            }
            else
            {
                yield return new WaitForEndOfFrame();
                done(ScreenCapture.CaptureScreenshotAsTexture());
            }
        }

        /// <summary>Points the camera and the HUD panel at render textures of a fixed size.</summary>
        void RenderOffscreen()
        {
            sceneTarget = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            uiTarget = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            Camera.main.targetTexture = sceneTarget;
            var document = FindAnyObjectByType<Hud>().GetComponent<UnityEngine.UIElements.UIDocument>();
            UnityEngine.UIElements.PanelSettings panel = Instantiate(document.panelSettings);
            panel.targetTexture = uiTarget;
            panel.clearColor = true;
            panel.colorClearValue = Color.clear;
            document.panelSettings = panel;
        }

        /// <summary>Blends the HUD over the 3D view, like the screen would show them.</summary>
        Texture2D CaptureOffscreen()
        {
            Color[] scene = Read(sceneTarget);
            Color[] ui = Read(uiTarget);
            for (int i = 0; i < scene.Length; i++)
            {
                Color over = ui[i];
                Color result = Color.Lerp(scene[i], new Color(over.r, over.g, over.b), over.a);
                result.a = 1f;
                scene[i] = result;
            }
            var shot = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            shot.SetPixels(scene);
            shot.Apply();
            return shot;
        }

        static Color[] Read(RenderTexture target)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            Color[] pixels = texture.GetPixels();
            Destroy(texture);
            return pixels;
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
                case "gameplay":
                    yield return FlyFor(director, 7f);
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
