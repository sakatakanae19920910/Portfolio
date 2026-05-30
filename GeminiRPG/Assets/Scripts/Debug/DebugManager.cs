using UnityEngine;

/// <summary>
/// デバッグ用の初期状態を設定するコンポーネント
/// BootScene か TownScene の任意の GameObject にアタッチして使う
/// ビルド時は設定を全部 OFF にすること（#if UNITY_EDITOR ガードで本番には影響しない）
/// </summary>
public class DebugManager : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("ミタマ石")]
    [Tooltip("ONにするとゲーム開始時にミタマ石を7個持った状態にする")]
    public bool debugFullMitama = false;

    [Header("きらきら星")]
    [Tooltip("ONにするとゲーム開始時にきらきら星を持った状態にする")]
    public bool debugHasKirakiraStar = false;

    [Header("主人公")]
    [Tooltip("ONにすると SelectedCharacter を上書きする")]
    public bool debugOverrideCharacter = false;

    [Tooltip("debugOverrideCharacter=true の時に使うキャラ名（Nagi / Mizuki）")]
    public string debugCharacter = "Nagi";

    // ==================== Unity ライフサイクル ====================

    void Awake()
    {
        // エディタ・開発ビルドのみ動作させる（リリースビルドでは一切実行されない）
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        // ミタマ石7個セット
        if (debugFullMitama)
        {
            GameData.MitamaCount = 7;
            Debug.Log("[DebugManager] MitamaCount = 7 にセットしました");
        }

        // きらきら星を持った状態にする
        if (debugHasKirakiraStar)
        {
            GameData.HasKirakiraStar = true;
            Debug.Log("[DebugManager] HasKirakiraStar = true にセットしました");
        }

        // 主人公を上書き
        if (debugOverrideCharacter)
        {
            GameData.SelectedCharacter = debugCharacter;
            Debug.Log($"[DebugManager] SelectedCharacter = {debugCharacter} にセットしました");
        }

#endif
    }
}
