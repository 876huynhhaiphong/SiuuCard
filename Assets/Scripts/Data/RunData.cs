using System.Collections.Generic;

/// <summary>Dữ liệu tạm thời của 1 run. Mất khi run kết thúc.</summary>
public class RunData
{
    public List<CardData> currentDeck = new List<CardData>();   // = nội dung Quyển sách trong run
    public int maxHP;
    public int currentHP;
    public int maxMana;
    public int drawPerTurn;
    public int gold;
    public int bookCapacity;

    public int combatsWon;
    public List<string> cardsGainedThisRun = new List<string>();

    public bool IsBookFull => currentDeck.Count >= bookCapacity;

    /// <summary>Thêm card thưởng. Trả về false nếu sách đầy (UI nên cho chọn ReplaceCard hoặc bỏ qua).</summary>
    public bool TryAddCard(CardData card)
    {
        if (card == null || IsBookFull) return false;
        currentDeck.Add(card);
        cardsGainedThisRun.Add(card.cardId);
        return true;
    }

    /// <summary>Thay một card trong sách bằng card thưởng khi sách đã đầy.</summary>
    public bool ReplaceCard(int index, CardData newCard)
    {
        if (newCard == null || index < 0 || index >= currentDeck.Count) return false;
        currentDeck[index] = newCard;
        cardsGainedThisRun.Add(newCard.cardId);
        return true;
    }

    /// <summary>Tạo RunData từ Meta: deck = Quyển sách hiện tại, chỉ số = gốc + bonus nâng cấp.</summary>
    public static RunData CreateFromMeta()
    {
        var meta = SaveManager.Instance.CurrentMeta;
        var stats = MetaProgressionManager.Instance.GetRunStartStats();
        var db = CardDatabase.Instance;

        return new RunData
        {
            currentDeck = db != null ? db.GetCards(meta.currentStarterDeck) : new List<CardData>(),
            maxHP = stats.maxHP,
            currentHP = stats.maxHP,
            maxMana = stats.maxMana,
            drawPerTurn = stats.drawPerTurn,
            gold = stats.startingGold,
            bookCapacity = stats.bookCapacity
        };
    }
}
