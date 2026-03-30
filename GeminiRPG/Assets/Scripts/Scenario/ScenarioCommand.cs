using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// シナリオCSVの1行分のデータ
/// 新フォーマット（5列方式）: Label, Command, Speaker, Text, Args
/// Args は key=value 形式のパラメータを ; 区切りで格納
/// </summary>
[System.Serializable]
public class ScenarioCommand
{
    // ==================== 基本フィールド（5列） ====================
    // CSVの各列に対応する固定フィールド

    /// <summary>
    /// ラベル名（ジャンプ先として使用）
    /// 例: "Ev_1_1_1Start", "Set_Nagi"
    /// </summary>
    public string Label;

    /// <summary>
    /// コマンド名（実行する処理の種類）
    /// 例: "Text", "TextFade", "Choice", "Jump", "Branch"
    /// </summary>
    public string Command;

    /// <summary>
    /// 話者名（会話文で「誰が喋っているか」）
    /// 例: "カイリ", "ナギ", "ルイ"
    /// </summary>
    public string Speaker;

    /// <summary>
    /// 表示テキスト（会話文やモノローグの本文）
    /// 例: "おはよう、ナギ。", "（……なんだか嫌な予感がする）"
    /// </summary>
    public string Text;

    // ==================== 汎用パラメータ ====================
    // コマンドごとに異なるパラメータを key=value 形式で格納

    /// <summary>
    /// 汎用パラメータ辞書
    /// CSV の Args 列を key=value 形式でパースした結果
    /// 例: { "time": "2.0", "color": "white", "face": "smile" }
    /// </summary>
    public Dictionary<string, string> Args;

    // ==================== コンストラクタ ====================

    /// <summary>
    /// コンストラクタ（CSV行から生成）
    /// </summary>
    /// <param name="label">ラベル名</param>
    /// <param name="command">コマンド名</param>
    /// <param name="speaker">話者名</param>
    /// <param name="text">表示テキスト</param>
    /// <param name="args">パースされたパラメータ辞書</param>
    public ScenarioCommand(
        string label,
        string command,
        string speaker,
        string text,
        Dictionary<string, string> args
    )
    {
        // 各フィールドに値を設定
        Label = label ?? "";       // null の場合は空文字に
        Command = command ?? "";
        Speaker = speaker ?? "";
        Text = text ?? "";
        Args = args ?? new Dictionary<string, string>();  // null の場合は空の辞書
    }

    // ==================== アクセサメソッド ====================
    // Args 辞書から型変換して値を取得するためのヘルパー

    /// <summary>
    /// 文字列パラメータを取得
    /// </summary>
    /// <param name="key">パラメータ名（例: "face", "jumpTo"）</param>
    /// <param name="defaultVal">キーが存在しない場合のデフォルト値</param>
    /// <returns>パラメータの値、なければデフォルト値</returns>
    public string GetString(string key, string defaultVal = "")
    {
        // 辞書にキーが存在するかチェック
        if (Args.TryGetValue(key, out string value))
        {
            // 存在すれば値を返す
            return value;
        }
        // 存在しなければデフォルト値を返す
        return defaultVal;
    }

    /// <summary>
    /// float パラメータを取得
    /// </summary>
    /// <param name="key">パラメータ名（例: "time", "vol"）</param>
    /// <param name="defaultVal">キーが存在しない or 変換失敗時のデフォルト値</param>
    /// <returns>パラメータの値を float に変換したもの、失敗時はデフォルト値</returns>
    public float GetFloat(string key, float defaultVal = 0f)
    {
        // 辞書にキーが存在するかチェック
        if (Args.TryGetValue(key, out string value))
        {
            // float に変換を試みる
            if (float.TryParse(value, out float result))
            {
                return result;
            }
            // 変換失敗時は警告を出す
            Debug.LogWarning($"ScenarioCommand: '{key}' の値 '{value}' を float に変換できません");
        }
        return defaultVal;
    }

    /// <summary>
    /// int パラメータを取得
    /// </summary>
    /// <param name="key">パラメータ名（例: "count", "level"）</param>
    /// <param name="defaultVal">キーが存在しない or 変換失敗時のデフォルト値</param>
    /// <returns>パラメータの値を int に変換したもの、失敗時はデフォルト値</returns>
    public int GetInt(string key, int defaultVal = 0)
    {
        // 辞書にキーが存在するかチェック
        if (Args.TryGetValue(key, out string value))
        {
            // int に変換を試みる
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            // 変換失敗時は警告を出す
            Debug.LogWarning($"ScenarioCommand: '{key}' の値 '{value}' を int に変換できません");
        }
        return defaultVal;
    }

    /// <summary>
    /// bool パラメータを取得
    /// "true", "1", "yes" を true として扱う（大文字小文字無視）
    /// </summary>
    /// <param name="key">パラメータ名（例: "loop", "skip"）</param>
    /// <param name="defaultVal">キーが存在しない場合のデフォルト値</param>
    /// <returns>パラメータの値を bool に変換したもの</returns>
    public bool GetBool(string key, bool defaultVal = false)
    {
        // 辞書にキーが存在するかチェック
        if (Args.TryGetValue(key, out string value))
        {
            // 小文字に変換して比較
            string lower = value.ToLower();
            // "true", "1", "yes" のいずれかなら true
            return lower == "true" || lower == "1" || lower == "yes";
        }
        return defaultVal;
    }

    /// <summary>
    /// 指定したキーのパラメータが存在するかチェック
    /// </summary>
    /// <param name="key">パラメータ名</param>
    /// <returns>存在すれば true</returns>
    public bool HasArg(string key)
    {
        return Args.ContainsKey(key);
    }

    // ==================== デバッグ用 ====================

    /// <summary>
    /// デバッグ用の文字列表現
    /// </summary>
    public override string ToString()
    {
        // Args の内容を文字列化
        string argsStr = "";
        foreach (var kvp in Args)
        {
            argsStr += $"{kvp.Key}={kvp.Value}; ";
        }
        return $"[{Label}] {Command}({Speaker}, {Text}, Args: {argsStr})";
    }
}
