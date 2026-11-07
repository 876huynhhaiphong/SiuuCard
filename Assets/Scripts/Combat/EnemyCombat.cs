using System;

public class EnemyCombat : CombatUnit
{
    public EnemyData Data { get; private set; }
    public EnemyAction CurrentIntent { get; private set; }

    public event Action<EnemyAction> OnIntentChanged;

    private int patternIndex;

    public void Initialize(EnemyData data)
    {
        Data = data;
        maxHP = data.maxHP;
        currentHP = maxHP;
        currentBlock = 0;
        patternIndex = 0;
        CurrentIntent = null;
    }

    /// <summary>Chọn hành động cho lượt tới (hiển thị intent cho người chơi thấy trước).</summary>
    public void PickNextIntent()
    {
        var pattern = Data != null ? Data.actionPattern : null;
        if (pattern == null || pattern.Length == 0)
            CurrentIntent = new EnemyAction { type = EnemyActionType.Attack, value = 5 };
        else
            CurrentIntent = pattern[patternIndex % pattern.Length];

        patternIndex++;
        OnIntentChanged?.Invoke(CurrentIntent);
    }

    public void ExecuteIntent(CombatUnit player)
    {
        if (CurrentIntent == null) return;

        switch (CurrentIntent.type)
        {
            case EnemyActionType.Attack:
                player.TakeDamage(CurrentIntent.value);
                break;
            case EnemyActionType.Block:
                GainBlock(CurrentIntent.value);
                break;
        }
    }
}
