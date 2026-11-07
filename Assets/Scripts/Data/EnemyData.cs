using System;
using UnityEngine;

public enum EnemyActionType { Attack, Block }

[Serializable]
public class EnemyAction
{
    public EnemyActionType type = EnemyActionType.Attack;
    public int value = 5;
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyId;
    public string enemyName;
    public Sprite artwork;
    public int maxHP = 30;

    [Tooltip("Enemy lặp lại tuần tự các hành động này, mỗi lượt một hành động.")]
    public EnemyAction[] actionPattern = new EnemyAction[]
    {
        new EnemyAction { type = EnemyActionType.Attack, value = 6 },
        new EnemyAction { type = EnemyActionType.Block,  value = 5 },
    };
}
