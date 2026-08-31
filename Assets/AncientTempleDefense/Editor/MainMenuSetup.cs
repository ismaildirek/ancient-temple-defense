using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class MainMenuSetup
    {
        private const string EntryScenePath = "Assets/Scenes/giris.unity";
        private const string GameplayScenePath = "Assets/Scenes/Map.unity";
        private const string BackgroundObjectName = "MainMenu_OpeningScreen_v2_0";
        private const string ButtonObjectName = "MainMenuPlayButton";
        private const string PlayButtonPath = "Assets/UI/Button_Play.png";

        [MenuItem("Ancient Temple Defense/Giriş Menüsünü Kur")]
        public static void ConfigureMainMenu()
        {
            ConfigurePlayButtonImport();
            Sprite playSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayButtonPath);
            if (playSprite == null)
            {
                throw new System.InvalidOperationException("Button_Play sprite'ı yüklenemedi.");
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(EntryScenePath, OpenSceneMode.Single);
            GameObject background = GameObject.Find(BackgroundObjectName);
            SpriteRenderer backgroundRenderer = background != null ? background.GetComponent<SpriteRenderer>() : null;
            if (backgroundRenderer == null)
            {
                throw new System.InvalidOperationException($"{BackgroundObjectName} SpriteRenderer bulunamadı.");
            }

            GameObject buttonObject = GameObject.Find(ButtonObjectName);
            if (buttonObject == null)
            {
                buttonObject = new GameObject(ButtonObjectName);
            }

            SpriteRenderer buttonRenderer = buttonObject.GetComponent<SpriteRenderer>();
            if (buttonRenderer == null)
            {
                buttonRenderer = buttonObject.AddComponent<SpriteRenderer>();
            }

            Bounds backgroundBounds = backgroundRenderer.bounds;
            float targetWidth = backgroundBounds.size.x * 0.285f;
            float scale = targetWidth / playSprite.bounds.size.x;
            buttonObject.transform.position = new Vector3(
                backgroundBounds.center.x,
                backgroundBounds.min.y + backgroundBounds.size.y * 0.174f,
                background.transform.position.z);
            buttonObject.transform.localScale = Vector3.one * scale;

            buttonRenderer.sprite = playSprite;
            buttonRenderer.sortingLayerID = backgroundRenderer.sortingLayerID;
            buttonRenderer.sortingOrder = backgroundRenderer.sortingOrder + 1;

            BoxCollider2D hitArea = buttonObject.GetComponent<BoxCollider2D>();
            if (hitArea == null)
            {
                hitArea = buttonObject.AddComponent<BoxCollider2D>();
            }
            hitArea.isTrigger = true;
            hitArea.offset = playSprite.bounds.center;
            hitArea.size = playSprite.bounds.size;

            MainMenuPlayButton playButton = buttonObject.GetComponent<MainMenuPlayButton>();
            if (playButton == null)
            {
                playButton = buttonObject.AddComponent<MainMenuPlayButton>();
            }

            SerializedObject serializedButton = new(playButton);
            serializedButton.FindProperty("anaKamera").objectReferenceValue = Camera.main;
            serializedButton.FindProperty("acilacakSahne").stringValue = "Map";
            serializedButton.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
        }

        private static void ConfigurePlayButtonImport()
        {
            TextureImporter importer = AssetImporter.GetAtPath(PlayButtonPath) as TextureImporter;
            if (importer == null)
            {
                throw new System.InvalidOperationException("Button_Play TextureImporter bulunamadı.");
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(PlayButtonPath, ImportAssetOptions.ForceUpdate);
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>
            {
                new(EntryScenePath, true)
            };

            bool mapFound = false;
            foreach (EditorBuildSettingsScene scene in existing)
            {
                if (scene.path == EntryScenePath)
                {
                    continue;
                }

                scenes.Add(scene);
                mapFound |= scene.path == GameplayScenePath;
            }

            if (!mapFound)
            {
                scenes.Add(new EditorBuildSettingsScene(GameplayScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
