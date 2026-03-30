using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 画面フェード（暗転・明転）を管理するコンポーネント
/// シングルトンパターンでどこからでもアクセス可能
/// 
/// セットアップ:
/// 1. Canvas を作成（Screen Space - Overlay）
/// 2. 画面全体を覆う Image を作成（色は黒、RaycastTarget は false）
/// 3. この FadeManager を Canvas にアタッチ
/// 4. fadeImage に Image をアサイン
/// </summary>
public class FadeManager : MonoBehaviour
{
    // ==================== シングルトン ====================

    /// <summary>
    /// FadeManager のシングルトンインスタンス
    /// </summary>
    public static FadeManager Instance { get; private set; }

    // ==================== Inspector設定 ====================

    [Header("Fade Settings")]
    [Tooltip("フェードに使用する Image（画面全体を覆う黒いImage）")]
    public Image fadeImage;

    [Tooltip("フェードのデフォルト色")]
    public Color fadeColor = Color.black;

    [Tooltip("デフォルトのフェード時間")]
    public float defaultDuration = 0.5f;

    [Header("Initial State")]
    [Tooltip("ゲーム開始時にフェードインするか")]
    public bool fadeInOnStart = true;

    // ==================== 内部変数 ====================

    // 現在フェード中かどうか
    private bool isFading = false;

    // フェード用のコルーチン（中断用）
    private Coroutine fadeCoroutine;

    // ==================== プロパティ ====================

    /// <summary>
    /// 現在フェード中かどうか
    /// </summary>
    public bool IsFading => isFading;

    // ==================== Unity ライフサイクル ====================

    void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            // このインスタンスをシングルトンとして登録
            Instance = this;
            
            // シーンをまたいでも破棄されないように設定
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既にインスタンスがある場合、このオブジェクトを破棄
            Destroy(gameObject);
            return;
        }

        // fadeImage の初期設定
        if (fadeImage != null)
        {
            // RaycastTarget を false にする（フェード中もUI操作を許可する場合）
            // クリック防止したい場合は true のまま
            fadeImage.raycastTarget = false;
        }
    }

    void Start()
    {
        // fadeImage が設定されていない場合は警告
        if (fadeImage == null)
        {
            Debug.LogWarning("FadeManager: fadeImage が設定されていません。Inspector で Image をアサインしてください。");
            return;
        }

        // 開始時にフェードインする設定の場合
        if (fadeInOnStart)
        {
            // 最初は真っ暗
            SetAlpha(1f);
            
            // フェードイン開始
            FadeIn(defaultDuration);
        }
        else
        {
            // フェードインしない場合は透明に
            SetAlpha(0f);
        }
    }

    // ==================== フェード操作 ====================

    /// <summary>
    /// フェードアウト（画面を暗くする）
    /// </summary>
    /// <param name="duration">フェード時間（秒）</param>
    public void FadeOut(float duration = -1f)
    {
        // 引数が負の場合はデフォルト値を使用
        if (duration < 0f) duration = defaultDuration;
        
        // フェードを実行
        StartFade(1f, duration);
    }

    /// <summary>
    /// フェードイン（画面を明るくする）
    /// </summary>
    /// <param name="duration">フェード時間（秒）</param>
    public void FadeIn(float duration = -1f)
    {
        // 引数が負の場合はデフォルト値を使用
        if (duration < 0f) duration = defaultDuration;
        
        // フェードを実行
        StartFade(0f, duration);
    }

    /// <summary>
    /// フェードを開始する
    /// </summary>
    /// <param name="targetAlpha">目標の透明度（0=透明, 1=不透明）</param>
    /// <param name="duration">フェード時間（秒）</param>
    public void StartFade(float targetAlpha, float duration)
    {
        // fadeImage がない場合は何もしない
        if (fadeImage == null)
        {
            Debug.LogWarning("FadeManager: fadeImage が設定されていません");
            return;
        }

        // 既存のフェードを中断
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // 新しいフェードを開始
        fadeCoroutine = StartCoroutine(DoFade(targetAlpha, duration));
    }

    /// <summary>
    /// フェードを実行するコルーチン
    /// </summary>
    IEnumerator DoFade(float targetAlpha, float duration)
    {
        // フェード中フラグを立てる
        isFading = true;

        // 現在の透明度を取得
        float startAlpha = fadeImage.color.a;
        
        // 経過時間
        float elapsed = 0f;

        // duration秒かけて透明度を変化させる
        while (elapsed < duration)
        {
            // 経過時間を更新
            elapsed += Time.deltaTime;
            
            // 0〜1 の補間値を計算
            float t = Mathf.Clamp01(elapsed / duration);
            
            // 透明度を線形補間
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            // Image の透明度を更新
            SetAlpha(currentAlpha);
            
            // 次のフレームまで待機
            yield return null;
        }

        // 最終値を確実に設定
        SetAlpha(targetAlpha);

        // フェード完了
        isFading = false;
        fadeCoroutine = null;
    }

    // ==================== ユーティリティ ====================

    /// <summary>
    /// Image の透明度を設定する
    /// </summary>
    void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeColor;
            color.a = alpha;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// 即座にフェードアウトする（アニメーションなし）
    /// </summary>
    public void SetFadeOutImmediate()
    {
        SetAlpha(1f);
    }

    /// <summary>
    /// 即座にフェードインする（アニメーションなし）
    /// </summary>
    public void SetFadeInImmediate()
    {
        SetAlpha(0f);
    }

    /// <summary>
    /// フェードアウト → 待機 → フェードインを連続実行
    /// カットシーンの切り替えなどに便利
    /// </summary>
    /// <param name="outDuration">フェードアウト時間</param>
    /// <param name="waitDuration">暗転中の待機時間</param>
    /// <param name="inDuration">フェードイン時間</param>
    public void FadeOutAndIn(float outDuration, float waitDuration, float inDuration)
    {
        StartCoroutine(DoFadeOutAndIn(outDuration, waitDuration, inDuration));
    }

    IEnumerator DoFadeOutAndIn(float outDuration, float waitDuration, float inDuration)
    {
        // フェードアウト
        yield return DoFade(1f, outDuration);
        
        // 待機
        yield return new WaitForSeconds(waitDuration);
        
        // フェードイン
        yield return DoFade(0f, inDuration);
    }

    /// <summary>
    /// フェード色を変更する
    /// </summary>
    /// <param name="color">新しいフェード色</param>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
        
        // 現在の透明度を維持しつつ色を変更
        if (fadeImage != null)
        {
            float currentAlpha = fadeImage.color.a;
            color.a = currentAlpha;
            fadeImage.color = color;
        }
    }
}
