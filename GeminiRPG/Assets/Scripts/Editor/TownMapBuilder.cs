#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 町タイルマップ設計図CSVを読み込み、Tilemapにタイルを自動配置するエディタ拡張。
/// メニュー: GeminiRPG > Build Town Map
///
/// 【使い方】
/// 1. GeminiRPG > Create Tile Mapping でマッピングアセットを作成
/// 2. TileMappingアセットのInspectorで各セルタイプにタイルを設定
/// 3. 町マップのシーンを開く（なければ新規作成）
/// 4. GeminiRPG > Build Town Map を実行
/// </summary>
public class TownMapBuilder : EditorWindow
{
    // === 定数 ===
    private const string CSV_PATH = "Assets/Resources/MapData/town_map.csv"; // CSVファイルのパス
    private const int MAP_WIDTH = 30;                                        // マップの横幅（タイル数）
    private const int MAP_HEIGHT = 35;                                       // マップの縦幅（タイル数）

    // レイヤー名の定数（TileMappingのlayerフィールドと一致させる）
    private static readonly string[] LAYER_NAMES = new string[]
    {
        "Ground",    // 0: 地面・草
        "Path",      // 1: 道・広場
        "Buildings", // 2: 建物の壁
        "Objects",   // 3: 木・岩・ベンチ等の装飾
        "Rooftop",   // 4: 屋根（将来的に透過演出用）
        "Collision"  // 5: 当たり判定（非表示）
    };

    // レイヤーごとのソートオーダー（描画順を決める）
    private static readonly int[] LAYER_SORT_ORDER = new int[]
    {
        0,   // Ground（一番下）
        1,   // Path
        2,   // Buildings
        3,   // Objects
        4,   // Rooftop（一番上）
        0    // Collision（非表示なので順番は関係ない）
    };

    // === Inspector設定フィールド ===
    private TileMapping tileMapping;       // タイルマッピングアセット
    private TileBase collisionTile;        // Collision用のタイル（何でもOK、非表示にする）
    private TileBase defaultGroundTile;    // ground/plazaの下地タイル（全面に敷く）
    private bool clearExisting = true;     // 既存タイルをクリアするか

    // ===================================================================
    // メニュー: マッピングアセット作成
    // ===================================================================
    [MenuItem("GeminiRPG/Create Tile Mapping")]
    public static void CreateTileMapping()
    {
        // 保存先フォルダの確認・作成
        if (!AssetDatabase.IsValidFolder("Assets/Resources/MapData"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "MapData");
        }

        // 新しいTileMappingアセットを作成
        string path = "Assets/Resources/MapData/TileMapping.asset";

        // 既存チェック
        TileMapping existing = AssetDatabase.LoadAssetAtPath<TileMapping>(path);
        if (existing != null)
        {
            // 既にある場合はそれをInspectorで開く
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log("TileMappingアセットは既に存在します。Inspectorで編集してください。");
            return;
        }

        // 新規作成
        TileMapping mapping = ScriptableObject.CreateInstance<TileMapping>();
        AssetDatabase.CreateAsset(mapping, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 作成したアセットをInspectorで開く
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mapping;
        EditorGUIUtility.PingObject(mapping);

        Debug.Log($"TileMappingアセットを作成しました: {path}");
        EditorUtility.DisplayDialog(
            "TileMapping作成完了",
            "TileMappingアセットを作成しました！\n\nInspectorで各セルタイプに使いたいタイルを設定してください。\n\n設定後、GeminiRPG > Build Town Map で配置を実行できます。",
            "OK"
        );
    }

    // ===================================================================
    // メニュー: ビルドウィンドウを開く
    // ===================================================================
    [MenuItem("GeminiRPG/Build Town Map")]
    public static void ShowWindow()
    {
        // エディタウィンドウを表示
        TownMapBuilder window = GetWindow<TownMapBuilder>("Town Map Builder");
        window.minSize = new Vector2(400, 300); // 最小サイズを設定
    }

    // ===================================================================
    // ウィンドウのUI描画
    // ===================================================================
    private void OnGUI()
    {
        GUILayout.Label("町タイルマップ自動配置", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // CSVファイル情報の表示
        EditorGUILayout.HelpBox(
            $"CSV: {CSV_PATH}\nサイズ: {MAP_WIDTH} × {MAP_HEIGHT} タイル",
            MessageType.Info
        );
        EditorGUILayout.Space();

        // タイルマッピング設定（ドラッグ&ドロップで設定可能）
        tileMapping = (TileMapping)EditorGUILayout.ObjectField(
            "Tile Mapping",         // ラベル
            tileMapping,            // 現在の値
            typeof(TileMapping),    // 型
            false                   // シーン内オブジェクトを許可しない
        );

        // 下地タイル設定
        defaultGroundTile = (TileBase)EditorGUILayout.ObjectField(
            "下地タイル（全面）",
            defaultGroundTile,
            typeof(TileBase),
            false
        );

        // コリジョン用タイル設定
        collisionTile = (TileBase)EditorGUILayout.ObjectField(
            "Collision用タイル",
            collisionTile,
            typeof(TileBase),
            false
        );

        EditorGUILayout.Space();

        // 既存タイルクリアのオプション
        clearExisting = EditorGUILayout.Toggle("既存タイルをクリア", clearExisting);

        EditorGUILayout.Space();

        // タイル設定状況のサマリー表示
        if (tileMapping != null)
        {
            int configured = 0;     // タイルが設定済みのエントリ数
            int total = 0;          // 全エントリ数

            foreach (var entry in tileMapping.entries)
            {
                total++;
                if (entry.tile != null) configured++;
            }

            // 設定状況に応じて色を変える
            MessageType msgType = configured == total ? MessageType.Info : MessageType.Warning;
            EditorGUILayout.HelpBox(
                $"タイル設定状況: {configured} / {total} セルタイプ設定済み",
                msgType
            );
        }

        EditorGUILayout.Space();

        // ビルド実行ボタン
        GUI.enabled = tileMapping != null; // マッピングが未設定なら無効化
        if (GUILayout.Button("マップを生成！", GUILayout.Height(40)))
        {
            BuildMap();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // 個別レイヤーのみ再生成ボタン
        EditorGUILayout.LabelField("個別レイヤー再生成", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (string layerName in LAYER_NAMES)
        {
            if (GUILayout.Button(layerName))
            {
                BuildSingleLayer(layerName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    // ===================================================================
    // マップ生成のメイン処理
    // ===================================================================
    private void BuildMap()
    {
        // --- 1. CSVを読み込む ---
        string[][] mapData = LoadCSV();
        if (mapData == null) return;

        // --- 2. シーン内にGrid + Tilemapの階層構造を作成or取得 ---
        GameObject gridObj = FindOrCreateGrid();
        Dictionary<string, Tilemap> tilemaps = FindOrCreateTilemaps(gridObj);

        // --- 3. 既存タイルのクリア（オプション） ---
        if (clearExisting)
        {
            foreach (var tilemap in tilemaps.Values)
            {
                tilemap.ClearAllTiles();
            }
            Debug.Log("既存タイルをクリアしました。");
        }

        // --- 4. 下地タイル（全面にgroundを敷く） ---
        if (defaultGroundTile != null && tilemaps.ContainsKey("Ground"))
        {
            Tilemap groundTilemap = tilemaps["Ground"];
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    // UnityのTilemapはY軸が上向き。CSVのy=0が上端なので反転する
                    Vector3Int pos = new Vector3Int(x, MAP_HEIGHT - 1 - y, 0);
                    groundTilemap.SetTile(pos, defaultGroundTile);
                }
            }
            Debug.Log($"下地タイルを {MAP_WIDTH * MAP_HEIGHT} マス配置しました。");
        }

        // --- 5. CSVデータに基づいてタイルを配置 ---
        int placedCount = 0;        // 配置したタイル数
        int skippedCount = 0;       // タイル未設定でスキップした数
        int collisionCount = 0;     // コリジョンタイル配置数

        HashSet<string> missingTypes = new HashSet<string>(); // 未設定のセルタイプを記録

        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                string cellType = mapData[y][x]; // CSVから読み取ったセルタイプ

                // 空やgroundのみの場合はスキップ（下地で対応済み）
                if (string.IsNullOrEmpty(cellType) || cellType == "ground")
                    continue;

                // マッピングからタイル情報を取得
                TileMapping.TileEntry entry = tileMapping.FindEntry(cellType);

                if (entry == null)
                {
                    // マッピングに存在しないタイプ
                    missingTypes.Add(cellType);
                    skippedCount++;
                    continue;
                }

                // タイルが未設定（Inspectorで選んでいない）
                if (entry.tile == null)
                {
                    missingTypes.Add(cellType);
                    skippedCount++;
                    continue;
                }

                // 配置先レイヤーのTilemapを取得
                string layerName = entry.layer;
                if (!tilemaps.ContainsKey(layerName))
                {
                    Debug.LogWarning($"レイヤー '{layerName}' が見つかりません: ({x}, {y}) = {cellType}");
                    continue;
                }

                // タイル配置（Y軸反転）
                Vector3Int pos = new Vector3Int(x, MAP_HEIGHT - 1 - y, 0);
                tilemaps[layerName].SetTile(pos, entry.tile);
                placedCount++;

                // コリジョンタイルも配置
                if (entry.hasCollision && collisionTile != null && tilemaps.ContainsKey("Collision"))
                {
                    tilemaps["Collision"].SetTile(pos, collisionTile);
                    collisionCount++;
                }
            }
        }

        // --- 6. 結果のログ出力 ---
        string resultMessage = $"マップ生成完了！\n\n" +
            $"配置タイル数: {placedCount}\n" +
            $"コリジョン: {collisionCount}\n" +
            $"スキップ（タイル未設定）: {skippedCount}";

        if (missingTypes.Count > 0)
        {
            resultMessage += $"\n\n⚠️ 以下のセルタイプのタイルが未設定です:\n";
            foreach (string type in missingTypes)
            {
                resultMessage += $"  • {type}\n";
            }
            resultMessage += "\nTileMappingアセットで設定してください。";
        }

        Debug.Log(resultMessage);
        EditorUtility.DisplayDialog("マップ生成完了", resultMessage, "OK");

        // シーンを「変更あり」状態にする（保存を促す）
        EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }

    // ===================================================================
    // 個別レイヤーだけ再生成
    // ===================================================================
    private void BuildSingleLayer(string targetLayer)
    {
        if (tileMapping == null)
        {
            EditorUtility.DisplayDialog("エラー", "TileMappingアセットを設定してください。", "OK");
            return;
        }

        string[][] mapData = LoadCSV();
        if (mapData == null) return;

        GameObject gridObj = FindOrCreateGrid();
        Dictionary<string, Tilemap> tilemaps = FindOrCreateTilemaps(gridObj);

        if (!tilemaps.ContainsKey(targetLayer))
        {
            Debug.LogError($"レイヤー '{targetLayer}' が見つかりません。");
            return;
        }

        // 対象レイヤーのみクリア
        tilemaps[targetLayer].ClearAllTiles();

        // 下地タイル（Groundレイヤーの場合のみ）
        if (targetLayer == "Ground" && defaultGroundTile != null)
        {
            Tilemap groundTilemap = tilemaps["Ground"];
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    Vector3Int pos = new Vector3Int(x, MAP_HEIGHT - 1 - y, 0);
                    groundTilemap.SetTile(pos, defaultGroundTile);
                }
            }
        }

        int placedCount = 0;

        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                string cellType = mapData[y][x];
                if (string.IsNullOrEmpty(cellType)) continue;

                TileMapping.TileEntry entry = tileMapping.FindEntry(cellType);
                if (entry == null || entry.tile == null) continue;

                // 対象レイヤーに一致するエントリのみ配置
                if (entry.layer == targetLayer)
                {
                    Vector3Int pos = new Vector3Int(x, MAP_HEIGHT - 1 - y, 0);
                    tilemaps[targetLayer].SetTile(pos, entry.tile);
                    placedCount++;
                }

                // Collisionレイヤー指定時はhasCollisionフラグのあるエントリも対象
                if (targetLayer == "Collision" && entry.hasCollision && collisionTile != null)
                {
                    Vector3Int pos = new Vector3Int(x, MAP_HEIGHT - 1 - y, 0);
                    tilemaps["Collision"].SetTile(pos, collisionTile);
                    placedCount++;
                }
            }
        }

        Debug.Log($"{targetLayer} レイヤーを再生成しました。配置タイル数: {placedCount}");
        EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }

    // ===================================================================
    // CSV読み込み
    // ===================================================================
    private string[][] LoadCSV()
    {
        // CSVファイルの存在チェック
        if (!File.Exists(CSV_PATH))
        {
            EditorUtility.DisplayDialog("エラー", $"CSVファイルが見つかりません:\n{CSV_PATH}", "OK");
            return null;
        }

        // CSVを行ごとに読み込む
        string[] lines = File.ReadAllLines(CSV_PATH);
        List<string[]> rows = new List<string[]>();

        foreach (string line in lines)
        {
            // コメント行と空行をスキップ
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            // カンマで分割してリストに追加
            rows.Add(trimmed.Split(','));
        }

        // サイズチェック
        if (rows.Count != MAP_HEIGHT)
        {
            Debug.LogWarning($"CSVの行数が想定と異なります: {rows.Count} 行（期待値: {MAP_HEIGHT}）");
        }

        return rows.ToArray();
    }

    // ===================================================================
    // Grid オブジェクトの検索または作成
    // ===================================================================
    private GameObject FindOrCreateGrid()
    {
        // 既存のGridを探す
        Grid existingGrid = FindAnyObjectByType<Grid>();
        if (existingGrid != null)
        {
            Debug.Log($"既存のGridを使用: {existingGrid.gameObject.name}");
            return existingGrid.gameObject;
        }

        // 新規作成
        GameObject gridObj = new GameObject("TownMap_Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        grid.cellSize = new Vector3(1, 1, 0);    // 1タイル = 1ユニット（48pxのPixelsPerUnit=48に合わせる）
        grid.cellLayout = GridLayout.CellLayout.Rectangle; // 長方形グリッド

        Debug.Log("新しいGridオブジェクトを作成しました: TownMap_Grid");
        return gridObj;
    }

    // ===================================================================
    // Tilemap レイヤーの検索または作成
    // ===================================================================
    private Dictionary<string, Tilemap> FindOrCreateTilemaps(GameObject gridObj)
    {
        Dictionary<string, Tilemap> result = new Dictionary<string, Tilemap>();

        for (int i = 0; i < LAYER_NAMES.Length; i++)
        {
            string layerName = LAYER_NAMES[i];

            // 既存のTilemapを子オブジェクトから探す
            Transform existing = gridObj.transform.Find(layerName);
            if (existing != null)
            {
                Tilemap tm = existing.GetComponent<Tilemap>();
                if (tm != null)
                {
                    result[layerName] = tm;
                    continue;
                }
            }

            // 新規作成
            GameObject tilemapObj = new GameObject(layerName);
            tilemapObj.transform.SetParent(gridObj.transform);    // Gridの子にする
            tilemapObj.transform.localPosition = Vector3.zero;     // 位置をリセット

            Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
            TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();

            // ソートオーダー設定（レイヤーの描画順）
            renderer.sortingOrder = LAYER_SORT_ORDER[i];

            // Collisionレイヤー特殊処理
            if (layerName == "Collision")
            {
                // Collisionレイヤーは非表示にする
                renderer.enabled = false;

                // TilemapCollider2D を追加（当たり判定用）
                TilemapCollider2D tilemapCollider = tilemapObj.AddComponent<TilemapCollider2D>();

                // CompositeCollider2D でパフォーマンス向上
                Rigidbody2D rb = tilemapObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;              // 動かない物体
                CompositeCollider2D composite = tilemapObj.AddComponent<CompositeCollider2D>();
                tilemapCollider.usedByComposite = true;             // コンポジットと統合
            }

            result[layerName] = tilemap;
            Debug.Log($"Tilemapレイヤーを作成: {layerName} (sortingOrder: {LAYER_SORT_ORDER[i]})");
        }

        return result;
    }
}
#endif
