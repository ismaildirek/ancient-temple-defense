using System;
using System.Collections.Generic;

namespace AncientTempleDefense.Progression
{
    public enum PlayerUpgradeType
    {
        LightDamage,
        HeavyDamage,
        UltimateDamage,
        MoveSpeed,
        AttackReach,
        UltimateCooldown,
        MaximumHealth,
        Armor
    }

    public readonly struct PlayerUpgradeCard
    {
        public PlayerUpgradeCard(PlayerUpgradeType type, string title, string description, string symbol)
        {
            Type = type;
            Title = title;
            Description = description;
            Symbol = symbol;
        }

        public PlayerUpgradeType Type { get; }
        public string Title { get; }
        public string Description { get; }
        public string Symbol { get; }
    }

    public static class PlayerUpgradeCatalog
    {
        private static readonly PlayerUpgradeCard[] Cards =
        {
            new(PlayerUpgradeType.LightDamage, "KESKİNLİK", "Hafif saldırı hasarı +1", "+1"),
            new(PlayerUpgradeType.HeavyDamage, "EZİCİ GÜÇ", "Ağır saldırı hasarı +1", "+1"),
            new(PlayerUpgradeType.UltimateDamage, "KADİM ÖFKE", "Ulti hasarı +2", "+2"),
            new(PlayerUpgradeType.MoveSpeed, "ÇEVİK ADIMLAR", "Hareket hızı %12 artar", "%12"),
            new(PlayerUpgradeType.AttackReach, "UZUN ERİŞİM", "Saldırı menzili %12 artar", "%12"),
            new(PlayerUpgradeType.UltimateCooldown, "HIZLI RİTÜEL", "Ulti bekleme süresi %15 azalır", "%15"),
            new(PlayerUpgradeType.MaximumHealth, "DEMİR İRADE", "Azami can +25 ve 25 can yenile", "+25"),
            new(PlayerUpgradeType.Armor, "ZIRHLI RUH", "Zırh +8; alınan hasar azalır", "+8")
        };

        public static IReadOnlyList<PlayerUpgradeCard> All => Cards;

        public static PlayerUpgradeCard[] CreateChoices(int wave, int choiceCount = 3)
        {
            int count = Math.Clamp(choiceCount, 1, Cards.Length);
            int[] indices = new int[Cards.Length];
            for (int index = 0; index < indices.Length; index++)
            {
                indices[index] = index;
            }

            Random random = new(unchecked(wave * 7919 + 104729));
            for (int index = indices.Length - 1; index > 0; index--)
            {
                int other = random.Next(index + 1);
                (indices[index], indices[other]) = (indices[other], indices[index]);
            }

            PlayerUpgradeCard[] result = new PlayerUpgradeCard[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = Cards[indices[index]];
            }

            return result;
        }
    }
}
