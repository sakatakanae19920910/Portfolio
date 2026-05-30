using UnityEngine;

/// <summary>
/// Resources フォルダに置く小さな設定ファイル。
/// SoundManager プレハブ（Assets/_Prefabs/）への参照だけを持つ。
///
/// ■ 作成手順（初回のみ）
///   1. Project ウィンドウで右クリック
///      → Create > GeminiRPG > SoundManager Bootstrap Config
///   2. 作成した .asset を Assets/Resources/ に移動
///      （Resources フォルダがなければ作成する）
///   3. Inspector で Sound Manager Prefab に
///      Assets/_Prefabs/SoundManager プレハブをアサイン
/// </summary>
[CreateAssetMenu(
    fileName = "SoundManagerBootstrapConfig",
    menuName  = "GeminiRPG/SoundManager Bootstrap Config"
)]
public class SoundManagerBootstrapConfig : ScriptableObject
{
    [Tooltip("Assets/_Prefabs/ にある SoundManager プレハブをアサイン")]
    public GameObject soundManagerPrefab;
}

/// <summary>
/// どのシーンから再生を始めても SoundManager を自動生成するクラス。
/// BootScene など既に SoundManager がある場合は何もしない。
/// </summary>
public static class SoundManagerBootstrap
{
    // AfterSceneLoad: シーン内の全 Awake() 完了後に呼ばれる
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoLoad()
    {
        // 既に SoundManager が存在すれば何もしない（BootScene から始めた場合など）
        if (SoundManager.Instance != null) return;

        // Resources/SoundManagerBootstrapConfig を読み込む（.asset ファイル）
        var config = Resources.Load<SoundManagerBootstrapConfig>("SoundManagerBootstrapConfig");
        if (config == null)
        {
            Debug.LogWarning(
                "[SoundManagerBootstrap] Resources/SoundManagerBootstrapConfig.asset が見つかりません。\n" +
                "Create > GeminiRPG > SoundManager Bootstrap Config で作成して Resources/ に置いてください。"
            );
            return;
        }

        if (config.soundManagerPrefab == null)
        {
            Debug.LogWarning("[SoundManagerBootstrap] SoundManagerBootstrapConfig に SoundManager プレハブがアサインされていません。");
            return;
        }

        // プレハブを生成（DontDestroyOnLoad は SoundManager.Awake() 内で自動設定される）
        Object.Instantiate(config.soundManagerPrefab);
        Debug.Log("[SoundManagerBootstrap] SoundManager を自動生成しました");
    }
}
