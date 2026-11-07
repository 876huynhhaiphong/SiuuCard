using System;
using UnityEngine;

/// <summary>
/// Pool mana của Player trong combat. Mana dùng cho 2 việc (cùng một pool):
///  1) Chơi card (manaCost)
///  2) Đổ mana vào card để cường hóa (TryInfuse), 1 mana = 1 điểm infusion.
/// </summary>
public class ManaSystem
{
    public int MaxMana { get; private set; }
    public int CurrentMana { get; private set; }

    public event Action<int, int> OnManaChanged;        // (current, max)
    public event Action<CardInstance> OnCardInfused;

    public ManaSystem(int maxMana)
    {
        MaxMana = maxMana;
        CurrentMana = maxMana;
    }

    public void SetMaxMana(int maxMana)
    {
        MaxMana = maxMana;
        CurrentMana = Mathf.Min(CurrentMana, MaxMana);
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }

    public void RefillMana()
    {
        CurrentMana = MaxMana;
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }

    public bool HasEnoughMana(int cost) => CurrentMana >= cost;

    public bool SpendMana(int cost)
    {
        if (cost < 0 || !HasEnoughMana(cost)) return false;
        CurrentMana -= cost;
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
        return true;
    }

    /// <summary>Đổ mana vào card. Trả về false nếu không đủ mana, card không có bậc, hoặc đã đạt bậc tối đa.</summary>
    public bool TryInfuse(CardInstance card, int amount = 1)
    {
        if (card == null || amount <= 0 || !card.CanInfuse) return false;

        amount = Mathf.Min(amount, card.MaxInfusableMana - card.InfusedMana);
        if (!SpendMana(amount)) return false;

        card.AddInfusion(amount);
        OnCardInfused?.Invoke(card);
        return true;
    }
}
