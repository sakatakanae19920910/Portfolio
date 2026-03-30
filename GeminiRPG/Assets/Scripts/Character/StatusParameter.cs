using UnityEngine;

/// <summary>
/// キャラクターのステータスパラメータ
/// 基礎値・装備補正・バフ補正・感情補正を管理
/// </summary>
[System.Serializable]  // Unityが「このクラスの中身をInspectorに表示して編集可能にする」と認識
public class StatusParameter
{
    // ==================== 基礎ステータス ====================
    [Header("基礎ステータス")]  // Inspectorに見出しを表示
    public int baseHP;             // 基礎HP
    public int baseMP;             // 基礎MP
    public int baseAttack;         // 基礎攻撃力
    public int baseDefense;        // 基礎防御力
    public int baseMagic;          // 基礎魔力
    public int baseMagicDefense;   // 基礎抗魔力
    public int baseSpeed;          // 基礎敏捷性
    public int baseTargetRate;     // 狙われ率（重み）

    // ==================== 装備補正 ====================
    [Header("装備補正")]  // Inspectorに見出しを表示
    public int equipHP;            // 装備によるHP補正
    public int equipMP;            // 装備によるMP補正
    public int equipAttack;        // 装備による攻撃力補正
    public int equipDefense;       // 装備による防御力補正
    public int equipMagic;         // 装備による魔力補正
    public int equipMagicDefense;  // 装備による抗魔力補正
    public int equipSpeed;         // 装備による敏捷性補正
    public int equipEvasion;       // 装備による回避率補正

    // ==================== バフ・デバフ補正 ====================
    [Header("バフ補正 (%)")]  // Inspectorに見出しを表示
    public float buffAttack = 1f;       // 攻撃力バフ（1.0 = 補正なし、1.2 = 20%増加）
    public float buffDefense = 1f;      // 防御力バフ
    public float buffMagic = 1f;        // 魔力バフ
    public float buffMagicDefense = 1f; // 抗魔力バフ
    public float buffSpeed = 1f;        // 敏捷性バフ

    [Header("回避バフ")]  // Inspectorに見出しを表示
    public int buffEvasion;      // 回避バフ（恐怖、スキル等）

    // ==================== 感情システム ====================
    [Header("感情システム")]  // Inspectorに見出しを表示
    public EmotionParameter emotion = new EmotionParameter();  // 感情パラメータ（new = インスタンス生成）
    public bool isBeast = false;  // 獣キャラかどうか（ペチなど。恐怖時の挙動が異なる）

    // ==================== 現在値 ====================
    [Header("現在値")]  // Inspectorに見出しを表示
    public int currentHP;  // 現在のHP
    public int currentMP;  // 現在のMP

    // ==================== 最終ステータス（計算済み） ====================
    // => は「式形式プロパティ」。呼ばれるたびに計算される読み取り専用プロパティ

    public int MaxHP => baseHP + equipHP;  // 最大HP = 基礎HP + 装備HP
    public int MaxMP => baseMP + equipMP;  // 最大MP = 基礎MP + 装備MP

    // 攻撃力 = (基礎攻撃力 + 装備攻撃力) * バフ補正 * 感情補正
    public int Attack
    {
        get  // get = 値を取得する際の処理
        {
            // 感情による補正を取得
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetAttackModifierForBeast(activeEmotions, isBeast);  // 感情補正を取得

            // Mathf.RoundToInt() = 小数を四捨五入して整数に変換
            return Mathf.RoundToInt((baseAttack + equipAttack) * buffAttack * emotionModifier);
        }
    }

    // 防御力 = (基礎防御力 + 装備防御力) * バフ補正 * 感情補正
    public int Defense
    {
        get
        {
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetDefenseModifier(activeEmotions);  // 感情補正を取得

            return Mathf.RoundToInt((baseDefense + equipDefense) * buffDefense * emotionModifier);
        }
    }

    // 魔力 = (基礎魔力 + 装備魔力) * バフ補正 * 感情補正
    public int Magic
    {
        get
        {
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetMagicModifierForBeast(activeEmotions, isBeast);  // 感情補正を取得

            return Mathf.RoundToInt((baseMagic + equipMagic) * buffMagic * emotionModifier);
        }
    }

    // 抗魔力 = (基礎抗魔力 + 装備抗魔力) * バフ補正 * 感情補正
    public int MagicDefense
    {
        get
        {
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetMagicDefenseModifier(activeEmotions);  // 感情補正を取得

            return Mathf.RoundToInt((baseMagicDefense + equipMagicDefense) * buffMagicDefense * emotionModifier);
        }
    }

    // 敏捷性 = (基礎敏捷性 + 装備敏捷性) * バフ補正 * 感情補正
    public int Speed
    {
        get
        {
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetSpeedModifier(activeEmotions);  // 感情補正を取得

            return Mathf.RoundToInt((baseSpeed + equipSpeed) * buffSpeed * emotionModifier);
        }
    }

    // 回避率 = 装備回避 + バフ回避 + 感情補正（獣キャラ専用）
    public int Evasion
    {
        get
        {
            EmotionType[] activeEmotions = emotion.GetActiveEmotions();  // 発動中の感情を取得
            float emotionModifier = EmotionEffect.GetEvasionModifier(activeEmotions, isBeast);  // 感情補正を取得

            // 回避率 = (装備回避 + バフ回避) * 感情補正
            return Mathf.RoundToInt((equipEvasion + buffEvasion) * emotionModifier);
        }
    }

    public int TargetRate => baseTargetRate;  // 狙われ率（感情による補正なし）

    // ==================== ユーティリティ ====================

    /// <summary>
    /// HP/MPを全回復
    /// </summary>
    public void FullRecover()
    {
        currentHP = MaxHP;
        currentMP = MaxMP;
    }

    /// <summary>
    /// バフをリセット（戦闘終了時など）
    /// </summary>
    public void ResetBuffs()
    {
        // バフ補正を初期値（1.0）に戻す
        buffAttack = 1f;       // 1f = 1.0（fはfloat型を示す）
        buffDefense = 1f;
        buffMagic = 1f;
        buffMagicDefense = 1f;
        buffSpeed = 1f;
        buffEvasion = 0;       // 回避バフは0に戻す

        // 感情システムの戦闘終了処理を呼び出す
        emotion.OnBattleEnd();  // 一時的な感情のリセット、戦闘回数カウント、3戦闘経過で感情値-10
    }

    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
    }

    /// <summary>
    /// 回復する
    /// </summary>
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(MaxHP, currentHP + amount);
    }

    /// <summary>
    /// MPを消費する
    /// </summary>
    public bool ConsumeMP(int cost)
    {
        if (currentMP < cost) return false;
        currentMP -= cost;
        return true;
    }

    /// <summary>
    /// MPを回復する
    /// </summary>
    public void RecoverMP(int amount)
    {
        currentMP = Mathf.Min(MaxMP, currentMP + amount);
    }

    /// <summary>
    /// 戦闘不能かどうか
    /// </summary>
    public bool IsDead => currentHP <= 0;
}
