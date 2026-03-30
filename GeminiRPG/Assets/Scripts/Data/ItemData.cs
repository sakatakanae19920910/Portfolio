using UnityEngine;

/// <summary>
/// 全アイテムの基底クラス
/// ScriptableObjectとして管理し、IDや名前などの共通情報を持つ
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public int id;              // アイテムID
    public string itemName;     // アイテム名
    [TextArea(3, 5)]
    public string description;  // 説明文
    public Sprite icon;         // アイコン画像

    [Header("Shop")]
    public int buyPrice;        // 購入価格（0なら非売品）
    public int sellPrice;       // 売却価格

    [Header("Gift")]
    public bool isGiftable;     // プレゼント可能か
    public string giftTarget;   // プレゼント対象キャラ
    public int empathyValue;    // 好感度上昇値

    /// <summary>
    /// アイテムの種類（継承先でオーバーライドして識別用にする）
    /// </summary>
    public abstract ItemType Type { get; }
}

/// <summary>
/// アイテムの種別定義
/// </summary>
public enum ItemType
{
    Consumable, // 消耗品
    Weapon,     // 武器
    Armor,      // 防具
    KeyItem     // 重要アイテム
}
