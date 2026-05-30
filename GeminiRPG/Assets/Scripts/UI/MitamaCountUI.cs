using UnityEngine;
using TMPro;

/// <summary>
/// ミタマ石の現在所持数を画面隅に表示するHUD
///
/// ■ Unity セットアップ手順
///   1. 各マップシーン（TownScene / TraderScene / WeaponShopScene / TavernScene 等）の
///      Canvas 内に空のGameObjectを作成（例: MitamaCountUI）
///   2. TextMeshPro - Text (UI) を子に追加
///   3. RectTransform を画面右上などに配置
///      アンカー: Top-Right / PivotX: 1 / PivotY: 1
///      Anchor Pos X: -20 / Anchor Pos Y: -20
///   4. このスクリプトを MitamaCountUI GameObjectにアタッチ
///   5. Inspector の Count Text に TextMeshPro コンポーネントをドラッグ
///
/// ■ 推奨フォント設定
///   - Font: KleeOne-Regular SDF（他のUIと統一）
///   - Font Size: 24
///   - Alignment: Right
///   - Color: 白 (#FFFFFF) / Shadow あり推奨
/// </summary>
public class MitamaCountUI : MonoBehaviour
{
    // ==================== Inspector設定 ====================

    [Header("UI References")]
    [Tooltip("ミタマ石の数を表示する TextMeshPro コンポーネント")]
    public TextMeshProUGUI countText;

    [Header("Display Settings")]
    [Tooltip("表示フォーマット。{0} に現在の所持数が入る")]
    public string format = "ミタマ石 × {0}";

    [Tooltip("ミタマ石が0個の場合はUIを非表示にするか\ntrueにすると占い師に会うまで表示されない")]
    public bool hideWhenZero = false;

    // ==================== 内部変数 ====================

    // 前フレームの所持数（変化があった時だけテキストを更新するための比較値）
    private int lastCount = -1;

    // ==================== Unity ライフサイクル ====================

    void Start()
    {
        // 初回表示を強制更新
        lastCount = -1;
        Refresh();
    }

    void Update()
    {
        // 所持数が変化した時だけテキストを更新（毎フレーム string.Format を呼ばないための最適化）
        if (GameData.MitamaCount != lastCount)
        {
            Refresh();
        }
    }

    // ==================== 内部メソッド ====================

    /// <summary>
    /// テキストを最新の所持数で更新する
    /// </summary>
    void Refresh()
    {
        // countText が設定されていなければ何もしない
        if (countText == null) return;

        int count = GameData.MitamaCount;
        lastCount = count;

        // hideWhenZero が true で0個の場合はUIごと非表示
        if (hideWhenZero && count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        // hideWhenZero が true だったが増えた場合は再表示
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // テキストを更新（例: "ミタマ石 × 3"）
        countText.text = string.Format(format, count);
    }
}
