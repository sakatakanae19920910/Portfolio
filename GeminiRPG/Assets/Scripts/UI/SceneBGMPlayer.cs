using UnityEngine;

/// <summary>
/// シーン開始時に指定した BGM を再生するコンポーネント。
/// 各シーンの空 GameObject にアタッチし、Inspector で BGM 名を設定するだけで使える。
/// </summary>
public class SceneBGMPlayer : MonoBehaviour
{
    [Header("BGM設定")]
    [Tooltip("SoundManager の BGM リストに登録されているクリップ名")]
    public string bgmName;                    // 再生する BGM 名

    [Tooltip("フェードインにかける秒数")]
    public float fadeDuration = 1.5f;         // デフォルト1.5秒でフェードイン

    void Start()
    {
        // SoundManager が存在しない・BGM名が未設定なら何もしない
        if (SoundManager.Instance == null || string.IsNullOrEmpty(bgmName)) return;

        SoundManager.Instance.PlayBGMWithFade(bgmName, fadeDuration);
    }
}
