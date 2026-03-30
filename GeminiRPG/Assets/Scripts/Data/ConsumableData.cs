using UnityEngine;

/// <summary>
/// 消耗品データ（MasterConsumable.csv対応）
/// </summary>
[CreateAssetMenu(fileName = "NewConsumable", menuName = "GeminiRPG/Item/Consumable")]
public class ConsumableData : ItemData
{
    public override ItemType Type => ItemType.Consumable;

    [Header("Consumable Type")]
    public ConsumableType consumableType; // 消耗品種別
    public TargetScope targetScope;       // 効果範囲

    [Header("Effect")]
    public int effectValue;               // 効果量
    public string element;                // 属性（攻撃アイテム用）

    [Header("Ailment")]
    public string ailmentCure;            // 治療する状態異常
    public string ailmentGrant;           // 付与する状態異常

    [Header("Stat Boost")]
    public string statBoost;              // 上昇するステータス
    public int statBoostValue;            // 上昇値

    [Header("Usability")]
    public bool usableInBattle;           // 戦闘中に使用可能か
    public bool usableInField;            // フィールドで使用可能か
}

/// <summary>
/// 消耗品種別
/// </summary>
public enum ConsumableType
{
    HpRecover,      // HP回復
    MpRecover,      // MP回復
    Revive,         // 蘇生
    FullRecover,    // 完全回復
    AilmentCure,    // 状態異常回復
    Attack,         // 攻撃アイテム
    Resist,         // 耐性付与
    StatBoost,      // ステータス上昇
    EmotionGrant,   // 感情付与
    Material        // 素材
}

/// <summary>
/// 効果範囲
/// </summary>
public enum TargetScope
{
    OneAlly,        // 味方単体
    AllAllies,      // 味方全体
    OneEnemy,       // 敵単体
    AllEnemies,     // 敵全体
    OneDeadAlly     // 戦闘不能味方単体
}
