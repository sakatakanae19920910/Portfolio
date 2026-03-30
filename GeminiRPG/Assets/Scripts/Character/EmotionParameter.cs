using UnityEngine;

/// <summary>
/// キャラクターの感情パラメータ
/// 喜び・怒り・哀しみ・恐怖の4値を管理
/// </summary>
[System.Serializable]  // Unityが「このクラスの中身をInspectorに表示して編集可能にする」と認識
public class EmotionParameter
{
    // ==================== 感情値 ====================
    [Header("感情値（10以上で感情変化）")]  // Inspectorに見出しを表示
    [Range(0, 100)]  // Inspectorでスライダー表示（0〜100の範囲に制限）
    public int joy;      // 喜び（レベルアップ、宝箱発見、なでなで、シナリオ展開で増加）

    [Range(0, 100)]  // 同様にスライダー表示
    public int anger;    // 怒り（味方戦闘不能、シナリオ展開で増加）

    [Range(0, 100)]  // 同様にスライダー表示
    public int sadness;  // 哀しみ（味方戦闘不能、シナリオ展開で増加）

    [Range(0, 100)]  // 同様にスライダー表示
    public int fear;     // 恐怖（敵の攻撃、先制攻撃、シナリオ展開で増加）

    // ==================== 感情変化状態 ====================
    [Header("感情変化状態（戦闘3回で自動リセット）")]  // Inspectorに見出しを表示
    public bool isEmotionActive;        // 感情変化中かどうか（いずれかの感情値が10以上になるとtrue）
    public int battleCountSinceChange;  // 感情変化後の戦闘回数（3回でリセット）

    // ==================== 一時的な感情変化 ====================
    [Header("一時的な感情変化（アイテム使用時、1戦闘のみ）")]  // Inspectorに見出しを表示
    public EmotionType temporaryEmotion = EmotionType.None;  // 駆逐の焔、追憶の果実などのアイテムで設定される一時的な感情

    // ==================== 定数 ====================
    // const = 定数（変更不可の固定値）。クラス内のどこからでも参照できる
    private const int EMOTION_THRESHOLD = 10;        // 感情変化が発動する閾値（この値以上で感情変化）
    private const int BATTLE_COUNT_TO_RESET = 3;     // リセットまでの戦闘回数（3回戦闘すると感情値-10）
    private const int RESET_AMOUNT = 10;             // リセット時に減少する感情値の量

    // ==================== 感情判定 ====================

    /// <summary>
    /// 現在発動している感情を取得（複数の場合もあり）
    /// </summary>
    /// <returns>発動中の感情の配列（EmotionType[]）</returns>
    public EmotionType[] GetActiveEmotions()
    {
        // 一時的な感情が優先（アイテムで設定された感情がある場合はそれを返す）
        if (temporaryEmotion != EmotionType.None)  // None = 「なし」の意味
        {
            return new EmotionType[] { temporaryEmotion };  // new EmotionType[] { } = EmotionType型の配列を新規作成
        }

        // System.Collections.Generic.List<T> = 可変長配列（要素数が動的に変わるリスト）
        var emotions = new System.Collections.Generic.List<EmotionType>();

        // 各感情値が閾値以上ならリストに追加
        if (joy >= EMOTION_THRESHOLD) emotions.Add(EmotionType.Joy);          // Add() = リストに要素を追加
        if (anger >= EMOTION_THRESHOLD) emotions.Add(EmotionType.Anger);
        if (sadness >= EMOTION_THRESHOLD) emotions.Add(EmotionType.Sadness);
        if (fear >= EMOTION_THRESHOLD) emotions.Add(EmotionType.Fear);

        return emotions.ToArray();  // ToArray() = Listを配列に変換して返す
    }

    /// <summary>
    /// 感情変化が発動しているかチェック
    /// </summary>
    /// <returns>感情変化が発動していればtrue、していなければfalse</returns>
    public bool HasEmotionChange()
    {
        // temporaryEmotion != EmotionType.None = 一時的な感情が設定されている
        // || = 「または」の意味（論理和）
        // GetActiveEmotions().Length > 0 = 発動中の感情が1つ以上ある
        return temporaryEmotion != EmotionType.None || GetActiveEmotions().Length > 0;
    }

    // ==================== 感情値操作 ====================

    /// <summary>
    /// 感情値を加算
    /// </summary>
    /// <param name="type">加算する感情の種類</param>
    /// <param name="amount">加算量</param>
    public void AddEmotion(EmotionType type, int amount)
    {
        // 感情変化中はパラメータは動かない（仕様書に記載）
        if (isEmotionActive) return;  // return = メソッドをここで終了

        // switch文 = typeの値に応じて処理を分岐
        switch (type)
        {
            case EmotionType.Joy:  // typeがJoyの場合
                // Mathf.Clamp(値, 最小値, 最大値) = 値を最小値〜最大値の範囲内に収める
                joy = Mathf.Clamp(joy + amount, 0, 100);  // 0〜100の範囲に制限
                break;  // switchから抜ける
            case EmotionType.Anger:  // typeがAngerの場合
                anger = Mathf.Clamp(anger + amount, 0, 100);
                break;
            case EmotionType.Sadness:  // typeがSadnessの場合
                sadness = Mathf.Clamp(sadness + amount, 0, 100);
                break;
            case EmotionType.Fear:  // typeがFearの場合
                fear = Mathf.Clamp(fear + amount, 0, 100);
                break;
        }

        // 感情変化が発動したかチェック
        CheckEmotionChange();
    }

    /// <summary>
    /// 一時的な感情を設定（アイテム使用時）
    /// 駆逐の焔（怒り）、追憶の果実（哀しみ）などのアイテムで使用
    /// </summary>
    /// <param name="type">設定する感情の種類</param>
    public void SetTemporaryEmotion(EmotionType type)
    {
        temporaryEmotion = type;  // 一時的な感情を設定
    }

    /// <summary>
    /// 感情変化が発動したかチェック
    /// </summary>
    private void CheckEmotionChange()  // private = このクラス内からのみ呼び出し可能
    {
        // HasEmotionChange() && !isEmotionActive = 感情変化が発動しているが、まだisEmotionActiveがfalseの場合
        // && = 「かつ」の意味（論理積）
        // ! = 「否定」の意味（!isEmotionActive = isEmotionActiveがfalseという意味）
        if (HasEmotionChange() && !isEmotionActive)
        {
            isEmotionActive = true;  // 感情変化中フラグをON
            battleCountSinceChange = 0;  // 戦闘回数カウンターをリセット
        }
    }

    // ==================== 戦闘関連 ====================

    /// <summary>
    /// 戦闘終了時の処理
    /// BattleManagerなどから呼び出される
    /// </summary>
    public void OnBattleEnd()
    {
        // 一時的な感情をリセット（1戦闘のみ有効なため）
        temporaryEmotion = EmotionType.None;

        // 感情変化中の場合、戦闘回数をカウント
        if (isEmotionActive)
        {
            battleCountSinceChange++;  // ++ = 1を加算（battleCountSinceChange = battleCountSinceChange + 1 と同じ）

            // 3戦闘経過でリセット
            if (battleCountSinceChange >= BATTLE_COUNT_TO_RESET)  // >= = 以上
            {
                ResetEmotions();  // 感情値をリセット
            }
        }
    }

    /// <summary>
    /// 感情値をリセット（全感情値-10）
    /// </summary>
    private void ResetEmotions()  // private = このクラス内からのみ呼び出し可能
    {
        // Mathf.Max(a, b) = aとbの大きい方を返す（0未満にならないようにする）
        joy = Mathf.Max(0, joy - RESET_AMOUNT);          // joy - 10 と 0 の大きい方を代入
        anger = Mathf.Max(0, anger - RESET_AMOUNT);      // anger - 10 と 0 の大きい方を代入
        sadness = Mathf.Max(0, sadness - RESET_AMOUNT);  // sadness - 10 と 0 の大きい方を代入
        fear = Mathf.Max(0, fear - RESET_AMOUNT);        // fear - 10 と 0 の大きい方を代入

        isEmotionActive = false;     // 感情変化中フラグをOFF
        battleCountSinceChange = 0;  // 戦闘回数カウンターをリセット
    }

    /// <summary>
    /// 感情値を完全にリセット（デバッグ用）
    /// </summary>
    public void ClearAllEmotions()
    {
        joy = 0;      // 喜びを0に
        anger = 0;    // 怒りを0に
        sadness = 0;  // 哀しみを0に
        fear = 0;     // 恐怖を0に
        isEmotionActive = false;     // 感情変化中フラグをOFF
        battleCountSinceChange = 0;  // 戦闘回数カウンターをリセット
        temporaryEmotion = EmotionType.None;  // 一時的な感情をクリア
    }
}

/// <summary>
/// 感情の種類
/// enumは「列挙型」。定義した名前に自動的に0, 1, 2...と番号が振られる
/// None=0, Joy=1, Anger=2, Sadness=3, Fear=4 という整数として扱われる
/// </summary>
public enum EmotionType
{
    None,       // なし（0）
    Joy,        // 喜び（1）
    Anger,      // 怒り（2）
    Sadness,    // 哀しみ（3）
    Fear        // 恐怖（4）
}
