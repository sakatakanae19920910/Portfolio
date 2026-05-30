#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// JSONファイルからUnity Rule Tileアセットを自動生成するエディタ拡張。
/// メニュー: GeminiRPG > Generate Rule Tile From JSON
///
/// 使い方:
/// 1. Pythonスクリプト(generate_autotile.py)でタイル画像+JSONを生成
/// 2. Unityに戻り、生成画像のPixels Per Unitを48に設定
/// 3. GeminiRPG > Generate Rule Tile From JSON を実行
/// 4. JSONファイルを選択 → Rule Tileが自動作成される
/// </summary>
public class RuleTileGenerator : EditorWindow
{
    // ===================================================================
    // メニュー: 1つのJSONからRule Tileを生成
    // ===================================================================
    [MenuItem("GeminiRPG/Generate Rule Tile From JSON")]
    public static void GenerateFromJSON()
    {
        string jsonPath = EditorUtility.OpenFilePanel(
            "Rule Tile設定JSONを選択",
            "Assets/Art/Tilemap/Tiles/Town",
            "json"
        );
        if (string.IsNullOrEmpty(jsonPath)) return;

        string result = GenerateFromJsonPath(jsonPath);
        EditorUtility.DisplayDialog("Rule Tile生成完了", result, "OK");
    }

    // ===================================================================
    // メニュー: 全Autotile_*フォルダのJSONから一括生成 (Town + Inside)
    // ===================================================================
    [MenuItem("GeminiRPG/Generate ALL Rule Tiles (Batch)")]
    public static void GenerateAllRuleTiles()
    {
        // Town と Inside 両方を検索
        var searchRoots = new[]
        {
            Path.Combine(Application.dataPath, "Art/Tilemap/Tiles/Town"),
            Path.Combine(Application.dataPath, "Art/Tilemap/Tiles/Inside"),
        };

        var jsonList = new System.Collections.Generic.List<string>();
        foreach (var root in searchRoots)
        {
            if (Directory.Exists(root))
                jsonList.AddRange(Directory.GetFiles(root, "*_rules.json", SearchOption.AllDirectories));
        }
        string[] jsonFiles = jsonList.ToArray();

        if (jsonFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("エラー", "JSONファイルが見つかりません。\nPythonスクリプトで先にタイル画像を生成してください。", "OK");
            return;
        }

        int successCount = 0;
        int skipCount = 0;
        List<string> errors = new List<string>();

        // プログレスバー表示
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            string jsonPath = jsonFiles[i];
            string fileName = Path.GetFileName(jsonPath);

            EditorUtility.DisplayProgressBar(
                "Rule Tile一括生成",
                $"処理中: {fileName} ({i + 1}/{jsonFiles.Length})",
                (float)(i + 1) / jsonFiles.Length
            );

            // 既存のRule Tileがあればスキップ
            string relativePath = "Assets" + jsonPath.Substring(Application.dataPath.Length);
            string directory = Path.GetDirectoryName(relativePath);
            string jsonText = File.ReadAllText(jsonPath);
            var config = JsonUtility.FromJson<RuleTileConfig>(jsonText);

            if (config == null) { errors.Add(fileName); continue; }

            string existingPath = Path.Combine(directory, config.name + "_RuleTile.asset");
            if (File.Exists(existingPath))
            {
                skipCount++;
                continue;
            }

            try
            {
                GenerateFromJsonPath(jsonPath);
                successCount++;
            }
            catch (System.Exception e)
            {
                errors.Add($"{fileName}: {e.Message}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        string message = $"Rule Tile一括生成完了！\n\n" +
            $"新規作成: {successCount}\n" +
            $"スキップ（既存）: {skipCount}\n" +
            $"エラー: {errors.Count}";

        if (errors.Count > 0)
        {
            message += "\n\nエラー詳細:\n";
            foreach (string err in errors) message += $"  • {err}\n";
        }

        Debug.Log(message);
        EditorUtility.DisplayDialog("一括生成完了", message, "OK");
    }

    // ===================================================================
    // JSONパスからRule Tileを生成する内部メソッド
    // ===================================================================
    private static string GenerateFromJsonPath(string jsonPath)
    {
        string relativePath = "Assets" + jsonPath.Substring(Application.dataPath.Length);
        string directory = Path.GetDirectoryName(relativePath);

        string jsonText = File.ReadAllText(jsonPath);
        var config = JsonUtility.FromJson<RuleTileConfig>(jsonText);

        if (config == null || config.rules == null)
            return "JSONの解析に失敗しました。";

        // 生成画像のインポート設定を一括修正
        FixImportSettings(directory, config.tileSize);

        // Rule Tileアセットを作成
        RuleTile ruleTile = ScriptableObject.CreateInstance<RuleTile>();

        string defaultSpriteName = config.name + "_15";
        Sprite defaultSprite = FindSprite(directory, defaultSpriteName);
        ruleTile.m_DefaultSprite = defaultSprite;
        ruleTile.m_DefaultColliderType = Tile.ColliderType.None;

        var tilingRules = new List<RuleTile.TilingRule>();

        foreach (var rule in config.rules)
        {
            var tilingRule = new RuleTile.TilingRule();

            // output が "random" かつ sprites 配列がある場合はランダム出力モード
            bool isRandom = rule.output == "random" && rule.sprites != null && rule.sprites.Length > 0;

            if (isRandom)
            {
                // 複数スプライトをランダム選択
                var spriteList = new List<Sprite>();
                foreach (var spriteName in rule.sprites)
                {
                    Sprite s = FindSprite(directory, spriteName);
                    if (s != null) spriteList.Add(s);
                    else Debug.LogWarning($"スプライトが見つかりません: {spriteName}");
                }
                if (spriteList.Count == 0) continue;
                tilingRule.m_Sprites = spriteList.ToArray();
                tilingRule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Random;
            }
            else
            {
                // 通常の単一スプライト
                Sprite sprite = FindSprite(directory, rule.sprite);
                if (sprite == null)
                {
                    Debug.LogWarning($"スプライトが見つかりません: {rule.sprite}");
                    continue;
                }
                tilingRule.m_Sprites = new Sprite[] { sprite };
                tilingRule.m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single;
            }

            // 8方向の全ポジション定義
            var allPositions = new Vector3Int[]
            {
                new Vector3Int(-1,  1, 0), new Vector3Int( 0,  1, 0), new Vector3Int( 1,  1, 0),
                new Vector3Int(-1,  0, 0),                             new Vector3Int( 1,  0, 0),
                new Vector3Int(-1, -1, 0), new Vector3Int( 0, -1, 0), new Vector3Int( 1, -1, 0),
            };

            // Unity 6では m_NeighborPositions に記載された全ポジションを評価するため、
            // 値0（don't care）のポジションをリストに追加しないことが重要。
            tilingRule.m_NeighborPositions = new List<Vector3Int>();
            tilingRule.m_Neighbors = new List<int>();
            for (int i = 0; i < rule.neighbors.Length; i++)
            {
                if (rule.neighbors[i] != 0)  // 0(don't care)はスキップ
                {
                    tilingRule.m_NeighborPositions.Add(allPositions[i]);
                    tilingRule.m_Neighbors.Add(rule.neighbors[i]);
                }
            }

            tilingRule.m_ColliderType = Tile.ColliderType.None;

            tilingRules.Add(tilingRule);
        }

        ruleTile.m_TilingRules = tilingRules;

        string assetPath = Path.Combine(directory, config.name + "_RuleTile.asset");
        AssetDatabase.CreateAsset(ruleTile, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"Rule Tile作成: {config.name} ({tilingRules.Count}ルール) → {assetPath}");

        return $"名前: {config.name}\nルール数: {tilingRules.Count}\n保存先: {assetPath}";
    }

    /// <summary>
    /// 指定フォルダ内のPNG画像のインポート設定を修正する
    /// </summary>
    private static void FixImportSettings(string directory, int pixelsPerUnit)
    {
        // フォルダ内の全テクスチャを検索
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { directory });
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) continue;

            bool needReimport = false;

            // Sprite Modeを Single に
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                needReimport = true;
            }

            // Pixels Per Unit を設定
            if (importer.spritePixelsPerUnit != pixelsPerUnit)
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                needReimport = true;
            }

            // Filter Mode を Point に
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                needReimport = true;
            }

            // 圧縮なし
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                needReimport = true;
            }

            if (needReimport)
            {
                importer.SaveAndReimport();
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"インポート設定を修正: {fixedCount}ファイル (PPU={pixelsPerUnit}, FilterMode=Point)");
        }
    }

    /// <summary>
    /// 指定フォルダ内からスプライト名でスプライトを検索する
    /// </summary>
    private static Sprite FindSprite(string directory, string spriteName)
    {
        // ファイルパスから検索
        string path = Path.Combine(directory, spriteName + ".png");
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (sprite != null) return sprite;

        // 見つからない場合、フォルダ内を検索
        string[] guids = AssetDatabase.FindAssets(spriteName + " t:Sprite", new[] { directory });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null && sprite.name == spriteName) return sprite;
        }

        return null;
    }

    // ===== JSON デシリアライズ用クラス =====

    [System.Serializable]
    private class RuleTileConfig
    {
        public string name;       // タイル名
        public int tileSize;      // タイルサイズ(px)
        public RuleEntry[] rules; // ルール配列
    }

    [System.Serializable]
    private class RuleEntry
    {
        public string sprite;     // 単一スプライト名 (output="single" 時)
        public string[] sprites;  // 複数スプライト名 (output="random" 時)
        public int[] neighbors;   // 隣接条件 [8方向]
        public string output;     // "single" または "random"
    }
}
#endif
