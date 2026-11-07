using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HUD tạm bằng IMGUI để test combat ngay mà chưa cần dựng UI thật.
/// Hover một hàng card để thấy mắt đổi màu; nút +1 để đổ mana; bấm tên card để chơi.
/// Hành động được dồn vào Update để tránh lỗi layout của IMGUI khi thay đổi state giữa chừng.
/// </summary>
public class CombatDebugHUD : MonoBehaviour
{
    [SerializeField] private CombatManager combat;
    [SerializeField] private EyeColorController eyes;

    private CardInstance hovered;
    private CardInstance pendingPlay;
    private CardInstance pendingInfuse;
    private bool pendingEndTurn;
    private bool pendingRestart;

    private void Update()
    {
        if (combat == null || combat.Deck == null) return;

        if (pendingPlay != null) combat.TryPlayCard(pendingPlay);
        if (pendingInfuse != null && combat.Turns.IsPlayerTurn) combat.Mana.TryInfuse(pendingInfuse, 1);
        if (pendingEndTurn) combat.EndTurn();
        if (pendingRestart) combat.RestartLastCombat();
        pendingPlay = pendingInfuse = null;
        pendingEndTurn = pendingRestart = false;

        // Card đã rời tay (đã chơi/discard) thì bỏ hover
        if (hovered != null && !combat.Deck.hand.Contains(hovered))
        {
            eyes?.ClearHovered(hovered);
            hovered = null;
        }
    }

    private void OnGUI()
    {
        if (combat == null || combat.Deck == null) return;

        var turns = combat.Turns;
        var p = combat.Player;
        var e = combat.Enemy;
        bool canAct = turns.IsPlayerTurn;
        bool repaint = Event.current.type == EventType.Repaint;

        GUILayout.BeginArea(new Rect(10, 10, 560, Screen.height - 20));

        GUILayout.Label($"State: {turns.State}");
        GUILayout.Label($"Player  HP {p.currentHP}/{p.maxHP}   Block {p.currentBlock}   Mana {combat.Mana.CurrentMana}/{combat.Mana.MaxMana}");

        string intent = e.CurrentIntent == null ? "-" : $"{e.CurrentIntent.type} {e.CurrentIntent.value}";
        string eName = e.Data != null ? e.Data.enemyName : "Enemy";
        GUILayout.Label($"{eName}  HP {e.currentHP}/{e.maxHP}   Block {e.currentBlock}   Intent: {intent}");
        GUILayout.Label($"Draw {combat.Deck.drawPile.Count} | Discard {combat.Deck.discardPile.Count} | Exhaust {combat.Deck.exhaustPile.Count}");
        GUILayout.Space(8);

        CardInstance newHover = null;
        var hand = new List<CardInstance>(combat.Deck.hand);
        foreach (var card in hand)
        {
            Rect row = GUILayout.BeginHorizontal();
            GUI.enabled = canAct;

            string label = $"{card.data.cardName} [{card.data.manaCost}]  " +
                           $"DMG {card.EffectiveDamage}  BLK {card.EffectiveBlock}  " +
                           $"infused {card.InfusedMana}/{card.MaxInfusableMana}";
            if (GUILayout.Button(label, GUILayout.Width(450))) pendingPlay = card;
            if (GUILayout.Button("+1 mana", GUILayout.Width(80))) pendingInfuse = card;

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (repaint && row.Contains(Event.current.mousePosition)) newHover = card;
        }

        if (repaint && newHover != hovered)
        {
            if (hovered != null) eyes?.ClearHovered(hovered);
            hovered = newHover;
            if (hovered != null) eyes?.SetHovered(hovered);
        }

        GUILayout.Space(8);
        GUI.enabled = canAct;
        if (GUILayout.Button("End Turn", GUILayout.Width(120))) pendingEndTurn = true;
        GUI.enabled = true;

        if (turns.IsCombatOver)
        {
            GUILayout.Label(turns.State == CombatState.Victory ? "VICTORY!" : "DEFEAT...");
            if (GUILayout.Button("Restart", GUILayout.Width(120))) pendingRestart = true;
        }

        GUILayout.EndArea();
    }
}
