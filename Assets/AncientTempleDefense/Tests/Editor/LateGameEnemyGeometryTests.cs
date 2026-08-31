using AncientTempleDefense.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AncientTempleDefense.Tests
{
    public sealed class LateGameEnemyGeometryTests
    {
        private const string PrefabRoot = "Assets/AncientTempleDefense/Generated/Prefabs";

        [TestCase("NewEnemy1", 0.22f, 0f)]
        [TestCase("NewEnemy2", 0.371f, 0f)]
        [TestCase("NewEnemy3", 0.477f, 0f)]
        [TestCase("NewEnemy4", 0.453f, 0f)]
        [TestCase("NewEnemy5", 0.459f, 0f)]
        [TestCase("Boss1Enemy", 0.752f, 0.011f)]
        [TestCase("Boss2Enemy", 0.544f, 0.332f)]
        public void PrefabUsesGroundedPivotAndTightCollider(string prefabName, float pivotX, float pivotY)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null);

            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            CapsuleCollider2D collider = prefab.GetComponent<CapsuleCollider2D>();
            Animator animator = prefab.GetComponent<Animator>();
            Assert.That(renderer?.sprite, Is.Not.Null);
            Assert.That(collider, Is.Not.Null);
            Assert.That(animator, Is.Not.Null);

            Sprite sprite = renderer.sprite;
            Vector2 normalizedPivot = new(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
            Assert.That(normalizedPivot.x, Is.EqualTo(pivotX).Within(0.012f));
            Assert.That(normalizedPivot.y, Is.EqualTo(pivotY).Within(0.012f));

            Bounds visibleBounds = VisibleBounds(sprite);
            float colliderBottom = collider.offset.y - collider.size.y * 0.5f;
            float colliderTop = collider.offset.y + collider.size.y * 0.5f;
            float colliderLeft = collider.offset.x - collider.size.x * 0.5f;
            float colliderRight = collider.offset.x + collider.size.x * 0.5f;

            Assert.That(Mathf.Abs(visibleBounds.min.y), Is.LessThan(0.035f), "Sprite ayak noktası root zeminiyle aynı olmalı.");
            Assert.That(colliderBottom, Is.GreaterThanOrEqualTo(visibleBounds.min.y - 0.01f));
            Assert.That(colliderTop, Is.LessThanOrEqualTo(visibleBounds.max.y + 0.01f));
            Assert.That(colliderLeft, Is.GreaterThanOrEqualTo(visibleBounds.min.x - 0.01f));
            Assert.That(colliderRight, Is.LessThanOrEqualTo(visibleBounds.max.x + 0.01f));
            Assert.That(animator.cullingMode, Is.EqualTo(AnimatorCullingMode.CullUpdateTransforms));
        }

        private static Bounds VisibleBounds(Sprite sprite)
        {
            Vector2[] vertices = sprite.vertices;
            Assert.That(vertices, Is.Not.Empty);
            Bounds bounds = new(vertices[0], Vector3.zero);
            for (int index = 1; index < vertices.Length; index++)
            {
                bounds.Encapsulate(vertices[index]);
            }

            return bounds;
        }
        [TestCase("NewEnemy1", 0f)]
        [TestCase("NewEnemy2", 0f)]
        [TestCase("NewEnemy3", 0f)]
        [TestCase("NewEnemy4", 0f)]
        [TestCase("NewEnemy5", 0f)]
        [TestCase("Boss1Enemy", 0f)]
        [TestCase("Boss2Enemy", 0f)]
        public void PrefabUsesPlayerGroundLevelOffset(string prefabName, float expectedOffset)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<EnemyCombatant>().SpawnYOffset, Is.EqualTo(expectedOffset).Within(0.001f));
        }
        [TestCase("Boss1Enemy", 6f)]
        [TestCase("Boss2Enemy", 3.3f)]
        public void BossPrefabUsesDoubleScale(string prefabName, float expectedScale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabRoot}/{prefabName}.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.localScale.x, Is.EqualTo(expectedScale).Within(0.001f));
        }
    }
}
