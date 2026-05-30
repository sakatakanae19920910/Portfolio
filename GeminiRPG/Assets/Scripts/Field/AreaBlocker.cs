using UnityEngine;

/// <summary>
/// 進行禁止エリア（階段・町外出口など）の入口に置くトリガー
/// プレイヤーが踏み込むと主人公のモノローグを表示してから一歩押し返す
///
/// ■ Unity セットアップ手順
///   1. 空のGameObjectを作成（例: Blocker_Stairs / Blocker_TownExit）
///   2. BoxCollider2D を追加して Is Trigger をオンにする
///      コライダーは入口の「敷居」の幅に合わせて横長に設定する
///   3. このスクリプトをアタッチ
///   4. Inspector で設定:
///      - Block CSV       : Demo_AreaBlock.csv をアサイン
///      - Pushback Distance : 押し返す距離（デフォルト 1.5 ユニット）
/// </summary>
[RequireComponent(typeof(Collider2D))]   // BoxCollider2D または CapsuleCollider2D が必要
public class AreaBlocker : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("Scenario")]
    [Tooltip("進入時に表示するシナリオCSV\nDemo_AreaBlock.csv をアサイン")]
    public TextAsset blockCSV;

    [Header("Pushback")]
    [Tooltip("セリフ終了後にプレイヤーを押し返す距離（ユニット）\n" +
             "1タイルは96px÷48PPU=2ユニット。デフォルト1.5でほぼ1歩分")]
    public float pushbackDistance = 1.5f;

    // ==================== 内部変数 ====================

    // ブロック処理の二重発火防止フラグ
    // true のとき「セリフ表示中 or 押し返し済み」でトリガーを無視する
    private bool isBlocking = false;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // Is Trigger がオフだと OnTriggerEnter2D が発火しないので警告
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"AreaBlocker ({gameObject.name}): " +
                             "BoxCollider2D の Is Trigger がオフです。オンにしてください。");
        }
    }

    // ==================== トリガー検出 ====================

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player タグ以外（NPCなど）は完全無視
        if (!other.CompareTag("Player")) return;

        // 既にブロック処理が走っている場合は何もしない（二重発火防止）
        if (isBlocking) return;

        // 他のシナリオ（NPC会話・朝のイベントなど）が実行中なら割り込まない
        var executor = FindAnyObjectByType<ScenarioExecutor>();
        if (executor != null && executor.IsRunning) return;

        // PlayerController が取得できなければ処理しない
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // ▼ 侵入した向きを記録する（ラムダで使うためローカル変数に保持）
        // FacingDirection は「最後に向いていた方向」なので、進んできた向きと一致する
        Vector2 enterDir = pc.FacingDirection;

        // ブロック処理を開始する
        isBlocking = true;

        if (executor != null && blockCSV != null)
        {
            // StartScenario を呼ぶと内部で CanMove=false になり、プレイヤーが止まる
            // 第3引数のコールバック（ラムダ）はシナリオの End コマンド後に呼ばれる
            executor.StartScenario(blockCSV, "", () =>
            {
                // ▼ セリフ終了後の処理
                // 侵入した向きの逆方向を目標地点にして、歩く速度で自動歩行させる
                // SetAutoWalkTarget は canMove=true 不要で動き、到着後に onComplete を呼ぶ
                Vector2 pushbackTarget = (Vector2)pc.transform.position
                                        - enterDir.normalized * pushbackDistance;
                pc.SetAutoWalkTarget(pushbackTarget, () =>
                {
                    // 目標地点に着いたら次の侵入に備えてフラグをリセット
                    isBlocking = false;
                });
            });
        }
        else
        {
            // CSV が未設定の場合はセリフなしで直接自動歩行させる（フォールバック）
            Vector2 pushbackTarget = (Vector2)pc.transform.position
                                    - enterDir.normalized * pushbackDistance;
            pc.SetAutoWalkTarget(pushbackTarget, () => isBlocking = false);
        }
    }
}
