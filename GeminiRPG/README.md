# 流転のジェミニ (Ruten no Gemini)

Unity製 2Dターン制RPG。個人開発プロジェクト。

「死後の世界」を舞台にしたダークファンタジーRPGで、感情パラメータシステムや独自のシナリオエンジンを自作しています。

---

## 技術スタック

- **エンジン**: Unity 2022 / Universal Render Pipeline (URP)
- **言語**: C#
- **外部ライブラリ**: DOTween（アニメーション）

---

## 実装のポイント

### 1. データ駆動型シナリオエンジン

CSVファイルをコマンドに変換し、順次実行するシナリオシステムを独自実装しました。

```
Chapter1.csv → ScenarioLoader → List<ScenarioCommand> → ScenarioExecutor
```

- `ScenarioLoader.cs` : CSVを行単位でパースし、引数を辞書型に変換
- `ScenarioCommand.cs` : 1行 = 1コマンドのデータクラス（Commandパターン）
- `ScenarioExecutor.cs` : コマンドを解釈して実行するエンジン（Strategyパターン）

対応コマンド: テキスト表示・フェード・BGM/SE・立ち絵・背景切替・選択肢・フラグ分岐・ジャンプ

### 2. 感情パラメータシステム

キャラクターごとに「共感・信頼・勇気」などの感情値を持ち、会話や戦闘の選択によって変化するオリジナルシステムです。感情値はストーリー分岐や立ち絵の表情差分に影響します。

- `EmotionParameter.cs` : 感情値の定義・管理
- `EmotionEffect.cs` : 感情変化のトリガー定義

### 3. Unityエディタ拡張ツール

開発効率を上げるためのエディタ拡張を実装しました。

- `ItemImporter.cs` : CSVからScriptableObjectを一括生成するインポーター
- `TownMapBuilder.cs` : CSVのマップデータからタイルマップを自動配置するツール
- `TileMapping.cs` : タイルIDとTileAssetの対応マッピング

### 4. サウンドマネージャー（Singleton）

BGM・BGS・ME・SEを種別ごとにAudioSourceプールで管理するシングルトン。フェードイン/アウト対応。`DontDestroyOnLoad`でシーンをまたいで永続化しています。

### 5. フィールドシステム

- `PlayerController.cs` : WASD/矢印キー/ゲームパッド対応のトップダウン2D移動
- `NPCTrigger.cs` : NPC接触で会話を起動するトリガー
- `MapTransition.cs` : マップ遷移（フェード演出込み）
- `CameraFollow.cs` : プレイヤー追従カメラ

---

## ディレクトリ構成

```
Assets/Scripts/
├── BootLoader.cs           # 起動シーン制御
├── SoundManager.cs         # サウンド管理（Singleton）
├── Character/              # キャラクター・感情システム
├── Data/                   # ScriptableObjectデータクラス
├── Editor/                 # エディタ拡張ツール
├── Player/                 # フィールド移動・カメラ
├── Scenario/               # シナリオエンジン
└── Test/                   # テストコード

Docs/
├── データ類準備/
│   ├── 機能仕様書/         # 各システムの機能仕様
│   └── 設計書・仕様書/     # クラス図・シーケンス図・データフロー図
└── 開発ログ/               # 月次の開発記録
```

---

## 開発状況

現在 **Phase 0.5** （2026年5月 EARLY TAKES出展に向けたデモ版制作中）。

- [x] シナリオエンジン（テキスト・選択肢・フラグ分岐・立ち絵・BGM）
- [x] フィールド移動・NPC会話・マップ遷移
- [x] 感情パラメータ基盤
- [x] アイテムデータ（武器・防具・消耗品・キーアイテム）
- [x] エディタ拡張（CSVインポーター・マップ自動配置）
- [ ] 戦闘システム（Phase 1）
- [ ] セーブ/ロード（Phase 2）
