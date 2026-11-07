using System;
using UnityEngine;

public struct RunStartStats
{
    public int maxHP;
    public int maxMana;
    public int drawPerTurn;
    public int startingGold;
    public int bookCapacity;
}

/// <summary>Level, Upgrade Points, chỉ số bonus cho Run, và xử lý khi kết thúc Run.</summary>
public class MetaProgressionManager : MonoBehaviour
{
    public const int BaseMaxHP = 50;
    public const int BaseMaxMana = 3;
    public const int BaseDrawPerTurn = 5;
    public const int UpgradeCost = 1;     // Upgrade Point cho mỗi lần nâng

    private static MetaProgressionManager instance;

    public static MetaProgressionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MetaProgressionManager>();
                if (instance == null)
                    new GameObject("MetaProgressionManager").AddComponent<MetaProgressionManager>();
            }
            return instance;
        }
    }

    public event Action<int> OnLevelUp;   // level mới

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private MetaSaveData Meta => SaveManager.Instance.CurrentMeta;

    // ---------- Level ----------

    public int ExpToNextLevel(int level) => 100 * level;

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        var meta = Meta;
        meta.currentExp += amount;

        while (meta.currentExp >= ExpToNextLevel(meta.playerLevel))
        {
            meta.currentExp -= ExpToNextLevel(meta.playerLevel);
            meta.playerLevel++;
            meta.upgradePoints += 1;            // Mỗi level được 1 Upgrade Point
            OnLevelUp?.Invoke(meta.playerLevel);
        }

        SaveManager.Instance.NotifyMetaChanged();
    }

    // ---------- Upgrade ----------

    public bool TryUpgrade(UpgradeType type)
    {
        var meta = Meta;
        if (meta.upgradePoints < UpgradeCost) return false;

        meta.upgradePoints -= UpgradeCost;
        SaveManager.Instance.ApplyUpgrade(type);   // Tăng chỉ số + notify
        SaveManager.Instance.SaveMeta();
        return true;
    }

    // ---------- Chỉ số cho Run ----------

    public RunStartStats GetRunStartStats()
    {
        var s = Meta.metaStats;
        return new RunStartStats
        {
            maxHP = BaseMaxHP + s.bonusMaxHP,
            maxMana = BaseMaxMana + s.bonusMaxMana,
            drawPerTurn = BaseDrawPerTurn + s.bonusDraw,
            startingGold = s.bonusStartingGold,
            bookCapacity = BookRules.GetCapacity(Meta)
        };
    }

    // ---------- Kết thúc Run ----------

    public int CalculateExp(RunResult result)
    {
        int exp = result.combatsWon * 20;
        exp += result.victory ? 100 : 10;
        return exp;
    }

    public void OnRunEnded(RunResult result)
    {
        // 1. Exp
        AddExp(CalculateExp(result));

        // 2. Card mới mở khóa (vào kho vĩnh viễn, KHÔNG tự vào sách)
        foreach (var cardId in result.newlyUnlockedCards)
            SaveManager.Instance.AddOwnedCard(cardId);

        // 3. Quest: chưa làm (QuestManager nằm ngoài phạm vi giai đoạn này)

        // 4. Lưu
        SaveManager.Instance.SaveMeta();
    }
}
