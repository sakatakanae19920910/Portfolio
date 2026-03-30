using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// CSVファイルからシナリオコマンドを読み込む
/// 新フォーマット（5列方式）: Label, Command, Speaker, Text, Args
/// Args 列は key=value 形式を ; 区切りでパース
/// </summary>
public static class ScenarioLoader
{
    // ==================== 定数 ====================

    /// <summary>
    /// 新フォーマットの基本カラム数（Label, Command, Speaker, Text, Args）
    /// </summary>
    private const int COLUMN_COUNT = 5;

    // ==================== メインのロード処理 ====================

    /// <summary>
    /// CSVファイルを読み込んでコマンドリストを返す
    /// </summary>
    /// <param name="csvFile">読み込むCSVファイル（TextAsset）</param>
    /// <returns>パースされた ScenarioCommand のリスト</returns>
    public static List<ScenarioCommand> LoadFromCSV(TextAsset csvFile)
    {
        // 結果を格納するリスト
        List<ScenarioCommand> commands = new List<ScenarioCommand>();

        // CSVファイルが null の場合はエラー
        if (csvFile == null)
        {
            Debug.LogError("ScenarioLoader: CSVファイルが null です！");
            return commands;
        }

        // CSVの内容を改行で分割
        string[] lines = csvFile.text.Split('\n');

        // 各行を処理（ヘッダー行 [0] はスキップ）
        for (int i = 1; i < lines.Length; i++)
        {
            // 行の前後の空白を除去
            string line = lines[i].Trim();

            // 空行をスキップ
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // コメント行（# で始まる）をスキップ
            if (line.StartsWith("#"))
            {
                continue;
            }

            // CSVをパース（ダブルクォート内のカンマを考慮）
            string[] values = ParseCSVLine(line);

            // 最低2列（Label, Command）がないとおかしいので警告
            if (values.Length < 2)
            {
                Debug.LogWarning($"ScenarioLoader: 行 {i + 1}: データが極端に不足しています（{values.Length}列）");
                continue;
            }

            // 各列の値を取得（足りない列は空文字で補完）
            string label = GetValueOrEmpty(values, 0);
            string command = GetValueOrEmpty(values, 1);
            string speaker = GetValueOrEmpty(values, 2);
            string text = GetValueOrEmpty(values, 3);
            string argsRaw = GetValueOrEmpty(values, 4);

            // Args 列をパースして辞書に変換
            Dictionary<string, string> args = ParseArgs(argsRaw, i + 1);

            // ScenarioCommand を生成してリストに追加
            ScenarioCommand cmd = new ScenarioCommand(label, command, speaker, text, args);
            commands.Add(cmd);
        }

        // 読み込み完了をログ出力
        Debug.Log($"ScenarioLoader: シナリオ読み込み完了: {commands.Count} コマンド");
        return commands;
    }

    // ==================== CSVパース ====================

    /// <summary>
    /// CSV行をパースして配列に分割
    /// ダブルクォートで囲まれた中のカンマは無視する
    /// </summary>
    /// <param name="line">CSVの1行</param>
    /// <returns>分割された値の配列</returns>
    private static string[] ParseCSVLine(string line)
    {
        // 正規表現を使って、ダブルクォート内のカンマを無視して分割
        // パターン説明:
        // ,(?=...)  → カンマの後ろを先読み
        // (?:[^"]*"[^"]*")* → "で囲まれていない部分と囲まれた部分のペアが0回以上
        // (?![^"]*") → "で囲まれた中にいないことを確認
        string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
        string[] rawValues = Regex.Split(line, pattern);

        // 各値の前後のダブルクォートを除去し、エスケープされた "" を " に戻す
        for (int j = 0; j < rawValues.Length; j++)
        {
            // 前後の空白を除去
            rawValues[j] = rawValues[j].Trim();
            // 前後の " を除去
            rawValues[j] = rawValues[j].TrimStart('"').TrimEnd('"');
            // "" を " に置換（CSVのエスケープ規則）
            rawValues[j] = rawValues[j].Replace("\"\"", "\"");
        }

        return rawValues;
    }

    /// <summary>
    /// 配列から値を取得（範囲外なら空文字を返す）
    /// </summary>
    /// <param name="values">値の配列</param>
    /// <param name="index">取得するインデックス</param>
    /// <returns>値、または空文字</returns>
    private static string GetValueOrEmpty(string[] values, int index)
    {
        // インデックスが配列の範囲内かチェック
        if (index < values.Length)
        {
            return values[index].Trim();
        }
        // 範囲外なら空文字を返す
        return "";
    }

    // ==================== Args パース ====================

    /// <summary>
    /// Args 列をパースして辞書に変換
    /// 形式: "key1=value1; key2=value2; ..."
    /// </summary>
    /// <param name="argsRaw">パースする文字列</param>
    /// <param name="lineNumber">エラーメッセージ用の行番号</param>
    /// <returns>パースされた辞書</returns>
    private static Dictionary<string, string> ParseArgs(string argsRaw, int lineNumber)
    {
        // 結果を格納する辞書
        Dictionary<string, string> args = new Dictionary<string, string>();

        // 空文字列なら空の辞書を返す
        if (string.IsNullOrEmpty(argsRaw))
        {
            return args;
        }

        // セミコロンで分割
        string[] pairs = argsRaw.Split(';');

        // 各ペアを処理
        foreach (string pair in pairs)
        {
            // 前後の空白を除去
            string trimmed = pair.Trim();

            // 空なら次へ
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // = で分割
            int eqIndex = trimmed.IndexOf('=');

            // = が見つからない場合は警告を出してスキップ
            if (eqIndex < 0)
            {
                Debug.LogWarning($"ScenarioLoader: 行 {lineNumber}: Args のフォーマットが不正です（= がありません）: '{trimmed}'");
                continue;
            }

            // キーと値を取得
            string key = trimmed.Substring(0, eqIndex).Trim();
            string value = trimmed.Substring(eqIndex + 1).Trim();

            // キーが空なら警告を出してスキップ
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"ScenarioLoader: 行 {lineNumber}: Args のキーが空です: '{trimmed}'");
                continue;
            }

            // 辞書に追加（既に存在する場合は上書き）
            args[key] = value;
        }

        return args;
    }

    // ==================== ラベル検索 ====================

    /// <summary>
    /// ラベルを検索してインデックスを返す
    /// </summary>
    /// <param name="commands">検索対象のコマンドリスト</param>
    /// <param name="labelName">検索するラベル名</param>
    /// <returns>見つかったインデックス、見つからなければ -1</returns>
    public static int FindLabelIndex(List<ScenarioCommand> commands, string labelName)
    {
        // 全コマンドをループして検索
        for (int i = 0; i < commands.Count; i++)
        {
            // ラベルが一致したらそのインデックスを返す
            if (commands[i].Label == labelName)
            {
                return i;
            }
        }

        // 見つからなかったらエラーを出して -1 を返す
        Debug.LogError($"ScenarioLoader: ラベル '{labelName}' が見つかりません！");
        return -1;
    }
}
