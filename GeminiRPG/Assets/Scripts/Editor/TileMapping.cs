#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// セルタイプと使用するTileアセットの対応を管理するScriptableObject。
/// Inspectorでタイルを視覚的にマッピングできる。
/// 作成: GeminiRPG > Create Tile Mapping
/// </summary>
[CreateAssetMenu(fileName = "TileMapping", menuName = "GeminiRPG/Tile Mapping")]
public class TileMapping : ScriptableObject
{
    [System.Serializable]
    public class TileEntry
    {
        [Tooltip("CSVのセルタイプ名（例: ground, path, pharmacy）")]
        public string cellType;        // CSVのセルタイプ名

        [Tooltip("このセルタイプに使用するタイルアセット")]
        public TileBase tile;          // 対応するタイルアセット

        [Tooltip("配置先のレイヤー名（Ground / Path / Buildings / Objects / Rooftop / Collision）")]
        public string layer = "Ground"; // 配置先レイヤー名

        [Tooltip("trueの場合、Collisionレイヤーにもタイルを配置（壁・建物など通行不可にしたいセル）")]
        public bool hasCollision;       // 当たり判定を付けるか
    }

    [Header("=== タイルマッピング設定 ===")]
    [Tooltip("各セルタイプに対応するタイルとレイヤーの設定リスト")]
    public TileEntry[] entries = new TileEntry[]
    {
        // --- Ground レイヤー ---
        new TileEntry { cellType = "ground",        layer = "Ground",    hasCollision = false },
        new TileEntry { cellType = "plaza",          layer = "Ground",    hasCollision = false },

        // --- Path レイヤー ---
        new TileEntry { cellType = "path",           layer = "Path",      hasCollision = false },
        new TileEntry { cellType = "path-main",      layer = "Path",      hasCollision = false },

        // --- Buildings レイヤー ---
        new TileEntry { cellType = "pharmacy",       layer = "Buildings", hasCollision = true },
        new TileEntry { cellType = "weapon",         layer = "Buildings", hasCollision = true },
        new TileEntry { cellType = "tavern",         layer = "Buildings", hasCollision = true },
        new TileEntry { cellType = "exchange",       layer = "Buildings", hasCollision = true },
        new TileEntry { cellType = "fortune",        layer = "Buildings", hasCollision = true },
        new TileEntry { cellType = "wall",           layer = "Buildings", hasCollision = true },

        // --- Rooftop レイヤー ---
        new TileEntry { cellType = "pharmacy-roof",  layer = "Rooftop",   hasCollision = false },
        new TileEntry { cellType = "weapon-roof",    layer = "Rooftop",   hasCollision = false },
        new TileEntry { cellType = "tavern-roof",    layer = "Rooftop",   hasCollision = false },
        new TileEntry { cellType = "exchange-roof",  layer = "Rooftop",   hasCollision = false },
        new TileEntry { cellType = "fortune-roof",   layer = "Rooftop",   hasCollision = false },

        // --- Objects レイヤー ---
        new TileEntry { cellType = "tree",           layer = "Objects",   hasCollision = true },
        new TileEntry { cellType = "bush",           layer = "Objects",   hasCollision = false },
        new TileEntry { cellType = "rock",           layer = "Objects",   hasCollision = true },
        new TileEntry { cellType = "fence",          layer = "Objects",   hasCollision = true },
        new TileEntry { cellType = "bench",          layer = "Objects",   hasCollision = true },
        new TileEntry { cellType = "door",           layer = "Objects",   hasCollision = false },
        new TileEntry { cellType = "water",          layer = "Objects",   hasCollision = true },

        // --- 特殊 ---
        new TileEntry { cellType = "entry",          layer = "Path",      hasCollision = false },
    };

    /// <summary>
    /// セルタイプ名からTileEntryを検索する
    /// </summary>
    /// <param name="cellType">CSVのセルタイプ名</param>
    /// <returns>対応するTileEntry。見つからなければnull</returns>
    public TileEntry FindEntry(string cellType)
    {
        // entries配列をループして一致するタイプを探す
        foreach (var entry in entries)
        {
            if (entry.cellType == cellType)
                return entry;
        }
        return null; // 見つからなかった場合
    }
}
#endif
