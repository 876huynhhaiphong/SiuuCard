using System;
using UnityEngine;

/// <summary>
/// Một bậc cường hóa. Khi tổng mana đã đổ vào card >= totalMana thì bậc này có hiệu lực.
/// Các bậc phải xếp tăng dần theo totalMana (CardData tự sort trong OnValidate).
/// </summary>
[Serializable]
public class InfuseTier
{
    [Min(1)] public int totalMana = 1;   // Tổng mana tích lũy cần để đạt bậc này
    public int bonusDamage;
    public int bonusBlock;
    public int bonusMagic;
    public Color eyeColor = Color.white; // Màu mắt nhân vật khi card ở bậc này
}

[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    public string cardId;                 // Unique ID (dùng để save)
    public string cardName;
    public Sprite artwork;
    [TextArea] public string description;

    public int manaCost = 1;
    public CardType cardType;
    public CardRarity rarity;

    // Combat values
    public int damage;
    public int block;
    public int magicNumber;               // Dùng đa năng (heal, draw, stack...)

    public bool exhaust;                  // Biến mất sau khi chơi (không vào Discard)
    public bool isPower;                  // Power card (hiệu ứng kéo dài)

    public TargetType targetType;

    // --- Cơ chế đổ mana ---
    public Color baseEyeColor = Color.white;                       // Màu mắt khi hover card chưa đổ mana
    public InfuseTier[] infuseTiers = Array.Empty<InfuseTier>();   // Các bậc cường hóa

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (infuseTiers != null && infuseTiers.Length > 1)
            Array.Sort(infuseTiers, (a, b) => a.totalMana.CompareTo(b.totalMana));
    }
#endif
}
