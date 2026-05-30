using UnityEngine;               // MonoBehaviour, GameObject など Unity 基本
using UnityEngine.UI;            // Button, Image など UI コンポーネント
using UnityEngine.SceneManagement; // SceneManager.LoadScene でシーン切替
using System.Collections;        // IEnumerator（コルーチン）
using DG.Tweening;               // DOFade などの DOTween アニメーション
using TMPro;                     // TextMeshProUGUI（テキスト表示）

/// <summary>
/// タイトル画面を管理するクラス
///
/// 担当する処理:
/// 1. 起動時に黒→通常へフェードイン
/// 2. START ボタンが押されたら黒にフェードアウト → IntroScene へ遷移
///
/// Scene 構成:
///   Canvas
///   ├── Background     (Image / 暗い単色背景)
///   ├── TitleVisual    (Image / タイトルロゴ・言語で差し替え)
///   ├── LanguagePanel  (空 GameObject / JP・EN ボタンをまとめる)
///   │   ├── JPButton   (Button / 「JP」テキスト)
///   │   └── ENButton   (Button / 「EN」テキスト)
///   ├── StartButton    (Button / 「S T A R T」)
///   └── FadeImage      (Image / 全画面黒・RaycastTarget=ON)
///
/// このスクリプトは Canvas と同じ階層の空 GameObject にアタッチする
/// </summary>
public class TitleManager : MonoBehaviour
{
    // ==================== Inspector 設定 ====================

    [Header("UI")]
    [Tooltip("START ボタン")]
    public Button startButton;          // クリックでシーン遷移

    [Tooltip("画面フェード用の黒い全画面 Image")]
    public Image fadeImage;             // 暗転・明転の演出に使う

    [Header("言語選択ボタン")]
    [Tooltip("JP ボタン（日本語選択）")]
    public Button jpButton;             // 日本語ボタン

    [Tooltip("EN ボタン（英語選択）")]
    public Button enButton;             // 英語ボタン

    [Tooltip("選択中ボタンの文字色")]
    public Color selectedColor = Color.white;       // 選択されている側の色

    [Tooltip("非選択ボタンの文字色")]
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.35f); // 薄くして非選択を表現

    [Header("タイトルビジュアル")]
    [Tooltip("日本語ロゴの GameObject（位置ごと切り替えるため別オブジェクト）")]
    public GameObject titleLogoJA;      // logo_ja_tate を持つ GameObject

    [Tooltip("英語ロゴの GameObject")]
    public GameObject titleLogoEN;      // logo_en を持つ GameObject

    [Header("シーン設定")]
    [Tooltip("START 後に遷移するシーン名")]
    public string nextSceneName = "IntroScene"; // Build Settings に登録が必要

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // フェード用 Image を黒・不透明で初期化（最初は真っ黒）
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 1f);  // alpha=1 → 真っ黒
            fadeImage.raycastTarget = false;                // 操作を通過させる
        }
        else
        {
            Debug.LogError("TitleManager: fadeImage が設定されていません！");
        }

        // ボタンにクリックイベントを登録
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("TitleManager: startButton が設定されていません！");
        }

        // 言語ボタンにイベントを登録
        if (jpButton != null) jpButton.onClick.AddListener(OnJpButtonClicked);
        if (enButton != null) enButton.onClick.AddListener(OnEnButtonClicked);

        // 起動時の言語設定を反映してボタン表示を更新
        UpdateLanguageButtonVisuals();

        // 起動直後: 黒→透明へフェードイン（1秒かけてタイトルを見せる）
        if (fadeImage != null)
        {
            fadeImage.DOFade(0f, 1.0f).SetEase(Ease.OutQuad);
        }

        // タイトルBGM再生（フェードインで流れ始める）
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGMWithFade("Theme_Main", fadeDuration: 1.5f);
    }

    // ==================== ボタンイベント ====================

    /// <summary>日本語ボタンが押されたときの処理</summary>
    void OnJpButtonClicked()
    {
        GameData.Language = "JA";
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE("Cursor");
        UpdateLanguageButtonVisuals();
    }

    /// <summary>英語ボタンが押されたときの処理</summary>
    void OnEnButtonClicked()
    {
        GameData.Language = "EN";
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE("Cursor");
        UpdateLanguageButtonVisuals();
    }

    /// <summary>
    /// 現在の GameData.Language に合わせてボタン色とタイトル画像を更新する
    /// </summary>
    void UpdateLanguageButtonVisuals()
    {
        bool isJP = GameData.Language == "JA";

        // ボタンのテキスト色で選択状態を表現（スプライト不要）
        if (jpButton != null)
        {
            // GetComponentInChildren で子の TextMeshProUGUI を取得して色を変える
            var jpText = jpButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (jpText != null) jpText.color = isJP ? selectedColor : unselectedColor;
        }
        if (enButton != null)
        {
            var enText = enButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (enText != null) enText.color = isJP ? unselectedColor : selectedColor;
        }

        // タイトルロゴを言語に合わせて表示切替（位置が違うので別 GameObject）
        if (titleLogoJA != null) titleLogoJA.SetActive(isJP);
        if (titleLogoEN != null) titleLogoEN.SetActive(!isJP);
    }

    /// <summary>
    /// START ボタンが押されたときの処理
    /// コルーチンでフェードアウトしてからシーン遷移する
    /// </summary>
    void OnStartButtonClicked()
    {
        // 二重クリック防止のためボタンを無効化
        startButton.interactable = false;
        // ボタン押しSE
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE("Decide");

        // フェードアウト → シーン遷移のコルーチンを開始
        StartCoroutine(TransitionToNextScene());
    }

    // ==================== コルーチン ====================

    /// <summary>
    /// 画面を黒にフェードアウトしてから次のシーンへ移動する
    /// IEnumerator はコルーチン（時間をかけた処理）に使う型
    /// yield return で「ここで一時停止」できる
    /// </summary>
    IEnumerator TransitionToNextScene()
    {
        // TransitionFader でフェードアウト（黒くなる・DontDestroyOnLoad でシーン跨いで生き残る）
        // IntroScene がロードされたら自動で 1 秒かけてフェードインする
        yield return StartCoroutine(TransitionFader.FadeOut(1.0f));

        // 黒画面で 1 秒待機してから遷移
        yield return new WaitForSeconds(1.0f);

        // 次のシーンへ遷移（IntroScene）
        SceneManager.LoadScene(nextSceneName);
    }
}
