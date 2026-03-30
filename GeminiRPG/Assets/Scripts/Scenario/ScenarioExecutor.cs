using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// シナリオコマンドを1つずつ実行するエンジン
/// 新フォーマット（5列方式）対応版
/// </summary>
public class ScenarioExecutor : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("UI References")]
    public TextMeshProUGUI mainText;           // メインテキスト表示
    public CanvasGroup mainTextCanvasGroup;    // テキストのフェード用
    public GameObject choiceButtonPrefab;      // 選択肢ボタンのプレハブ
    public Transform canvasTransform;          // ボタン配置用のCanvas
    public Image fadeImage;                    // 画面フェード用のImage

    [Header("Speaker UI")]
    public TextMeshProUGUI speakerText;        // 話者名を表示するテキスト
    public GameObject speakerPanel;            // 話者名パネル（非表示切替用）

    [Header("Character Display")]
    public Image characterImage;               // 立ち絵（右固定）

    [Header("Background")]
    public Image backgroundImage;              // 背景画像

    [Header("Scenario Data")]
    public TextAsset scenarioCSV;              // シナリオCSVファイル

    [Header("Input Actions")]
    public InputActionAsset inputActions;      // InputSystem_Actions アセット

    // ==================== 内部変数 ====================

    // Input Action の参照
    private InputAction submitAction;          // 決定ボタン（Enter, A ボタン等）
    private InputAction clickAction;           // マウスクリック

    // シナリオ実行状態
    private List<ScenarioCommand> commands;    // 読み込んだコマンドリスト
    private int currentIndex = 0;              // 現在実行中のコマンドインデックス
    private bool isWaiting = false;            // 選択肢待ち状態
    private bool isWaitingForClick = false;    // クリック待ち状態
    private bool isRunning = false;            // シナリオ実行中かどうか
    private Coroutine currentScenarioCoroutine; // 現在実行中のコルーチン

    // シナリオ終了時のコールバック
    private System.Action onScenarioEnd;

    // ゲーム状態
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();  // フラグ管理
    private List<GameObject> activeChoiceButtons = new List<GameObject>();    // 表示中の選択肢ボタン

    // キャラクター表示状態
    private string displayedCharacter = null;  // 現在表示中のキャラクター名
    private string currentFaceID = null;       // 現在の表情ID

    // サウンド
    private AudioSource currentBGM;            // 現在再生中のBGM

    // ==================== プロパティ ====================

    /// <summary>
    /// シナリオ実行中かどうか
    /// </summary>
    public bool IsRunning => isRunning;

    // ==================== 話者名→キャラクター名マッピング ====================

    /// <summary>
    /// 日本語の話者名を英語のキャラクター名に変換する辞書
    /// 画像ファイル名は英語なので変換が必要
    /// </summary>
    private static readonly Dictionary<string, string> speakerToCharacterName = new Dictionary<string, string>
    {
        { "ナギ", "Nagi" },
        { "カイリ", "Kairi" },
        { "ミズキ", "Mizuki" },
        { "ルイ", "Rui" },
        { "ペチ", "Peti" },
        { "カグヤ", "Kaguya" },
        { "ハヤテ", "Hayate" },
    };

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // fadeImage の初期化
        if (fadeImage != null)
        {
            fadeImage.color = new Color(1, 1, 1, 0);
            fadeImage.raycastTarget = false;
            fadeImage.transform.SetAsFirstSibling();
        }
        else
        {
            Debug.LogError("ScenarioExecutor: fadeImage が設定されていません！");
        }

        // Input Action の初期化
        InitializeInputActions();

        // Inspector で scenarioCSV が設定されていれば自動実行
        if (scenarioCSV != null)
        {
            StartScenario(scenarioCSV);
        }
    }

    // ==================== 外部からのシナリオ開始 ====================

    /// <summary>
    /// 外部からシナリオを開始する
    /// NPCTrigger などから呼び出す
    /// </summary>
    /// <param name="csv">実行するシナリオCSV</param>
    /// <param name="startLabel">開始ラベル（空の場合は最初から）</param>
    /// <param name="onEnd">シナリオ終了時のコールバック</param>
    public void StartScenario(TextAsset csv, string startLabel = "", System.Action onEnd = null)
    {
        // 既に実行中なら停止
        if (isRunning && currentScenarioCoroutine != null)
        {
            StopCoroutine(currentScenarioCoroutine);
            CleanupScenario();
        }

        // CSVが null なら何もしない
        if (csv == null)
        {
            Debug.LogError("ScenarioExecutor: StartScenario に null が渡されました");
            return;
        }

        // コールバックを保存
        onScenarioEnd = onEnd;

        // CSVを読み込む
        commands = ScenarioLoader.LoadFromCSV(csv);

        // 開始位置を決定
        currentIndex = 0;
        if (!string.IsNullOrEmpty(startLabel))
        {
            int labelIndex = ScenarioLoader.FindLabelIndex(commands, startLabel);
            if (labelIndex >= 0)
            {
                currentIndex = labelIndex;
            }
        }

        // シナリオ実行開始
        isRunning = true;
        currentScenarioCoroutine = StartCoroutine(ExecuteScenario());
    }

    /// <summary>
    /// シナリオを停止する
    /// </summary>
    public void StopScenario()
    {
        if (currentScenarioCoroutine != null)
        {
            StopCoroutine(currentScenarioCoroutine);
        }
        CleanupScenario();
    }

    /// <summary>
    /// シナリオ終了時のクリーンアップ
    /// </summary>
    private void CleanupScenario()
    {
        isRunning = false;
        isWaiting = false;
        isWaitingForClick = false;
        currentScenarioCoroutine = null;

        // 選択肢ボタンを削除
        foreach (GameObject btn in activeChoiceButtons)
        {
            Destroy(btn);
        }
        activeChoiceButtons.Clear();

        // テキストをクリア
        if (mainText != null)
        {
            mainText.text = "";
        }

        // 話者パネルを非表示
        if (speakerPanel != null)
        {
            speakerPanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Input Action を無効化（メモリリーク防止）
        submitAction?.Disable();
        clickAction?.Disable();
    }

    // ==================== Input System 初期化 ====================

    /// <summary>
    /// Input Action を初期化する
    /// </summary>
    void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("ScenarioExecutor: inputActions が設定されていません！");
            return;
        }

        // UI アクションマップを取得
        var uiActionMap = inputActions.FindActionMap("UI");
        if (uiActionMap == null)
        {
            Debug.LogError("ScenarioExecutor: UI アクションマップが見つかりません");
            return;
        }

        // Submit と Click アクションを取得して有効化
        submitAction = uiActionMap.FindAction("Submit");
        clickAction = uiActionMap.FindAction("Click");
        submitAction?.Enable();
        clickAction?.Enable();
    }

    // ==================== メインループ ====================

    /// <summary>
    /// シナリオを最初から最後まで実行するメインループ
    /// </summary>
    IEnumerator ExecuteScenario()
    {
        while (currentIndex < commands.Count)
        {
            ScenarioCommand cmd = commands[currentIndex];
            Debug.Log($"★ 実行: currentIndex={currentIndex}, Label='{cmd.Label}', Command='{cmd.Command}'");

            // ★修正ポイント: コマンドが空の場合は次へ進む（yield break ではなく continue）
            if (string.IsNullOrEmpty(cmd.Command))
            {
                currentIndex++;
                continue;
            }

            yield return ExecuteCommand(cmd);
            currentIndex++;
        }

        Debug.Log("ScenarioExecutor: シナリオ終了");

        // シナリオ終了処理
        OnScenarioComplete();
    }

    /// <summary>
    /// シナリオ完了時の処理
    /// </summary>
    private void OnScenarioComplete()
    {
        // 状態をリセット
        isRunning = false;
        currentScenarioCoroutine = null;

        // コールバックを呼び出し
        onScenarioEnd?.Invoke();
        onScenarioEnd = null;
    }

    /// <summary>
    /// 1つのコマンドを実行する
    /// </summary>
    IEnumerator ExecuteCommand(ScenarioCommand cmd)
    {
        switch (cmd.Command)
        {
            // テキスト系
            case "TextFade":
                yield return HandleTextFade(cmd);
                break;
            case "Text":
                yield return HandleText(cmd);
                break;
            case "Monologue":
                yield return HandleMonologue(cmd);
                break;
            case "MonologueFade":
                yield return HandleMonologueFade(cmd);
                break;
            case "TextWait":
                yield return HandleTextWait(cmd);
                break;

            // 選択肢
            case "Choice":
                yield return HandleChoice(cmd);
                break;
            case "Stop":
                yield return HandleStop(cmd);
                break;

            // 待機
            case "Wait":
                yield return HandleWait(cmd);
                break;

            // フロー制御
            case "Jump":
                HandleJump(cmd);
                break;
            case "SetFlag":
                HandleSetFlag(cmd);
                break;
            case "Branch":
                HandleBranch(cmd);
                break;

            // 画面効果
            case "FadeOut":
                yield return HandleFadeOut(cmd);
                break;
            case "FadeIn":
                yield return HandleFadeIn(cmd);
                break;

            // サウンド
            case "BGM":
                yield return HandleBGM(cmd);
                break;
            case "SE":
                yield return HandleSE(cmd);
                break;

            // キャラクター
            case "CharHide":
                yield return HandleCharHide(cmd);
                break;

            // 背景
            case "BGImage":
                yield return HandleBGImage(cmd);
                break;

            // 終了
            case "End":
                HandleEnd();
                break;

            default:
                Debug.LogWarning($"ScenarioExecutor: 未実装のコマンド: '{cmd.Command}'");
                break;
        }
    }

    // ==================== テキスト系コマンド ====================

    /// <summary>
    /// TextFade: フェードイン → 表示 → フェードアウト
    /// Args: time=秒数
    /// </summary>
    IEnumerator HandleTextFade(ScenarioCommand cmd)
    {
        // 表示時間を取得（デフォルト2秒）
        float displayTime = cmd.GetFloat("time", 2.0f);

        // 改行記号を変換（CSV内では ¥n と書く）
        string text = cmd.Text.Replace("¥n", "\n");

        mainText.text = text;

        // フェードイン（0.5秒）
        if (mainTextCanvasGroup != null)
        {
            yield return FadeText(0f, 1f, 0.5f);
        }

        // 表示時間待機
        yield return new WaitForSeconds(displayTime);

        // フェードアウト（0.5秒）
        if (mainTextCanvasGroup != null)
        {
            yield return FadeText(1f, 0f, 0.5f);
        }

        mainText.text = "";
    }

    /// <summary>
    /// Text: テキストを表示してクリック待ち
    /// Args: face=表情ID
    /// </summary>
    IEnumerator HandleText(ScenarioCommand cmd)
    {
        // 改行記号を変換
        string text = cmd.Text.Replace("¥n", "\n");

        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        // 話者が指定されている場合、立ち絵を表示/更新
        if (!string.IsNullOrEmpty(cmd.Speaker))
        {
            string faceID = cmd.GetString("face", "normal");
            yield return ShowOrUpdateSpeakerCharacter(cmd.Speaker, faceID);
        }

        // 話者名を表示
        ShowSpeaker(cmd.Speaker);

        // テキストを設定
        mainText.text = text;
        if (mainTextCanvasGroup != null)
        {
            mainTextCanvasGroup.alpha = 1f;
        }

        // クリック待ち
        yield return WaitForClick();
    }

    /// <summary>
    /// Monologue: 心の声（斜体で表示）
    /// Args: face=表情ID
    /// </summary>
    IEnumerator HandleMonologue(ScenarioCommand cmd)
    {
        string text = cmd.Text.Replace("¥n", "\n");

        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        // 話者が指定されている場合、立ち絵を表示/更新
        if (!string.IsNullOrEmpty(cmd.Speaker))
        {
            string faceID = cmd.GetString("face", "normal");
            yield return ShowOrUpdateSpeakerCharacter(cmd.Speaker, faceID);
        }

        // 心の声は基本的に話者名を非表示（指定があれば表示）
        ShowSpeaker(cmd.Speaker);

        // 斜体で表示
        mainText.text = $"<i>{text}</i>";
        if (mainTextCanvasGroup != null)
        {
            mainTextCanvasGroup.alpha = 1f;
        }

        yield return WaitForClick();
    }

    /// <summary>
    /// MonologueFade: 心の声（フェードあり）
    /// Args: time=秒数
    /// </summary>
    IEnumerator HandleMonologueFade(ScenarioCommand cmd)
    {
        float displayTime = cmd.GetFloat("time", 2.0f);
        string text = cmd.Text.Replace("¥n", "\n");

        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        mainText.text = $"<i>{text}</i>";

        if (mainTextCanvasGroup != null)
        {
            yield return FadeText(0f, 1f, 0.5f);
        }

        yield return new WaitForSeconds(displayTime);

        if (mainTextCanvasGroup != null)
        {
            yield return FadeText(1f, 0f, 0.5f);
        }

        mainText.text = "";
    }

    /// <summary>
    /// TextWait: テキストをパッと表示して指定秒数後に消える（フェードなし）
    /// Args: time=秒数
    /// </summary>
    IEnumerator HandleTextWait(ScenarioCommand cmd)
    {
        // 表示時間を取得（デフォルト2秒）
        float displayTime = cmd.GetFloat("time", 2.0f);

        // 改行記号を変換
        string text = cmd.Text.Replace("¥n", "\n");

        // テキストをパッと表示
        mainText.text = text;
        if (mainTextCanvasGroup != null)
        {
            mainTextCanvasGroup.alpha = 1f;
        }

        // 指定秒数待機
        yield return new WaitForSeconds(displayTime);

        // テキストをパッと消す
        mainText.text = "";
    }

    /// <summary>
    /// テキストのフェード処理
    /// </summary>
    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        mainTextCanvasGroup.alpha = startAlpha;
        yield return mainTextCanvasGroup.DOFade(endAlpha, duration)
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();
    }

    // ==================== 選択肢系コマンド ====================

    /// <summary>
    /// Choice: 選択肢ボタンを生成
    /// Args: jumpTo=ジャンプ先ラベル
    /// </summary>
    IEnumerator HandleChoice(ScenarioCommand cmd)
    {
        string buttonText = cmd.Text;
        string jumpLabel = cmd.GetString("jumpTo");

        GameObject btn = Instantiate(choiceButtonPrefab, canvasTransform);
        btn.SetActive(true);
        btn.transform.SetAsLastSibling();
        btn.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

        activeChoiceButtons.Add(btn);

        Button buttonComponent = btn.GetComponent<Button>();
        buttonComponent.interactable = true;
        buttonComponent.onClick.AddListener(() => OnChoiceSelected(jumpLabel));

        yield return null;
    }

    /// <summary>
    /// 選択肢がクリックされた時の処理
    /// </summary>
    void OnChoiceSelected(string jumpLabel)
    {
        Debug.Log($"★ OnChoiceSelected: jumpLabel='{jumpLabel}'");

        // すべてのボタンを削除
        foreach (GameObject btn in activeChoiceButtons)
        {
            Destroy(btn);
        }
        activeChoiceButtons.Clear();

        // 指定ラベルへジャンプ
        int labelIndex = ScenarioLoader.FindLabelIndex(commands, jumpLabel);
        if (labelIndex >= 0)
        {
            currentIndex = labelIndex - 1;
        }

        isWaiting = false;
    }

    /// <summary>
    /// Stop: 選択肢が押されるまで待機
    /// </summary>
    IEnumerator HandleStop(ScenarioCommand cmd)
    {
        mainText.text = "";
        if (mainTextCanvasGroup != null)
        {
            mainTextCanvasGroup.alpha = 1f;
        }

        LayoutChoiceButtons();

        isWaiting = true;
        while (isWaiting)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 選択肢ボタンを中央に配置
    /// </summary>
    void LayoutChoiceButtons()
    {
        int count = activeChoiceButtons.Count;
        if (count == 0) return;

        // 最大テキスト幅を計算
        float maxTextWidth = 0f;
        foreach (GameObject btn in activeChoiceButtons)
        {
            TextMeshProUGUI textComponent = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.ForceMeshUpdate();
                float textWidth = textComponent.preferredWidth;
                if (textWidth > maxTextWidth)
                {
                    maxTextWidth = textWidth;
                }
            }
        }

        float padding = 40f;
        float buttonWidth = maxTextWidth + padding;
        float buttonHeight = 60f;
        float spacing = 10f;

        float totalHeight = (count * buttonHeight) + ((count - 1) * spacing);
        float startY = totalHeight / 2f - buttonHeight / 2f;

        for (int i = 0; i < count; i++)
        {
            RectTransform rect = activeChoiceButtons[i].GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            float yPos = startY - (i * (buttonHeight + spacing));
            rect.anchoredPosition = new Vector2(0, yPos);
            rect.localScale = Vector3.one;
        }
    }

    // ==================== 待機コマンド ====================

    /// <summary>
    /// Wait: 指定秒数待機
    /// Args: time=秒数
    /// </summary>
    IEnumerator HandleWait(ScenarioCommand cmd)
    {
        float seconds = cmd.GetFloat("time", 1.0f);
        yield return new WaitForSeconds(seconds);
    }

    // ==================== フロー制御コマンド ====================

    /// <summary>
    /// Jump: 指定ラベルへジャンプ
    /// Args: jumpTo=ラベル名
    /// </summary>
    void HandleJump(ScenarioCommand cmd)
    {
        string labelName = cmd.GetString("jumpTo");
        Debug.Log($"★ HandleJump: jumpTo='{labelName}'");

        int labelIndex = ScenarioLoader.FindLabelIndex(commands, labelName);
        if (labelIndex >= 0)
        {
            currentIndex = labelIndex - 1;
        }
    }

    /// <summary>
    /// SetFlag: フラグをONにする
    /// Args: flag=フラグ名
    /// </summary>
    void HandleSetFlag(ScenarioCommand cmd)
    {
        string flagName = cmd.GetString("flag");
        if (!string.IsNullOrEmpty(flagName))
        {
            flags[flagName] = true;
            Debug.Log($"★ SetFlag: '{flagName}' = true");
        }
    }

    /// <summary>
    /// Branch: フラグがtrueなら指定ラベルへジャンプ（JumpIf方式）
    /// Args: flag=フラグ名; jumpTo=ジャンプ先ラベル
    /// ★修正: 旧方式（次の行をスキップ）から JumpIf 方式に変更
    /// </summary>
    void HandleBranch(ScenarioCommand cmd)
    {
        string flagName = cmd.GetString("flag");
        string jumpTo = cmd.GetString("jumpTo");

        Debug.Log($"★ Branch: flag='{flagName}', jumpTo='{jumpTo}'");

        // フラグが存在し、true の場合
        if (!string.IsNullOrEmpty(flagName) && flags.ContainsKey(flagName) && flags[flagName])
        {
            // jumpTo が指定されていればジャンプ
            if (!string.IsNullOrEmpty(jumpTo))
            {
                int labelIndex = ScenarioLoader.FindLabelIndex(commands, jumpTo);
                if (labelIndex >= 0)
                {
                    currentIndex = labelIndex - 1;
                    Debug.Log($"★ Branch: '{flagName}' is true → ジャンプ先: '{jumpTo}'");
                }
            }
        }
        // フラグがfalseまたは存在しない場合は何もしない（次の行へ進む）
    }

    // ==================== 画面効果コマンド ====================

    /// <summary>
    /// FadeOut: 画面を白転/暗転
    /// Args: time=秒数; color=white/black
    /// </summary>
    IEnumerator HandleFadeOut(ScenarioCommand cmd)
    {
        if (fadeImage == null)
        {
            Debug.LogError("ScenarioExecutor: fadeImage が null です！");
            yield break;
        }

        float duration = cmd.GetFloat("time", 2.0f);
        string colorName = cmd.GetString("color", "white").ToLower();

        // 色を設定
        Color targetColor = colorName == "black" ? Color.black : Color.white;

        fadeImage.transform.SetAsLastSibling();

        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        fadeRect.anchoredPosition = Vector2.zero;

        fadeImage.raycastTarget = true;
        fadeImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0);

        yield return fadeImage.DOFade(1f, duration)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();
    }

    /// <summary>
    /// FadeIn: 暗転/白転から復帰
    /// Args: time=秒数
    /// </summary>
    IEnumerator HandleFadeIn(ScenarioCommand cmd)
    {
        if (fadeImage == null)
        {
            Debug.LogError("ScenarioExecutor: fadeImage が null です！");
            yield break;
        }

        float duration = cmd.GetFloat("time", 2.0f);

        yield return fadeImage.DOFade(0f, duration)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        fadeImage.raycastTarget = false;
    }

    // ==================== サウンドコマンド ====================

    /// <summary>
    /// BGM: BGMを再生・停止
    /// Args: name=BGM名; action=Play/Stop; vol=音量
    /// </summary>
    IEnumerator HandleBGM(ScenarioCommand cmd)
    {
        string action = cmd.GetString("action", "Play");
        string bgmName = cmd.GetString("name");

        if (SoundManager.Instance == null)
        {
            Debug.LogError("ScenarioExecutor: SoundManager が見つかりません！");
            yield break;
        }

        if (action == "Play")
        {
            // 現在のBGMがあればフェードアウト
            if (currentBGM != null && currentBGM.isPlaying)
            {
                yield return currentBGM.DOFade(0f, 1.0f).WaitForCompletion();
                SoundManager.Instance.StopBGM();
            }

            // 新しいBGMを再生
            currentBGM = SoundManager.Instance.PlayBGM(bgmName);
            if (currentBGM != null)
            {
                currentBGM.volume = 0f;
                yield return currentBGM.DOFade(
                    SoundManager.Instance.masterVolume * SoundManager.Instance.bgmVolume,
                    1.0f
                ).WaitForCompletion();
            }
        }
        else if (action == "Stop")
        {
            if (currentBGM != null && currentBGM.isPlaying)
            {
                yield return currentBGM.DOFade(0f, 1.0f).WaitForCompletion();
                SoundManager.Instance.StopBGM();
                currentBGM = null;
            }
        }
    }

    /// <summary>
    /// SE: 効果音を再生
    /// Args: name=SE名
    /// </summary>
    IEnumerator HandleSE(ScenarioCommand cmd)
    {
        string seName = cmd.GetString("name");

        if (SoundManager.Instance == null)
        {
            Debug.LogError("ScenarioExecutor: SoundManager が見つかりません！");
            yield break;
        }

        SoundManager.Instance.PlaySE(seName);
        yield return null;
    }

    // ==================== キャラクター表示コマンド ====================

    /// <summary>
    /// CharHide: 立ち絵を非表示にする
    /// </summary>
    IEnumerator HandleCharHide(ScenarioCommand cmd)
    {
        if (displayedCharacter == null)
        {
            Debug.LogWarning("ScenarioExecutor: CharHide: 表示中のキャラクターがいません");
            yield break;
        }

        if (characterImage == null)
        {
            Debug.LogError("ScenarioExecutor: CharHide: characterImage が設定されていません");
            yield break;
        }

        yield return characterImage.DOFade(0f, 0.3f).SetEase(Ease.InQuad).WaitForCompletion();

        characterImage.gameObject.SetActive(false);
        displayedCharacter = null;
        currentFaceID = null;

        // テキストと話者名もクリア
        if (mainText != null)
        {
            mainText.text = "";
        }
        ShowSpeaker(null);  // 話者パネルを非表示
    }

    /// <summary>
    /// キャラクター立ち絵を読み込む
    /// パス: Resources/Characters/{キャラ名}/{キャラ名}_{表情ID}
    /// </summary>
    Sprite LoadCharacterSprite(string characterName, string faceID)
    {
        string path = $"Characters/{characterName}/{characterName}_{faceID}";
        Sprite sprite = Resources.Load<Sprite>(path);

        // 見つからなければデフォルト画像を試す
        if (sprite == null)
        {
            path = $"Characters/{characterName}/{characterName}";
            sprite = Resources.Load<Sprite>(path);
        }

        return sprite;
    }

    /// <summary>
    /// 話者の立ち絵を表示または更新する
    /// </summary>
    IEnumerator ShowOrUpdateSpeakerCharacter(string speakerName, string faceID)
    {
        if (string.IsNullOrEmpty(speakerName))
        {
            yield break;
        }

        if (characterImage == null)
        {
            Debug.LogError("ScenarioExecutor: characterImage が設定されていません");
            yield break;
        }

        // 話者名からキャラクター名を取得
        string characterName;
        if (!speakerToCharacterName.TryGetValue(speakerName, out characterName))
        {
            characterName = speakerName;
        }

        string actualFaceID = string.IsNullOrEmpty(faceID) ? "normal" : faceID;

        // ケース1: キャラクターがまだ表示されていない → フェードインで表示
        if (displayedCharacter == null)
        {
            Sprite characterSprite = LoadCharacterSprite(characterName, actualFaceID);
            if (characterSprite == null)
            {
                Debug.LogWarning($"ScenarioExecutor: 立ち絵が見つかりません: {characterName}_{actualFaceID}");
                yield break;
            }

            characterImage.sprite = characterSprite;
            characterImage.preserveAspect = true;
            characterImage.color = new Color(1, 1, 1, 0);
            characterImage.gameObject.SetActive(true);

            yield return characterImage.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();

            displayedCharacter = characterName;
            currentFaceID = actualFaceID;
        }
        // ケース2: 別のキャラクターが表示中 → クロスフェードで切り替え
        else if (displayedCharacter != characterName)
        {
            Sprite newSprite = LoadCharacterSprite(characterName, actualFaceID);
            if (newSprite == null)
            {
                Debug.LogWarning($"ScenarioExecutor: 立ち絵が見つかりません: {characterName}_{actualFaceID}");
                yield break;
            }

            yield return characterImage.DOFade(0f, 0.15f).SetEase(Ease.InQuad).WaitForCompletion();
            characterImage.sprite = newSprite;
            characterImage.preserveAspect = true;
            yield return characterImage.DOFade(1f, 0.15f).SetEase(Ease.OutQuad).WaitForCompletion();

            displayedCharacter = characterName;
            currentFaceID = actualFaceID;
        }
        // ケース3: 同じキャラクターで表情だけ変更
        else if (currentFaceID != actualFaceID)
        {
            Sprite newSprite = LoadCharacterSprite(characterName, actualFaceID);
            if (newSprite == null)
            {
                Debug.LogWarning($"ScenarioExecutor: 立ち絵が見つかりません: {characterName}_{actualFaceID}");
                yield break;
            }

            characterImage.sprite = newSprite;
            currentFaceID = actualFaceID;
        }
        // ケース4: 同じキャラクター、同じ表情 → 何もしない
    }

    // ==================== 背景画像コマンド ====================

    /// <summary>
    /// BGImage: 背景画像を変更
    /// Args: name=画像名
    /// </summary>
    IEnumerator HandleBGImage(ScenarioCommand cmd)
    {
        string imageName = cmd.GetString("name");

        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogError("ScenarioExecutor: BGImage: name が指定されていません");
            yield break;
        }

        if (backgroundImage == null)
        {
            Debug.LogError("ScenarioExecutor: backgroundImage が設定されていません");
            yield break;
        }

        string path = $"Backgrounds/{imageName}";
        Sprite bgSprite = Resources.Load<Sprite>(path);

        if (bgSprite == null)
        {
            Debug.LogWarning($"ScenarioExecutor: BGImage: 背景画像が見つかりません: {path}");
            yield break;
        }

        float fadeDuration = 0.5f;

        // 現在背景があればフェードアウト
        if (backgroundImage.sprite != null)
        {
            yield return backgroundImage.DOFade(0f, fadeDuration / 2).WaitForCompletion();
        }

        backgroundImage.sprite = bgSprite;
        backgroundImage.color = new Color(1, 1, 1, 0);

        yield return backgroundImage.DOFade(1f, fadeDuration / 2).WaitForCompletion();
    }

    // ==================== 終了コマンド ====================

    /// <summary>
    /// End: シナリオ終了
    /// </summary>
    void HandleEnd()
    {
        currentIndex = commands.Count;
    }

    // ==================== ヘルパーメソッド ====================

    /// <summary>
    /// クリックまたは決定ボタンが押されるまで待機
    /// </summary>
    IEnumerator WaitForClick()
    {
        isWaitingForClick = true;

        // 最初のフレームはスキップ（前のクリックを検知しないため）
        yield return null;

        while (isWaitingForClick)
        {
            bool submitPressed = submitAction != null && submitAction.WasPressedThisFrame();
            bool clickPressed = clickAction != null && clickAction.WasPressedThisFrame();

            if (submitPressed || clickPressed)
            {
                isWaitingForClick = false;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 話者名を表示
    /// </summary>
    void ShowSpeaker(string speakerName)
    {
        if (speakerText == null) return;

        if (string.IsNullOrEmpty(speakerName))
        {
            if (speakerPanel != null) speakerPanel.SetActive(false);
            speakerText.text = "";
        }
        else
        {
            if (speakerPanel != null) speakerPanel.SetActive(true);
            speakerText.text = speakerName;
        }
    }
}
