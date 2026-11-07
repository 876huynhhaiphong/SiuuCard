using System;
using System.Collections.Generic;

[Serializable]
public class MetaSaveData
{
    public int saveVersion = 1;

    public int playerLevel = 1;
    public int currentExp = 0;
    public int upgradePoints = 0;

    // Kho card vĩnh viễn (mỗi cardId xuất hiện 1 lần)
    public List<string> ownedCardIds = new List<string>();

    // Nội dung Quyển sách = deck khởi đầu mỗi run. Cho phép trùng cardId, tối đa = sức chứa sách.
    public List<string> currentStarterDeck = new List<string>();

    public MetaStats metaStats = new MetaStats();
    public List<QuestProgress> questProgress = new List<QuestProgress>();

    public int gold = 0;
}
