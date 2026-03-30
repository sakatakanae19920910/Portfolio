# 機能仕様書一覧

**作成日**: 2026-01-20
**最終更新**: 2026-01-25

本フォルダには、現時点で実装されているスクリプトの機能仕様書を格納しています。

---

## ドキュメント一覧

| ファイル | 対象スクリプト |
|---------|---------------|
| [シナリオシステム.md](シナリオシステム.md) | ScenarioCommand, ScenarioLoader, ScenarioExecutor |
| [プレイヤーシステム.md](プレイヤーシステム.md) | PlayerController, NPCTrigger, CameraFollow, MapTransition, FadeManager |
| [サウンドシステム.md](サウンドシステム.md) | SoundManager, BootLoader |
| [キャラクターシステム.md](キャラクターシステム.md) | CharacterData, StatusParameter, EmotionParameter, EmotionEffect |
| [アイテムシステム.md](アイテムシステム.md) | ItemData, WeaponData, ArmorData, ConsumableData, KeyItemData, ItemImporter |

---

## スクリプト配置

```
Assets/Scripts/
├── Scenario/
│   ├── ScenarioCommand.cs
│   ├── ScenarioLoader.cs
│   └── ScenarioExecutor.cs
├── Player/
│   ├── PlayerController.cs
│   ├── NPCTrigger.cs
│   ├── CameraFollow.cs
│   ├── MapTransition.cs
│   └── FadeManager.cs
├── Character/
│   ├── CharacterData.cs
│   ├── StatusParameter.cs
│   ├── EmotionParameter.cs
│   └── EmotionEffect.cs
├── Data/
│   ├── ItemData.cs
│   ├── WeaponData.cs
│   ├── ArmorData.cs
│   ├── ConsumableData.cs
│   └── KeyItemData.cs
├── Editor/
│   └── ItemImporter.cs
├── SoundManager.cs
├── BootLoader.cs
└── Test/
    └── EmotionSystemTest.cs
```

---

## 対象スクリプト（全20ファイル）

Phase 0.5 で使用中のスクリプトを中心に説明。  
Test/ 配下のテスト用スクリプトは仕様書から除外。
