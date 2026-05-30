using UnityEngine;
using System.Collections;

/// <summary>
/// スプライトアニメーションを「一回再生 → 待機 → 一回再生」でループさせるコンポーネント。
/// Animator の Loop Time を OFF にした上でこのスクリプトを同じ GameObject にアタッチする。
///
/// ■ セットアップ手順
///   1. .anim ファイルを選択 → Inspector の Loop Time を OFF にする
///   2. このスクリプトを MitamaSparkle GameObject にアタッチ
///   3. Inspector で Wait Seconds を調整（デフォルト: 5秒）
/// </summary>
[RequireComponent(typeof(Animator))]
public class SparkleLoop : MonoBehaviour
{
    [Tooltip("キラッと光った後、次の再生まで待つ秒数")]
    public float waitSeconds = 5f;

    // Animator・SpriteRenderer コンポーネントへの参照
    private Animator anim;
    private SpriteRenderer sr;

    // アニメーションクリップの長さ（秒）
    private float clipLength = 0.5f;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        // アニメーションクリップの長さを自動取得する
        var clips = anim.runtimeAnimatorController?.animationClips;
        if (clips != null && clips.Length > 0)
            clipLength = clips[0].length;

        // ループコルーチンを開始する
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            // 再生前に表示する
            SetVisible(true);

            // アニメーションを最初から再生する
            anim.Play(0, 0, 0f);

            // クリップが終わるまで待つ
            yield return new WaitForSeconds(clipLength);

            // 待機中は透明にする
            SetVisible(false);

            // インターバル（次のキラッまでの待機）
            yield return new WaitForSeconds(waitSeconds);
        }
    }

    // SpriteRenderer の表示／非表示を切り替える
    void SetVisible(bool visible)
    {
        if (sr != null) sr.enabled = visible;
    }
}
