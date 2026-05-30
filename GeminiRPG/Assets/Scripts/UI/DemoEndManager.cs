using UnityEngine;               // MonoBehaviour, Application など Unity 基本
using UnityEngine.UI;            // Button, Image など UI コンポーネント
using UnityEngine.SceneManagement; // SceneManager.LoadScene でシーン切替
using System.Collections;        // IEnumerator（コルーチン）
using DG.Tweening;               // DOFade などの DOTween アニメーション
using TMPro;                     // TextMeshProUGUI（テキスト表示）

/// <summary>
/// デモ版終了画面を管理するクラス
///
/// デモシナリオの最後（Intro.csv 等で LoadScene:DemoEndScene が呼ばれた後）に表示される
/// 担当する処理:
/// 1. 起動時に黒→コンテンツへフェードイン
/// 2. 「Steamページを見る」ボタン → ブラウザで Steam ストアページを開く
/// 3. 「X（旧Twitter）をフォローする」ボタン → ブラウザで X ページを開く
/// 4. 「もう一度遊ぶ」ボタン → TitleScene へ戻る
///
/// Scene 構成:
///   Canvas
///   ├── Background    (Image / 暗い単色背景)
///   ├── ContentPanel  (CanvasGroup / フェードイン対象まとめ)
///   │     ├── TitleText     (TMP / 「流転のジェミニ」)
///   │     ├── MessageText   (TMP / お礼メッセージ)
///   │     ├── SteamButton    (Button / Steamページボタン)
///   │     ├── WishlistButton (Button / Xフォローボタン)
///   │     └── RetryButton   (Button / もう一度遊ぶ)
///   └── FadeImage     (Image / 全画面黒)
/// </summary>
public class DemoEndManager : MonoBehaviour
{
    // ==================== Inspector 設定 ====================

    [Header("UI")]
    [Tooltip("コンテンツ全体をまとめた CanvasGroup（フェード対象）")]
    public CanvasGroup contentPanel;        // alpha を 0→1 にしてフェードイン

    [Tooltip("「Steamページを見る」ボタン")]
    public Button steamButton;

    [Tooltip("「X（旧Twitter）をフォローする」ボタン")]
    public Button wishlistButton;

    [Tooltip("「もう一度遊ぶ」ボタン")]
    public Button retryButton;

    [Tooltip("画面フェード用の黒い全画面 Image")]
    public Image fadeImage;

    [Header("言語対応テキスト")]
    [Tooltip("お礼メッセージ（1行目）")]
    public TextMeshProUGUI messageText;

    [Tooltip("Steam告知メッセージ（2行目）")]
    public TextMeshProUGUI messageText2;

    [Tooltip("Steam ボタンのラベル")]
    public TextMeshProUGUI steamButtonLabel;

    [Tooltip("X フォローボタンのラベル")]
    public TextMeshProUGUI wishlistButtonLabel;

    [Tooltip("もう一度遊ぶボタンのラベル")]
    public TextMeshProUGUI retryButtonLabel;

    [Header("設定")]
    [Tooltip("Steam ストアページの URL")]
    public string steamUrl = "https://store.steampowered.com/app/4764140/?utm_source=unityroom_demo&utm_medium=in_game_button&utm_campaign=demo_end";

    [Tooltip("X（旧Twitter）フォロー先の URL")]
    public string wishlistUrl = "https://x.com/salmonDance_k";

    [Tooltip("「もう一度遊ぶ」で遷移するシーン名")]
    public string titleSceneName = "TitleScene";

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // コンテンツを最初は非表示にしておく（フェードインで見せるため）
        if (contentPanel != null)
        {
            contentPanel.alpha = 0f;        // 透明
            contentPanel.interactable = false; // 透明中はクリック無効
            contentPanel.blocksRaycasts = false;
        }

        // フェード用 Image を黒・不透明で初期化
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 1f);  // 真っ黒
            fadeImage.raycastTarget = false;
        }

        // ボタンにイベントを登録
        if (steamButton != null)
            steamButton.onClick.AddListener(OnSteamClicked);
        else
            Debug.LogError("DemoEndManager: steamButton が設定されていません！");

        if (wishlistButton != null)
            wishlistButton.onClick.AddListener(OnWishlistClicked);
        else
            Debug.LogError("DemoEndManager: wishlistButton が設定されていません！");

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        else
            Debug.LogError("DemoEndManager: retryButton が設定されていません！");

        // 言語に合わせてテキストを切り替え
        if (GameData.Language == "EN") ApplyEnglishText();

        // 起動時: 黒→コンテンツへの2段階フェードイン
        StartCoroutine(FadeInSequence());
    }

    // ==================== 言語切り替え ====================

    /// <summary>EN 選択時に全テキストを英語に切り替える</summary>
    void ApplyEnglishText()
    {
        if (messageText != null)
            messageText.text = "Thank you for playing all the way through!";
        if (messageText2 != null)
            messageText2.text = "Our Steam page is now live.\nWe'd be thrilled if you'd add it to your Wishlist.\nFor the latest updates, follow us on X (formerly Twitter).";
        if (steamButtonLabel != null)
            steamButtonLabel.text = "View Steam Page";
        if (wishlistButtonLabel != null)
            wishlistButtonLabel.text = "Follow on X";
        if (retryButtonLabel != null)
            retryButtonLabel.text = "Play Again";
    }

    // ==================== コルーチン ====================

    /// <summary>
    /// 2段階のフェードイン演出
    /// 1. 黒を透明に（0.8秒）
    /// 2. コンテンツを表示（0.5秒）
    /// </summary>
    IEnumerator FadeInSequence()
    {
        // 1. 黒→透明（画面を見せる）
        if (fadeImage != null)
        {
            yield return fadeImage.DOFade(0f, 0.8f)
                .SetEase(Ease.OutQuad)
                .WaitForCompletion();               // 終わるまで待つ
        }

        // 少し間をおいてからコンテンツを表示
        yield return new WaitForSeconds(0.2f);

        // 2. コンテンツをフェードイン（透明→不透明）
        if (contentPanel != null)
        {
            contentPanel.interactable = true;       // ボタン操作を有効化
            contentPanel.blocksRaycasts = true;
            contentPanel.DOFade(1f, 0.5f)
                .SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// フェードアウト → シーン遷移の共通処理
    /// </summary>
    IEnumerator FadeOutAndLoad(string sceneName)
    {
        // ボタンを無効化（二重操作防止）
        if (steamButton != null)    steamButton.interactable = false;
        if (wishlistButton != null) wishlistButton.interactable = false;
        if (retryButton != null)    retryButton.interactable = false;

        // フェードアウト: 透明→黒
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
            yield return fadeImage.DOFade(1f, 0.5f)
                .SetEase(Ease.InQuad)
                .WaitForCompletion();
        }

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }

    // ==================== ボタンイベント ====================

    /// <summary>
    /// Steamボタンが押されたときの処理
    /// Application.OpenURL は WebGL でも動作し、ブラウザの新しいタブで開く
    /// </summary>
    void OnSteamClicked()
    {
        Application.OpenURL(steamUrl);      // ブラウザで Steam ストアページを開く
    }

    /// <summary>
    /// Xフォローボタンが押されたときの処理
    /// </summary>
    void OnWishlistClicked()
    {
        Application.OpenURL(wishlistUrl);   // ブラウザで X ページを開く
    }

    /// <summary>
    /// 「もう一度遊ぶ」ボタンが押されたときの処理
    /// </summary>
    void OnRetryClicked()
    {
        // 前回プレイのデータをリセットしてから遷移（フラグ・アイテム・主人公選択を初期化）
        GameData.Reset();
        StartCoroutine(FadeOutAndLoad(titleSceneName));
    }
}
