using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NPCに話しかけるとシナリオを開始するトリガー
/// NPC オブジェクトにアタッチして使用
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class NPCTrigger : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("Scenario Settings")]
    [Tooltip("話しかけた時に実行するシナリオCSV")]
    public TextAsset scenarioCSV;

    [Tooltip("シナリオ開始ラベル（空の場合は最初から）")]
    public string startLabel = "";

    [Header("Input Actions")]
    [Tooltip("InputSystem_Actions アセットをアサイン")]
    public InputActionAsset inputActions;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("話しかけられる時に表示するアイコン")]
    public GameObject interactIcon;

    // ==================== 内部変数 ====================

    // プレイヤーが範囲内にいるかどうか
    private bool playerInRange = false;

    // プレイヤーへの参照
    private PlayerController playerController;

    // シナリオ実行中かどうか
    private bool isInConversation = false;

    // Input Action の参照
    private InputAction interactAction;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // Collider2D を Trigger に設定
        var collider = GetComponent<Collider2D>();
        collider.isTrigger = true;

        // アイコンを非表示に
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        // Input Action の初期化
        InitializeInputActions();
    }

    void OnEnable()
    {
        interactAction?.Enable();
    }

    void OnDisable()
    {
        interactAction?.Disable();
    }

    void OnDestroy()
    {
        // コールバックを解除
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
    }

    // ==================== 初期化 ====================

    /// <summary>
    /// Input Action を初期化する
    /// </summary>
    void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError($"NPCTrigger ({gameObject.name}): inputActions が設定されていません！");
            return;
        }

        var playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError($"NPCTrigger ({gameObject.name}): Player アクションマップが見つかりません");
            return;
        }

        interactAction = playerActionMap.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
            interactAction.Enable();
        }
    }

    // ==================== トリガー検出 ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーかどうかチェック
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();

            // アイコンを表示
            if (interactIcon != null && !isInConversation)
            {
                interactIcon.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーが範囲外に出た
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerController = null;

            // アイコンを非表示
            if (interactIcon != null)
            {
                interactIcon.SetActive(false);
            }
        }
    }

    // ==================== インタラクト ====================

    /// <summary>
    /// Interact ボタンが押された時のコールバック
    /// </summary>
    void OnInteract(InputAction.CallbackContext context)
    {
        // プレイヤーが範囲内にいない場合は何もしない
        if (!playerInRange)
        {
            return;
        }

        // 既に会話中なら何もしない
        if (isInConversation)
        {
            return;
        }

        // シナリオが設定されていない場合は警告
        if (scenarioCSV == null)
        {
            Debug.LogWarning($"NPCTrigger ({gameObject.name}): scenarioCSV が設定されていません");
            return;
        }

        // 会話開始
        StartConversation();
    }

    /// <summary>
    /// 会話を開始する
    /// </summary>
    void StartConversation()
    {
        isInConversation = true;

        // プレイヤーの移動を禁止
        if (playerController != null)
        {
            playerController.CanMove = false;
        }

        // アイコンを非表示
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        // ScenarioExecutor を探してシナリオを開始
        var executor = FindAnyObjectByType<ScenarioExecutor>();
        if (executor != null)
        {
            Debug.Log($"NPCTrigger: シナリオ開始 - {scenarioCSV.name}");

            // シナリオを開始（終了時に EndConversation が呼ばれる）
            executor.StartScenario(scenarioCSV, startLabel, EndConversation);
        }
        else
        {
            Debug.LogError("NPCTrigger: ScenarioExecutor が見つかりません");
            EndConversation();
        }
    }

    /// <summary>
    /// 会話を終了する
    /// ScenarioExecutor から呼び出される想定
    /// </summary>
    public void EndConversation()
    {
        isInConversation = false;

        // プレイヤーの移動を許可
        if (playerController != null)
        {
            playerController.CanMove = true;
        }

        // プレイヤーがまだ範囲内にいればアイコンを表示
        if (interactIcon != null && playerInRange)
        {
            interactIcon.SetActive(true);
        }
    }
}
