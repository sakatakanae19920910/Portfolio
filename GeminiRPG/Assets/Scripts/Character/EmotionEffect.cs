using UnityEngine;

/// <summary>
/// 感情によるステータス補正効果を管理するクラス
/// 各感情がステータスに与える影響を定義
/// </summary>
public static class EmotionEffect  // static = インスタンス化不要、クラス名.メソッド名で直接呼び出せる
{
    // ==================== 補正値の定数定義 ====================
    // const = 定数（変更不可の固定値）

    private const float JOY_ALL_BOOST = 1.05f;      // 喜び：全能力+5% → 1.05倍
    private const float ANGER_ATTACK_BOOST = 1.15f; // 怒り：攻撃+15% → 1.15倍
    private const float SADNESS_MAGIC_BOOST = 1.15f; // 哀しみ：魔力+15% → 1.15倍
    private const float FEAR_DEBUFF = 0.85f;        // 恐怖：攻撃・魔力-15% → 0.85倍
    private const float FEAR_BEAST_EVASION = 1.5f;  // 恐怖（獣キャラ）：回避率1.5倍

    // ==================== ステータス補正の取得 ====================

    /// <summary>
    /// 感情による攻撃力補正を取得
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <returns>補正倍率（1.0 = 変化なし、1.15 = 15%増加、0.85 = 15%減少）</returns>
    public static float GetAttackModifier(EmotionType[] emotions)
    {
        float modifier = 1.0f;  // float = 小数を扱える型。1.0f のfは「float型の1.0」という意味

        // foreach = 配列の要素を1つずつ取り出して処理
        // foreach (型 変数名 in 配列) { 処理 }
        foreach (var emotion in emotions)  // var = 型を自動推論（ここではEmotionType）
        {
            // switch文 = emotionの値に応じて処理を分岐
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;      // *= は modifier = modifier * JOY_ALL_BOOST と同じ（累積で掛け算）
                    break;
                case EmotionType.Anger:  // 怒りの場合
                    modifier *= ANGER_ATTACK_BOOST; // 攻撃力を1.15倍
                    break;
                case EmotionType.Fear:  // 恐怖の場合
                    modifier *= FEAR_DEBUFF;        // 攻撃力を0.85倍（-15%）
                    break;
                // 哀しみは攻撃力に影響しないのでcaseなし
            }
        }

        return modifier;  // 最終的な補正倍率を返す（例：喜び+怒り = 1.05 * 1.15 = 1.2075倍）
    }

    /// <summary>
    /// 感情による魔力補正を取得
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <returns>補正倍率</returns>
    public static float GetMagicModifier(EmotionType[] emotions)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;       // 魔力を1.05倍
                    break;
                case EmotionType.Sadness:  // 哀しみの場合
                    modifier *= SADNESS_MAGIC_BOOST; // 魔力を1.15倍
                    break;
                case EmotionType.Fear:  // 恐怖の場合
                    modifier *= FEAR_DEBUFF;         // 魔力を0.85倍（-15%）
                    break;
                // 怒りは魔力に影響しないのでcaseなし
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 感情による防御力補正を取得
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <returns>補正倍率</returns>
    public static float GetDefenseModifier(EmotionType[] emotions)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;  // 防御力を1.05倍
                    break;
                // 他の感情は防御力に影響しないのでcaseなし
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 感情による抗魔力補正を取得
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <returns>補正倍率</returns>
    public static float GetMagicDefenseModifier(EmotionType[] emotions)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;  // 抗魔力を1.05倍
                    break;
                // 他の感情は抗魔力に影響しないのでcaseなし
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 感情による敏捷性補正を取得
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <returns>補正倍率</returns>
    public static float GetSpeedModifier(EmotionType[] emotions)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;  // 敏捷性を1.05倍
                    break;
                // 他の感情は敏捷性に影響しないのでcaseなし
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 感情による回避率補正を取得（獣キャラ専用）
    /// 恐怖状態の獣キャラは攻撃・魔力のデバフなしで回避率1.5倍
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <param name="isBeast">獣キャラかどうか（ペチなど）</param>
    /// <returns>回避率の補正倍率</returns>
    public static float GetEvasionModifier(EmotionType[] emotions, bool isBeast)
    {
        // isBeast = bool型（真偽値）。true（真）またはfalse（偽）の2値のみを取る

        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        // 獣キャラでない場合は回避率補正なし
        if (!isBeast) return modifier;  // !isBeast = 「獣キャラでない」という意味

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            if (emotion == EmotionType.Fear)  // 恐怖の場合
            {
                modifier *= FEAR_BEAST_EVASION;  // 回避率を1.5倍
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 獣キャラの恐怖による攻撃力補正を取得（デバフなし）
    /// 通常キャラは恐怖で攻撃-15%だが、獣キャラはデバフなし
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <param name="isBeast">獣キャラかどうか</param>
    /// <returns>補正倍率</returns>
    public static float GetAttackModifierForBeast(EmotionType[] emotions, bool isBeast)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;      // 攻撃力を1.05倍
                    break;
                case EmotionType.Anger:  // 怒りの場合
                    modifier *= ANGER_ATTACK_BOOST; // 攻撃力を1.15倍
                    break;
                case EmotionType.Fear:  // 恐怖の場合
                    // 獣キャラの場合はデバフなし、通常キャラはデバフあり
                    if (!isBeast)  // 獣キャラでない場合のみ
                    {
                        modifier *= FEAR_DEBUFF;  // 攻撃力を0.85倍（-15%）
                    }
                    // 獣キャラの場合は何もしない（デバフなし）
                    break;
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }

    /// <summary>
    /// 獣キャラの恐怖による魔力補正を取得（デバフなし）
    /// </summary>
    /// <param name="emotions">発動中の感情の配列</param>
    /// <param name="isBeast">獣キャラかどうか</param>
    /// <returns>補正倍率</returns>
    public static float GetMagicModifierForBeast(EmotionType[] emotions, bool isBeast)
    {
        float modifier = 1.0f;  // 初期値は1.0（補正なし）

        foreach (var emotion in emotions)  // 配列の要素を1つずつ処理
        {
            switch (emotion)
            {
                case EmotionType.Joy:  // 喜びの場合
                    modifier *= JOY_ALL_BOOST;       // 魔力を1.05倍
                    break;
                case EmotionType.Sadness:  // 哀しみの場合
                    modifier *= SADNESS_MAGIC_BOOST; // 魔力を1.15倍
                    break;
                case EmotionType.Fear:  // 恐怖の場合
                    // 獣キャラの場合はデバフなし、通常キャラはデバフあり
                    if (!isBeast)  // 獣キャラでない場合のみ
                    {
                        modifier *= FEAR_DEBUFF;  // 魔力を0.85倍（-15%）
                    }
                    // 獣キャラの場合は何もしない（デバフなし）
                    break;
            }
        }

        return modifier;  // 最終的な補正倍率を返す
    }
}
