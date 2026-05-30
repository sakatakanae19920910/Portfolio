using UnityEngine;

/// <summary>
/// 選択した主人公に応じてGameObjectを表示/非表示にするスクリプト
/// カイリ（ナギ編）やルイ（ミズキ編）など、主人公によって異なるキャラに付ける
/// </summary>
public class ShowIfCharacter : MonoBehaviour
{
    [Tooltip("表示する主人公名（Nagi または Mizuki）")]
    public string requiredCharacter;

    void Start()
    {
        // 選択キャラが一致しない場合はGameObjectごと非表示
        if (!string.IsNullOrEmpty(requiredCharacter) &&
            GameData.SelectedCharacter != requiredCharacter)
        {
            gameObject.SetActive(false);
        }
    }
}
