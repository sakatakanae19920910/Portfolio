# WebGL ビルド対応チェックリスト

**対象**: 流転のジェミニ ショートデモ（EARLY TAKES出展用）
**ビルド期限**: 2026-05-23（EARLY TAKES 前日）
**重要**: 5/23より前に一度テストビルドを吐くこと！

---

## ⚠️ 最優先：今すぐテストビルドを出す

未完成で構わない。「ビルドパイプラインが通るか」だけ確認する。
期限直前（5/23）の発覚はメンタルが崩壊する。

---

## Step 1: TMP フォント確認（最重要・豆腐文字対策）

WebGL では **Dynamic Font Asset は動作しない**。
日本語グリフ（ひらがな・カタカナ・漢字）をAtlasに焼き込んだ **Static Font Asset** が必要。

### 確認手順

1. `Assets/Fonts/` の `.asset` ファイルをダブルクリック → Font Asset Viewer が開く
2. **Atlas Population Mode** を確認
   - `Static` → OK ✅
   - `Dynamic` → NG ❌ → 再生成が必要

### フォントアセット再生成手順（Dynamicの場合）

1. Unity メニュー → **Window > TextMeshPro > Font Asset Creator** を開く
2. **Source Font File**: 該当の .ttf ファイルを設定
3. **Atlas Population Mode**: `Static` を選択
4. **Render Mode**: `SDFAA`（SDFより滑らか・高品質）
5. **Sampling Point Size**: `Auto Sizing`（atlasに収まる最大サイズを自動計算）
6. **Padding**: `5` px（0にすると文字の周りにグレーの枠が出る）
7. **Packing Method**: `Fast`
8. **Atlas Resolution**: `4096 x 4096`
9. **Character Set**: `Characters from File`
10. **Character File**: `Assets/Fonts/demo_characters.txt`
11. **Generate Font Atlas** → 完了まで待つ
12. **Save** → 既存の `.asset` に上書き保存

> **補足**: KleeOneは全漢字をカバーしていないため `Unicode Range` ではなく `Characters from File` を使う。
> シナリオCSVやC#スクリプトから自動抽出 + ひらがな/カタカナ完全網羅で約1100字。

> **注意**: KleeOne・ShipporiMincho・NotoSansJP それぞれで同じ作業が必要。
> Fallback フォントが Dynamic だと豆腐文字の原因になる。

---

## Step 2: Build Settings 設定

1. **File > Build Settings** を開く
2. **Platform** を `WebGL` に変更 → **Switch Platform**（時間がかかる）
3. **Scenes In Build** に以下が全部入っているか確認:
   - `Boot`
   - `Title`
   - `IntroScene`
   - `TownScene`
   - `PartnerHomeScene`（パートナーの家）
   - `TraderScene`（交換屋）
   - `WeaponShopScene`（武器屋）
   - `TavernScene`（酒場）
   - `DemoEndScene`（デモ終了）
4. **Player Settings > WebGL**:
   - **Compression Format**: `Brotli`（itch.io・unityroomは対応済み）
   - **Exception Support**: `None`（ビルドサイズ大幅削減）
   - **Initial Memory Size**: `256` MB（日本語フォントが重い場合は`512`に増やす）
   - **Color Space**: `Linear`（現状のまま）

---

## Step 3: ビルド実行

1. **File > Build Settings > Build**
2. 出力先フォルダを作成: `Build/WebGL/`
3. ビルド完了まで待つ（初回は15〜30分かかることがある）

### ビルドサイズの目安
| 構成                          | 目安サイズ                                |
| ----------------------------- | ----------------------------------------- |
| フォントなし                  | 20〜40 MB                                 |
| 日本語フォント（ASCII range） | 40〜60 MB                                 |
| **日本語フォント（フル）**    | **100〜200 MB**（Brotli圧縮後は1/3〜1/4） |

> Brotli圧縮後200MBを超えるとitch.ioでアップロードに時間がかかる。
> 漢字rangeを絞るか、Atlas Resolution（4096×4096）で複数フォントをまとめることで削減できる。

---

## Step 4: ローカルテスト

> ⚠️ **Brotli圧縮はローカルHTTPで動かない**: ブラウザのセキュリティ仕様でBrotliはHTTPS必須。
> ローカルテスト時は **Compression Format を Gzip** に変えてビルドすること。
> unityroom/itch.ioにアップロードするときはBrotliに戻す。

WebGLビルドはブラウザのCORSポリシーで `file://` から直接開けない。
ローカルサーバーを立てる必要がある。

```bash
# Build/WebGL/WebGL フォルダで実行
cd Build/WebGL/WebGL
python3 -m http.server 8080
# → ブラウザで http://localhost:8080 を開く
```

> GzipビルドならPythonの標準サーバーで動く。Brotliの場合は.br用カスタムサーバーが別途必要。

---

## Step 5: ブラウザで動作確認チェックリスト

### 基本動作
- [ ] タイトル画面が表示される
- [ ] START ボタンを押してイントロへ遷移できる
- [ ] プレイヤーが歩ける
- [ ] NPC に話しかけて会話が開始できる
- [ ] 会話テキストが日本語で正しく表示される（豆腐文字でない）
- [ ] 会話中にクリック/Enterで次のセリフへ進める
- [ ] シーン遷移（町↔室内）ができる
- [ ] デモ終了画面が表示される

### フォント・グラフィック
- [ ] KleeOne フォント（本文）が正しく表示される
- [ ] ShipporiMincho フォント（話者名）が正しく表示される
- [ ] 立ち絵が表示される
- [ ] 背景画像が表示される

### オーディオ（BGM/SE 選定後に確認）
- [ ] タイトルまたはゲーム内でBGMが鳴る
- [ ] SFX が鳴る
- [ ] START ボタン押下後にBGMが再生される（WebGLの自動再生ブロック対策済み）

### ブラウザ互換性
- [ ] Chrome で動作する（メイン）
- [ ] Firefox で動作する（確認推奨）
- [ ] Safari は後回しでOK（WebAudioの挙動が独特）

---

## Step 6: itch.io / unityroom へのアップロード

### itch.io の場合
1. itch.io でプロジェクトページを作成（非公開）
2. **Kind of project**: `HTML`
3. **Upload**: ビルドフォルダの中身を ZIP に固めてアップロード
   - `Build/WebGL/` の中身を全部 ZIP（フォルダごとではなく中身）
4. **Embed options**: `SharedArrayBuffer` を有効にする（Unity WebGL に必要）
5. **Viewport dimensions**: `960 × 540`（現在の Reference Resolution）
6. 非公開のまま自分のURLでテスト

### unityroom の場合
1. unityroom でプロジェクトを作成
2. ZIPをアップロード
3. 処理完了後にブラウザでテスト

---

## 既知の問題と対処法

| 症状                           | 原因                                     | 対処                                   |
| ------------------------------ | ---------------------------------------- | -------------------------------------- |
| 日本語が□（豆腐）になる        | Font AssetがDynamic / 日本語グリフ未収録 | Step 1 の再生成を実行                  |
| BGMが鳴らない                  | ブラウザのAutoPlay Policy                | SoundManager修正済み（コード対応完了） |
| 画面が真っ黒のまま             | シーンがBuild Settingsに未登録           | Step 2 で全シーンを追加                |
| `SharedArrayBuffer` エラー     | itch.io の設定不足                       | Step 6 の SharedArrayBuffer を有効化   |
| ビルドが巨大（200MB超）        | 日本語フォントのグリフ数が多い           | Unicode Rangeを必要な範囲に絞る        |
| ブラウザコンソールに `abort()` | Exception Handlingがフル                 | Player Settings で `None` に変更       |

---

## SoundManager の WebGL 対応について（コード修正済み）

`Assets/Scripts/SoundManager.cs` に WebGL 用の自動再生ブロック対策を実装済み。

**動作**:
- ユーザーが最初にキーやクリックをした時点でオーディオアンロック
- アンロック前に `PlayBGM()` が呼ばれた場合、BGM名をキューに保持
- アンロック後にキューされたBGMを自動再生

エディタ上では通常通り動作する（`#if UNITY_WEBGL && !UNITY_EDITOR` でガード済み）。

---

## Steam ストアページ提出について（手作業）

**Steamの審査に3〜5営業日かかる**。5/22公開を目指すなら 5/17〜5/18 中に投げないと間に合わない。

### 今すぐ投げるべき理由
- 画像は後から差し替え可能
- ラフなメインビジュアルでも審査は通る（解像度・文字の見切れがなければOK）
- Coming Soon ページなので完成品である必要はない

### 最低限必要な素材（今あるもので代用可）
- カプセル画像（616×353px）← ラフでもOK
- 縦型カプセル（374×448px）
- ヘッダー画像（460×215px）
- スクリーンショット 5枚以上（Unity エディタで撮った画面でもOK）
- 紹介文（日本語）← 既に完成済み
