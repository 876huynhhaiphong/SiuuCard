using System;
using UnityEngine;

/// <summary>
/// Bản runtime của một card trong deck. CardData là asset dùng chung nên mọi trạng thái
/// riêng của từng lá (mana đã đổ vào...) nằm ở đây.
/// </summary>
[Serializable]
public class CardInstance
{
    public CardData data;
    public int InfusedMana { get; private set; }

    public CardInstance(CardData data)
    {
        this.data = data;
    }

    // ---- Infusion ----
    private InfuseTier[] Tiers => data.infuseTiers ?? Array.Empty<InfuseTier>();

    /// <summary>Chỉ số bậc hiện tại, -1 nếu chưa đạt bậc nào.</summary>
    public int TierIndex
    {
        get
        {
            int idx = -1;
            var tiers = Tiers;
            for (int i = 0; i < tiers.Length; i++)
                if (InfusedMana >= tiers[i].totalMana) idx = i;
            return idx;
        }
    }

    public InfuseTier CurrentTier
    {
        get { int i = TierIndex; return i >= 0 ? Tiers[i] : null; }
    }

    public InfuseTier NextTier
    {
        get { int i = TierIndex + 1; return i < Tiers.Length ? Tiers[i] : null; }
    }

    public int MaxInfusableMana => Tiers.Length == 0 ? 0 : Tiers[Tiers.Length - 1].totalMana;
    public bool CanInfuse => InfusedMana < MaxInfusableMana;

    /// <summary>Màu mắt tương ứng trạng thái hiện tại của card.</summary>
    public Color CurrentEyeColor => CurrentTier != null ? CurrentTier.eyeColor : data.baseEyeColor;

    /// <summary>Chỉ ManaSystem nên gọi hàm này (vì nó trừ mana tương ứng).</summary>
    public void AddInfusion(int amount)
    {
        InfusedMana = Mathf.Min(InfusedMana + amount, MaxInfusableMana);
    }

    public void ResetInfusion() => InfusedMana = 0;

    // ---- Giá trị hiệu dụng (đã cộng bonus từ bậc cường hóa) ----
    public int EffectiveDamage => data.damage + (CurrentTier?.bonusDamage ?? 0);
    public int EffectiveBlock => data.block + (CurrentTier?.bonusBlock ?? 0);
    public int EffectiveMagic => data.magicNumber + (CurrentTier?.bonusMagic ?? 0);
}
