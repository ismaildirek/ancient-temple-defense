using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AncientTempleDefense.Allies;
using AncientTempleDefense.Audio;
using AncientTempleDefense.Economy;
using AncientTempleDefense.Enemies;
using AncientTempleDefense.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AncientTempleDefense.Editor
{
    public static class RosterExpansionSetup
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";
        private const string AnimationRoot = "Assets/AncientTempleDefense/Generated/Animations/RosterExpansion";
        private const string AnimatorRoot = "Assets/AncientTempleDefense/Generated/Animators";
        private const string LowRoot = "Assets/Characters_assets/low_soldier/Animations";
        private const string Boss3Root = "Assets/Characters_assets/BOSS_3/Sprites";
        private const string Boss4Root = "Assets/Characters_assets/BOSS_4/Wooden Arakocra/Art";
        private const string MapPath = "Assets/Scenes/Map.unity";
        private const string BattleAudio = "Assets/Musics/Leohpaz/RPG_Essentials_Free/10_Battle_SFX";
        private const string MagicAudio = "Assets/Musics/Leohpaz/RPG_Essentials_Free/8_Atk_Magic_SFX";

        [MenuItem("Tools/Ancient Temple Defense/Build Low Soldiers And Boss 3-4")]
        public static void Apply()
        {
            EnsureFolders();
            GameObject light = BuildLowSoldier("LowSoldierLight", "Light Bandit/LightBandit", 90, 2.2f, 1.52f, true);
            GameObject heavy = BuildLowSoldier("LowSoldierHeavy", "Heavy Bandit/HeavyBandit", 110, 1.85f, 1.48f, true);
            GameObject boss3 = BuildBoss3();
            GameObject boss4 = BuildBoss4();
            ConfigureMap(light, heavy, boss3, boss4);
            LatestGameplaySetup.Apply();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Low Soldier, BOSS_3, BOSS_4, dört boss güçlendirmesi ve final zafer akışı kuruldu.");
        }

        private static GameObject BuildLowSoldier(string name, string source, int health, float speed, float targetHeight, bool sourceFacesLeft)
        {
            string folder = AnimationRoot + "/" + name;
            EnsureFolder(folder);
            Dictionary<string, AnimationClip> clips = new()
            {
                ["Idle"] = CopyClip($"{LowRoot}/{source}_Idle.anim", folder + "/Idle.anim", true),
                ["Run"] = CopyClip($"{LowRoot}/{source}_Run.anim", folder + "/Run.anim", true),
                ["Attack1"] = CopyClip($"{LowRoot}/{source}_Attack.anim", folder + "/Attack1.anim", false),
                ["Attack2"] = CopyClip($"{LowRoot}/{source}_Attack.anim", folder + "/Attack2.anim", false),
                ["Hit"] = CopyClip($"{LowRoot}/{source}_Hurt.anim", folder + "/Hit.anim", false),
                ["Death"] = CopyClip($"{LowRoot}/{source}_Death.anim", folder + "/Death.anim", false)
            };
            AnimatorController controller = CreateController(AnimatorRoot + "/" + name + ".controller", clips);
            GameObject root = CreateActorRoot(name, controller, FirstSprite(clips["Idle"]), targetHeight, false);
            FriendlyDefender defender = root.AddComponent<FriendlyDefender>();
            SerializedObject data = new(defender);
            data.FindProperty("azamiCan").intValue = health;
            data.FindProperty("saldiriHasari").intValue = 1;
            data.FindProperty("hareketHizi").floatValue = speed;
            data.FindProperty("saldiriMenzili").floatValue = 1.25f;
            data.FindProperty("saldiriBeklemeSuresi").floatValue = 1.18f;
            data.FindProperty("kaynakVarsayilanYonuSola").boolValue = sourceFacesLeft;
            data.ApplyModifiedPropertiesWithoutUndo();
            ConfigureAudio(root.AddComponent<EnemyAudioController>(), false, false);
            return SavePrefab(root, PrefabRoot + "/" + name + ".prefab");
        }

        private static GameObject BuildBoss3()
        {
            string folder = AnimationRoot + "/Boss3";
            EnsureFolder(folder);
            Dictionary<string, AnimationClip> clips = new()
            {
                ["Idle"] = CreateTextureClip(Boss3Root + "/Idle.png", folder + "/Idle.anim", true, 10f),
                ["Walk"] = CreateTextureClip(Boss3Root + "/Move.png", folder + "/Walk.anim", true, 12f),
                ["Attack"] = CreateTextureClip(Boss3Root + "/Attack.png", folder + "/Attack.anim", false, 13f),
                ["Cast"] = CreateTextureClip(Boss3Root + "/Attack.png", folder + "/Cast.anim", false, 11f),
                ["Spell"] = CreateTextureClip(Boss3Root + "/Attack.png", folder + "/Spell.anim", false, 9f),
                ["Hit"] = CreateTextureClip(Boss3Root + "/Take Hit.png", folder + "/Hit.anim", false, 12f),
                ["Death"] = CreateTextureClip(Boss3Root + "/Death.png", folder + "/Death.anim", false, 10f)
            };
            return BuildBoss("Boss3Enemy", 3, clips, "Walk", "Attack", "Cast", "Spell", 3.65f, 20, false);
        }

        private static GameObject BuildBoss4()
        {
            string folder = AnimationRoot + "/Boss4";
            EnsureFolder(folder);
            Sprite[] idle = LoadFolderSprites(Boss4Root + "/Sprites/Idle");
            Sprite[] attack1 = LoadFolderSprites(Boss4Root + "/Sprites/Attack 1");
            Sprite[] attack2 = LoadFolderSprites(Boss4Root + "/Sprites/Attack 2");
            Dictionary<string, AnimationClip> clips = new()
            {
                ["Idle"] = CreateClip(idle, folder + "/Idle.anim", true, 8f),
                ["Walk"] = CreateClip(idle, folder + "/Walk.anim", true, 10f),
                ["Attack"] = CreateClip(attack1, folder + "/Attack.anim", false, 12f),
                ["Cast"] = CreateClip(attack2, folder + "/Cast.anim", false, 10f),
                ["Spell"] = CreateClip(attack2, folder + "/Spell.anim", false, 8f),
                ["Hit"] = CreateClip(idle.Take(Math.Min(2, idle.Length)).ToArray(), folder + "/Hit.anim", false, 7f),
                ["Death"] = CreateClip(attack2.Reverse().ToArray(), folder + "/Death.anim", false, 7f)
            };
            return BuildBoss("Boss4Enemy", 4, clips, "Walk", "Attack", "Cast", "Spell", 4.1f, 26, false);
        }

        private static GameObject BuildBoss(string name, int tier, Dictionary<string, AnimationClip> clips,
            string move, string attack, string heavy, string special, float targetHeight, int baseDamage, bool sourceFacesLeft)
        {
            AnimatorController controller = CreateController(AnimatorRoot + "/" + name + ".controller", clips);
            GameObject root = CreateActorRoot(name, controller, FirstSprite(clips["Idle"]), targetHeight, true);
            EnemyCombatant combatant = root.AddComponent<EnemyCombatant>();
            SerializedObject combatantData = new(combatant);
            combatantData.FindProperty("beklemeAnimasyonu").stringValue = "Idle";
            combatantData.FindProperty("hasarAlmaAnimasyonu").stringValue = "Hit";
            combatantData.FindProperty("ölümAnimasyonu").stringValue = "Death";
            combatantData.FindProperty("doğmaYOfseti").floatValue = tier == 4 ? 0.85f : 0f;
            combatantData.ApplyModifiedPropertiesWithoutUndo();
            BossEnemyBrain brain = root.AddComponent<BossEnemyBrain>();
            SerializedObject brainData = new(brain);
            brainData.FindProperty("bossSeviyesi").intValue = tier;
            brainData.FindProperty("beklemeAnimasyonu").stringValue = "Idle";
            brainData.FindProperty("hareketAnimasyonu").stringValue = move;
            brainData.FindProperty("normalSaldırıAnimasyonu").stringValue = attack;
            brainData.FindProperty("ağırSaldırıAnimasyonu").stringValue = heavy;
            brainData.FindProperty("özelSaldırıAnimasyonu").stringValue = special;
            brainData.FindProperty("hareketHızı").floatValue = tier == 3 ? 1.45f : 1.58f;
            brainData.FindProperty("saldırıMenzili").floatValue = tier == 3 ? 2.15f : 2.4f;
            brainData.FindProperty("temelSaldırıHasarı").intValue = baseDamage;
            brainData.FindProperty("ağırHasarÇarpanı").floatValue = tier == 3 ? 1.9f : 2.1f;
            brainData.FindProperty("özelHasarÇarpanı").floatValue = tier == 3 ? 2.8f : 3.2f;
            brainData.FindProperty("özelSaldırıBeklemeSüresi").floatValue = tier == 3 ? 4.4f : 3.8f;
            brainData.FindProperty("özelİçinGerekenSaldırı").intValue = 1;
            brainData.FindProperty("kaynakVarsayilanYonuSola").boolValue = sourceFacesLeft;
            brainData.ApplyModifiedPropertiesWithoutUndo();
            root.AddComponent<EnemyLootDropper>();
            ConfigureAudio(root.AddComponent<EnemyAudioController>(), true, tier == 3);
            return SavePrefab(root, PrefabRoot + "/" + name + ".prefab");
        }

        private static GameObject CreateActorRoot(string name, RuntimeAnimatorController controller, Sprite sprite, float targetHeight, bool boss)
        {
            GameObject root = new(name);
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = boss ? 11 : 9;
            float visibleHeight = VisibleBounds(sprite).size.y;
            root.transform.localScale = Vector3.one * (targetHeight / Mathf.Max(.01f, visibleHeight));
            Animator animator = root.AddComponent<Animator>(); animator.runtimeAnimatorController = controller; animator.applyRootMotion = false; animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            Rigidbody2D body = root.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f; body.freezeRotation = true; body.interpolation = RigidbodyInterpolation2D.Interpolate;
            CapsuleCollider2D collider = root.AddComponent<CapsuleCollider2D>(); ConfigureCollider(collider, sprite, root.transform.localScale.y); collider.isTrigger = true;
            return root;
        }

        private static void ConfigureAudio(EnemyAudioController audio, bool boss, bool magicBoss)
        {
            SerializedObject data = new(audio);
            string attackOne = boss ? BattleAudio + "/22_Slash_04.wav" : "Assets/Musics/SwordSoundPack/SWORD_09.wav";
            string attackTwo = magicBoss ? MagicAudio + "/18_Thunder_02.wav" : boss ? BattleAudio + "/03_Claw_03.wav" : "Assets/Musics/SwordSoundPack/SWORD_12.wav";
            ConfigureAudioSet(data, "birinciSaldırıSesleri", attackOne, boss ? .78f : .48f);
            ConfigureAudioSet(data, "ikinciSaldırıSesleri", attackTwo, boss ? .82f : .50f);
            ConfigureAudioSet(data, "hasarAlmaSesleri", BattleAudio + "/15_Impact_flesh_02.wav", boss ? .78f : .52f);
            ConfigureAudioSet(data, "ölümSesleri", BattleAudio + "/69_Enemy_death_01.wav", boss ? .88f : .58f);
            ConfigureAudioSet(data, "özelSaldırıSesleri", magicBoss ? MagicAudio + "/30_Earth_02.wav" : MagicAudio + "/25_Wind_01.wav", boss ? .86f : .48f);
            data.FindProperty("eşZamanlıSesSayısı").intValue = boss ? 5 : 3;
            data.FindProperty("üçBoyutluSesKarışımı").floatValue = boss ? .22f : .16f;
            data.FindProperty("saldırıTemasZamanı").floatValue = boss ? .56f : .48f;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureAudioSet(SerializedObject owner, string property, string path, float volume)
        {
            SerializedProperty set = owner.FindProperty(property);
            SerializedProperty clips = set.FindPropertyRelative("sesKlipleri"); clips.arraySize = 1;
            SerializedProperty item = clips.GetArrayElementAtIndex(0);
            item.FindPropertyRelative("sesKlibi").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            item.FindPropertyRelative("vuruşZirvesiSaniyesi").floatValue = .08f;
            set.FindPropertyRelative("sesSeviyesi").floatValue = volume;
            set.FindPropertyRelative("enDüşükPerde").floatValue = .94f;
            set.FindPropertyRelative("enYüksekPerde").floatValue = 1.06f;
            set.FindPropertyRelative("yumuşakBitişSüresi").floatValue = .14f;
        }

        private static void ConfigureMap(GameObject light, GameObject heavy, GameObject boss3, GameObject boss4)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(MapPath, OpenSceneMode.Single);
            DefenseShopPanel shop = UnityEngine.Object.FindFirstObjectByType<DefenseShopPanel>(FindObjectsInactive.Include) ?? throw new InvalidOperationException("Savunma mağazası bulunamadı.");
            SerializedObject shopData = new(shop);
            SerializedProperty lows = shopData.FindProperty("dusukAskerPrefabları"); lows.arraySize = 2;
            lows.GetArrayElementAtIndex(0).objectReferenceValue = light; lows.GetArrayElementAtIndex(1).objectReferenceValue = heavy;
            shopData.FindProperty("dusukAskerFiyati").intValue = 10; shopData.ApplyModifiedPropertiesWithoutUndo();

            EnemyWaveSpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemyWaveSpawner>() ?? throw new InvalidOperationException("Wave sistemi bulunamadı.");
            SerializedObject wave = new(spawner);
            wave.FindProperty("birinciBossCanÇarpanı").floatValue = 6f; wave.FindProperty("birinciBossHasarÇarpanı").floatValue = 1.8f;
            wave.FindProperty("ikinciBossCanÇarpanı").floatValue = 9f; wave.FindProperty("ikinciBossHasarÇarpanı").floatValue = 2.2f;
            wave.FindProperty("üçüncüBossPrefabı").objectReferenceValue = boss3.GetComponent<EnemyCombatant>(); wave.FindProperty("üçüncüBossWave").intValue = 17;
            wave.FindProperty("üçüncüBossCanÇarpanı").floatValue = 12f; wave.FindProperty("üçüncüBossHasarÇarpanı").floatValue = 2.7f;
            wave.FindProperty("dördüncüBossPrefabı").objectReferenceValue = boss4.GetComponent<EnemyCombatant>(); wave.FindProperty("dördüncüBossWave").intValue = 22;
            wave.FindProperty("dördüncüBossCanÇarpanı").floatValue = 16f; wave.FindProperty("dördüncüBossHasarÇarpanı").floatValue = 3.2f;
            wave.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }

        private static AnimationClip CopyClip(string source, string output, bool loop)
        {
            AnimationClip original = AssetDatabase.LoadAssetAtPath<AnimationClip>(source) ?? throw new InvalidOperationException("Animasyon bulunamadı: " + source);
            DeleteIfExists(output); AnimationClip clip = new(); EditorUtility.CopySerialized(original, clip); clip.name = Path.GetFileNameWithoutExtension(output);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, settings); AssetDatabase.CreateAsset(clip, output); return clip;
        }

        private static AnimationClip CreateTextureClip(string texture, string output, bool loop, float rate)
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texture).OfType<Sprite>().OrderBy(s => TrailingNumber(s.name)).ToArray();
            if (sprites.Length == 0) throw new InvalidOperationException("Sprite bulunamadı: " + texture);
            return CreateClip(sprites, output, loop, rate);
        }

        private static Sprite[] LoadFolderSprites(string folder)
        {
            return AssetDatabase.FindAssets("t:Sprite", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).Distinct()
                .OrderBy(TrailingNumber).SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>()).ToArray();
        }

        private static AnimationClip CreateClip(Sprite[] sprites, string output, bool loop, float rate)
        {
            if (sprites.Length == 0) throw new InvalidOperationException("Animasyon karesi bulunamadı: " + output);
            DeleteIfExists(output); AnimationClip clip = new() { frameRate = rate };
            ObjectReferenceKeyframe[] frames = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++) frames[i] = new ObjectReferenceKeyframe { time = i / rate, value = sprites[i] };
            frames[^1] = new ObjectReferenceKeyframe { time = sprites.Length / rate, value = loop ? sprites[0] : sprites[^1] };
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = string.Empty, type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, frames);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, output); return clip;
        }

        private static AnimatorController CreateController(string path, IReadOnlyDictionary<string, AnimationClip> clips)
        {
            DeleteIfExists(path); AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path); AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (KeyValuePair<string, AnimationClip> pair in clips) { AnimatorState state = machine.AddState(pair.Key); state.motion = pair.Value; if (pair.Key == "Idle") machine.defaultState = state; }
            return controller;
        }

        private static Sprite FirstSprite(AnimationClip clip)
        {
            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                foreach (ObjectReferenceKeyframe frame in AnimationUtility.GetObjectReferenceCurve(clip, binding)) if (frame.value is Sprite sprite) return sprite;
            throw new InvalidOperationException("İlk sprite bulunamadı: " + clip.name);
        }

        private static Bounds VisibleBounds(Sprite sprite)
        {
            Bounds bounds = new(sprite.vertices[0], Vector3.zero); for (int i = 1; i < sprite.vertices.Length; i++) bounds.Encapsulate(sprite.vertices[i]); return bounds;
        }

        private static void ConfigureCollider(CapsuleCollider2D collider, Sprite sprite, float scale)
        {
            Bounds bounds = VisibleBounds(sprite); float min = .5f / Mathf.Max(.01f, scale);
            Vector2 size = new(Mathf.Max(min, bounds.size.x * .76f), Mathf.Max(min, bounds.size.y * .94f));
            collider.size = size; collider.offset = new Vector2(bounds.center.x, bounds.min.y + size.y * .5f); collider.direction = size.x > size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
        }

        private static GameObject SavePrefab(GameObject root, string path) { GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root); return saved; }
        private static int TrailingNumber(string value) { string name = Path.GetFileNameWithoutExtension(value); int i = name.Length - 1; while (i >= 0 && char.IsDigit(name[i])) i--; return int.TryParse(name[(i + 1)..], out int n) ? n : 0; }
        private static void DeleteIfExists(string path) { if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null) AssetDatabase.DeleteAsset(path); }
        private static void EnsureFolders() { EnsureFolder(AnimationRoot); EnsureFolder(AnimationRoot + "/LowSoldierLight"); EnsureFolder(AnimationRoot + "/LowSoldierHeavy"); EnsureFolder(AnimationRoot + "/Boss3"); EnsureFolder(AnimationRoot + "/Boss4"); EnsureFolder(AnimatorRoot); }
        private static void EnsureFolder(string path) { string[] parts = path.Split('/'); string current = parts[0]; for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; } }
    }
}
