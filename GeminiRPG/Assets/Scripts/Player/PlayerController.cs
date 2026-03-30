using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの移動を制御するコンポーネント
/// Input System を使用して WASD/矢印キー/ゲームパッドで4方向移動
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("Movement Settings")]
    [Tooltip("移動速度（ユニット/秒）")]
    public float moveSpeed = 5f;

    [Header("Input Actions")]
    [Tooltip("InputSystem_Actions アセットをアサイン")]
    public InputActionAsset inputActions;

    [Header("Animation (Optional)")]
    [Tooltip("Animator コンポーネント（なくても動作可）")]
    public Animator animator;

    // ==================== 内部変数 ====================

    // Rigidbody2D コンポーネント（物理演算用）
    private Rigidbody2D rb;

    // Input Action の参照
    private InputAction moveAction;      // 移動入力（Vector2）
    private InputAction interactAction;  // インタラクト入力（Button）

    // 現在の移動入力値
    private Vector2 moveInput;

    // 移動可能かどうかのフラグ（会話中などは false）
    private bool canMove = true;

    // 最後に向いていた方向（アニメーション用）
    private Vector2 lastDirection = Vector2.down;

    // ==================== プロパティ ====================

    /// <summary>
    /// 外部から移動可能状態を制御するためのプロパティ
    /// 会話中やイベント中は false にする
    /// </summary>
    public bool CanMove
    {
        get => canMove;
        set
        {
            canMove = value;
            // 移動不可になったら速度をゼロに
            if (!canMove && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // ==================== Unity ライフサイクル ====================

    void Awake()
    {
        // Rigidbody2D を取得
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D の設定（念のため）
        rb.gravityScale = 0f;           // 2D トップダウンなので重力なし
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // 回転を固定
    }

    void Start()
    {
        // Input Action の初期化
        InitializeInputActions();
    }

    void OnEnable()
    {
        // アクションを有効化
        moveAction?.Enable();
        interactAction?.Enable();
    }

    void OnDisable()
    {
        // アクションを無効化（メモリリーク防止）
        moveAction?.Disable();
        interactAction?.Disable();
    }

    void Update()
    {
        // 移動入力を取得
        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        // アニメーション更新
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // 物理演算での移動処理
        Move();
    }

    // ==================== 初期化 ====================

    /// <summary>
    /// Input Action を初期化する
    /// </summary>
    void InitializeInputActions()
    {
        // InputActionAsset が設定されていない場合はエラー
        if (inputActions == null)
        {
            Debug.LogError("PlayerController: inputActions が設定されていません！");
            return;
        }

        // Player アクションマップを取得
        var playerActionMap = inputActions.FindActionMap("Player");

        if (playerActionMap == null)
        {
            Debug.LogError("PlayerController: Player アクションマップが見つかりません");
            return;
        }

        // Move アクションを取得（WASD, 矢印キー, 左スティック）
        moveAction = playerActionMap.FindAction("Move");

        // Interact アクションを取得（決定ボタン）
        interactAction = playerActionMap.FindAction("Interact");

        // Interact のコールバックを設定
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
        }

        // アクションを有効化
        moveAction?.Enable();
        interactAction?.Enable();
    }

    // ==================== 移動処理 ====================

    /// <summary>
    /// FixedUpdate で呼ばれる移動処理
    /// </summary>
    void Move()
    {
        // 移動不可の場合は何もしない
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 入力がなければ停止
        if (moveInput.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 4方向に正規化（斜め移動を防ぐ場合はコメントアウトを外す）
        // moveInput = NormalizeToFourDirections(moveInput);

        // 移動方向を記録（アニメーション用）
        lastDirection = moveInput.normalized;

        // Rigidbody2D で移動（物理演算を使うことで壁との衝突が自然に処理される）
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    /// <summary>
    /// 入力を4方向に正規化する（オプション）
    /// 斜め移動を禁止したい場合に使用
    /// </summary>
    Vector2 NormalizeToFourDirections(Vector2 input)
    {
        // 入力がほぼゼロなら何もしない
        if (input.sqrMagnitude < 0.01f)
        {
            return Vector2.zero;
        }

        // 水平と垂直、どちらが大きいかで方向を決定
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            // 水平方向（左右）
            return new Vector2(Mathf.Sign(input.x), 0);
        }
        else
        {
            // 垂直方向（上下）
            return new Vector2(0, Mathf.Sign(input.y));
        }
    }

    // ==================== インタラクト ====================

    /// <summary>
    /// Interact ボタンが押された時のコールバック
    /// </summary>
    void OnInteract(InputAction.CallbackContext context)
    {
        // 移動不可の場合（会話中など）はインタラクトも無効
        if (!canMove)
        {
            return;
        }

        // 目の前にある NPC やオブジェクトを検出
        // （NPCTrigger 側で処理するので、ここでは何もしない）
        // 必要ならここで Raycast などを追加
    }

    // ==================== アニメーション ====================

    /// <summary>
    /// アニメーションパラメータを更新
    /// </summary>
    void UpdateAnimation()
    {
        // Animator がない場合は何もしない
        if (animator == null)
        {
            return;
        }

        // 移動中かどうか
        bool isMoving = moveInput.sqrMagnitude > 0.01f && canMove;

        // Animator のパラメータを設定
        // ※ Animator Controller 側で以下のパラメータを用意する必要がある
        //   - MoveX (Float): 移動方向 X
        //   - MoveY (Float): 移動方向 Y
        //   - IsMoving (Bool): 移動中かどうか

        animator.SetFloat("MoveX", lastDirection.x);
        animator.SetFloat("MoveY", lastDirection.y);
        animator.SetBool("IsMoving", isMoving);
    }

    // ==================== 外部からの制御 ====================

    /// <summary>
    /// プレイヤーを指定位置に瞬間移動させる
    /// マップ切り替え時などに使用
    /// </summary>
    public void Teleport(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// プレイヤーの向きを変える
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        lastDirection = direction.normalized;
        UpdateAnimation();
    }
}
