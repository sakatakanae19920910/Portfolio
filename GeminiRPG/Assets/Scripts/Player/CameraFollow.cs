using UnityEngine;

/// <summary>
/// カメラがプレイヤーを追従するコンポーネント
/// Main Camera にアタッチして使用する
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("Target Settings")]
    [Tooltip("追従するターゲット（通常はプレイヤー）")]
    public Transform target;

    [Tooltip("ターゲットが未設定の場合、Playerタグのオブジェクトを自動検索")]
    public bool autoFindPlayer = true;

    [Header("Follow Settings")]
    [Tooltip("カメラの追従速度（大きいほど素早く追従）")]
    [Range(1f, 20f)]
    public float smoothSpeed = 5f;

    [Tooltip("ターゲットからのオフセット（カメラ位置の調整用）")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Boundary Settings")]
    [Tooltip("カメラの移動範囲を制限するか")]
    public bool useBoundary = false;

    [Tooltip("カメラ移動範囲の最小座標")]
    public Vector2 boundaryMin = new Vector2(-10f, -10f);

    [Tooltip("カメラ移動範囲の最大座標")]
    public Vector2 boundaryMax = new Vector2(10f, 10f);

    [Header("Dead Zone")]
    [Tooltip("ターゲットがこの範囲内にいる間はカメラが動かない")]
    public bool useDeadZone = false;

    [Tooltip("デッドゾーンのサイズ（ワールド座標）")]
    public Vector2 deadZoneSize = new Vector2(1f, 1f);

    // ==================== 内部変数 ====================

    // 現在のカメラ目標位置
    private Vector3 targetPosition;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // ターゲットが未設定の場合、Playerタグを持つオブジェクトを検索
        if (target == null && autoFindPlayer)
        {
            // "Player" タグがついたゲームオブジェクトを検索
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // 見つかったらそのTransformをターゲットに設定
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraFollow: Playerを自動検出しました");
            }
            else
            {
                // 見つからない場合は警告
                Debug.LogWarning("CameraFollow: Playerタグを持つオブジェクトが見つかりません");
            }
        }

        // 初期位置をターゲット位置に設定（ゲーム開始時にカメラがジャンプしないように）
        if (target != null)
        {
            // ターゲット位置 + オフセット を計算
            Vector3 initialPosition = target.position + offset;
            
            // 境界制限を適用
            if (useBoundary)
            {
                initialPosition = ApplyBoundary(initialPosition);
            }
            
            // カメラを初期位置に設定
            transform.position = initialPosition;
        }
    }

    void LateUpdate()
    {
        // LateUpdate を使用する理由:
        // プレイヤーの移動（Update/FixedUpdate）が完了した後にカメラを動かすため
        // これにより、カメラのブレを防止できる

        // ターゲットがない場合は何もしない
        if (target == null)
        {
            return;
        }

        // ターゲットの位置を取得し、オフセットを適用
        targetPosition = target.position + offset;

        // デッドゾーンを使用する場合
        if (useDeadZone)
        {
            targetPosition = ApplyDeadZone(targetPosition);
        }

        // 境界制限を使用する場合
        if (useBoundary)
        {
            targetPosition = ApplyBoundary(targetPosition);
        }

        // スムーズにカメラを移動（Lerp = 線形補間）
        // Lerp(現在位置, 目標位置, 補間率) で徐々に目標に近づく
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,           // 現在のカメラ位置
            targetPosition,               // 目標位置
            smoothSpeed * Time.deltaTime  // 補間率（時間ベース）
        );

        // カメラ位置を更新
        transform.position = smoothedPosition;
    }

    // ==================== 境界制限 ====================

    /// <summary>
    /// カメラ位置に境界制限を適用する
    /// </summary>
    /// <param name="position">制限前の位置</param>
    /// <returns>制限後の位置</returns>
    Vector3 ApplyBoundary(Vector3 position)
    {
        // X座標を最小値と最大値の範囲内にクランプ（制限）
        position.x = Mathf.Clamp(position.x, boundaryMin.x, boundaryMax.x);
        
        // Y座標を最小値と最大値の範囲内にクランプ
        position.y = Mathf.Clamp(position.y, boundaryMin.y, boundaryMax.y);

        // Z座標はオフセットのまま（2Dゲームでは通常-10）
        return position;
    }

    // ==================== デッドゾーン ====================

    /// <summary>
    /// デッドゾーンを適用する
    /// ターゲットがデッドゾーン内にいる間はカメラが動かない
    /// </summary>
    /// <param name="targetPos">ターゲットの位置</param>
    /// <returns>調整後のカメラ目標位置</returns>
    Vector3 ApplyDeadZone(Vector3 targetPos)
    {
        // 現在のカメラ位置
        Vector3 currentPos = transform.position;

        // ターゲットとカメラの差分
        float deltaX = targetPos.x - currentPos.x;
        float deltaY = targetPos.y - currentPos.y;

        // デッドゾーンの半分のサイズ
        float halfWidth = deadZoneSize.x / 2f;
        float halfHeight = deadZoneSize.y / 2f;

        // X方向: デッドゾーン内なら現在位置を維持
        if (Mathf.Abs(deltaX) < halfWidth)
        {
            targetPos.x = currentPos.x;
        }
        else
        {
            // デッドゾーンを超えた分だけ移動
            targetPos.x = currentPos.x + (deltaX - Mathf.Sign(deltaX) * halfWidth);
        }

        // Y方向: デッドゾーン内なら現在位置を維持
        if (Mathf.Abs(deltaY) < halfHeight)
        {
            targetPos.y = currentPos.y;
        }
        else
        {
            // デッドゾーンを超えた分だけ移動
            targetPos.y = currentPos.y + (deltaY - Mathf.Sign(deltaY) * halfHeight);
        }

        return targetPos;
    }

    // ==================== 外部からの制御 ====================

    /// <summary>
    /// カメラを指定位置に瞬間移動させる
    /// マップ切り替え時などに使用
    /// </summary>
    /// <param name="position">移動先の位置</param>
    public void SetPositionImmediate(Vector3 position)
    {
        // Z座標はオフセットを維持
        position.z = offset.z;
        
        // 境界制限を適用
        if (useBoundary)
        {
            position = ApplyBoundary(position);
        }
        
        // 即座に位置を設定
        transform.position = position;
    }

    /// <summary>
    /// ターゲットを変更する
    /// </summary>
    /// <param name="newTarget">新しいターゲット</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// カメラの境界を設定する
    /// マップごとに異なる境界を使う場合に便利
    /// </summary>
    /// <param name="min">最小座標</param>
    /// <param name="max">最大座標</param>
    public void SetBoundary(Vector2 min, Vector2 max)
    {
        boundaryMin = min;
        boundaryMax = max;
        useBoundary = true;
    }

    /// <summary>
    /// カメラの境界を無効化する
    /// </summary>
    public void DisableBoundary()
    {
        useBoundary = false;
    }

    // ==================== デバッグ用 ====================

    void OnDrawGizmosSelected()
    {
        // エディタ上で境界を可視化（選択時のみ）

        // 境界を緑色の矩形で表示
        if (useBoundary)
        {
            Gizmos.color = Color.green;
            
            // 境界の中心と大きさを計算
            Vector3 center = new Vector3(
                (boundaryMin.x + boundaryMax.x) / 2f,
                (boundaryMin.y + boundaryMax.y) / 2f,
                0f
            );
            Vector3 size = new Vector3(
                boundaryMax.x - boundaryMin.x,
                boundaryMax.y - boundaryMin.y,
                0f
            );
            
            // ワイヤーフレームの矩形を描画
            Gizmos.DrawWireCube(center, size);
        }

        // デッドゾーンを黄色の矩形で表示
        if (useDeadZone)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneSize.x, deadZoneSize.y, 0f));
        }
    }
}
