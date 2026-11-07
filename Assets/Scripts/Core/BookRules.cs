using System.Collections.Generic;

/// <summary>
/// Luật của Quyển sách (deck building): sức chứa giới hạn, chỉ bỏ vào sách card đã sở hữu.
/// Cho phép nhiều bản của cùng một card (chỉ bị giới hạn bởi sức chứa).
/// </summary>
public static class BookRules
{
    public const int BaseCapacity = 20;

    public static int GetCapacity(MetaSaveData meta) => BaseCapacity + meta.metaStats.bonusBookSlots;

    public static int CopiesInBook(MetaSaveData meta, string cardId)
    {
        int n = 0;
        foreach (var id in meta.currentStarterDeck)
            if (id == cardId) n++;
        return n;
    }

    public static bool CanAdd(MetaSaveData meta, string cardId)
    {
        return meta.ownedCardIds.Contains(cardId)
            && meta.currentStarterDeck.Count < GetCapacity(meta);
    }

    public static bool TryAdd(MetaSaveData meta, string cardId)
    {
        if (!CanAdd(meta, cardId)) return false;
        meta.currentStarterDeck.Add(cardId);
        return true;
    }

    public static bool RemoveOne(MetaSaveData meta, string cardId)
    {
        return meta.currentStarterDeck.Remove(cardId);   // Remove chỉ xóa 1 bản đầu tiên
    }

    /// <summary>Dọn sách sau khi load: bỏ card không còn sở hữu, cắt phần vượt sức chứa.</summary>
    public static void Validate(MetaSaveData meta)
    {
        int capacity = GetCapacity(meta);
        var owned = new HashSet<string>(meta.ownedCardIds);
        var cleaned = new List<string>();

        foreach (var id in meta.currentStarterDeck)
        {
            if (cleaned.Count >= capacity) break;
            if (!owned.Contains(id)) continue;
            cleaned.Add(id);
        }
        meta.currentStarterDeck = cleaned;
    }
}
