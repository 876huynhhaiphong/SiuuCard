using System;
using UnityEngine;

public abstract class CombatUnit : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP;
    public int currentBlock;

    public event Action<int, int> OnHPChanged;   // (current, max)
    public event Action<int> OnBlockChanged;
    public event Action<CombatUnit> OnDied;

    public virtual bool IsDead => currentHP <= 0;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;

        int absorbed = Mathf.Min(currentBlock, amount);
        currentBlock -= absorbed;
        amount -= absorbed;
        OnBlockChanged?.Invoke(currentBlock);

        if (amount > 0)
        {
            currentHP = Mathf.Max(0, currentHP - amount);
            OnHPChanged?.Invoke(currentHP, maxHP);
            if (IsDead) OnDied?.Invoke(this);
        }
    }

    public virtual void GainBlock(int amount)
    {
        if (amount <= 0) return;
        currentBlock += amount;
        OnBlockChanged?.Invoke(currentBlock);
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public virtual void ResetBlock()
    {
        currentBlock = 0;
        OnBlockChanged?.Invoke(currentBlock);
    }
}
