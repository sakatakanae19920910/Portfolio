using UnityEngine;

/// <summary>
/// 感情システムの動作確認用テストスクリプト
/// Unity Editorで実行して動作を確認する
/// </summary>
public class EmotionSystemTest : MonoBehaviour
{
    // ==================== 使い方 ====================
    // 1. Unity Editorで空のGameObjectを作成
    // 2. このスクリプトをアタッチ
    // 3. Playボタンを押してConsoleを確認

    void Start()
    {
        Debug.Log("=== 感情システムのテスト開始 ===");

        // テスト1: 基本的な感情値の加算
        TestBasicEmotionAddition();

        // テスト2: 感情変化の発動
        TestEmotionActivation();

        // テスト3: 獣キャラの恐怖効果
        TestBeastFearEffect();

        // テスト4: 戦闘終了処理
        TestBattleEndProcessing();

        // テスト5: ステータス補正の計算
        TestStatusModification();

        Debug.Log("=== 感情システムのテスト完了 ===");
    }

    /// <summary>
    /// テスト1: 基本的な感情値の加算
    /// </summary>
    void TestBasicEmotionAddition()
    {
        Debug.Log("--- テスト1: 基本的な感情値の加算 ---");

        // EmotionParameterインスタンスを作成（new = インスタンス生成）
        EmotionParameter emotion = new EmotionParameter();

        // 喜びを5加算
        emotion.AddEmotion(EmotionType.Joy, 5);
        Debug.Log($"喜び5加算後: {emotion.joy}");  // $"..." = 文字列補間。{}内の変数を埋め込める

        // 怒りを8加算
        emotion.AddEmotion(EmotionType.Anger, 8);
        Debug.Log($"怒り8加算後: {emotion.anger}");

        // 感情変化は発動していない（閾値10未満のため）
        Debug.Log($"感情変化が発動しているか: {emotion.HasEmotionChange()}");  // false が表示されるはず
    }

    /// <summary>
    /// テスト2: 感情変化の発動
    /// </summary>
    void TestEmotionActivation()
    {
        Debug.Log("--- テスト2: 感情変化の発動 ---");

        EmotionParameter emotion = new EmotionParameter();

        // 喜びを15加算（閾値10を超える）
        emotion.AddEmotion(EmotionType.Joy, 15);
        Debug.Log($"喜び15加算後: {emotion.joy}");

        // 感情変化が発動
        Debug.Log($"感情変化が発動しているか: {emotion.HasEmotionChange()}");  // true が表示されるはず

        // 発動中の感情を取得
        EmotionType[] activeEmotions = emotion.GetActiveEmotions();
        Debug.Log($"発動中の感情数: {activeEmotions.Length}");  // 1 が表示されるはず
        if (activeEmotions.Length > 0)
        {
            Debug.Log($"発動中の感情: {activeEmotions[0]}");  // Joy が表示されるはず
        }

        // 感情変化中は感情値が動かない
        emotion.AddEmotion(EmotionType.Anger, 10);
        Debug.Log($"感情変化中に怒り10加算しても増えない: {emotion.anger}");  // 0 が表示されるはず
    }

    /// <summary>
    /// テスト3: 獣キャラの恐怖効果
    /// </summary>
    void TestBeastFearEffect()
    {
        Debug.Log("--- テスト3: 獣キャラの恐怖効果 ---");

        // 通常キャラの恐怖効果（攻撃・魔力-15%）
        EmotionType[] fearEmotion = new EmotionType[] { EmotionType.Fear };
        float normalAttack = EmotionEffect.GetAttackModifierForBeast(fearEmotion, false);  // false = 通常キャラ
        Debug.Log($"通常キャラの恐怖時攻撃補正: {normalAttack}");  // 0.85（-15%）が表示されるはず

        // 獣キャラの恐怖効果（デバフなし、回避率1.5倍）
        float beastAttack = EmotionEffect.GetAttackModifierForBeast(fearEmotion, true);  // true = 獣キャラ
        Debug.Log($"獣キャラの恐怖時攻撃補正: {beastAttack}");  // 1.0（補正なし）が表示されるはず

        float beastEvasion = EmotionEffect.GetEvasionModifier(fearEmotion, true);
        Debug.Log($"獣キャラの恐怖時回避率補正: {beastEvasion}");  // 1.5（1.5倍）が表示されるはず
    }

    /// <summary>
    /// テスト4: 戦闘終了処理
    /// </summary>
    void TestBattleEndProcessing()
    {
        Debug.Log("--- テスト4: 戦闘終了処理 ---");

        EmotionParameter emotion = new EmotionParameter();

        // 一時的な感情を設定
        emotion.SetTemporaryEmotion(EmotionType.Anger);
        Debug.Log($"一時的な感情を設定: {emotion.temporaryEmotion}");  // Anger が表示されるはず

        // 戦闘終了処理（一時的な感情はリセットされる）
        emotion.OnBattleEnd();
        Debug.Log($"戦闘終了後の一時的な感情: {emotion.temporaryEmotion}");  // None が表示されるはず

        // 感情変化後、3戦闘で感情値-10のテスト
        emotion.AddEmotion(EmotionType.Joy, 15);  // 喜び15（感情変化発動）
        Debug.Log($"感情変化発動: 喜び={emotion.joy}");

        // 1戦闘目
        emotion.OnBattleEnd();
        Debug.Log($"1戦闘目終了: 喜び={emotion.joy}, 戦闘回数={emotion.battleCountSinceChange}");

        // 2戦闘目
        emotion.OnBattleEnd();
        Debug.Log($"2戦闘目終了: 喜び={emotion.joy}, 戦闘回数={emotion.battleCountSinceChange}");

        // 3戦闘目（感情値-10でリセット）
        emotion.OnBattleEnd();
        Debug.Log($"3戦闘目終了: 喜び={emotion.joy}, isEmotionActive={emotion.isEmotionActive}");  // 喜び5, false が表示されるはず
    }

    /// <summary>
    /// テスト5: ステータス補正の計算
    /// </summary>
    void TestStatusModification()
    {
        Debug.Log("--- テスト5: ステータス補正の計算 ---");

        // StatusParameterを作成
        StatusParameter status = new StatusParameter
        {
            baseAttack = 100,    // 基礎攻撃力100
            baseMagic = 100,     // 基礎魔力100
            baseDefense = 50,    // 基礎防御力50
            equipAttack = 20,    // 装備攻撃力+20
            equipMagic = 20,     // 装備魔力+20
            equipDefense = 10,   // 装備防御力+10
            isBeast = false      // 通常キャラ
        };

        // 感情なしの状態
        Debug.Log($"感情なし: 攻撃={status.Attack}, 魔力={status.Magic}, 防御={status.Defense}");
        // 攻撃=120, 魔力=120, 防御=60 が表示されるはず

        // 喜びを発動（全能力+5%）
        status.emotion.AddEmotion(EmotionType.Joy, 15);
        Debug.Log($"喜び発動: 攻撃={status.Attack}, 魔力={status.Magic}, 防御={status.Defense}");
        // 攻撃=126 (120*1.05), 魔力=126, 防御=63 が表示されるはず

        // 怒りを追加発動（攻撃+15%）
        status.emotion.ClearAllEmotions();  // 一度リセット
        status.emotion.AddEmotion(EmotionType.Anger, 15);
        Debug.Log($"怒り発動: 攻撃={status.Attack}, 魔力={status.Magic}");
        // 攻撃=138 (120*1.15), 魔力=120 が表示されるはず

        // 哀しみを発動（魔力+15%）
        status.emotion.ClearAllEmotions();
        status.emotion.AddEmotion(EmotionType.Sadness, 15);
        Debug.Log($"哀しみ発動: 攻撃={status.Attack}, 魔力={status.Magic}");
        // 攻撃=120, 魔力=138 (120*1.15) が表示されるはず

        // 恐怖を発動（攻撃・魔力-15%）
        status.emotion.ClearAllEmotions();
        status.emotion.AddEmotion(EmotionType.Fear, 15);
        Debug.Log($"恐怖発動: 攻撃={status.Attack}, 魔力={status.Magic}");
        // 攻撃=102 (120*0.85), 魔力=102 が表示されるはず
    }
}
