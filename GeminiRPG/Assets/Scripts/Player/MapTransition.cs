using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// マップ遷移を管理するコンポーネント
/// プレイヤーが特定のエリアに入ると別のマップへ移動する
/// 
/// 使い方:
/// 1. 空のGameObjectを作成（ドア、階段、マップ端など）
/// 2. BoxCollider2D を追加し、Is Trigger をオン
/// 3. このスクリプトを追加
/// 4. 遷移先を設定
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MapTransition : MonoBehaviour
{
    // ==================== 遷移タイプ ====================

    /// <summary>
    /// 遷移の種類
    /// </summary>
    public enum TransitionType
    {
        /// <summary>同一シーン内でテレポート（高速、ロードなし）</summary>
        Teleport,
        
        /// <summary>別シーンへ遷移（Scenes in Buildに登録が必要）</summary>
        SceneChange
    }

    // ==================== Inspector設定 ====================

    [Header("Transition Settings")]
    [Tooltip("遷移の種類")]
    public TransitionType transitionType = TransitionType.Teleport;

    [Header("Teleport Settings（同一シーン内移動）")]
    [Tooltip("テレポート先の位置（Transformをドラッグ）")]
    public Transform teleportDestination;

    [Tooltip("テレポート後のプレイヤーの向き")]
    public Vector2 arrivalDirection = Vector2.down;

    [Header("Scene Change Settings（シーン遷移）")]
    [Tooltip("遷移先のシーン名")]
    public string targetSceneName;

    [Tooltip("遷移先シーンでのスポーン位置名（SpawnPointのGameObject名）")]
    public string spawnPointName = "SpawnPoint";

    [Header("Fade Settings")]
    [Tooltip("フェードを使用するか")]
    public bool useFade = true;

    [Tooltip("フェード時間（秒）")]
    [Range(0.1f, 2f)]
    public float fadeDuration = 0.3f;

    [Header("Audio Settings（オプション）")]
    [Tooltip("遷移時に再生するSE（ドアの音など）")]
    public AudioClip transitionSound;

    // ==================== 内部変数 ====================

    // 遷移中かどうか（連続トリガー防止）
    private bool isTransitioning = false;

    // プレイヤーコントローラーの参照
    private PlayerController playerController;

    // カメラ追従の参照
    private CameraFollow cameraFollow;

    // ==================== 静的変数（シーン間データ受け渡し用） ====================

    // 次のシーンでのスポーン位置名
    private static string nextSpawnPointName;

    // 次のシーンでのプレイヤーの向き
    private static Vector2 nextArrivalDirection;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // Collider が Trigger でない場合は警告
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"MapTransition ({gameObject.name}): Collider の Is Trigger がオフです。オンにしてください。");
        }

        // シーン開始時にスポーンポイントをチェック
        CheckSpawnPoint();
    }

    /// <summary>
    /// シーンロード後にプレイヤーをスポーンポイントに配置する
    /// </summary>
    void CheckSpawnPoint()
    {
        // スポーン位置名が設定されている場合
        if (!string.IsNullOrEmpty(nextSpawnPointName))
        {
            // このオブジェクトがスポーンポイントとして指定されているか確認
            if (gameObject.name == nextSpawnPointName)
            {
                // プレイヤーを検索
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                
                if (player != null)
                {
                    // プレイヤーをこの位置にテレポート
                    player.transform.position = transform.position;

                    // プレイヤーの向きを設定
                    PlayerController pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        pc.SetDirection(nextArrivalDirection);
                    }

                    // カメラを即座に移動
                    CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
                    if (cam != null)
                    {
                        cam.SetPositionImmediate(player.transform.position);
                    }

                    Debug.Log($"スポーンポイント '{nextSpawnPointName}' にプレイヤーを配置しました");
                }

                // 使用済みなのでクリア
                nextSpawnPointName = null;
            }
        }
    }

    // ==================== トリガー検出 ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤー以外は無視
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // 既に遷移中なら無視（連続トリガー防止）
        if (isTransitioning)
        {
            return;
        }

        // 遷移開始
        StartCoroutine(DoTransition(other.gameObject));
    }

    // ==================== 遷移処理 ====================

    /// <summary>
    /// 遷移を実行するコルーチン
    /// </summary>
    IEnumerator DoTransition(GameObject player)
    {
        // 遷移中フラグを立てる
        isTransitioning = true;

        // プレイヤーの参照を取得
        playerController = player.GetComponent<PlayerController>();
        cameraFollow = Camera.main?.GetComponent<CameraFollow>();

        // プレイヤーの移動を禁止
        if (playerController != null)
        {
            playerController.CanMove = false;
        }

        // 遷移SEを再生
        if (transitionSound != null)
        {
            AudioSource.PlayClipAtPoint(transitionSound, player.transform.position);
        }

        // フェードアウト
        if (useFade)
        {
            yield return StartCoroutine(FadeOut());
        }

        // 遷移タイプによって処理を分岐
        switch (transitionType)
        {
            case TransitionType.Teleport:
                yield return StartCoroutine(DoTeleport(player));
                break;

            case TransitionType.SceneChange:
                yield return StartCoroutine(DoSceneChange());
                break;
        }
    }

    /// <summary>
    /// 同一シーン内でのテレポート
    /// </summary>
    IEnumerator DoTeleport(GameObject player)
    {
        // テレポート先が設定されていない場合はエラー
        if (teleportDestination == null)
        {
            Debug.LogError($"MapTransition ({gameObject.name}): teleportDestination が設定されていません！");
            isTransitioning = false;
            if (playerController != null) playerController.CanMove = true;
            yield break;
        }

        // プレイヤーをテレポート
        player.transform.position = teleportDestination.position;

        // プレイヤーの向きを設定
        if (playerController != null)
        {
            playerController.SetDirection(arrivalDirection);
        }

        // カメラを即座に移動（ワープ感を出すため）
        if (cameraFollow != null)
        {
            cameraFollow.SetPositionImmediate(teleportDestination.position);
        }

        // 少し待機（画面の切り替わりを自然に）
        yield return new WaitForSeconds(0.1f);

        // フェードイン
        if (useFade)
        {
            yield return StartCoroutine(FadeIn());
        }

        // プレイヤーの移動を許可
        if (playerController != null)
        {
            playerController.CanMove = true;
        }

        // 遷移完了
        isTransitioning = false;
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    IEnumerator DoSceneChange()
    {
        // シーン名が設定されていない場合はエラー
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"MapTransition ({gameObject.name}): targetSceneName が設定されていません！");
            isTransitioning = false;
            if (playerController != null) playerController.CanMove = true;
            yield break;
        }

        // 次のシーンでのスポーン情報を保存
        nextSpawnPointName = spawnPointName;
        nextArrivalDirection = arrivalDirection;

        // シーンをロード
        Debug.Log($"シーン '{targetSceneName}' に遷移します（スポーン: {spawnPointName}）");
        SceneManager.LoadScene(targetSceneName);

        // シーン遷移後はこのオブジェクトは破棄されるので、ここで終了
    }

    // ==================== フェード処理 ====================

    /// <summary>
    /// 画面をフェードアウト（暗くする）
    /// </summary>
    IEnumerator FadeOut()
    {
        // FadeManagerのシングルトンを使用（効率的）
        FadeManager fadeManager = FadeManager.Instance;
        
        if (fadeManager != null)
        {
            // FadeManager のフェードアウトを呼び出し
            fadeManager.FadeOut(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
        else
        {
            // FadeManager がない場合は待機のみ
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    /// <summary>
    /// 画面をフェードイン（明るくする）
    /// </summary>
    IEnumerator FadeIn()
    {
        // FadeManagerのシングルトンを使用（効率的）
        FadeManager fadeManager = FadeManager.Instance;
        
        if (fadeManager != null)
        {
            // FadeManager のフェードインを呼び出し
            fadeManager.FadeIn(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }
        else
        {
            // FadeManager がない場合は待機のみ
            yield return new WaitForSeconds(fadeDuration);
        }
    }

    // ==================== デバッグ用 ====================

    void OnDrawGizmos()
    {
        // エディタ上で遷移ポイントを可視化

        // 遷移タイプによって色を変える
        switch (transitionType)
        {
            case TransitionType.Teleport:
                Gizmos.color = Color.cyan;    // テレポートは水色
                break;
            case TransitionType.SceneChange:
                Gizmos.color = Color.magenta; // シーン遷移はマゼンタ
                break;
        }

        // Collider の範囲を表示
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        // テレポート先への線を描画
        if (transitionType == TransitionType.Teleport && teleportDestination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, teleportDestination.position);
            Gizmos.DrawWireSphere(teleportDestination.position, 0.3f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // 選択時に詳細情報を表示

        // 到着時の向きを矢印で表示
        if (teleportDestination != null || !string.IsNullOrEmpty(targetSceneName))
        {
            Vector3 arrowStart = teleportDestination != null 
                ? teleportDestination.position 
                : transform.position;
            
            Gizmos.color = Color.green;
            Vector3 arrowEnd = arrowStart + (Vector3)arrivalDirection.normalized * 0.5f;
            Gizmos.DrawLine(arrowStart, arrowEnd);
        }
    }
}
