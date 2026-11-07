using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Điều khiển luồng lượt: PlayerTurn ↔ EnemyTurn, và kết thúc combat (Victory/Defeat).
/// Luồng Player Turn: reset block → refill mana → rút bài → (người chơi hành động) → End Turn → discard tay.
/// </summary>
public class TurnManager : MonoBehaviour
{
    [SerializeField] private float enemyActionDelay = 0.8f;

    public CombatState State { get; private set; } = CombatState.Setup;
    public bool IsPlayerTurn => State == CombatState.PlayerTurn;
    public bool IsCombatOver => State == CombatState.Victory || State == CombatState.Defeat;

    public event Action<CombatState> OnStateChanged;
    public event Action OnPlayerTurnStarted;
    public event Action OnEnemyTurnStarted;
    public event Action<CombatState> OnCombatEnded;   // Victory hoặc Defeat

    private DeckManager deck;
    private ManaSystem mana;
    private PlayerCombat player;
    private EnemyCombat enemy;
    private int drawPerTurn;
    private Coroutine enemyRoutine;

    public void Initialize(DeckManager deck, ManaSystem mana, PlayerCombat player, EnemyCombat enemy, int drawPerTurn)
    {
        this.deck = deck;
        this.mana = mana;
        this.player = player;
        this.enemy = enemy;
        this.drawPerTurn = drawPerTurn;
    }

    public void StartCombat()
    {
        StopEnemyRoutine();
        SetState(CombatState.Setup);

        player.ResetBlock();
        enemy.ResetBlock();
        enemy.PickNextIntent();

        StartPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        SetState(CombatState.PlayerTurn);

        player.ResetBlock();
        mana.RefillMana();
        deck.DrawCards(drawPerTurn);

        OnPlayerTurnStarted?.Invoke();
    }

    public void EndPlayerTurn()
    {
        if (!IsPlayerTurn) return;

        deck.DiscardHand();
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        if (CheckCombatEnd()) return;

        SetState(CombatState.EnemyTurn);
        enemy.ResetBlock();
        OnEnemyTurnStarted?.Invoke();

        StopEnemyRoutine();
        enemyRoutine = StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(enemyActionDelay);

        enemy.ExecuteIntent(player);

        if (CheckCombatEnd()) yield break;

        yield return new WaitForSeconds(enemyActionDelay * 0.5f);
        OnEnemyTurnFinished();
    }

    public void OnEnemyTurnFinished()
    {
        enemy.PickNextIntent();   // Intent cho lượt tới, hiện ra trong lúc người chơi suy nghĩ
        StartPlayerTurn();
    }

    /// <summary>Trả về true nếu combat đã kết thúc (khác doc: doc để void, trả bool để caller dừng sớm).</summary>
    public bool CheckCombatEnd()
    {
        if (IsCombatOver) return true;

        if (enemy.IsDead) { EndCombat(CombatState.Victory); return true; }
        if (player.IsDead) { EndCombat(CombatState.Defeat); return true; }
        return false;
    }

    private void EndCombat(CombatState result)
    {
        StopEnemyRoutine();
        SetState(result);
        OnCombatEnded?.Invoke(result);
    }

    private void StopEnemyRoutine()
    {
        if (enemyRoutine != null)
        {
            StopCoroutine(enemyRoutine);
            enemyRoutine = null;
        }
    }

    private void SetState(CombatState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }
}
