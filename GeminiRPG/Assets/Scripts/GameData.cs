using System.Collections.Generic;

/// <summary>
/// シーンをまたいで共有するゲームデータ
/// static クラスなのでどこからでも GameData.XXX と書くだけでアクセスできる
/// MonoBehaviour ではないので DontDestroyOnLoad 不要、メモリ上に残り続ける
/// </summary>
public static class GameData
{
    /// <summary>
    /// プレイヤーが選択した主人公
    /// "Nagi"（ナギ編）または "Mizuki"（ミズキ編）
    /// Intro.csv の SetPlayerChar コマンドで書き込まれる
    /// </summary>
    public static string SelectedCharacter = "Nagi";

    /// <summary>
    /// ミタマ石の所持数
    /// GiveItem name=MitamaStone コマンドで加算される
    /// </summary>
    public static int MitamaCount = 0;

    /// <summary>
    /// きらきら星を所持しているか
    /// GiveItem name=KirakiraStar コマンドで true になる（同時にミタマ石7個を消費）
    /// </summary>
    public static bool HasKirakiraStar = false;

    /// <summary>
    /// シーンをまたぐ永続フラグ辞書
    /// SetFlag コマンドで書き込まれ、Branch コマンドで参照される
    /// 例: "WeaponShopOwnerVisited" = true（武器屋店主に初回話しかけた）
    /// </summary>
    public static Dictionary<string, bool> PersistentFlags = new Dictionary<string, bool>();

    /// <summary>
    /// 表示言語の設定。"JA"（日本語）または "EN"（英語）。
    /// タイトル画面の言語ボタンで切り替える。
    /// </summary>
    public static string Language = "JA";

    /// <summary>
    /// 全データを初期値に戻す。「もう一度遊ぶ」時に呼ぶこと。
    /// </summary>
    public static void Reset()
    {
        SelectedCharacter = "Nagi";
        MitamaCount       = 0;
        HasKirakiraStar   = false;
        PersistentFlags.Clear();
        // Language はリセットしない（プレイヤーの設定を保持）
    }
}
