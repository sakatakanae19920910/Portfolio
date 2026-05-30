using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// フィールドに配置するミタマ石の拾得スクリプト
/// プレイヤーが近づいて Interact ボタンを押すと入手する（触れただけでは取得しない）
///
/// ■ Unity セットアップ手順
///   1. 空のGameObjectを作成（例: MitamaStone_01）
///   2. SpriteRenderer を追加してミタマ石のスプライトをセット
///   3. CircleCollider2D（または CapsuleCollider2D）を追加
///      ※ isTrigger は Start() で自動 ON になるので手動設定不要
///   4. このスクリプトをアタッチ
///   5. Inspector でパラメータを設定:
///      - Stone ID       : 同じシーン内で重複しない文字列（例: Town_01, Town_02）
///      - Pickup CSV     : Demo_MitamaStone_Pickup.csv をアサイン
///      - Input Actions  : InputSystem_Actions アセットをアサイン
///      - Interact Icon  : 頭上に表示するアイコン GameObject（任意）
///   6. シーンに必要なだけ複製して配置（各々 Stone ID を変えること）
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MitamaStoneItem : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("Stone Settings")]
    [Tooltip("このミタマ石を識別するユニークID（シーン内で重複禁止）\n例: Town_01 / Tavern_01 など")]
    public string stoneID = "Field_01";

    [Header("Scenario")]
    [Tooltip("拾得時に表示するシナリオCSV\nDemo_MitamaStone_Pickup.csv をアサイン")]
    public TextAsset pickupCSV;

    [Header("Facing Check")]
    [Tooltip("ONにすると、プレイヤーがこのオブジェクトの方を向いている時だけ取得できる")]
    public bool requireFacing = false;

    [Tooltip("requireFacing=true の時、どのくらい正面を向いていれば反応するかの閾値\n1.0=真正面のみ / 0.5=約60°以内 / 0.0=後ろ以外全部")]
    [Range(0f, 1f)]
    public float facingThreshold = 0.5f;

    [Header("Input Actions")]
    [Tooltip("InputSystem_Actions アセットをアサイン（NPCTrigger と同じアセット）")]
    public InputActionAsset inputActions;

    [Header("Visual Feedback (Optional)")]
    [Tooltip("プレイヤーが近づいた時に表示するアイコン（？マークなど）")]
    public GameObject interactIcon;


    // ==================== 内部変数 ====================

    // プレイヤーが範囲内にいるかどうか
    private bool playerInRange = false;

    // 既に拾得処理が走っているかどうか（二重実行防止）
    private bool isPickingUp = false;

    // 拾得直後のクールダウン（Interact ボタンの二重反応防止）
    private bool isCoolingDown = false;

    // プレイヤーへの参照（移動制御に使用）
    private PlayerController playerController;

    // InputSystem の Interact アクション
    private InputAction interactAction;

    // ==================== 内部プロパティ ====================

    /// <summary>
    /// GameData.PersistentFlags に保存するキー名
    /// "MitamaStone_" + stoneID の形式
    /// </summary>
    private string FlagKey => $"MitamaStone_{stoneID}";

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // Collider2D を必ずトリガーにする（Inspector 設定を上書き）
        GetComponent<Collider2D>().isTrigger = true;

        // 既に拾われていた場合（シーン再訪時など）はそのまま非表示にする
        if (GameData.PersistentFlags.TryGetValue(FlagKey, out bool alreadyPicked) && alreadyPicked)
        {
            gameObject.SetActive(false);
            return; // 非表示にしたらそれ以上初期化不要
        }

        // アイコンを初期状態では非表示にする
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        // Input Action を初期化する
        InitializeInputActions();
    }

    void OnEnable()
    {
        // 再アクティブ時にコールバックを再登録（InitializeInputActions後のみ有効）
        if (interactAction != null)
            interactAction.performed += OnInteract;
    }

    void OnDisable()
    {
        // コールバックだけ解除する（Disable()は呼ばない）
        // InputActionは全石で共有しているため、Disable()すると他の石のInteractも止まる
        if (interactAction != null)
            interactAction.performed -= OnInteract;
    }

    void OnDestroy()
    {
        // コールバックを解除（メモリリーク防止）
        if (interactAction != null)
            interactAction.performed -= OnInteract;
    }

    // ==================== 初期化 ====================

    /// <summary>
    /// InputSystem の Interact アクションを初期化してコールバックを登録する
    /// </summary>
    void InitializeInputActions()
    {
        // inputActions アセットが未設定の場合はエラーログを出して終了
        if (inputActions == null)
        {
            Debug.LogError($"MitamaStoneItem ({gameObject.name}): inputActions が設定されていません！");
            return;
        }

        // "Player" アクションマップを取得
        var playerActionMap = inputActions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError($"MitamaStoneItem ({gameObject.name}): Player アクションマップが見つかりません");
            return;
        }

        // "Interact" アクションを取得してコールバックを登録
        // ※ OnEnable() は Start() より前に呼ばれるため、この時点で初めて登録する
        interactAction = playerActionMap.FindAction("Interact");
        if (interactAction != null)
        {
            interactAction.performed += OnInteract;
            interactAction.Enable();
        }
    }

    // ==================== トリガー検出 ====================

    /// <summary>
    /// プレイヤーが範囲内に入った時：playerInRange を true にしてアイコンを表示する
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Player タグ以外は無視
        if (!other.CompareTag("Player")) return;

        // プレイヤーの参照を保存
        playerInRange = true;
        playerController = other.GetComponent<PlayerController>();

        // インタラクトアイコンを表示（拾得中でなければ）
        if (interactIcon != null && !isPickingUp)
        {
            interactIcon.SetActive(true);
        }
    }

    /// <summary>
    /// プレイヤーが範囲外に出た時：playerInRange を false にしてアイコンを非表示にする
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        // Player タグ以外は無視
        if (!other.CompareTag("Player")) return;

        // プレイヤーの参照をクリア
        playerInRange = false;
        playerController = null;

        // アイコンを非表示にする
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }
    }

    // ==================== インタラクト ====================

    /// <summary>
    /// Interact ボタンが押された時のコールバック
    /// プレイヤーが範囲内にいる場合のみ拾得処理を実行する
    /// </summary>
    void OnInteract(InputAction.CallbackContext context)
    {
        // プレイヤーが範囲内にいない場合は何もしない
        if (!playerInRange) return;

        // 既に拾得処理中なら何もしない（二重実行防止）
        if (isPickingUp) return;

        // 拾得直後のクールダウン中は無視
        if (isCoolingDown) return;

        // 向きチェック（requireFacing=true の場合のみ）
        if (requireFacing && !IsPlayerFacing()) return;

        // 拾得処理を実行する
        Pickup();
    }

    // ==================== 向きチェック ====================

    /// <summary>
    /// プレイヤーがこのオブジェクトの方向を向いているか判定する
    /// プレイヤーの FacingDirection と、プレイヤー→アイテムの方向の内積で判定
    /// </summary>
    bool IsPlayerFacing()
    {
        // playerController が取得できていない場合は通す（フォールバック）
        if (playerController == null) return true;

        // プレイヤーからこのアイテムへの方向（正規化）
        Vector2 toItem = ((Vector2)transform.position - (Vector2)playerController.transform.position).normalized;

        // プレイヤーが向いている方向との内積を計算
        // 内積が facingThreshold 以上 = プレイヤーがこちらを向いている
        float dot = Vector2.Dot(playerController.FacingDirection, toItem);

        return dot >= facingThreshold;
    }

    // ==================== 拾得処理 ====================

    /// <summary>
    /// ミタマ石を拾得する：カウント加算 → シナリオ表示 → 石を非表示にする
    /// </summary>
    void Pickup()
    {
        // 二重実行防止フラグを立てる
        isPickingUp = true;

        // アイコンを非表示にする
        if (interactIcon != null)
        {
            interactIcon.SetActive(false);
        }

        // 拾得フラグを立てる（シーンをまたいでも持続する）
        GameData.PersistentFlags[FlagKey] = true;

        // ミタマ石を1個加算してSEを鳴らす
        GameData.MitamaCount++;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySE("ItemGet");
        Debug.Log($"[MitamaStoneItem] 拾得完了 ID={stoneID} | 合計={GameData.MitamaCount}個");

        // ScenarioExecutor を取得して拾得メッセージを表示する
        var executor = FindAnyObjectByType<ScenarioExecutor>();

        if (executor != null && pickupCSV != null)
        {
            // メッセージ表示中はプレイヤーを止める
            if (playerController != null) playerController.CanMove = false;

            // シナリオ終了時のコールバックでプレイヤー移動を再開して石を消す
            executor.StartScenario(pickupCSV, "", () =>
            {
                // プレイヤーの移動を再開
                if (playerController != null) playerController.CanMove = true;

                // 石のGameObjectを非表示にする
                // （ScenarioExecutorのコルーチンは executor 側で動くので、石を消しても問題なし）
                gameObject.SetActive(false);
            });
        }
        else
        {
            // シナリオなしの場合はそのまま石を消す
            gameObject.SetActive(false);
        }
    }
}
