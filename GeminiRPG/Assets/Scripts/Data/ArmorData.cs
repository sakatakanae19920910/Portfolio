using UnityEngine;

/// <summary>
/// 防具データ（MasterArmor.csv対応）
/// </summary>
[CreateAssetMenu(fileName = "NewArmor", menuName = "GeminiRPG/Item/Armor")]
public class ArmorData : ItemData
{
    public override ItemType Type => ItemType.Armor;

    [Header("Armor Type")]
    public ArmorType armorType;         // 防具種別

    [Header("Stats")]
    public int defense;                 // 物理防御
    public int magicDefense;            // 魔法防御
    public int agilityMod;              // 敏捷補正

    [Header("Resistances")]
    public string elementResist;        // 属性耐性（複数可、セミコロン区切り）
    public string ailmentResist;        // 状態異常耐性
    public int ailmentResistRate;       // 耐性確率

    [Header("Special")]
    public string specialEffect;        // 特殊効果
    public string equipChar;            // 装備可能キャラ
}

/// <summary>
/// 防具種別
/// </summary>
public enum ArmorType
{
    Shield,     // 盾
    LightArmor, // 軽装
    HeavyArmor, // 重装
    Robe,       // 霊装
    SubArmor,   // 補助防具
    Accessory   // 装飾品
}
