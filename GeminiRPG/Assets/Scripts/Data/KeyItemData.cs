using UnityEngine;

/// <summary>
/// 重要アイテムデータ（MasterKeyitem.csv対応）
/// プレゼント不可のため、IGiftable関連プロパティなし
/// </summary>
[CreateAssetMenu(fileName = "NewKeyItem", menuName = "GeminiRPG/Item/KeyItem")]
public class KeyItemData : ItemData
{
    public override ItemType Type => ItemType.KeyItem;

    [Header("Key Item Type")]
    public KeyItemType keyItemType;     // 重要アイテム種別

    [Header("Passive Effect")]
    public string passiveEffect;        // 所持効果ID
    public string passiveTarget;        // 効果対象キャラ

    [Header("Use Action")]
    public string useAction;            // 使用時アクションID
}

/// <summary>
/// 重要アイテム種別
/// </summary>
public enum KeyItemType
{
    Story,       // ストーリー進行用
    Passive,     // 所持効果あり
    Exploration, // 探索用（鍵など）
    Document,    // 読み物（文書片）
    Special      // 特殊用途
}
