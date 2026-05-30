using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// シーン遷移時のフェードオーバーレイ。
/// FadeManager がなくてもセットアップ不要で動く自己完結型。
/// MapTransition が自動生成して使う。手動で使う場合は Create() は不要。
///
/// フロー（シーン遷移）:
///   FadeOut() → 画面が黒くなる → SceneManager.LoadScene() → 新シーンロード → 自動でFadeIn → 消滅
///
/// フロー（同一シーン内テレポート）:
///   FadeOut() → テレポート → FadeIn() → 消滅
/// </summary>
public class TransitionFader : MonoBehaviour
{
    // ==================== 静的インスタンス管理 ====================

    // 現在アクティブなフェーダー（同時に複数生成しないようにする）
    private static TransitionFader _active;

    /// <summary>フェードが進行中かどうか。MapTransition がトリガー無視の判定に使う。</summary>
    public static bool IsActive => _active != null;

    // ==================== 内部コンポーネント ====================

    // フェードに使うImage（真っ黒な全画面パネル）
    private Image _image;

    // フェード時間
    private float _duration;

    // ==================== 静的ファクトリ ====================

    /// <summary>
    /// TransitionFader を生成する。既にアクティブな場合はそれを返す。
    /// </summary>
    public static TransitionFader GetOrCreate(float duration)
    {
        if (_active != null) return _active;

        // Canvas を生成
        GameObject go = new GameObject("[TransitionFade]");
        DontDestroyOnLoad(go); // シーン遷移後も生き残る

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // 最前面に表示

        // 全画面を覆う Image を生成
        GameObject imageGO = new GameObject("Panel");
        imageGO.transform.SetParent(go.transform, false);

        Image img = imageGO.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f); // 最初は透明
        img.raycastTarget = false; // 背後のUIをブロックしない

        // フルストレッチ（画面全体を覆う）
        RectTransform rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // コンポーネントを登録
        TransitionFader fader = go.AddComponent<TransitionFader>();
        fader._image = img;
        fader._duration = duration;
        _active = fader;
        return fader;
    }

    // ==================== 公開コルーチン ====================

    /// <summary>
    /// 画面を黒くフェードアウトする。完了まで待機する。
    /// シーン遷移用途では FadeOut 後に sceneLoaded で自動 FadeIn されるので FadeIn() を呼ばなくてよい。
    /// </summary>
    public static IEnumerator FadeOut(float duration)
    {
        TransitionFader fader = GetOrCreate(duration);

        // シーンロード後に自動でフェードインするよう登録
        SceneManager.sceneLoaded += fader.OnSceneLoaded;

        yield return fader.StartCoroutine(fader.DoFade(0f, 1f));
    }

    /// <summary>
    /// すでに黒画面の状態でシーン遷移する場合に使う。
    /// フェードアウトはスキップして即座に黒にし、新シーンロード後に fadeInDuration 秒でフェードインする。
    /// </summary>
    public static IEnumerator FadeOutInstant(float fadeInDuration)
    {
        TransitionFader fader = GetOrCreate(fadeInDuration);
        fader._image.color = new Color(0f, 0f, 0f, 1f); // 即座に黒
        SceneManager.sceneLoaded += fader.OnSceneLoaded;
        yield break;
    }

    /// <summary>
    /// 黒から透明にフェードインする。完了まで待機する。
    /// 同一シーン内テレポート後に呼ぶ用途。シーン遷移では自動で呼ばれるので不要。
    /// </summary>
    public static IEnumerator FadeIn()
    {
        if (_active == null) yield break;

        // sceneLoaded 登録を外す（Teleport では発火しないので残ったまま）
        SceneManager.sceneLoaded -= _active.OnSceneLoaded;

        yield return _active.StartCoroutine(_active.DoFadeInAndDestroy());
    }

    // ==================== 内部処理 ====================

    /// <summary>
    /// 新しいシーンがロードされたとき自動でフェードインする
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 一度だけ発火
        StartCoroutine(DoFadeInAndDestroy());
    }

    /// <summary>
    /// フェードイン後に自分を消滅させる
    /// </summary>
    IEnumerator DoFadeInAndDestroy()
    {
        yield return DoFade(1f, 0f);
        _active = null;
        Destroy(gameObject);
    }

    /// <summary>
    /// アルファ値を from → to に duration 秒かけて変化させる
    /// </summary>
    IEnumerator DoFade(float from, float to)
    {
        _image.color = new Color(0f, 0f, 0f, from);
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _image.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t));
            yield return null;
        }
        _image.color = new Color(0f, 0f, 0f, to);
    }
}
