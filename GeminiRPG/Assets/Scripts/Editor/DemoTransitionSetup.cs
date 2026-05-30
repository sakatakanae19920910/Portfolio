using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// デモ版の全シーンにマップ遷移オブジェクトを一括作成するエディタツール。
/// Unity メニュー「GeminiRPG > 場所移動を一括セットアップ」から実行。
///
/// 各遷移につき 2 種類のオブジェクトを作る:
///   ExitToX    … 出口トリガー（MapTransition + BoxCollider2D）。ドアの端に置く。
///   SpawnFromX … スポーン地点（SpawnPoint のみ、コライダーなし）。ドアの内側・少し奥に置く。
/// 2 つを分けることで「到着直後に出口を再発火」する無限ループを防いでいる。
/// </summary>
public class DemoTransitionSetup
{
    // ==================== シーンパス ====================

    const string SCENE_PARTNER_HOUSE = "Assets/Scenes/PartnerHouse1FScene.unity";
    const string SCENE_TOWN          = "Assets/Scenes/TownScene.unity";
    const string SCENE_TRADER        = "Assets/Scenes/TraderScene.unity";
    const string SCENE_WEAPON_SHOP   = "Assets/Scenes/WeaponShopScene.unity";
    const string SCENE_TAVERN        = "Assets/Scenes/TavernScene.unity";

    // ==================== メニュー項目 ====================

    [MenuItem("GeminiRPG/場所移動を一括セットアップ")]
    static void SetupAllTransitions()
    {
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            bool save = EditorUtility.DisplayDialog(
                "未保存の変更があります",
                "セットアップ前に現在のシーンを保存しますか？",
                "保存して続ける", "キャンセル");
            if (!save) return;
            EditorSceneManager.SaveOpenScenes();
        }

        // ==================== PartnerHouse1FScene ====================
        // 出口: ドア1（→TownScene） / ドア2（→TownScene、別スポーン）
        // スポーン: TownSceneから来たとき ×2
        SetupScene(SCENE_PARTNER_HOUSE,
            exits: new[]
            {
                new ExitConfig("ExitToTown",  "TownScene", "SpawnFromPartnerHouse", Vector2.down),
                new ExitConfig("ExitToTown2", "TownScene", "SpawnFromPartnerHouse2", Vector2.down),
            },
            spawns: new[]
            {
                "SpawnFromTown",  // ドア1から来たとき
                "SpawnFromTown2", // ドア2から来たとき
            }
        );

        // ==================== TownScene ====================
        // 出口: 各施設ドア前 ×4（+ パートナーの家ドア2）
        // スポーン: 各施設から戻ってきたとき
        SetupScene(SCENE_TOWN,
            exits: new[]
            {
                new ExitConfig("ExitToPartnerHouse",  "PartnerHouse1FScene", "SpawnFromTown",  Vector2.up),
                new ExitConfig("ExitToPartnerHouse2", "PartnerHouse1FScene", "SpawnFromTown2", Vector2.up),
                new ExitConfig("ExitToTrader",        "TraderScene",         "SpawnFromTown",  Vector2.up),
                new ExitConfig("ExitToWeaponShop",    "WeaponShopScene",     "SpawnFromTown",  Vector2.up),
                new ExitConfig("ExitToTavern",        "TavernScene",         "SpawnFromTown",  Vector2.up),
            },
            spawns: new[]
            {
                "SpawnFromPartnerHouse",  // パートナーの家ドア1から来たとき
                "SpawnFromPartnerHouse2", // パートナーの家ドア2から来たとき
                "SpawnFromTrader",        // 交換屋から来たとき
                "SpawnFromWeaponShop",    // 武器屋から来たとき
                "SpawnFromTavern",        // 酒場から来たとき
            }
        );

        // ==================== TraderScene ====================
        SetupScene(SCENE_TRADER,
            exits:  new[] { new ExitConfig("ExitToTown", "TownScene", "SpawnFromTrader", Vector2.down) },
            spawns: new[] { "SpawnFromTown" }
        );

        // ==================== WeaponShopScene ====================
        SetupScene(SCENE_WEAPON_SHOP,
            exits:  new[] { new ExitConfig("ExitToTown", "TownScene", "SpawnFromWeaponShop", Vector2.down) },
            spawns: new[] { "SpawnFromTown" }
        );

        // ==================== TavernScene ====================
        SetupScene(SCENE_TAVERN,
            exits:  new[] { new ExitConfig("ExitToTown", "TownScene", "SpawnFromTavern", Vector2.down) },
            spawns: new[] { "SpawnFromTown" }
        );

        // 元のシーンに戻る
        if (!string.IsNullOrEmpty(originalScenePath))
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog(
            "場所移動セットアップ完了！",
            "5シーンにオブジェクトを作成しました。\n\n" +
            "【各シーンでやること】\n" +
            "  ExitToX   → ドアの端（踏んだら遷移）に移動\n" +
            "  SpawnFromX → ドアの内側・少し奥（到着位置）に移動\n\n" +
            "詳細はコンソールを確認してください。",
            "OK");
    }

    // ==================== ヘルパー ====================

    /// <summary>
    /// 指定シーンを開き、出口トリガーとスポーン地点を作成して保存する。
    /// 既に同名オブジェクトが存在する場合は一度削除してから作り直す（古い構成の上書きに対応）。
    /// </summary>
    static void SetupScene(string scenePath, ExitConfig[] exits, string[] spawns)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 既存オブジェクトを全部削除してから作り直す（古いバージョンのゴミを確実に消す）
        foreach (var cfg in exits)
            DestroyIfExists(cfg.Name, scene.name);
        foreach (var spawnName in spawns)
            DestroyIfExists(spawnName, scene.name);

        // --- 出口トリガー（MapTransition + BoxCollider2D）---
        foreach (var cfg in exits)
        {
            // 削除済みなので常に新規作成

            GameObject go = new GameObject(cfg.Name);

            // BoxCollider2D: isTrigger=ON でプレイヤーが踏んだとき発火
            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 1f); // 幅2タイル・高さ1タイル（ドア幅に合わせて調整）

            // MapTransition: 別シーンへの遷移設定
            MapTransition mt = go.AddComponent<MapTransition>();
            mt.transitionType   = MapTransition.TransitionType.SceneChange;
            mt.targetSceneName  = cfg.TargetScene;
            mt.spawnPointName   = cfg.SpawnPointName;
            mt.arrivalDirection = cfg.ArrivalDir;
            mt.useFade          = true;
            mt.fadeDuration     = 0.3f;

            Undo.RegisterCreatedObjectUndo(go, $"Create {cfg.Name}");
            Debug.Log($"[出口作成] {scene.name}: '{cfg.Name}' → {cfg.TargetScene} (spawn: {cfg.SpawnPointName})");
        }

        // --- スポーン地点（SpawnPoint のみ、コライダーなし）---
        foreach (var spawnName in spawns)
        {
            // 削除済みなので常に新規作成
            GameObject go = new GameObject(spawnName);
            go.AddComponent<SpawnPoint>();

            Undo.RegisterCreatedObjectUndo(go, $"Create {spawnName}");
            Debug.Log($"[スポーン作成] {scene.name}: '{spawnName}'");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[保存] {scene.name}");
    }

    // ==================== データ構造 ====================

    /// <summary>
    /// 同名オブジェクトが存在すれば削除する
    /// </summary>
    static void DestroyIfExists(string name, string sceneName)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Debug.Log($"[削除] {sceneName}: 既存の '{name}' を削除して作り直します");
            Undo.DestroyObjectImmediate(existing);
        }
    }

    /// <summary>出口トリガー1つ分の設定</summary>
    struct ExitConfig
    {
        public string  Name;           // GameObjectの名前（例: "ExitToTown"）
        public string  TargetScene;    // 遷移先シーン名
        public string  SpawnPointName; // 遷移先のスポーン地点名（SpawnPoint のオブジェクト名と一致させる）
        public Vector2 ArrivalDir;     // 到着時のプレイヤーの向き

        public ExitConfig(string name, string targetScene, string spawnPointName, Vector2 arrivalDir)
        {
            Name           = name;
            TargetScene    = targetScene;
            SpawnPointName = spawnPointName;
            ArrivalDir     = arrivalDir;
        }
    }
}
