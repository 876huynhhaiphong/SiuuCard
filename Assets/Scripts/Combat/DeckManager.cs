using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý 4 pile: Draw, Hand, Discard, Exhaust. Là class C# thuần để test độc lập
/// với scene; CombatManager sẽ giữ một instance.
/// </summary>
public class DeckManager
{
    public readonly List<CardInstance> drawPile = new();
    public readonly List<CardInstance> hand = new();
    public readonly List<CardInstance> discardPile = new();
    public readonly List<CardInstance> exhaustPile = new();

    private readonly ManaSystem mana;

    public event Action<CardInstance> OnCardDrawn;
    public event Action<CardInstance> OnCardPlayed;
    public event Action<CardInstance> OnCardDiscarded;
    public event Action<CardInstance> OnCardExhausted;
    public event Action OnDrawPileShuffled;

    public DeckManager(ManaSystem mana)
    {
        this.mana = mana;
    }

    public void InitializeDeck(List<CardData> starterCards)
    {
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        foreach (var data in starterCards)
            drawPile.Add(new CardInstance(data));

        Shuffle(drawPile);
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) break;
                ShuffleDiscardIntoDraw();
            }

            if (drawPile.Count > 0)
            {
                var card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
                OnCardDrawn?.Invoke(card);
            }
        }
    }

    public void DiscardHand()
    {
        var copy = new List<CardInstance>(hand);
        foreach (var card in copy)
            MoveToDiscard(card);
    }

    public void ShuffleDiscardIntoDraw()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
        OnDrawPileShuffled?.Invoke();
    }

    /// <summary>
    /// Chơi một card từ tay. Trừ mana = manaCost (phần mana đã đổ vào card trước đó là
    /// khoản trả thêm, không được hoàn lại), áp hiệu ứng theo giá trị đã cường hóa.
    /// </summary>
    public bool TryPlayCard(CardInstance card, CombatUnit caster, CombatUnit target)
    {
        if (card == null || !hand.Contains(card)) return false;

        var data = card.data;
        bool needsTarget = data.targetType == TargetType.Enemy || data.targetType == TargetType.AllEnemies;
        if (needsTarget && target == null) return false;

        if (!mana.SpendMana(data.manaCost)) return false;

        ApplyEffects(card, caster, target);
        OnCardPlayed?.Invoke(card);

        if (data.exhaust || data.isPower)
            MoveToExhaust(card);
        else
            MoveToDiscard(card);

        return true;
    }

    public void MoveToDiscard(CardInstance card)
    {
        hand.Remove(card);
        card.ResetInfusion();
        discardPile.Add(card);
        OnCardDiscarded?.Invoke(card);
    }

    public void MoveToExhaust(CardInstance card)
    {
        hand.Remove(card);
        card.ResetInfusion();
        exhaustPile.Add(card);
        OnCardExhausted?.Invoke(card);
    }

    // MVP: chỉ damage + block. magicNumber (heal/draw/stack...) sẽ do hệ thống CardEffect xử lý sau.
    // AllEnemies tạm coi như đánh `target` vì hiện chỉ có 1 enemy.
    private void ApplyEffects(CardInstance card, CombatUnit caster, CombatUnit target)
    {
        if (card.EffectiveDamage > 0 && target != null)
            target.TakeDamage(card.EffectiveDamage);

        if (card.EffectiveBlock > 0 && caster != null)
            caster.GainBlock(card.EffectiveBlock);
    }

    private static void Shuffle(List<CardInstance> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
