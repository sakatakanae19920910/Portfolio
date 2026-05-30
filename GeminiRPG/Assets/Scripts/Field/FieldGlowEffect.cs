using UnityEngine;
using DG.Tweening;

/// <summary>
/// フィールドオブジェクト（本棚・宝箱など）に付けるキラキラ演出コンポーネント。
/// DOTween による点滅と浮遊を組み合わせて「調べられるオブジェクト」を視覚的に示す。
/// URP 2D Renderer に依存しないため WebGL・全環境で動く。
///
/// ■ Unity セットアップ手順
///   1. 光らせたい GameObject（例: WeaponShopBookshelf）を Hierarchy で選択
///   2. Inspector で「Add Component」→「FieldGlowEffect」を追加
///   3. Inspector でパラメータを調整:
///      - Float Enabled  : 上下に浮かぶか（本棚は OFF 推奨）
///      - Glow Enabled   : 透明度を点滅させるか（ON 推奨）
///      - Glow Min Alpha : 点滅の最小透明度（0=完全透明 〜 1=変化なし）
///      - Glow Duration  : 1サイクルの秒数
/// </summary>
public class FieldGlowEffect : MonoBehaviour
{
    [Header("Float Animation（上下に浮かぶ）")]
    [Tooltip("ONにすると上下にゆらゆら浮く（本棚など固定オブジェクトはOFF推奨）")]
    public bool floatEnabled = false;

    [Tooltip("浮く高さ（ユニット）")]
    public float floatHeight = 0.1f;

    [Tooltip("上下1往復の秒数")]
    public float floatDuration = 1.2f;

    [Header("Glow Animation（点滅・光る）")]
    [Tooltip("ONにするとキラキラ点滅する")]
    public bool glowEnabled = true;

    [Tooltip("点滅の最小透明度（0=完全消灯 / 0.3=うっすら / 1.0=変化なし）")]
    [Range(0f, 1f)]
    public float glowMinAlpha = 0.4f;

    [Tooltip("点滅1サイクルの秒数（小さいほど速くチカチカする）")]
    public float glowDuration = 0.9f;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // 浮遊アニメーション
        if (floatEnabled)
        {
            // 現在の Y 座標を基準に上下に揺れる
            transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject); // GameObject が Destroy されたら自動停止
        }

        // 点滅アニメーション（SpriteRenderer の alpha を往復させる）
        if (glowEnabled)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOFade(glowMinAlpha, glowDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetLink(gameObject); // GameObject が Destroy されたら自動停止
            }
        }
    }
}
