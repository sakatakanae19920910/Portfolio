using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ドアの開閉を管理するスクリプト
/// ドアのGameObjectにアタッチして使用する
/// プレイヤーが近づいてInteractボタンを押すと開閉が切り替わる
/// </summary>
[RequireComponent(typeof(Collider2D))]         // 衝突判定用コライダー必須
[RequireComponent(typeof(SpriteRenderer))]     // スプライト表示用必須
public class DoorController : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("スプライト")]
    [Tooltip("閉じているときのスプライト")]
    public Sprite closedSprite;

    [Tooltip("開いているときのスプライト")]
    public Sprite openSprite;

    [Header("設定")]
    [Tooltip("最初から開いている状態にするか")]
    public bool startOpen = false;

    [Tooltip("InputSystem_Actions アセットをアサイン")]
    public InputActionAsset inputActions;

    // ==================== 内部変数 ====================

    // 現在開いているかどうか
    private bool isOpen;

    // プレイヤーが近くにいるかどうか
    private bool playerInRange = false;

    // コンポーネントへの参照
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    // インタラクト用InputAction
    private InputAction interactAction;


    // ==================== Unity ライフサイクル ====================

    void Awake()
    {
        // コンポーネントを取得
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        // Triggerになっているか確認
        if (!doorCollider.isTrigger)
            Debug.LogWarning($"DoorController ({gameObject.name}): ColliderのIs TriggerがOFFです！ONにしてください");

        // Input Action の初期化
        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                interactAction = playerMap.FindAction("Interact");
                if (interactAction != null)
                {
                    interactAction.performed += OnInteract;
                    interactAction.Enable();
                }
                else
                    Debug.LogError($"DoorController ({gameObject.name}): 'Interact' アクションが見つかりません");
            }
            else
                Debug.LogError($"DoorController ({gameObject.name}): 'Player' アクションマップが見つかりません");
        }
        else
            Debug.LogError($"DoorController ({gameObject.name}): inputActions が未設定です！");

        // 初期状態を設定
        isOpen = startOpen;
        ApplyState();
    }

    void OnDestroy()
    {
        // コールバックを解除（メモリリーク防止）
        if (interactAction != null)
            interactAction.performed -= OnInteract;
    }

    // ==================== トリガー検出 ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    // ==================== インタラクト ====================

    /// <summary>
    /// Interactボタンが押されたときのコールバック
    /// </summary>
    void OnInteract(InputAction.CallbackContext context)
    {
        // プレイヤーが近くにいない場合は無視
        if (!playerInRange)
            return;

        // 開閉を切り替える
        isOpen = !isOpen;
        ApplyState();
    }

    // ==================== 外部から呼び出せる公開メソッド ====================

    /// <summary>
    /// ドアを開ける。MapTransition などの外部スクリプトから呼ぶ用。
    /// </summary>
    public void OpenDoor()
    {
        isOpen = true;
        ApplyState();
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE("DoorOpen");
    }

    /// <summary>
    /// ドアを閉める。外部スクリプトから呼ぶ用。
    /// </summary>
    public void CloseDoor()
    {
        isOpen = false;
        ApplyState();
    }

    // ==================== 状態の適用 ====================

    /// <summary>
    /// isOpen の状態をスプライトとコライダーに反映する
    /// </summary>
    void ApplyState()
    {
        // スプライトを切り替える
        if (isOpen)
        {
            // 開いている → 開いたスプライト・コライダー無効（通れる）
            if (openSprite != null)
                spriteRenderer.sprite = openSprite;
            doorCollider.enabled = false;
        }
        else
        {
            // 閉じている → 閉じたスプライト・コライダー有効（通れない）
            if (closedSprite != null)
                spriteRenderer.sprite = closedSprite;
            doorCollider.enabled = true;
        }
    }
}
