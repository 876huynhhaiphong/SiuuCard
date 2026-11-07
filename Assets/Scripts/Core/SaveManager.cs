using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton: load/save MetaSaveData (JSON) và API đọc/ghi meta data.
/// Tự tạo nếu chưa có trong scene, không cần kéo thả.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;

    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SaveManager>();
                if (instance == null)
                    new GameObject("SaveManager").AddComponent<SaveManager>();   // Awake sẽ gán instance
            }
            return instance;
        }
    }

    public MetaSaveData CurrentMeta { get; private set; }
    public event Action OnMetaChanged;

    public string SavePath => Path.Combine(Application.persistentDataPath, "meta_save.json");

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (CurrentMeta == null) LoadMeta();
    }

    private void OnApplicationPause(bool paused) { if (paused && CurrentMeta != null) SaveMeta(); }
    private void OnApplicationQuit() { if (CurrentMeta != null) SaveMeta(); }

    // ---------- Load / Save ----------

    public void LoadMeta()
    {
        MetaSaveData loaded = null;

        if (File.Exists(SavePath))
        {
            try
            {
                loaded = JsonUtility.FromJson<MetaSaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"Không đọc được save, giữ bản sao .corrupt và tạo save mới. {e.Message}");
                try { File.Copy(SavePath, SavePath + ".corrupt", true); } catch { }
            }
        }

        bool isNew = loaded == null;
        CurrentMeta = loaded ?? CreateNewMeta();
        Sanitize(CurrentMeta);

        if (isNew) SaveMeta();
        OnMetaChanged?.Invoke();
    }

    public void SaveMeta()
    {
        if (CurrentMeta == null) return;

        string json = JsonUtility.ToJson(CurrentMeta, true);
        string tmp = SavePath + ".tmp";

        // Ghi file tạm rồi thay thế, tránh hỏng save nếu game tắt giữa chừng
        File.WriteAllText(tmp, json);
        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(tmp, SavePath);
    }

    public void ResetMeta()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        CurrentMeta = CreateNewMeta();
        Sanitize(CurrentMeta);
        SaveMeta();
        OnMetaChanged?.Invoke();
    }

    private MetaSaveData CreateNewMeta()
    {
        var meta = new MetaSaveData();
        var db = CardDatabase.Instance;
        if (db != null)
        {
            foreach (var id in db.StarterDeckIds)
            {
                if (!meta.ownedCardIds.Contains(id)) meta.ownedCardIds.Add(id);
                meta.currentStarterDeck.Add(id);
            }
        }
        return meta;
    }

    private static void Sanitize(MetaSaveData meta)
    {
        meta.ownedCardIds ??= new System.Collections.Generic.List<string>();
        meta.currentStarterDeck ??= new System.Collections.Generic.List<string>();
        meta.questProgress ??= new System.Collections.Generic.List<QuestProgress>();
        meta.metaStats ??= new MetaStats();
        if (meta.playerLevel < 1) meta.playerLevel = 1;
        BookRules.Validate(meta);
    }

    public void NotifyMetaChanged() => OnMetaChanged?.Invoke();

    // ---------- Helpers ----------

    public void AddOwnedCard(string cardId)
    {
        if (string.IsNullOrEmpty(cardId) || HasCard(cardId)) return;
        CurrentMeta.ownedCardIds.Add(cardId);
        NotifyMetaChanged();
    }

    public bool HasCard(string cardId) => CurrentMeta.ownedCardIds.Contains(cardId);

    public void AddUpgradePoint(int amount)
    {
        CurrentMeta.upgradePoints += amount;
        NotifyMetaChanged();
    }

    /// <summary>Chỉ tăng chỉ số. Việc trừ Upgrade Point do MetaProgressionManager.TryUpgrade xử lý.</summary>
    public void ApplyUpgrade(UpgradeType type)
    {
        var s = CurrentMeta.metaStats;
        switch (type)
        {
            case UpgradeType.MaxHP:        s.bonusMaxHP += 5; break;
            case UpgradeType.MaxMana:      s.bonusMaxMana += 1; break;
            case UpgradeType.DrawPerTurn:  s.bonusDraw += 1; break;
            case UpgradeType.StartingGold: s.bonusStartingGold += 25; break;
            case UpgradeType.BookSlots:    s.bonusBookSlots += 2; break;
        }
        NotifyMetaChanged();
    }

    // ---------- Quyển sách ----------

    public bool AddCardToBook(string cardId)
    {
        bool ok = BookRules.TryAdd(CurrentMeta, cardId);
        if (ok) { SaveMeta(); NotifyMetaChanged(); }
        return ok;
    }

    public bool RemoveCardFromBook(string cardId)
    {
        bool ok = BookRules.RemoveOne(CurrentMeta, cardId);
        if (ok) { SaveMeta(); NotifyMetaChanged(); }
        return ok;
    }
}
