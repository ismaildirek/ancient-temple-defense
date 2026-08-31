using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AncientTempleDefense.Editor
{
    public static class PresentationPostProcessingSetup
    {
        private const string ScenePath = "Assets/Scenes/Map.unity";
        private const string ProfilePath = "Assets/AncientTempleDefense/Generated/Settings/PresentationVolume.asset";

        [MenuItem("Tools/Ancient Temple Defense/Apply Presentation Post Processing")]
        public static void Apply()
        {
            EnsureFolder("Assets/AncientTempleDefense/Generated/Settings");
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            if (!profile.TryGet(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
            color.active = true;
            color.postExposure.Override(-0.10f);
            color.contrast.Override(12f);
            color.saturation.Override(-8f);
            color.colorFilter.Override(new Color(0.96f, 0.97f, 1f, 1f));

            if (!profile.TryGet(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
            vignette.active = true;
            vignette.intensity.Override(0.18f);
            vignette.smoothness.Override(0.35f);
            vignette.rounded.Override(false);

            if (!profile.TryGet(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.intensity.Override(0.08f);
            bloom.threshold.Override(1.10f);
            bloom.scatter.Override(0.55f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            GameObject root = GameObject.Find("PresentationPostProcessing") ?? new GameObject("PresentationPostProcessing");
            Volume volume = root.GetComponent<Volume>() ?? root.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;

            Camera camera = Camera.main;
            if (camera != null) camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Sunum post-processing uygulandı: Color Adjustments + Vignette.");
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
