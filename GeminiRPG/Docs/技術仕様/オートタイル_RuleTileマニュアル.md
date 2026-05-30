# オートタイル → Unity Rule Tile 変換マニュアル

> 対象読者: Claude Code（将来のセッションで同じ作業を再現するための参照）  
> 作成: 2026-04-23  
> プロジェクト: 流転のジェミニ（GeminiRPG）

---

## 目次

1. [全体の仕組み（何をやっているか）](#1-全体の仕組み)
2. [ファイル構成と役割](#2-ファイル構成と役割)
3. [Unity 6 の座標系（重要）](#3-unity-6-の座標系重要)
4. [RPG Maker タイルの種類と処理方法](#4-rpg-maker-タイルの種類と処理方法)
5. [A2タイル（床・水面）の処理](#5-a2タイル床水面の処理)
6. [A1タイル（水・アニメーション）の処理](#6-a1タイル水アニメーションの処理)
7. [A4タイル（壁）の処理](#7-a4タイル壁の処理)
8. [Rule Tile JSON の書式](#8-rule-tile-json-の書式)
9. [よくあるバグと対処法](#9-よくあるバグと対処法)
10. [実際の作業コマンド手順](#10-実際の作業コマンド手順)

---

## 1. 全体の仕組み

```
RPG Maker の画像(PNG)
      ↓  generate_autotile.py  （Python）
個別スプライト + rules.json
      ↓  Unity Ctrl+R でリフレッシュ
      ↓  GeminiRPG > Generate ALL Rule Tiles (Batch)  （Unity C#）
Rule Tile アセット（.asset）
      ↓  タイルマップに配置
自動的に隣接するタイルに合わせた絵が表示される
```

**ポイント**: RPG Maker のタイル画像は「1枚の大きな画像にたくさんのパターンが詰め込まれている」。Pythonスクリプトがそれを切り出してUnityが理解できる個別ファイルに変換する。

---

## 2. ファイル構成と役割

| ファイル | 役割 |
|---|---|
| `generate_autotile.py` | RPG Maker画像 → スプライト+JSON を生成するPythonスクリプト |
| `Assets/Scripts/Editor/RuleTileGenerator.cs` | JSON → Unity Rule Tile (.asset) を生成するUnityエディタ拡張 |
| `Assets/Art/Tilemap/Tiles/Inside/Autotile_*/` | 生成されたスプライト群（.png）と rules.json |
| `Assets/Art/Tilemap/Tiles/Town/Autotile_*/` | 同上（町セット） |

### 入力画像の対応表

| RPG Maker画像ファイル | タイル種別 | --prefix | 格納先 |
|---|---|---|---|
| `fsm_Inside01_A2.png` | A2（床） | `inside` | `Tiles/Inside/` |
| `fsm_Inside01_A1.png` | A1（水・草） | `inside_a1` | `Tiles/Inside/` |
| `fsm_Inside01_A4.png` | A4（壁） | `inside_a4` | `Tiles/Inside/` |
| `fsm_Town01_A2.png` | A2（床） | `town` | `Tiles/Town/` |
| `fsm_Town01_A4.png` | A4（壁） | `town_a4` | `Tiles/Town/` |

---

## 3. Unity 6 の座標系（重要）

**Unity 6 の2Dタイルマップは標準Y座標（Y+ = 視覚的に上）**

```
上方向 = Y+
下方向 = Y-
```

Rule Tile の `neighbors` 配列のインデックスは以下の順番:

```
インデックス: [TL=0, T=1, TR=2, L=3, R=4, BL=5, B=6, BR=7]

配置イメージ:
  TL  T  TR       0  1  2
   L  ■   R   →  3  ■  4   ■ = 自分のタイル
  BL  B  BR       5  6  7
```

- `T (index=1)` = Y+1 = **視覚的に上**のセル
- `B (index=6)` = Y-1 = **視覚的に下**のセル

### 値の意味

| 値 | 意味 |
|---|---|
| `0` | don't care（このセルは何でもよい → RuleTileGeneratorでスキップされる） |
| `1` | This（同じタイルが隣にある） |
| `2` | NotThis（違うタイルが隣にある） |

> ⚠️ **重要な罠**: Unity 6では `m_NeighborPositions` に 0（don't care）のポジションを含めると「EmptyTile必須」と誤判定される。`RuleTileGenerator.cs` は `neighbors[i] != 0` のときのみリストに追加するよう実装済み。

---

## 4. RPG Maker タイルの種類と処理方法

### シート構造

RPG MakerのA2/A1シートは幅768px固定。グループ（1種類のタイル）は96×144pxの単位。

```
┌──┬──┬──┬──┬──┬──┬──┬──┐  ← 横8グループ (各96px)
│00│01│02│03│04│05│06│07│
├──┼──┼──┼──┼──┼──┼──┼──┤  ← 縦4行 (各144px)
│08│09│10│11│12│13│14│15│  = 合計32グループ
├──┼──┼──┼──┼──┼──┼──┼──┤
│16│17│...                │
├──┼──┼──┼──┼──┼──┼──┼──┤
│24│25│...                │
└──┴──┴──┴──┴──┴──┴──┴──┘
```

A4シートは幅768px × 高さ720px。グループは96×240px（天井144px + 壁96px）。

### タイル種別ごとの処理関数

| 種別 | 処理 | 生成スプライト数 |
|---|---|---|
| A2（通常床） | `generate_group()` | 31枚（16基本 + 15内角） |
| A1（水・アニメーション） | `generate_group()` | 31枚（同上） |
| A1（草・スイレン等） | `generate_simple_tiles()` | 6枚（そのまま出力） |
| A4天井部 | `generate_group()` | 31枚 |
| A4壁面部 | `generate_wall_face()` | 16枚（4段×4水平パターン） |

---

## 5. A2タイル（床・水面）の処理

### クォーター合成方式

A2タイルのグループには6枚（2×3）のソースタイルが入っている。それぞれの48×48タイルを24×24の「クォーター」（4分割）に切り出して組み合わせることで、31種類のパターンを作る。

```
グループ内の6タイル配置（96×144px）:
┌──────┬──────┐
│tile_0│tile_1│  row0 (y=0  ～47)
├──────┼──────┤
│tile_2│tile_3│  row1 (y=48 ～95)
├──────┼──────┤
│tile_4│tile_5│  row2 (y=96 ～143)
└──────┴──────┘

各タイルを24×24の4分割に切り出す:
  (col,row) = グループ内24px単位オフセット
  col=0,row=0 = tile_0左上
  col=1,row=0 = tile_0右上
  col=0,row=1 = tile_0左下
  col=1,row=1 = tile_0右下
  col=2,row=0 = tile_1左上 ...
```

### ビットマスク（16基本パターン）

```
ビット: bit0=N(上), bit1=E(右), bit2=S(下), bit3=W(左)
 0 = 孤立
 1 = N
 2 = E
 3 = N+E
... (15まで)
15 = N+E+S+W（全接続、内部タイル）
```

### 内角コンボ（15枚）

全4方向が接続している状態（pattern_15）で、斜め方向（NW/NE/SW/SE）に異なるタイルがある場合の15パターン。`ic_`プレフィックスのスプライトとして生成される。

**JSON内の順番**: 内角コンボ（先）→ 基本16パターン（後）  
Rule Tileはリストの先頭から評価して最初にマッチしたルールを使うため、より具体的なルール（内角）を先に書く。

---

## 6. A1タイル（水・アニメーション）の処理

A1シートのグループは2種類に分かれる。

### 種類A: 通常A1（水・滝・沼など）

`generate_group()` でA2と同じクォーター合成。

例: `inside_water_pool`, `inside_waterfall`, `inside_dark_void`

### 種類B: シンプルタイル（草・スイレン葉など）

`generate_simple_tiles()` で6枚のソースタイルをそのまま出力。

> **なぜ分けるか**: スイレンの葉は直径35px程度の丸い画像。これを24×24に切るとバラバラな断片になる。6枚のソースタイルはどれも似た内容（完全な葉の画像）なので、そのままランダム出力で使うのが正解。

**シンプルタイルに該当するグループ**（`INSIDE_A1_SIMPLE` 定数で管理）:
```python
INSIDE_A1_SIMPLE = {3, 11}  # グループ3=inside_a1_grass, 11=inside_a1_grass_2
```

新しいシンプルタイルを追加する場合はここにインデックスを追加する。

### シンプルタイルの出力

```
src0.png ～ src5.png  （有効ピクセルがあるもののみ）
<name>_rules.json
```

JSONのルール:
```json
{
  "sprites": ["tile_src0", "tile_src1", ...],
  "neighbors": [0,0,0,0,0,0,0,0],
  "output": "random"
}
```

Unity Rule Tileは配置時に `sprites` の中からランダムに1枚を選ぶ。

---

## 7. A4タイル（壁）の処理

### A4グループの構造

A4の各グループは240px高（他と異なる）:

```
oy+0 ～ oy+143  (144px): 天井部分 → generate_group() で31スプライト
oy+144 ～ oy+239 (96px): 壁面部分 → generate_wall_face() で16スプライト
```

### 壁面部分の構造（96px = 2行×48px）

```
oy+144: 行0（48px）= 壁の上端タイル。棚や梁の装飾が入る。
oy+192: 行1（48px）= 壁のボディ。無地の繰り返し部分。
```

水平方向は2タイル（96px）:
- 左タイル（0 ～ 47px）: 左端用（左側に柱エッジ）
- 右タイル（48 ～ 95px）: 右端用（右側に柱エッジ）

### 生成される16種のスプライト

**4段 × 4水平パターン = 16枚**

#### 4つの垂直位置

| スプライト名 | T（上）| B（下）| 意味 |
|---|---|---|---|
| `wall_top_*` | `2` (NotThis) | `1` (This) | 壁の視覚的最上段（棚/梁つき） |
| `wall_body_*` | `1` (This) | `1` (This) | 壁の中間段（繰り返し） |
| `wall_bot_*` | `1` (This) | `2` (NotThis) | 壁の視覚的最下段（下縁） |
| `wall_mid` / `wall_iso` | `2` (NotThis) | `2` (NotThis) | 孤立1段（上下とも異なる） |

> **wall_topの上にあるセルは何か**: 天井タイルや空（何もない）= NotThis (2)  
> **wall_botの下にあるセルは何か**: 床タイルや空 = NotThis (2)

#### 4つの水平パターン

| サフィックス | L（左）| R（右）| 意味 |
|---|---|---|---|
| `_mid` | `1` (This) | `1` (This) | 左右とも同タイル（中央） |
| `_l` | `2` (NotThis) | `1` (This) | 左が異（左端） |
| `_r` | `1` (This) | `2` (NotThis) | 右が異（右端） |
| `_iso` | `2` (NotThis) | `2` (NotThis) | 孤立 |

#### スプライトの作り方

```
wall_top_mid  = 行0の中央部分（left右半 + right左半）
wall_top_l    = 行0の left_top そのまま（左端の柱エッジ入り）
wall_top_r    = 行0の right_top そのまま（右端の柱エッジ入り）
wall_top_iso  = 行0の孤立（left左半 + right右半）

wall_body_*   = (行0下半24px + 行1上半24px) から同様に構成
               ← 上のwall_topと下のwall_topがシームレスに繋がるため

wall_bot_mid  = 行1の中央部分（left右半 + right左半）
wall_bot_l    = 行1の left_body そのまま
wall_bot_r    = 行1の right_body そのまま
wall_bot_iso  = 行1の孤立
```

> **wall_bodyの合成理由**: 同じwall_bodyが上下に続く場合、上のwall_topの「底」と下のwall_topの「頭」が重なって見える場所がbodyになる。行0下半（棚/梁の下端）と行1上半（ボディの上端）を合成することで継ぎ目なく繋がる。

---

## 8. Rule Tile JSON の書式

### 通常のA2/A4タイル（単一スプライト）

```json
{
  "name": "inside_wood_brown",
  "tileSize": 48,
  "rules": [
    {
      "sprite": "inside_wood_brown_ic_all",
      "neighbors": [2, 1, 2, 1, 1, 2, 1, 2]
    },
    {
      "sprite": "inside_wood_brown_15",
      "neighbors": [0, 1, 0, 1, 1, 0, 1, 0]
    }
  ]
}
```

- `sprite`: スプライト名（ファイル名からの.pngを除いたもの）
- `neighbors`: 8方向の条件 [TL, T, TR, L, R, BL, B, BR]
- `output` フィールドは省略可（省略時は single 扱い）

### ランダム出力タイル（シンプルタイル）

```json
{
  "name": "inside_a1_grass",
  "tileSize": 48,
  "rules": [
    {
      "sprites": ["inside_a1_grass_src0", "inside_a1_grass_src1", ...],
      "neighbors": [0, 0, 0, 0, 0, 0, 0, 0],
      "output": "random"
    }
  ]
}
```

- `sprites`（複数形）: スプライト名の配列
- `output: "random"`: Unity Rule Tile の Random モードを使用

### RuleTileGenerator.cs が JSON を解釈するロジック

```
output == "random" && sprites != null && sprites.Length > 0
  → OutputSprite.Random、sprites配列からすべてロード
それ以外
  → OutputSprite.Single、sprite（単数形）をロード
```

---

## 9. よくあるバグと対処法

### バグ1: 壁の棚/梁が下に来る（上下逆）

**症状**: 壁の一番上の段に棚や梁が出るはずが、一番下に出る。  
**原因**: neighbors配列のT(上)とB(下)が逆になっている。  
**診断**: wall_top の neighbors を確認。`T=2（index 1）, B=1（index 6）` が正解。逆なら修正。  

```python
# 正解
("wall_top_mid",  top_mid, [0, 2, 0, 1, 1, 0, 1, 0]),
#                                ^ T=2 (上=異)        ^ B=1 (下=同)
```

### バグ2: 壁の最下段がbodyと同じ見た目になる

**症状**: 壁の一番下の段がbodyタイルと同じ見た目で、下縁が表示されない。  
**原因**: wall_botスプライトとしてbody_mid（行0下半+行1上半の合成）を使っていた。  
**修正**: wall_botには行1（bottom_mid = make_mid(left_body, right_body)）を使う。

```python
# 正解
bot_mid = make_mid(left_body, right_body)   # 行1のみで構成
("wall_bot_mid", bot_mid, [0, 1, 0, 1, 1, 0, 2, 0]),
```

### バグ3: A1草・スイレンがバラバラな形になる

**症状**: スイレンの葉が欠けた断片や意味不明な形のパターンとして表示される。  
**原因**: A2クォーター合成で24×24に切ると、35px程度の円形グラフィックが分断される。  
**修正**: `INSIDE_A1_SIMPLE` に対象グループのインデックスを追加して `generate_simple_tiles()` を使う。

### バグ4: don't care(0)を含む位置でタイルが表示されない

**症状**: 特定の条件でタイルが表示されず空白になる。  
**原因**: Unity 6 では `m_NeighborPositions` にポジションを追加すると「そのタイルが必要」と解釈される。値0のポジションが含まれていると「EmptyTile必須」と誤判定。  
**修正**: `RuleTileGenerator.cs` の以下の処理を確認（実装済み）:

```csharp
if (rule.neighbors[i] != 0)  // 0(don't care)はスキップ
{
    tilingRule.m_NeighborPositions.Add(allPositions[i]);
    tilingRule.m_Neighbors.Add(rule.neighbors[i]);
}
```

### バグ5: Rule Tile アセットが古いまま（スプライト変更が反映されない）

**症状**: Pythonスクリプトで画像を更新したが、タイルマップの表示が変わらない。  
**対処**:
1. Unity で `Ctrl+R`（または `Assets > Refresh`）
2. 古い Rule Tile .asset を削除
3. `GeminiRPG > Generate ALL Rule Tiles (Batch)` を実行

バッチ処理は既存の .asset がある場合スキップするため、削除してから実行する必要がある。

---

## 10. 実際の作業コマンド手順

### 基本コマンド形式

```bash
python3 generate_autotile.py \
    "<入力PNG>" \
    "<出力ルートディレクトリ>" \
    --prefix <プレフィックス> \
    [--groups "0,1,3-5"]  # 特定グループのみ再生成する場合
```

### よく使うコマンド一覧

```bash
# 内装A2（床）全グループ生成
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Inside01_A2.png" \
    "Assets/Art/Tilemap/Tiles/Inside" \
    --prefix inside

# 内装A1（水・草）全グループ生成
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Inside01_A1.png" \
    "Assets/Art/Tilemap/Tiles/Inside" \
    --prefix inside_a1

# 内装A1 特定グループのみ（スイレン草グループ3と11）
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Inside01_A1.png" \
    "Assets/Art/Tilemap/Tiles/Inside" \
    --prefix inside_a1 \
    --groups "3,11"

# 内装A4（壁）全グループ生成
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Inside01_A4.png" \
    "Assets/Art/Tilemap/Tiles/Inside" \
    --prefix inside_a4

# 内装A4 特定グループのみ再生成
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Inside01_A4.png" \
    "Assets/Art/Tilemap/Tiles/Inside" \
    --prefix inside_a4 \
    --groups "0,1,2"

# 町A4（外壁）全グループ生成
python3 generate_autotile.py \
    "Assets/Art/Tilemap/Tilesets/Town_Departure/fsm_Town01_A4.png" \
    "Assets/Art/Tilemap/Tiles/Town" \
    --prefix town_a4
```

### Unity側の操作手順

```
1. Python実行後
2. Unityウィンドウをクリック（フォーカス） or Ctrl+R でリフレッシュ
3. 再生成したいRule Tileがある場合は既存の .asset ファイルを削除
   （Projectウィンドウで Autotile_*/〇〇_RuleTile.asset を選択してDelete）
4. メニュー: GeminiRPG > Generate ALL Rule Tiles (Batch)
5. 完了ダイアログで「新規作成 N件、スキップ M件」を確認
```

### グループインデックスの確認方法

どのインデックスがどのタイルか確認するには `INSIDE_A1_NAMES`、`INSIDE_A4_NAMES` などの辞書を `generate_autotile.py` 内で参照。または実際に画像を見てグループを数える（左上から横方向に0,1,2...）。

---

## 付録: A4壁面スプライト全16種の neighbors 早見表

```
インデックス: [TL, T, TR, L, R, BL, B, BR]

最上段:
  wall_top_mid:  [0, 2, 0, 1, 1, 0, 1, 0]  左右あり・真ん中
  wall_top_r:    [0, 2, 0, 1, 2, 0, 1, 0]  右端
  wall_top_l:    [0, 2, 0, 2, 1, 0, 1, 0]  左端
  wall_top_iso:  [0, 2, 0, 2, 2, 0, 1, 0]  孤立

中間段:
  wall_body_mid: [0, 1, 0, 1, 1, 0, 1, 0]
  wall_body_r:   [0, 1, 0, 1, 2, 0, 1, 0]
  wall_body_l:   [0, 1, 0, 2, 1, 0, 1, 0]
  wall_body_iso: [0, 1, 0, 2, 2, 0, 1, 0]

最下段:
  wall_bot_mid:  [0, 1, 0, 1, 1, 0, 2, 0]
  wall_bot_r:    [0, 1, 0, 1, 2, 0, 2, 0]
  wall_bot_l:    [0, 1, 0, 2, 1, 0, 2, 0]
  wall_bot_iso:  [0, 1, 0, 2, 2, 0, 2, 0]

孤立段（1段だけの壁）:
  wall_mid:      [0, 2, 0, 1, 1, 0, 2, 0]
  wall_r:        [0, 2, 0, 1, 2, 0, 2, 0]
  wall_l:        [0, 2, 0, 2, 1, 0, 2, 0]
  wall_iso:      [0, 2, 0, 2, 2, 0, 2, 0]
```

---

*作成: Claude Code (claude-sonnet-4-6) — 2026-04-23*  
*このファイルは将来のClaude Codeセッションが同じ作業を再現するための参照マニュアルです。*
