# Prefab（プレハブ）マニュアル

**作成日**: 2026-05-12
**対象**: Unity初心者向け・このプロジェクトでの使い方

---

## Prefabとは何か

**「設計図」** です。

GameObjectをPrefabにすると、その設定・構成・参照を**ファイルとして保存**できます。
保存した設計図を複数のシーンに「置く」ことで、同じものを何度でも使い回せます。

```
Prefabなし（悪い例）
  TownScene      → DialogueSystem を手作業で作る
  PartnerHouse   → DialogueSystem を手作業で作る（同じ作業を繰り返す）
  IntroScene     → DialogueSystem を手作業で作る（また繰り返す）
  → どれかを修正するとき、全シーンでやり直し

Prefabあり（良い例）
  DialogueSystem.prefab（設計図を1つ作る）
  TownScene      → 設計図を置くだけ
  PartnerHouse   → 設計図を置くだけ
  IntroScene     → 設計図を置くだけ
  → 設計図を1箇所修正すると、全シーンに自動反映
```

---

## 用語の整理

| 用語 | 意味 |
|------|------|
| **Prefab** | Projectウィンドウに保存された設計図ファイル（青いアイコン） |
| **インスタンス** | シーンに置いたPrefabのコピー（Hierarchyに表示される） |
| **Prefab化** | シーンのGameObjectを設計図ファイルとして保存すること |
| **Apply** | インスタンスで変えた内容を設計図に書き戻すこと |
| **Revert** | インスタンスの変更を設計図の状態に戻すこと |

---

## Prefab化の手順

### Step 1: GameObjectを完成させる

シーン上でGameObjectを作り、**設定を全部終わらせてから**Prefab化する。
（参照の登録・色設定など）

### Step 2: Projectウィンドウにドラッグ

HierarchyのGameObjectを、Projectウィンドウの保存したいフォルダにドラッグ。

```
このプロジェクトの場合:
  Hierarchy の「DialogueSystem」
    ↓ ドラッグ
  Project の「Assets/_Prefabs/」フォルダ
```

ダイアログが出たら **「Original Prefab」** を選ぶ。

> 「Variant」は既存Prefabの派生版を作るときに使う（今は使わない）

### Step 3: 確認

- Projectウィンドウに **青いアイコン** の `DialogueSystem.prefab` が現れる → 成功
- HierarchyのGameObject名が **青字** になる → シーン上のものがPrefabインスタンスになった

---

## 他のシーンで使う手順

### 使いたいシーンを開く

File → Open Scene で別のシーンを開く。

### PrefabをHierarchyにドラッグ

Projectウィンドウの `Assets/_Prefabs/DialogueSystem.prefab` を
Hierarchyウィンドウにドラッグするだけ。

```
これだけで
  - Canvasの階層構造
  - RectTransformの数値
  - ScenarioExecutorの参照設定
  - 色設定
がすべてコピーされた状態で置かれる。
```

---

## Prefabを修正する方法

修正には2つのルートがある。

### ルートA: Prefabファイルを直接開いて編集（推奨）

Projectウィンドウの `DialogueSystem.prefab` をダブルクリック。
→ シーンから切り離された**Prefab編集モード**に入る（画面上部にPrefab名が表示される）。
→ ここで変えた内容は**自動的に全インスタンスに反映**される。
→ 左上の「＜」ボタンでシーンに戻る。

### ルートB: シーン上のインスタンスを編集して反映

シーン上のインスタンスを変更 → Inspectorの上部に **「Overrides」** ボタンが出る
→ クリック → **「Apply All」** を押すと設計図に書き戻される。

```
注意:
  「Apply All」しないと変更はそのシーンだけに残り、他のシーンには反映されない。
  修正したのに他のシーンで反映されていないときは Apply を忘れていることが多い。
```

---

## このプロジェクトでのPrefab一覧

| Prefab名 | 場所 | 用途 |
|---------|------|------|
| `DialogueSystem.prefab` | `Assets/_Prefabs/` | 会話UI一式（これから作る） |
| `ChoiceButton_Prefab.prefab` | `Assets/_Prefabs/` | 選択肢ボタン（作成済み） |
| `FadeImage.prefab` | `Assets/_Prefabs/` | フェード用黒画面（作成済み） |

---

## よくある失敗

### 「他のシーンに置いたら参照が全部空欄になった」

原因: ScenarioExecutorの参照登録が終わる**前**にPrefab化した。

対処:
1. TownSceneのインスタンスで参照を登録し直す
2. Overrides → Apply All で設計図に書き戻す

---

### 「シーンで色を変えたのに他のシーンに反映されない」

原因: Apply Allしていない。

対処: Overrides → Apply All

---

### 「Prefabを編集したのにシーンが変わっていない」

原因: Prefab編集モードではなく、シーン上のGameObjectを直接変えてしまった（Apply忘れ）。

対処: Prefabファイルをダブルクリックして編集モードで変更する。

---

## まとめ

```
作業フロー

1. TownSceneで DialogueSystem を組む
   └ Canvas・スクリプト参照・色 を全部設定

2. _Prefabs/ にドラッグ → Prefab化

3. 他のシーン（PartnerHouseなど）には
   Prefabをドラッグするだけ

4. 修正が必要になったら
   Prefabをダブルクリック → 編集 → 戻る
   （全シーンに自動反映）
```
