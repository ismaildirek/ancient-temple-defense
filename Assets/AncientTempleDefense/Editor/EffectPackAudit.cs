using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AncientTempleDefense.Editor
{
    public static class EffectPackAudit
    {
        private const string EffectRoot = "Assets/100BestEffectPack/Effects";
        private static readonly string ReportPath = Path.Combine(
            Application.dataPath,
            "AncientTempleDefense",
            "Generated",
            "Reports",
            "100BestEffectPackAudit.txt");

        [MenuItem("Tools/Ancient Temple Defense/Audit 100 Best Effect Pack")]
        public static void RunFromCommandLine()
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { EffectRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            List<string> report = new()
            {
                "100BestEffectPack Unity 6 / URP Denetimi",
                $"Prefab sayısı: {prefabPaths.Length}",
                "Kategori|Efekt|Particle|Loop|Light|Renderer|DesteklenmeyenShader|AzamiSüre"
            };

            int particleTotal = 0;
            int rendererTotal = 0;
            int unsupportedMaterialTotal = 0;
            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);
                ParticleSystemRenderer[] renderers = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
                int looping = particles.Count(particle => particle.main.loop);
                float maximumDuration = particles.Length == 0
                    ? 0f
                    : particles.Max(particle => particle.main.duration);
                int unsupported = renderers
                    .SelectMany(renderer => renderer.sharedMaterials)
                    .Where(material => material != null)
                    .Distinct()
                    .Count(material => material.shader == null || !material.shader.isSupported);
                int lights = prefab.GetComponentsInChildren<Light>(true).Length;

                particleTotal += particles.Length;
                rendererTotal += renderers.Length;
                unsupportedMaterialTotal += unsupported;
                report.Add(string.Join(
                    "|",
                    Path.GetFileName(Path.GetDirectoryName(path)),
                    Path.GetFileNameWithoutExtension(path),
                    particles.Length,
                    looping,
                    lights,
                    renderers.Length,
                    unsupported,
                    maximumDuration.ToString("0.00")));
            }

            report.Insert(2, $"Particle toplamı: {particleTotal}");
            report.Insert(3, $"Renderer toplamı: {rendererTotal}");
            report.Insert(4, $"Desteklenmeyen material/shader toplamı: {unsupportedMaterialTotal}");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Temp");
            File.WriteAllLines(ReportPath, report);
            Debug.Log($"100BestEffectPack denetimi tamamlandı: {prefabPaths.Length} prefab, {particleTotal} particle, {unsupportedMaterialTotal} desteklenmeyen material/shader. Rapor: {ReportPath}");
        }
    }
}
