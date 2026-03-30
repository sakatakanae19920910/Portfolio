using UnityEngine;

/// <summary>
/// 武器データ（MasterWeapon.csv対応）
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "GeminiRPG/Item/Weapon")]
public class WeaponData : ItemData
{
    public override ItemType Type => ItemType.Weapon;

    [Header("Weapon Type")]
    public WeaponType weaponType;       // 武器種別
    public PhysicalAttribute physicalAttr; // 物理属性（斬/刺/打）

    [Header("Stats")]
    public int attack;                  // 攻撃力
    public int agilityMod;              // 敏捷補正
    public int mpCost;                  // MP消費（杖系）

    [Header("Elements & Bonus")]
    public string elements;             // 属性（複数可、セミコロン区切り）
    public string raceBonus;            // 種族特効（複数可）

    [Header("Ailment")]
    public string ailmentGrant;         // 付与する状態異常
    public int ailmentRate;             // 付与確率

    [Header("Special")]
    public string specialEffect;        // 特殊効果
    public string equipChar;            // 装備可能キャラ
}

/// <summary>
/// 武器種別
/// </summary>
public enum WeaponType
{
    Dagger,     // 短剣
    Sword,      // 片手剣
    Club,       // 棍
    Wand,       // 片手杖
    Spear,      // 槍
    GreatSword, // 両手剣
    Hammer,     // 大槌
    Staff       // 両手杖
}

/// <summary>
/// 物理攻撃属性
/// </summary>
public enum PhysicalAttribute
{
    None,
    Slash,  // 斬
    Pierce, // 刺
    Strike  // 打
}
