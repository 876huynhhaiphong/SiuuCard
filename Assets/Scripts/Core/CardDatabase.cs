using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tra cứu cardId → CardData. Tạo asset tại Assets/Resources/CardDatabase.asset
/// (tên file PHẢI là "CardDatabase" và nằm trong thư mục Resources).
/// </summary>
[CreateAssetMenu(fileName = "CardDatabase", menuName = "Game/Card Database")]
public class CardDatabase : ScriptableObject
{
    [SerializeField] private List<CardData> cards = new List<CardData>();

    [Tooltip("Deck khởi đầu cho New Game (cho phép trùng, ví dụ 5 Strike + 4 Defend).")]
    [SerializeField] private List<CardData> starterDeck = new List<CardData>();

    private Dictionary<string, CardData> lookup;
    private static CardDatabase instance;

    public static CardDatabase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<CardDatabase>("CardDatabase");
                if (instance == null)
                    Debug.LogError("Không tìm thấy CardDatabase. Tạo asset tên 'CardDatabase' trong thư mục Assets/Resources.");
            }
            return instance;
        }
    }

    public IReadOnlyList<CardData> AllCards => cards;

    public List<string> StarterDeckIds
    {
        get
        {
            var ids = new List<string>();
            foreach (var c in starterDeck)
                if (c != null && !string.IsNullOrEmpty(c.cardId)) ids.Add(c.cardId);
            return ids;
        }
    }

    private void OnEnable() => lookup = null;

    private void BuildLookup()
    {
        lookup = new Dictionary<string, CardData>();
        foreach (var c in cards)
        {
            if (c == null) continue;
            if (string.IsNullOrEmpty(c.cardId))
            {
                Debug.LogWarning($"CardData '{c.name}' chưa có cardId, bỏ qua.");
                continue;
            }
            if (lookup.ContainsKey(c.cardId))
            {
                Debug.LogWarning($"Trùng cardId '{c.cardId}' ({c.name}).");
                continue;
            }
            lookup.Add(c.cardId, c);
        }
    }

    public CardData GetCard(string cardId)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(cardId, out var card) ? card : null;
    }

    public List<CardData> GetCards(IEnumerable<string> ids)
    {
        var result = new List<CardData>();
        foreach (var id in ids)
        {
            var card = GetCard(id);
            if (card != null) result.Add(card);
            else Debug.LogWarning($"CardDatabase: không có card '{id}'.");
        }
        return result;
    }

#if UNITY_EDITOR
    [ContextMenu("Collect All CardData In Project")]
    private void CollectAllCards()
    {
        cards.Clear();
        foreach (var guid in UnityEditor.AssetDatabase.FindAssets("t:CardData"))
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var card = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (card != null) cards.Add(card);
        }
        lookup = null;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
