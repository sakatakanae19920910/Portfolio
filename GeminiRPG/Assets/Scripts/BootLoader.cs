using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム起動時の初期化処理
/// Boot シーンに配置する
/// </summary>
public class BootLoader : MonoBehaviour
{
    [Header("遷移先シーン")]
    public string nextSceneName = "Title";

    void Start()
    {
        Debug.Log("=== ゲーム起動：初期化開始 ===");

        // SoundManager などの常駐マネージャーが生成される
        // （各マネージャーの Awake() で DontDestroyOnLoad される）

        // 初期化完了後、タイトルシーンへ遷移
        Debug.Log($"初期化完了 → {nextSceneName} へ遷移");
        SceneManager.LoadScene(nextSceneName);
    }
}
