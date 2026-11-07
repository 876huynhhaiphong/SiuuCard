using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD tạm (IMGUI, góc phải màn hình) để test Meta: Level/Exp, nâng cấp, Quyển sách,
/// và bắt đầu combat bằng deck + chỉ số lấy từ Meta.
/// </summary>
public class MetaDebugHUD : MonoBehaviour
{
    [SerializeField] private CombatManager combat;
    [SerializeField] private EnemyData enemy;

    private readonly Queue<Action> pending = new Queue<Action>();
    private Vector2 scroll;

    private void Update()
    {
        // Mọi thay đổi dữ liệu chạy ở đây (không chạy trong OnGUI) để IMGUI không lệch layout
        while (pending.Count > 0) pending.Dequeue().Invoke();
    }

    private void OnGUI()
    {
        var save = SaveManager.Instance;
        var meta = save.CurrentMeta;
        var prog = MetaProgressionManager.Instance;
        var db = CardDatabase.Instance;
        if (meta == null) return;

        GUILayout.BeginArea(new Rect(Screen.width - 420, 10, 410, Screen.height - 20));

        GUILayout.Label($"Level {meta.playerLevel}   EXP {meta.currentExp}/{prog.ExpToNextLevel(meta.playerLevel)}   Upgrade Points: {meta.upgradePoints}");
        var s = meta.metaStats;
        GUILayout.Label($"Bonus: HP +{s.bonusMaxHP} | Mana +{s.bonusMaxMana} | Draw +{s.bonusDraw} | Gold +{s.bonusStartingGold} | Book +{s.bonusBookSlots}");

        if (GUILayout.Button("+50 EXP")) pending.Enqueue(() => prog.AddExp(50));

        GUI.enabled = meta.upgradePoints >= MetaProgressionManager.UpgradeCost;
        foreach (UpgradeType t in Enum.GetValues(typeof(UpgradeType)))
        {
            var type = t;
            if (GUILayout.Button($"Upgrade {type}")) pending.Enqueue(() => prog.TryUpgrade(type));
        }
        GUI.enabled = true;

        GUILayout.Space(6);
        GUILayout.Label($"Quyển sách: {meta.currentStarterDeck.Count}/{BookRules.GetCapacity(meta)}");

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(220));
        var shown = new HashSet<string>();
        foreach (var id in meta.ownedCardIds)
        {
            if (!shown.Add(id)) continue;
            var card = db != null ? db.GetCard(id) : null;
            string label = card != null ? card.cardName : id;
            int inBook = BookRules.CopiesInBook(meta, id);
            string cardId = id;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}  (trong sách: {inBook})", GUILayout.Width(250));
            if (GUILayout.Button("+", GUILayout.Width(30))) pending.Enqueue(() => save.AddCardToBook(cardId));
            if (GUILayout.Button("-", GUILayout.Width(30))) pending.Enqueue(() => save.RemoveCardFromBook(cardId));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Space(6);
        if (GUILayout.Button("Start Combat From Meta"))
        {
            pending.Enqueue(() =>
            {
                if (combat == null || enemy == null)
                {
                    Debug.LogWarning("MetaDebugHUD: chưa gán CombatManager hoặc EnemyData.");
                    return;
                }
                var run = RunData.CreateFromMeta();
                combat.StartCombat(run.currentDeck, enemy, run.maxHP, run.maxMana, run.drawPerTurn);
            });
        }
        if (GUILayout.Button("Reset Meta (New Game)")) pending.Enqueue(() => save.ResetMeta());

        GUILayout.EndArea();
    }
}
