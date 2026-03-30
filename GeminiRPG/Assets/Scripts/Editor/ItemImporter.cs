#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// CSVファイルからアイテムデータをインポートするエディタ拡張
/// メニュー: GeminiRPG > Import Item Data
/// </summary>
public class ItemImporter : EditorWindow
{
    private const string WEAPON_CSV_PATH = "Assets/Resources/Item/MasterWeapon.csv";
    private const string ARMOR_CSV_PATH = "Assets/Resources/Item/MasterArmor.csv";
    private const string CONSUMABLE_CSV_PATH = "Assets/Resources/Item/MasterConsumable.csv";
    private const string KEYITEM_CSV_PATH = "Assets/Resources/Item/MasterKeyitem.csv";
    private const string OUTPUT_PATH = "Assets/Resources/Data/Items";

    [MenuItem("GeminiRPG/Import Item Data")]
    public static void ImportAllItems()
    {
        // 出力フォルダを作成
        CreateOutputFolders();

        int weaponCount = ImportWeapons();
        int armorCount = ImportArmors();
        int consumableCount = ImportConsumables();
        int keyItemCount = ImportKeyItems();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "インポート完了",
            $"武器: {weaponCount}件\n防具: {armorCount}件\n消耗品: {consumableCount}件\n重要アイテム: {keyItemCount}件\n\n合計: {weaponCount + armorCount + consumableCount + keyItemCount}件のアイテムをインポートしました！",
            "OK"
        );
    }

    private static void CreateOutputFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Data");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Items"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Data", "Items");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Items/Weapons"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Data/Items", "Weapons");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Items/Armors"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Data/Items", "Armors");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Items/Consumables"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Data/Items", "Consumables");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Data/Items/KeyItems"))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Data/Items", "KeyItems");
        }
    }

    // ==================== 武器インポート ====================
    private static int ImportWeapons()
    {
        if (!File.Exists(WEAPON_CSV_PATH))
        {
            Debug.LogWarning($"武器CSVが見つかりません: {WEAPON_CSV_PATH}");
            return 0;
        }

        string[] lines = File.ReadAllLines(WEAPON_CSV_PATH);
        int count = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length < 18) continue;

            // ID,Name,WeaponType,PhysicalAttr,Attack,Elements,RaceBonus,AilmentGrant,AilmentRate,AgilityMod,MpCost,SpecialEffect,EquipChar,BuyPrice,SellPrice,IsGiftable,GiftTarget,EmpathyValue,Description
            int id = ParseInt(values[0]);
            string itemName = values[1];

            string assetPath = $"Assets/Resources/Data/Items/Weapons/{id}_{SanitizeFileName(itemName)}.asset";
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);

            if (weapon == null)
            {
                weapon = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(weapon, assetPath);
            }

            weapon.id = id;
            weapon.itemName = itemName;
            weapon.weaponType = ParseEnum<WeaponType>(values[2]);
            weapon.physicalAttr = ParseEnum<PhysicalAttribute>(values[3]);
            weapon.attack = ParseInt(values[4]);
            weapon.elements = values[5];
            weapon.raceBonus = values[6];
            weapon.ailmentGrant = values[7];
            weapon.ailmentRate = ParseInt(values[8]);
            weapon.agilityMod = ParseInt(values[9]);
            weapon.mpCost = ParseInt(values[10]);
            weapon.specialEffect = values[11];
            weapon.equipChar = values[12];
            weapon.buyPrice = ParseInt(values[13]);
            weapon.sellPrice = ParseInt(values[14]);
            weapon.isGiftable = ParseBool(values[15]);
            weapon.giftTarget = values[16];
            weapon.empathyValue = ParseInt(values[17]);
            weapon.description = values.Length > 18 ? values[18] : "";

            EditorUtility.SetDirty(weapon);
            count++;
        }

        return count;
    }

    // ==================== 防具インポート ====================
    private static int ImportArmors()
    {
        if (!File.Exists(ARMOR_CSV_PATH))
        {
            Debug.LogWarning($"防具CSVが見つかりません: {ARMOR_CSV_PATH}");
            return 0;
        }

        string[] lines = File.ReadAllLines(ARMOR_CSV_PATH);
        int count = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length < 16) continue;

            // ID,Name,ArmorType,Defense,MagicDefense,ElementResist,AilmentResist,AilmentResistRate,AgilityMod,SpecialEffect,EquipChar,BuyPrice,SellPrice,IsGiftable,GiftTarget,EmpathyValue,Description
            int id = ParseInt(values[0]);
            string itemName = values[1];

            string assetPath = $"Assets/Resources/Data/Items/Armors/{id}_{SanitizeFileName(itemName)}.asset";
            ArmorData armor = AssetDatabase.LoadAssetAtPath<ArmorData>(assetPath);

            if (armor == null)
            {
                armor = ScriptableObject.CreateInstance<ArmorData>();
                AssetDatabase.CreateAsset(armor, assetPath);
            }

            armor.id = id;
            armor.itemName = itemName;
            armor.armorType = ParseEnum<ArmorType>(values[2]);
            armor.defense = ParseInt(values[3]);
            armor.magicDefense = ParseInt(values[4]);
            armor.elementResist = values[5];
            armor.ailmentResist = values[6];
            armor.ailmentResistRate = ParseInt(values[7]);
            armor.agilityMod = ParseInt(values[8]);
            armor.specialEffect = values[9];
            armor.equipChar = values[10];
            armor.buyPrice = ParseInt(values[11]);
            armor.sellPrice = ParseInt(values[12]);
            armor.isGiftable = ParseBool(values[13]);
            armor.giftTarget = values[14];
            armor.empathyValue = ParseInt(values[15]);
            armor.description = values.Length > 16 ? values[16] : "";

            EditorUtility.SetDirty(armor);
            count++;
        }

        return count;
    }

    // ==================== 消耗品インポート ====================
    private static int ImportConsumables()
    {
        if (!File.Exists(CONSUMABLE_CSV_PATH))
        {
            Debug.LogWarning($"消耗品CSVが見つかりません: {CONSUMABLE_CSV_PATH}");
            return 0;
        }

        string[] lines = File.ReadAllLines(CONSUMABLE_CSV_PATH);
        int count = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length < 17) continue;

            // ID,Name,ConsumableType,TargetScope,EffectValue,Element,AilmentCure,AilmentGrant,StatBoost,StatBoostValue,UsableInBattle,UsableInField,BuyPrice,SellPrice,IsGiftable,GiftTarget,EmpathyValue,Description
            int id = ParseInt(values[0]);
            string itemName = values[1];

            string assetPath = $"Assets/Resources/Data/Items/Consumables/{id}_{SanitizeFileName(itemName)}.asset";
            ConsumableData consumable = AssetDatabase.LoadAssetAtPath<ConsumableData>(assetPath);

            if (consumable == null)
            {
                consumable = ScriptableObject.CreateInstance<ConsumableData>();
                AssetDatabase.CreateAsset(consumable, assetPath);
            }

            consumable.id = id;
            consumable.itemName = itemName;
            consumable.consumableType = ParseEnum<ConsumableType>(values[2]);
            consumable.targetScope = ParseEnum<TargetScope>(values[3]);
            consumable.effectValue = ParseInt(values[4]);
            consumable.element = values[5];
            consumable.ailmentCure = values[6];
            consumable.ailmentGrant = values[7];
            consumable.statBoost = values[8];
            consumable.statBoostValue = ParseInt(values[9]);
            consumable.usableInBattle = ParseBool(values[10]);
            consumable.usableInField = ParseBool(values[11]);
            consumable.buyPrice = ParseInt(values[12]);
            consumable.sellPrice = ParseInt(values[13]);
            consumable.isGiftable = ParseBool(values[14]);
            consumable.giftTarget = values[15];
            consumable.empathyValue = ParseInt(values[16]);
            consumable.description = values.Length > 17 ? values[17] : "";

            EditorUtility.SetDirty(consumable);
            count++;
        }

        return count;
    }

    // ==================== 重要アイテムインポート ====================
    private static int ImportKeyItems()
    {
        if (!File.Exists(KEYITEM_CSV_PATH))
        {
            Debug.LogWarning($"重要アイテムCSVが見つかりません: {KEYITEM_CSV_PATH}");
            return 0;
        }

        string[] lines = File.ReadAllLines(KEYITEM_CSV_PATH);
        int count = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

            string[] values = ParseCSVLine(line);
            if (values.Length < 6) continue;

            // ID,Name,KeyItemType,PassiveEffect,PassiveTarget,UseAction,Description
            int id = ParseInt(values[0]);
            string itemName = values[1];

            string assetPath = $"Assets/Resources/Data/Items/KeyItems/{id}_{SanitizeFileName(itemName)}.asset";
            KeyItemData keyItem = AssetDatabase.LoadAssetAtPath<KeyItemData>(assetPath);

            if (keyItem == null)
            {
                keyItem = ScriptableObject.CreateInstance<KeyItemData>();
                AssetDatabase.CreateAsset(keyItem, assetPath);
            }

            keyItem.id = id;
            keyItem.itemName = itemName;
            keyItem.keyItemType = ParseEnum<KeyItemType>(values[2]);
            keyItem.passiveEffect = values[3];
            keyItem.passiveTarget = values[4];
            keyItem.useAction = values[5];
            keyItem.description = values.Length > 6 ? values[6] : "";
            // 重要アイテムは購入・売却・プレゼント不可
            keyItem.buyPrice = 0;
            keyItem.sellPrice = 0;
            keyItem.isGiftable = false;
            keyItem.giftTarget = "";
            keyItem.empathyValue = 0;

            EditorUtility.SetDirty(keyItem);
            count++;
        }

        return count;
    }

    // ==================== ユーティリティ ====================
    private static string[] ParseCSVLine(string line)
    {
        // カンマで区切るが、ダブルクォート内のカンマは無視
        string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
        string[] values = Regex.Split(line, pattern);

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Trim().Trim('"').Replace("\"\"", "\"");
        }

        return values;
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out int result))
        {
            return result;
        }
        return 0;
    }

    private static bool ParseBool(string value)
    {
        return value.ToUpper() == "TRUE";
    }

    private static T ParseEnum<T>(string value) where T : struct
    {
        if (System.Enum.TryParse<T>(value, true, out T result))
        {
            return result;
        }
        return default(T);
    }

    private static string SanitizeFileName(string name)
    {
        // ファイル名に使えない文字を除去
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }

    // ==================== エクスポート機能 ====================
    [MenuItem("GeminiRPG/Export Item Data to CSV")]
    public static void ExportAllItems()
    {
        int weaponCount = ExportWeapons();
        int armorCount = ExportArmors();
        int consumableCount = ExportConsumables();
        int keyItemCount = ExportKeyItems();

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "エクスポート完了",
            $"武器: {weaponCount}件\n防具: {armorCount}件\n消耗品: {consumableCount}件\n重要アイテム: {keyItemCount}件\n\n合計: {weaponCount + armorCount + consumableCount + keyItemCount}件のアイテムをCSVにエクスポートしました！",
            "OK"
        );
    }

    private static int ExportWeapons()
    {
        string[] guids = AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/Resources/Data/Items/Weapons" });
        if (guids.Length == 0) return 0;

        // ScriptableObjectをIDでマッピング
        var weaponMap = new System.Collections.Generic.Dictionary<int, WeaponData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData w = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (w != null) weaponMap[w.id] = w;
        }

        // 既存CSVを読み込んで、データ行だけ更新
        string[] lines = File.Exists(WEAPON_CSV_PATH) ? File.ReadAllLines(WEAPON_CSV_PATH) : new string[0];
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        bool headerWritten = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            
            // ヘッダー行
            if (!headerWritten && trimmed.StartsWith("ID,"))
            {
                sb.AppendLine(line);
                headerWritten = true;
                continue;
            }
            
            // コメント行・空行はそのまま保持
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                sb.AppendLine(line);
                continue;
            }

            // データ行：IDを取得して対応するScriptableObjectで置換
            string[] values = ParseCSVLine(trimmed);
            if (values.Length > 0 && int.TryParse(values[0], out int id) && weaponMap.TryGetValue(id, out WeaponData w))
            {
                sb.AppendLine($"{w.id},{EscapeCSV(w.itemName)},{w.weaponType},{w.physicalAttr},{w.attack},{w.elements},{w.raceBonus},{w.ailmentGrant},{w.ailmentRate},{w.agilityMod},{w.mpCost},{w.specialEffect},{w.equipChar},{w.buyPrice},{w.sellPrice},{w.isGiftable.ToString().ToUpper()},{w.giftTarget},{w.empathyValue},{EscapeCSV(w.description)}");
            }
            else
            {
                sb.AppendLine(line); // 対応するデータがなければ元のまま
            }
        }

        File.WriteAllText(WEAPON_CSV_PATH, sb.ToString(), System.Text.Encoding.UTF8);
        return guids.Length;
    }

    private static int ExportArmors()
    {
        string[] guids = AssetDatabase.FindAssets("t:ArmorData", new[] { "Assets/Resources/Data/Items/Armors" });
        if (guids.Length == 0) return 0;

        var armorMap = new System.Collections.Generic.Dictionary<int, ArmorData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ArmorData a = AssetDatabase.LoadAssetAtPath<ArmorData>(path);
            if (a != null) armorMap[a.id] = a;
        }

        string[] lines = File.Exists(ARMOR_CSV_PATH) ? File.ReadAllLines(ARMOR_CSV_PATH) : new string[0];
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        bool headerWritten = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            
            if (!headerWritten && trimmed.StartsWith("ID,"))
            {
                sb.AppendLine(line);
                headerWritten = true;
                continue;
            }
            
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                sb.AppendLine(line);
                continue;
            }

            string[] values = ParseCSVLine(trimmed);
            if (values.Length > 0 && int.TryParse(values[0], out int id) && armorMap.TryGetValue(id, out ArmorData a))
            {
                sb.AppendLine($"{a.id},{EscapeCSV(a.itemName)},{a.armorType},{a.defense},{a.magicDefense},{a.elementResist},{a.ailmentResist},{a.ailmentResistRate},{a.agilityMod},{a.specialEffect},{a.equipChar},{a.buyPrice},{a.sellPrice},{a.isGiftable.ToString().ToUpper()},{a.giftTarget},{a.empathyValue},{EscapeCSV(a.description)}");
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        File.WriteAllText(ARMOR_CSV_PATH, sb.ToString(), System.Text.Encoding.UTF8);
        return guids.Length;
    }

    private static int ExportConsumables()
    {
        string[] guids = AssetDatabase.FindAssets("t:ConsumableData", new[] { "Assets/Resources/Data/Items/Consumables" });
        if (guids.Length == 0) return 0;

        var consumableMap = new System.Collections.Generic.Dictionary<int, ConsumableData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ConsumableData c = AssetDatabase.LoadAssetAtPath<ConsumableData>(path);
            if (c != null) consumableMap[c.id] = c;
        }

        string[] lines = File.Exists(CONSUMABLE_CSV_PATH) ? File.ReadAllLines(CONSUMABLE_CSV_PATH) : new string[0];
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        bool headerWritten = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            
            if (!headerWritten && trimmed.StartsWith("ID,"))
            {
                sb.AppendLine(line);
                headerWritten = true;
                continue;
            }
            
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                sb.AppendLine(line);
                continue;
            }

            string[] values = ParseCSVLine(trimmed);
            if (values.Length > 0 && int.TryParse(values[0], out int id) && consumableMap.TryGetValue(id, out ConsumableData c))
            {
                sb.AppendLine($"{c.id},{EscapeCSV(c.itemName)},{c.consumableType},{c.targetScope},{c.effectValue},{c.element},{c.ailmentCure},{c.ailmentGrant},{c.statBoost},{c.statBoostValue},{c.usableInBattle.ToString().ToUpper()},{c.usableInField.ToString().ToUpper()},{c.buyPrice},{c.sellPrice},{c.isGiftable.ToString().ToUpper()},{c.giftTarget},{c.empathyValue},{EscapeCSV(c.description)}");
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        File.WriteAllText(CONSUMABLE_CSV_PATH, sb.ToString(), System.Text.Encoding.UTF8);
        return guids.Length;
    }

    private static int ExportKeyItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:KeyItemData", new[] { "Assets/Resources/Data/Items/KeyItems" });
        if (guids.Length == 0) return 0;

        var keyItemMap = new System.Collections.Generic.Dictionary<int, KeyItemData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            KeyItemData k = AssetDatabase.LoadAssetAtPath<KeyItemData>(path);
            if (k != null) keyItemMap[k.id] = k;
        }

        string[] lines = File.Exists(KEYITEM_CSV_PATH) ? File.ReadAllLines(KEYITEM_CSV_PATH) : new string[0];
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        bool headerWritten = false;
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            
            if (!headerWritten && trimmed.StartsWith("ID,"))
            {
                sb.AppendLine(line);
                headerWritten = true;
                continue;
            }
            
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
            {
                sb.AppendLine(line);
                continue;
            }

            string[] values = ParseCSVLine(trimmed);
            if (values.Length > 0 && int.TryParse(values[0], out int id) && keyItemMap.TryGetValue(id, out KeyItemData k))
            {
                sb.AppendLine($"{k.id},{EscapeCSV(k.itemName)},{k.keyItemType},{k.passiveEffect},{k.passiveTarget},{k.useAction},{EscapeCSV(k.description)}");
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        File.WriteAllText(KEYITEM_CSV_PATH, sb.ToString(), System.Text.Encoding.UTF8);
        return guids.Length;
    }

    private static string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }
}
#endif
