using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 全シーンの NPC コライダーを一括更新するエディタツール。
/// Unity メニュー「GeminiRPG > NPC コライダーを一括更新」から実行。
///
/// やること:
///   - NPCTrigger がついている全オブジェクトの CapsuleCollider2D を探す
///   - size / offset を NPCTrigger の設定値に合わせて上書き
///   - なければ新規追加
/// </summary>
public class NPCColliderSetup
{
    static readonly string[] TARGET_SCENES = new[]
    {
        "Assets/Scenes/TownScene.unity",
        "Assets/Scenes/PartnerHouse1FScene.unity",
        "Assets/Scenes/TraderScene.unity",
        "Assets/Scenes/WeaponShopScene.unity",
        "Assets/Scenes/TavernScene.unity",
    };

    [MenuItem("GeminiRPG/NPCコライダーを一括更新")]
    static void UpdateAllNPCColliders()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            bool save = EditorUtility.DisplayDialog("未保存の変更", "続ける前に保存しますか？", "保存", "キャンセル");
            if (!save) return;
            EditorSceneManager.SaveOpenScenes();
        }

        int total = 0;

        foreach (var scenePath in TARGET_SCENES)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int count = UpdateSceneNPCs();
            total += count;
            Debug.Log($"[{scene.name}] {count} 体の NPC コライダーを更新しました");
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalScenePath))
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog("完了", $"合計 {total} 体の NPC コライダーを更新しました。", "OK");
    }

    /// <summary>
    /// 現在開いているシーンの NPCTrigger 全件を処理して件数を返す
    /// </summary>
    static int UpdateSceneNPCs()
    {
        int count = 0;
        foreach (var trigger in Object.FindObjectsByType<NPCTrigger>(FindObjectsSortMode.None))
        {
            // 既存の non-trigger CapsuleCollider2D を探す
            CapsuleCollider2D body = null;
            foreach (var col in trigger.GetComponents<CapsuleCollider2D>())
            {
                if (!col.isTrigger) { body = col; break; }
            }

            // なければ追加
            if (body == null)
            {
                body = trigger.gameObject.AddComponent<CapsuleCollider2D>();
                Undo.RegisterCreatedObjectUndo(body, "Add NPC Body Collider");
            }

            // サイズ・オフセットを NPCTrigger の設定値に合わせて更新
            Undo.RecordObject(body, "Update NPC Body Collider");
            body.isTrigger  = false;
            body.size       = trigger.bodyColliderSize;
            body.offset     = trigger.bodyColliderOffset;
            body.direction  = CapsuleDirection2D.Vertical;

            EditorUtility.SetDirty(trigger.gameObject);
            count++;
        }
        return count;
    }
}
