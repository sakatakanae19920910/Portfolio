using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// キャラクターの基本データ（ScriptableObject）
/// Unity Editor から設定可能
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "GeminiRPG/Character Data")]
public class CharacterData : ScriptableObject
{
    // ==================== 基本情報 ====================
    [Header("基本情報")]
    public string characterId;
    public string characterName;
    [TextArea(2, 4)]
    public string description;

    // ==================== グラフィック ====================
    [Header("グラフィック")]
    public Sprite faceIcon;         // 顔グラフィック
    public Sprite battleSprite;     // 戦闘時スプライト
    public RuntimeAnimatorController walkAnimator;  // 歩行アニメーション

    // ==================== キャラクター特性 ====================
    [Header("キャラクター特性")]  // Inspectorに見出しを表示
    [Tooltip("獣キャラの場合はチェック（ペチなど）。恐怖時の挙動が異なる。")]  // Inspector上でマウスオーバーすると説明が表示される
    public bool isBeast = false;  // 獣キャラかどうか（恐怖時に攻撃・魔力のデバフなし、回避率1.5倍）

    // ==================== 初期ステータス ====================
    [Header("初期ステータス (Lv1)")]  // Inspectorに見出しを表示
    public int baseHP = 100;           // 初期HP
    public int baseMP = 30;            // 初期MP
    public int baseAttack = 20;        // 初期攻撃力
    public int baseDefense = 10;       // 初期防御力
    public int baseMagic = 20;         // 初期魔力
    public int baseMagicDefense = 10;  // 初期抗魔力
    public int baseSpeed = 10;         // 初期敏捷性
    public int baseTargetRate = 100;   // 初期狙われ率

    // ==================== 成長率 ====================
    [Header("成長率 (レベルアップ時の上昇値)")]
    [Tooltip("HP/MP/攻撃/魔力のみ成長。防御/抗魔/敏捷は固定（装備依存）")]
    public int growthHP = 10;
    public int growthMP = 3;
    public int growthAttack = 2;
    public int growthMagic = 2;

    // ==================== スキル ====================
    [Header("習得スキル")]
    public List<LearnableSkill> learnableSkills = new List<LearnableSkill>();

    // ==================== メソッド ====================

    /// <summary>
    /// 指定レベルでのステータスを取得
    /// </summary>
    /// <param name="level">レベル（1以上の整数）</param>
    /// <returns>レベルに応じたステータスパラメータ</returns>
    public StatusParameter GetStatusAtLevel(int level)
    {
        // var = 型を自動推論（ここではStatusParameter型）
        // new StatusParameter { } = オブジェクト初期化子。プロパティを一括設定
        var status = new StatusParameter
        {
            // 成長するステータス（レベルアップで増加）
            baseHP = baseHP + growthHP * (level - 1),          // HP = 初期HP + 成長HP × (レベル - 1)
            baseMP = baseMP + growthMP * (level - 1),          // MP = 初期MP + 成長MP × (レベル - 1)
            baseAttack = baseAttack + growthAttack * (level - 1),  // 攻撃力 = 初期攻撃力 + 成長攻撃力 × (レベル - 1)
            baseMagic = baseMagic + growthMagic * (level - 1),    // 魔力 = 初期魔力 + 成長魔力 × (レベル - 1)

            // 固定ステータス（装備依存、レベルアップで増加しない）
            baseDefense = baseDefense,          // 防御力は固定
            baseMagicDefense = baseMagicDefense,  // 抗魔力は固定
            baseSpeed = baseSpeed,              // 敏捷性は固定
            baseTargetRate = baseTargetRate,    // 狙われ率は固定

            // 獣フラグを設定
            isBeast = isBeast  // CharacterDataのisBeastをStatusParameterに引き継ぐ
        };

        status.FullRecover();  // HP/MPを最大値に回復
        return status;  // 生成したステータスを返す
    }

    /// <summary>
    /// 指定レベルで習得しているスキルIDリストを取得
    /// </summary>
    public List<int> GetLearnedSkillIds(int level)
    {
        var skills = new List<int>();
        foreach (var skill in learnableSkills)
        {
            if (skill.learnLevel <= level)
            {
                skills.Add(skill.skillId);
            }
        }
        return skills;
    }
}

/// <summary>
/// 習得可能スキル
/// </summary>
[System.Serializable]
public class LearnableSkill
{
    public int skillId;
    public int learnLevel;
}