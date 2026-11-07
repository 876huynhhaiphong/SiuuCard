using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Khởi tạo toàn bộ combat, giữ reference tới Player/Enemy, và là cổng duy nhất để UI
/// chơi card / kết thúc lượt. Chạy độc lập (dùng debug deck) trước khi nối Map/Meta.
/// </summary>
[RequireComponent(typeof(TurnManager))]
public class CombatManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private PlayerCombat player;
    [SerializeField] private EnemyCombat enemy;
    [SerializeField] private EyeColorController eyes;   // Có thể để trống

    [Header("Debug start (để test combat độc lập)")]
    [SerializeField] private bool autoStartDebug = true;
    [SerializeField] private List<CardData> debugDeck = new();
    [SerializeField] private EnemyData debugEnemy;
    [SerializeField] private int debugPlayerMaxHP = 50;
    [SerializeField] private int debugMaxMana = 3;
    [SerializeField] private int debugDrawPerTurn = 5;

    public TurnManager Turns { get; private set; }
    public DeckManager Deck { get; private set; }
    public ManaSystem Mana { get; private set; }
    public PlayerCombat Player => player;
    public EnemyCombat Enemy => enemy;

    /// <summary>true = thắng, false = thua.</summary>
    public event Action<bool> OnCombatFinished;

    private List<CardData> lastDeck;
    private EnemyData lastEnemy;
    private int lastMaxHP, lastMaxMana, lastDraw;

    private void Awake()
    {
        Turns = GetComponent<TurnManager>();
    }

    private void Start()
    {
        if (autoStartDebug && debugEnemy != null && debugDeck.Count > 0)
            StartCombat(debugDeck, debugEnemy, debugPlayerMaxHP, debugMaxMana, debugDrawPerTurn);
    }

    public void StartCombat(List<CardData> deckCards, EnemyData enemyData,
                            int playerMaxHP = 50, int maxMana = 3, int drawPerTurn = 5)
    {
        lastDeck = deckCards; lastEnemy = enemyData;
        lastMaxHP = playerMaxHP; lastMaxMana = maxMana; lastDraw = drawPerTurn;

        if (Turns != null) Turns.OnCombatEnded -= HandleCombatEnded;

        player.Initialize(playerMaxHP);
        enemy.Initialize(enemyData);

        Mana = new ManaSystem(maxMana);
        Deck = new DeckManager(Mana);
        Deck.InitializeDeck(deckCards);

        if (eyes != null) eyes.Bind(Mana);

        Turns.Initialize(Deck, Mana, player, enemy, drawPerTurn);
        Turns.OnCombatEnded += HandleCombatEnded;
        Turns.StartCombat();
    }

    public void RestartLastCombat()
    {
        if (lastDeck != null)
            StartCombat(lastDeck, lastEnemy, lastMaxHP, lastMaxMana, lastDraw);
    }

    /// <summary>UI gọi khi người chơi thả/bấm một card lên enemy (hoặc self).</summary>
    public bool TryPlayCard(CardInstance card)
    {
        if (!Turns.IsPlayerTurn) return false;

        bool ok = Deck.TryPlayCard(card, player, enemy);
        if (ok) Turns.CheckCombatEnd();
        return ok;
    }

    /// <summary>Gắn vào nút End Turn.</summary>
    public void EndTurn() => Turns.EndPlayerTurn();

    private void HandleCombatEnded(CombatState result)
    {
        OnCombatFinished?.Invoke(result == CombatState.Victory);
    }
}
