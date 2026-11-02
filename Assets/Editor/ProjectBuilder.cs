using System;
using System.IO;
using FlappyDragon.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace FlappyDragon.EditorTools
{
    /// <summary>
    /// Generates the materials, the art asset, the scene and the player settings from code, and
    /// builds the players, so every binary asset can be recreated from text. Batch mode:
    /// <code>
    /// Unity -batchmode -quit -projectPath . -executeMethod FlappyDragon.EditorTools.ProjectBuilder.SetUpProject
    /// Unity -batchmode -quit -projectPath . -executeMethod FlappyDragon.EditorTools.ProjectBuilder.BuildWindows -buildOutput &lt;folder&gt;
    /// Unity -batchmode -quit -projectPath . -executeMethod FlappyDragon.EditorTools.ProjectBuilder.BuildWebGL -buildOutput &lt;folder&gt;
    /// </code>
    /// </summary>
    public static class ProjectBuilder
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string ArtFolder = "Assets/Art";
        const string MaterialFolder = "Assets/Art/Materials";
        const string ArtPath = "Assets/Art/GameArt.asset";
        const string SoftDotPath = "Assets/Art/SoftDot.png";
        const string AudioFolder = "Assets/Audio";
        const string LayoutPath = "Assets/UI/Hud.uxml";
        const string ThemePath = "Assets/UI/HudTheme.tss";
        const string PanelSettingsPath = "Assets/UI/HudPanelSettings.asset";

        static readonly Color SkyColor = new Color32(0x8f, 0xd3, 0xf2, 0xff);

        [MenuItem("Flappy Dragon/Regenerate Art, Scene and Player Settings")]
        public static void SetUpProject()
        {
            GameArt art = CreateArt();
            PanelSettings panel = CreatePanelSettings();
            CreateScene(art, panel);
            ConfigurePlayer();
            AssetDatabase.SaveAssets();
            Debug.Log("Flappy Dragon project set up.");
        }

        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "FlappyDragon.exe");

        public static void BuildWebGL() => Build(BuildTarget.WebGL, string.Empty);

        // ---- Art ----

        static GameArt CreateArt()
        {
            Directory.CreateDirectory(MaterialFolder);
            var art = AssetDatabase.LoadAssetAtPath<GameArt>(ArtPath);
            if (art == null)
            {
                art = ScriptableObject.CreateInstance<GameArt>();
                AssetDatabase.CreateAsset(art, ArtPath);
            }

            art.DragonScales = Lit("DragonScales", "#e2552f", 0.25f);
            art.DragonBelly = Lit("DragonBelly", "#ffcf6b", 0.2f);
            art.DragonWing = Lit("DragonWing", "#b8322a", 0.15f);
            art.DragonHorn = Lit("DragonHorn", "#fff1d6", 0.35f);
            art.EyeWhite = Lit("EyeWhite", "#ffffff", 0.6f);
            art.EyeDark = Lit("EyeDark", "#1d1410", 0.7f);

            art.Rock = Lit("Rock", "#9a8e84", 0.08f);
            art.RockDark = Lit("RockDark", "#6f655f", 0.05f);
            art.Grass = Lit("Grass", "#78c257", 0.1f);
            art.Dirt = Lit("Dirt", "#a5683d", 0.05f);
            art.HillNear = Lit("HillNear", "#5aa85a", 0.05f);
            art.MountainFar = Lit("MountainFar", "#7f9fc4", 0.05f);
            art.Snow = Lit("Snow", "#f4f8ff", 0.2f);
            art.Cloud = Lit("Cloud", "#ffffff", 0f, emission: "#3a4450");

            art.CoinBronze = Lit("CoinBronze", "#d38a4a", 0.65f, metallic: 0.6f, emission: "#3a1c08");
            art.CoinSilver = Lit("CoinSilver", "#aeb9c6", 0.7f, metallic: 0.7f, emission: "#1a2028");
            art.CoinGold = Lit("CoinGold", "#ffcc33", 0.75f, metallic: 0.6f, emission: "#5a3c00");
            art.Bat = Lit("Bat", "#5b3b7a", 0.2f);
            art.BatWing = Lit("BatWing", "#3c2752", 0.1f);
            art.Fire = Lit("Fire", "#ff9a1f", 0f, emission: "#ff6a00");

            Texture2D dot = CreateSoftDot();
            art.ParticleAdditive = Particle("ParticleAdditive", dot, additive: true);
            art.ParticleAlpha = Particle("ParticleAlpha", dot, additive: false);

            art.Flap = Clip("flap");
            art.Point = Clip("point");
            art.Coin = Clip("coin");
            art.Hit = Clip("hit");
            art.Fall = Clip("fall");
            art.Fireball = Clip("fireball");
            art.Burst = Clip("burst");
            art.Medal = Clip("medal");
            art.Click = Clip("click");
            art.Music = Clip("sky_theme");

            EditorUtility.SetDirty(art);
            return art;
        }

        /// <summary>A flat-coloured Standard material (the low-poly facets come from the meshes).</summary>
        static Material Lit(string name, string color, float smoothness, float metallic = 0f, string emission = null)
        {
            Material material = MaterialAt(name, Shader.Find("Standard"));
            material.color = Hex(color);
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", metallic);
            if (emission != null)
            {
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                material.SetColor("_EmissionColor", Hex(emission));
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Particle(string name, Texture2D texture, bool additive)
        {
            Material material = MaterialAt(name, Shader.Find("Legacy Shaders/Particles/" + (additive ? "Additive" : "Alpha Blended")));
            material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material MaterialAt(string name, Shader shader)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            return material;
        }

        /// <summary>A small white disc with a soft edge for every particle effect.</summary>
        static Texture2D CreateSoftDot()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha * (3f - 2f * alpha)));
                }
            }
            texture.Apply();
            File.WriteAllBytes(SoftDotPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SoftDotPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SoftDotPath);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SoftDotPath);
        }

        static AudioClip Clip(string name)
        {
            string path = $"{AudioFolder}/{name}.ogg";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"Missing {path}: run python tools/audio/build.py first.");
            }
            return clip;
        }

        static Material CreateSky()
        {
            Material sky = MaterialAt("Sky", Shader.Find("Skybox/Procedural"));
            sky.SetColor("_SkyTint", Hex("#7fb8ff"));
            sky.SetColor("_GroundColor", Hex("#9fcfe0"));
            sky.SetFloat("_AtmosphereThickness", 0.75f);
            sky.SetFloat("_Exposure", 1.25f);
            sky.SetFloat("_SunSize", 0.05f);
            EditorUtility.SetDirty(sky);
            return sky;
        }

        // ---- UI ----

        static PanelSettings CreatePanelSettings()
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }
            panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            // Designed at 1280x720; matching the height keeps phones in portrait readable.
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1280, 720);
            panel.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panel.match = 1f;
            panel.clearColor = false;
            EditorUtility.SetDirty(panel);
            return panel;
        }

        // ---- Scene ----

        static void CreateScene(GameArt art, PanelSettings panel)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = SkyColor;
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 220f;
            cameraObject.transform.position = new Vector3(0f, 0.4f, -18f);
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Hex("#fff1d8");
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.55f;
            lightObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);

            RenderSettings.skybox = CreateSky();
            RenderSettings.sun = light;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("#b9dcff");
            RenderSettings.ambientEquatorColor = Hex("#d8e6d0");
            RenderSettings.ambientGroundColor = Hex("#8a7a64");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Hex("#bfe4f5");
            RenderSettings.fogStartDistance = 34f;
            RenderSettings.fogEndDistance = 150f;

            var ui = new GameObject("HUD");
            var document = ui.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(LayoutPath);
            Hud hud = ui.AddComponent<Hud>();

            var game = new GameObject("Game");
            var director = game.AddComponent<GameDirector>();
            var serialized = new SerializedObject(director);
            serialized.FindProperty("art").objectReferenceValue = art;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Trysac";
            PlayerSettings.productName = "Flappy Dragon";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            // Static hosts such as GitHub Pages do not send Content-Encoding for .br/.gz files.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
        }

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;

        // ---- Builds ----

        static void Build(BuildTarget target, string fileName)
        {
            string folder = Argument("-buildOutput");
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException("Pass -buildOutput <folder> outside the repository.");
            }
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = target,
                locationPathName = string.IsNullOrEmpty(fileName) ? folder : Path.Combine(folder, fileName),
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"Build {target}: {summary.result}, {summary.totalSize / (1024 * 1024f):0.0} MB, {summary.totalErrors} errors.");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
