# Unity 2Dタイルマップ作成 完全マニュアル

## 目次

1. [クイックチェックリスト](#クイックチェックリスト毎回やることリスト)
2. [プロジェクトの準備](#1-プロジェクトの準備)
3. [タイル素材のインポート](#2-タイル素材のインポート)
4. [Tile Paletteの作成](#3-tile-paletteの作成)
5. [Tilemapの配置とレイヤー設計](#4-tilemapの配置とレイヤー設計)
6. [タイルの描画](#5-タイルの描画)
7. [当たり判定(コライダー)](#6-当たり判定コライダー)
8. [Rule Tile(自動境界タイル)](#7-rule-tile自動境界タイル)
9. [アニメーションタイル](#8-アニメーションタイル)
10. [カメラの設定](#9-カメラの設定)
11. [スクリプトからタイルを動的に操作する](#11-スクリプトからタイルを動的に操作する)
12. [ScriptableTileでタイルにデータを持たせる](#12-scriptabletileでタイルにデータを持たせる)
13. [Tiled連携](#13-tiled連携)
14. [その他できること](#14-その他できること)
15. [よくあるトラブルと対処](#15-よくあるトラブルと対処)
16. [ショートカット一覧](#16-ショートカット一覧)
17. [新規マップ作成チェックリスト](#17-新規マップ作成チェックリスト)
18. [付録A: レンダーパイプライン(SRP/URP/HDRP)について](#付録a-レンダーパイプラインsrpurphdrpについて)

---

## クイックチェックリスト(毎回やることリスト)

新しいマップを作るたびに、このリストを上から順にこなせば完成します。詳細はそれぞれの章を参照してください。

- [ ] Package Managerで「2D Tilemap Editor」「2D Tilemap Extras」の有無を確認
- [ ] Project Settingsで「Pixels Per Unit」の基準値を決める
- [ ] 素材PNGをAssetsにインポート
- [ ] Inspectorで Sprite Mode / PPU / Filter Mode / Compression を設定
- [ ] Sprite EditorでSliceして分割
- [ ] Window > 2D > Tile Paletteでパレット作成
- [ ] 分割済みスプライトをパレットにドラッグしてタイルアセット生成
- [ ] HierarchyでGrid + Tilemapを配置
- [ ] レイヤーを3~4枚に分割(背景 / 地面 / 前景 / オブジェクト)
- [ ] 各Tilemapの Sorting Layer と Order in Layer を設定
- [ ] 地面レイヤーに Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D(Static)
- [ ] タイルを描く
- [ ] カメラのProjectionとSizeを調整
- [ ] 必要ならPixel Perfect Cameraを追加
- [ ] Playで動作確認

---

## 1. プロジェクトの準備

### パッケージの確認

`Window > Package Manager`を開き、左上のドロップダウンを「Unity Registry」に切り替えて以下を確認します。

- **2D Tilemap Editor**: Tilemap機能の本体です。2Dテンプレートで作成したプロジェクトには最初から入っています
- **2D Tilemap Extras**: Rule TileやAnimated Tileなど拡張機能が入っています。これは自分で追加する必要があることが多いです
- **2D Pixel Perfect**(任意): ピクセルアートをきれいに表示したい場合に追加します

入っていないパッケージは選択して「Install」を押せば追加されます。

### プロジェクト全体の設定を揃える

あとで苦しまないために、最初にプロジェクト全体のルールを決めておきます。

`Edit > Project Settings > Editor`で以下を確認します。

- **Default Behavior Mode**: 「2D」にする
- **Sprite Packer**: 必要に応じて「Sprite Atlas V2」

`Edit > Project Settings > Quality`にアンチエイリアス設定があります。ピクセルアートの場合は切っておくと余計なぼかしが入りません。

ただし実際の設定場所は使っているレンダーパイプラインによって変わります(詳細は**付録A**を参照してください)。

- **Built-in Render Pipeline**: Project Settings > Qualityの「Anti Aliasing」項目
- **URP(Universal Render Pipeline)**: URP Assetの「Anti Aliasing (MSAA)」とCameraの「Anti-aliasing」の2か所
- **HDRP**: Cameraの Frame Settings

新しめのUnityで新規2Dプロジェクトを作ると、デフォルトでURPが使われていることが多いです。その場合はProject Settings > Qualityに「A Scriptable Render Pipeline is in use」という警告が出て、アンチエイリアス関連の項目が非表示になります。

---

## 2. タイル素材のインポート

### PNGをAssetsに入れる

エクスプローラから直接Assetsフォルダにドラッグするか、Projectビューにドロップします。素材フォルダは`Assets/Sprites/Tiles`のように切っておくと後で探しやすくなります。

### Inspectorでの設定(重要)

インポートしたPNGを選択して、Inspectorで以下を設定します。

| 項目                | 設定値                                              | 理由                               |
| ------------------- | --------------------------------------------------- | ---------------------------------- |
| **Texture Type**    | Sprite (2D and UI)                                  | 2Dとして扱うため                   |
| **Sprite Mode**     | Multiple(1枚に複数タイル)/ Single(1タイル1ファイル) | 素材の構成による                   |
| **Pixels Per Unit** | タイルのピクセル数に合わせる(例: 16)                | プロジェクト全体で統一する         |
| **Mesh Type**       | Full Rect                                           | タイル用途ではこちらが安定         |
| **Filter Mode**     | Point (no filter)                                   | ピクセルアートの場合。ぼやけを防ぐ |
| **Compression**     | None                                                | 画質劣化を防ぐ                     |
| **Wrap Mode**       | Clamp                                               | タイル境界の色漏れを防ぐ           |

設定を変えたら、Inspectorの下にある「Apply」を必ず押します。押し忘れると反映されません。

### Sprite Editorでスライス

Sprite Modeが「Multiple」の場合、Inspectorの「Sprite Editor」ボタンを押してエディタを開きます。

1. 左上の「Slice」をクリックします
2. Typeを「Grid By Cell Size」にします
3. Pixel Sizeにタイル1枚のサイズ(例: X=16, Y=16)を入力します
4. Offsetが必要な場合(素材に余白や枠線がある場合)は設定します
5. 「Slice」を押します
6. 右上の「Apply」を押して確定します

Applyを押し忘れると分割結果が保存されません。ここは特に忘れやすいポイントです。

---

## 3. Tile Paletteの作成

### パレットウィンドウを開く

`Window > 2D > Tile Palette`でパレットウィンドウが開きます。作業中は常に表示しておくので、タブとしてSceneビューの隣などに固定しておくと便利です。

### 新規パレットの作成

1. パレットウィンドウ上部の「Create New Palette」ボタンを押します
2. Nameに適当な名前を入力します(例: `MainPalette`)
3. Gridは通常「Rectangular」でOKです
4. Cell Sizeは「Automatic」で問題ありません
5. 「Create」を押すと保存先を聞かれるので、`Assets/Palettes`のようなフォルダを指定します

### タイルアセットの生成

分割済みのスプライト、またはスプライト入りのPNGをパレットウィンドウ内にドラッグします。保存先フォルダを聞かれるので、`Assets/Tiles`のようなフォルダを指定します。

これでPNGから分割された各スプライトごとに`.asset`ファイル(Tileアセット)が生成されます。以降はこの`.asset`がタイルとして使われます。

---

## 4. Tilemapの配置とレイヤー設計

### シーンにTilemapを配置

Hierarchyで右クリック→`2D Object > Tilemap > Rectangular`を選びます。これでGridオブジェクトと、その子としてTilemapが1枚作られます。

### 推奨レイヤー構成

規模にもよりますが、最低でも以下のように分けておくと後で困りません。

1. **BG_Far**: 遠景(雲、背景の山など。当たり判定なし)
2. **BG_Near**: 近景背景(建物の奥など。当たり判定なし)
3. **Ground**: 地面、壁(当たり判定あり)
4. **Platform**: すり抜け床(片方向だけ当たり判定)
5. **Foreground**: 前景(木の葉、手前の柱など。当たり判定なし)

### レイヤーの追加方法

Gridオブジェクトを右クリック→`2D Object > Tilemap`で子のTilemapを追加します。名前は用途がわかるように変えます(例: `Tilemap_Ground`)。

### Sorting Layerの設定

`Edit > Project Settings > Tags and Layers`でSorting Layerを事前に作っておきます。上から下の順が描画順(下にあるものが手前に表示される)なので、以下のように並べます。

1. Default
2. BG_Far
3. BG_Near
4. Ground
5. Player(プレイヤーなどのキャラクター用)
6. Foreground

各Tilemapを選んで、Tilemap Rendererコンポーネントの「Sorting Layer」を対応するものに、「Order in Layer」で細かい順序を調整します。

---

## 5. タイルの描画

### Active Tilemapの確認

パレットウィンドウ上部に「Active Tilemap」という項目があります。ここで指定したTilemapに対して描画されます。

描く前に必ずここを確認してください。間違ったレイヤーに描いてしまう事故の原因の大半はここの確認ミスです。

### 描画ツール

パレットウィンドウ上部のアイコンで切り替えます。

| ツール | ショートカット | 用途                        |
| ------ | -------------- | --------------------------- |
| Select | S              | タイルを選択                |
| Move   | M              | 選択したタイルを移動        |
| Brush  | B              | 1枚ずつ描画                 |
| Box    | U              | 四角形範囲を塗る            |
| Picker | I              | 既に置かれたタイルを拾う    |
| Eraser | D              | 消しゴム(Shift+Brushでも可) |
| Fill   | G              | 塗りつぶし                  |

### 描画の基本操作

- **クリック**: タイルを1枚置く
- **ドラッグ**: ドラッグした範囲に連続で置く
- **Shift + クリック / ドラッグ**: タイルを消す
- **Ctrl + Z**: アンドゥ(通常のUnity操作と同じ)

---

## 6. 当たり判定(コライダー)

### 基本の3点セット

当たり判定が必要なTilemap(Groundレイヤーなど)には、以下の3つのコンポーネントをセットで付けます。

1. **Tilemap Collider 2D**
   - タイル1枚ごとにコライダーを生成します
   - ただしこのままだと隣接するタイルの境界でプレイヤーが引っかかる原因になるので、次のCompositeと組み合わせます

2. **Composite Collider 2D**
   - Tilemap Collider 2Dの「Used By Composite」にチェックを入れると、隣接するコライダーがひとつに統合されます
   - 境界の引っかかりが解消され、パフォーマンスも向上します

3. **Rigidbody 2D**
   - Composite Collider 2Dが要求するため自動で追加されます
   - **Body Typeを必ず「Static」にします**。Dynamicのままだと地面が重力で落下します

### 特殊形状のタイル(坂道など)

生成したタイルアセット(`.asset`)を選択し、Inspectorで以下を設定します。

- **Collider Type**: Sprite(スプライトの形に合わせる)/ Grid(タイル全体)/ None(当たり判定なし)を選べます
- さらに細かく形を指定したい場合は、Sprite EditorのCustom Physics Shapeでポリゴンを編集します

### すり抜け床(Platform)

上からは乗れて下からはすり抜けたい床の場合、別Tilemapに分離した上で、Platform Effector 2Dを使うか、コライダーのIs Triggerとスクリプトで制御します。

---

## 7. Rule Tile(自動境界タイル)

### Rule Tileとは

隣接するタイルの配置に応じて、自動で見た目のスプライトを切り替えてくれるタイルです。地面の角や縁取りの絵を自動選択してくれるので、描画作業が大幅に楽になります。

2D Tilemap Extrasパッケージが必要です。

### 作成手順

1. Projectビューで右クリック→`Create > 2D > Tiles > Rule Tile`
2. 生成された`.asset`を選択
3. Inspectorの「Default Sprite」にフォールバック用のスプライトを指定
4. 「Tiling Rules」の「+」ボタンでルールを追加
5. 各ルールで、中央セルを中心とした3×3の枠の各マスを以下のいずれかに設定します
   - **緑の矢印**: ここには同じRule Tileが接している必要がある
   - **赤のX**: ここには同じRule Tileが接していてはいけない
   - **何もなし**: どちらでもよい
6. 条件が揃ったときに使うスプライトをルールごとに指定

### よく使うパターン

16パターンの地形用テンプレートはインターネット上に多数公開されています。検索キーワード: `unity rule tile 16 template`

最初は既存テンプレートを真似して作り、慣れたら独自ルールを足していくのが効率的です。

---

## 8. アニメーションタイル

### 作成手順

1. Projectビューで右クリック→`Create > 2D > Tiles > Animated Tile`
2. Inspectorの「Number of Animated Sprites」でフレーム数を指定
3. 各フレームにスプライトをドラッグして登録
4. 「Minimum Speed」「Maximum Speed」でアニメ速度を指定(単位: フレーム/秒)

### 使いどころ

水面の揺らぎ、松明の炎、キラキラ光るアイテム、動く機械パーツなどに使います。Rule Tileの中にAnimated Tileを埋め込むこともできます(条件に応じてアニメタイルを表示)。

---

## 9. カメラの設定

### 基本設定

Main Cameraを選択し、以下を確認します。

- **Projection**: Orthographic(2Dの標準)
- **Size**: 表示したい縦方向の半分の長さ(ワールド単位)。例えば縦180ピクセル表示したくてPPU=16なら、Size = 180 / 32 = 5.625
- **Clear Flags**: Solid Color または Skybox
- **Background**: 背景色

### Pixel Perfect Camera(ピクセルアート向け)

2D Pixel Perfectパッケージを入れている場合、Main Cameraに「Pixel Perfect Camera」コンポーネントを追加します。

- **Assets Pixels Per Unit**: プロジェクトのPPUと揃える
- **Reference Resolution**: 基準解像度(例: 320×180)
- **Upscale Render Texture**: チェックを入れると拡大時のブレが減る
- **Pixel Snapping**: チェックを入れるとスプライトがピクセルグリッドに吸着する

### カメラ追従

Cinemachineパッケージを使うのが一般的です。

1. Package ManagerでCinemachineをインストール
2. メニューの`Cinemachine > Create 2D Camera`で仮想カメラを作成
3. 「Follow」にプレイヤーを指定
4. 必要に応じてConfinerコンポーネントでカメラの移動範囲を制限

---

## 11. スクリプトからタイルを動的に操作する

実行中にタイルを書き換えると、破壊可能な地形、スイッチ連動の扉、建築システムなどを実装できます。ここではAPIの基本と、よく使う実装パターンを押さえます。

### 必要なusing

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
```

### Tilemapへの参照を取得する

スクリプトのフィールドとして持ち、Inspectorから割り当てるのが基本です。

```csharp
[SerializeField] private Tilemap groundTilemap;
[SerializeField] private TileBase destructibleTile;
```

動的に取得したい場合は`GameObject.FindWithTag("Ground").GetComponent<Tilemap>()`などを使いますが、Inspector割り当ての方が事故が少なく済みます。

### 座標の変換

Tilemapはセル座標(整数のVector3Int)で管理しますが、プレイヤーや弾の位置はワールド座標(小数のVector3)です。両者を変換する関数を覚えておきます。

```csharp
// ワールド座標 → セル座標
Vector3Int cellPos = groundTilemap.WorldToCell(worldPosition);

// セル座標 → ワールド座標(セルの左下)
Vector3 worldPos = groundTilemap.CellToWorld(cellPos);

// セルの中心座標
Vector3 center = groundTilemap.GetCellCenterWorld(cellPos);
```

### タイルの取得・設置・削除

```csharp
// 指定座標のタイルを取得(無ければnull)
TileBase tile = groundTilemap.GetTile(cellPos);

// タイルを設置
groundTilemap.SetTile(cellPos, myTile);

// タイルを削除(nullを設定)
groundTilemap.SetTile(cellPos, null);

// 複数をまとめて書き換え(高速)
Vector3Int[] positions = { ... };
TileBase[] tiles = { ... };
groundTilemap.SetTiles(positions, tiles);
```

大量に書き換える場面では、`SetTile`を1つずつ呼ぶより`SetTiles`や`SetTilesBlock`でまとめた方が圧倒的に速くなります。

### 実装例1: 弾が当たったタイルを壊す

```csharp
void OnCollisionEnter2D(Collision2D collision)
{
    Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
    if (tilemap == null) return;

    foreach (var contact in collision.contacts)
    {
        // 接触点から法線方向に少し内側を指す
        Vector3 hitPos = contact.point - contact.normal * 0.1f;
        Vector3Int cellPos = tilemap.WorldToCell(hitPos);
        tilemap.SetTile(cellPos, null);
    }
}
```

`contact.normal * 0.1f`を引くのは、接触点がセル境界ちょうどに乗ると隣のセルを指してしまう場合があるからです。

### 実装例2: プレイヤーの足元のタイルを調べる

```csharp
Vector3 feetPos = transform.position + Vector3.down * 0.5f;
Vector3Int cellPos = groundTilemap.WorldToCell(feetPos);
TileBase currentTile = groundTilemap.GetTile(cellPos);

if (currentTile == iceTile)
{
    // 氷の上なので滑る処理
}
```

タイル種類ごとに挙動を変えたい場合、次章のScriptableTileを使うとさらにきれいに書けます。

### 実装例3: 範囲を一括で書き換える

```csharp
BoundsInt area = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(10, 5, 1));
TileBase[] tiles = new TileBase[area.size.x * area.size.y * area.size.z];
for (int i = 0; i < tiles.Length; i++) tiles[i] = floorTile;
groundTilemap.SetTilesBlock(area, tiles);
```

部屋を丸ごと生成したり消去したりする場面で使います。

### コライダーの更新に注意

Composite Collider 2Dを使っている場合、タイルを書き換えてもコライダーが自動で再生成されないことがあります。必要に応じて手動で更新してください。

```csharp
groundTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
groundTilemap.GetComponent<CompositeCollider2D>().GenerateGeometry();
```

毎フレーム呼ぶと重いので、書き換えが発生したタイミングでだけ呼ぶようにします。

### セーブ/ロードでの応用

プレイヤーが改変したマップを保存したい場合、マップ全体を保存するとデータが膨大になります。「変更があったセル座標とタイルID」の辞書だけを保存し、ロード時に`SetTiles`で一括復元するのが定石です。

---

## 12. ScriptableTileでタイルにデータを持たせる

通常のTileは見た目(スプライト)しか持ちませんが、Tileクラスを継承した自作クラスを作ると、各タイルにゲームロジック用のデータを持たせられます。マップを描くだけで仕様が反映される、非常に強力な仕組みです。

### カスタムTileクラスの作成

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTerrainTile", menuName = "Tiles/Terrain Tile")]
public class TerrainTile : Tile
{
    [Header("地形パラメータ")]
    public string terrainName = "grass";
    public float moveSpeedMultiplier = 1.0f;
    public int damagePerSecond = 0;
    public bool isSlippery = false;
    public AudioClip footstepSound;
}
```

`[CreateAssetMenu]`属性を付けると、Projectビューの右クリックメニューからアセットとして生成できるようになります。

### アセットの作成と設定

Projectビューで右クリック→`Create > Tiles > Terrain Tile`で`.asset`ファイルが生成されます。InspectorでSprite(見た目)と、追加した各パラメータを設定します。

例として、以下のようなタイルを作れます。

- GrassTile: moveSpeedMultiplier=1.0、footstepSound=草の足音
- SandTile: moveSpeedMultiplier=0.7、footstepSound=砂の足音
- IceTile: moveSpeedMultiplier=1.2、isSlippery=true、footstepSound=氷の足音
- LavaTile: damagePerSecond=10、footstepSound=焼ける音

作ったタイルはTile Paletteにドラッグすれば、通常のタイルと同じように描けます。

### 実行時にデータを読む

プレイヤーのスクリプトなどから、足元のタイルを取得してキャストします。

```csharp
void Update()
{
    Vector3Int cellPos = groundTilemap.WorldToCell(transform.position);
    TileBase tile = groundTilemap.GetTile(cellPos);

    if (tile is TerrainTile terrain)
    {
        // 移動速度に係数をかける
        currentSpeed = baseSpeed * terrain.moveSpeedMultiplier;

        // 継続ダメージ
        if (terrain.damagePerSecond > 0)
        {
            TakeDamage(terrain.damagePerSecond * Time.deltaTime);
        }

        // 足音を切り替える
        currentFootstep = terrain.footstepSound;
    }
}
```

`is`パターンを使うと、型チェックとキャストが同時にでき、nullも暗黙に弾けます。

### メリット

最大の強みは、マップ編集がそのままゲームロジックになる点です。「ここは氷だから滑る」といった設定を別途スクリプトや外部テーブルで管理する必要がなくなります。

プログラマーとマップデザイナーの分業もやりやすくなります。プログラマーがタイルクラスとパラメータを定義し、デザイナーがInspectorで値を調整し、あとはマップを描くだけで仕様が反映されるからです。

### 拡張: Rule Tileにもデータを載せる

Rule Tileを継承すれば、「見た目を自動で切り替えつつデータも持つタイル」が作れます。

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewTerrainRuleTile", menuName = "Tiles/Terrain Rule Tile")]
public class TerrainRuleTile : RuleTile
{
    public float moveSpeedMultiplier = 1.0f;
    public int damagePerSecond = 0;
}
```

2D Tilemap Extrasパッケージが入っていれば`RuleTile`を継承できます。ビルドエラーが出た場合は名前空間(`UnityEngine.Tilemaps`)とパッケージの導入状況を確認してください。

---

## 13. Tiled連携

### Tiledとは

Tiledはタイルマップ専用の無料エディタで、複数のゲームエンジンで使える標準フォーマット(`.tmx`、`.tsx`)でマップを保存します。

公式サイト: https://www.mapeditor.org/

### 導入するかの判断基準

以下に当てはまるなら導入の価値があります。

- マップの数が多い、または1マップあたりの規模が大きい
- レベルデザイナーにUnityを触らせずに作業させたい
- 既にTiled形式の素材やサンプルを流用したい
- マップ上にカスタムプロパティ(敵のスポーン位置、宝箱の中身など)を多数置きたい

Unity標準のTile Paletteで困っていないなら、無理に導入する必要はありません。学習コストと得られる機能のトレードオフになります。

### セットアップ(Super Tiled2Unity使用)

Tiledで作ったマップをUnityに取り込むには、Super Tiled2Unityというインポーターを使うのが一般的です。

1. Tiled本体をインストールし、マップを作成する
2. UnityのPackage Managerで「+」→「Add package from git URL」を選び、Super Tiled2UnityのgitURLを入力する
3. Tiledで作った`.tmx`ファイル、タイルセット`.tsx`、使用している画像ファイルをUnityのAssets以下にコピーする
4. Unityが自動でインポートし、マップがPrefabとして生成される
5. 生成されたPrefabをシーンに配置する

最新のURLやインストール手順は、Super Tiled2UnityのGitHubリポジトリを参照してください(URLやバージョンが時期によって変わります)。

### 基本的なワークフロー

1. Tiled側でタイルセットを読み込み、レイヤーを分けてマップを描く
2. Tiled側でオブジェクトレイヤーに「スポーン地点」「敵」などを配置し、カスタムプロパティで種類や設定値を与える
3. `.tmx`ファイルを編集・保存すると、Unity側で再インポートが走る
4. Unity側でカスタムプロパティを読み取って、実際のGameObjectに変換するスクリプトを用意する

### カスタムプロパティの例

Tiledのオブジェクトに`type=EnemySpawn`、`enemyKind=Slime`、`count=3`のようなプロパティを設定しておくと、Unity側で「このオブジェクトはスライムを3体出すスポーン地点だ」と解釈できます。

Super Tiled2Unityでは「Custom Importers」という仕組みで、こうした変換ロジックを実装します。

### 注意点

- 当たり判定: Tiledで付けたコライダー情報はインポートされますが、期待どおりの形になるようTiled側で事前に設定しておく必要があります
- パフォーマンス: マップが大きいとインポート時間がかかります
- バージョン互換性: TiledとSuper Tiled2Unityのバージョン組み合わせによって挙動が違うことがあります
- 学習コスト: 設定項目が多く、最初の立ち上げに時間がかかります

### 代替案

導入コストが重く感じるなら、以下の選択肢も検討してください。

- **Tilemap Prefab Brush**: Unity標準Tilemapの拡張で、「複数タイル+オブジェクトをひとまとめにしたブラシ」を定義できます。再利用したい小単位のパターンがある場合はこれで足りることが多いです
- **LDtk**: Tiledと似たコンセプトの別エディタで、LDtkToUnityというインポーターがあります。現代的なUIを好むなら検討の価値があります

---

## 14. その他できること

この章は「踏み込むとこんなこともできる」という紹介枠です。必要になってから調べれば十分なので、まずは名前とできることだけ把握しておいてください。

### プロシージャル生成(ランダムマップ)

起動するたびに違う地形を自動生成する技術です。ローグライクのダンジョン、無限に広がるマップ、自動ステージ生成などに使います。

よく使われるアルゴリズムは以下の4系統です。

- **ランダムウォーク**: 1点からランダムに歩いて洞窟状の通路を掘る
- **BSP(Binary Space Partitioning)**: 空間を再帰的に分割して部屋を配置する。ローグライクの定番
- **セルオートマトン**: ランダムなノイズを反復処理して有機的な洞窟を作る
- **Perlin Noise**: 連続的なノイズ値から高低を作る。島や山脈向け

実装の基本は「純粋な配列操作で生成ロジックを書き、結果を`SetTilesBlock`でTilemapに流し込む」というパターンです。描画とロジックを分離できるので、テストや再現(同じシードで同じマップ)が容易になります。

### Cinemachine詳細(高度なカメラ制御)

基本的な追従はマニュアル前半で触れました。踏み込むと以下のようなことができます。

- デッドゾーンとソフトゾーンの調整でマリオ風のカメラ挙動を再現
- Polygon Confiner 2Dでマップの形に合わせてカメラ可動範囲を制限
- State-Driven Cameraで部屋ごとにカメラを切り替え(メトロイドヴァニア風)
- Impulse Sourceを使った画面シェイク
- Target Groupで複数プレイヤーを自動フレーミング
- Timelineと組み合わせたカットシーン制御

横スクロールアクションや探索型2Dゲームを本格的に作るなら、深掘りする価値があります。Package Managerで `com.unity.cinemachine` を追加してください。

### 必要になったら追記

この2項目はどちらも奥が深く、本格的に使うなら独立したマニュアルが1冊書ける規模です。ゲームの仕様として必要になった時点で調べ、個別の章としてこのマニュアルに追記する想定で余白を空けておきます。

---

## 15. よくあるトラブルと対処

| 症状                                     | 原因                                                            | 対処                                                                           |
| ---------------------------------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| タイルがぼやける                         | Filter ModeがBilinear/Trilinear                                 | Point (no filter)にする                                                        |
| タイル間に細い線や隙間が見える           | テクスチャフィルタによる色のにじみ、またはカメラ位置が半端な値  | Extrude Edgesを1~2に、Pixel Perfect Cameraを導入                               |
| 重なり順がおかしい(前景が背景の奥に表示) | Sorting Layer / Order in Layerの設定ミス                        | 各Tilemap Rendererで確認                                                       |
| プレイヤーが地面の継ぎ目で引っかかる     | Tilemap Collider 2Dを単独で使っている                           | Composite Collider 2Dを併用する                                                |
| プレイヤーが地面をすり抜ける             | Rigidbody 2DのCollision DetectionがDiscrete、または速度が速すぎ | Continuousに変更                                                               |
| パレットに入れたタイルがシーンに描けない | Active Tilemapが未選択、または別レイヤーが選択されている        | パレット上部のActive Tilemapを確認                                             |
| Applyを押したのに設定が反映されない      | 別のアセットを選択する前に変更した                              | 必ず変更後にApplyボタンを押す                                                  |
| Rule Tileが正しく切り替わらない          | 隣接条件の設定ミス、または条件の順序                            | ルールは上から順に評価されるので、狭い条件ほど上に                             |
| Cinemachineでカメラがガタつく            | Follow Offsetの値、Lookaheadの値                                | Body設定を見直し、Pixel Perfect Cameraと併用時はExtensionでPixel Perfectを追加 |

---

## 16. ショートカット一覧

### Tile Palette内

| キー      | 動作               |
| --------- | ------------------ |
| B         | ブラシ             |
| U         | 四角塗り           |
| G         | 塗りつぶし         |
| I         | スポイト           |
| S         | 選択               |
| M         | 移動               |
| D         | 消しゴム           |
| [         | ブラシローテート左 |
| ]         | ブラシローテート右 |
| Shift + [ | 左右反転           |
| Shift + ] | 上下反転           |

### Unity共通(参考)

| キー             | 動作                         |
| ---------------- | ---------------------------- |
| F                | 選択オブジェクトにフォーカス |
| Ctrl + Z         | アンドゥ                     |
| Ctrl + Shift + Z | リドゥ                       |
| Ctrl + D         | 複製                         |
| Ctrl + S         | 保存                         |

---

## 17. 新規マップ作成チェックリスト

コピーして使えるように、もう一度貼っておきます。

### 初期セットアップ(プロジェクト開始時に1回)

- [ ] 2D Tilemap EditorとExtrasがインストール済み
- [ ] プロジェクト全体のPPU基準値が決まっている
- [ ] Sorting Layerが設定済み
- [ ] フォルダ構成が整っている(Sprites / Tiles / Palettes)

### マップ追加時(毎回)

- [ ] 素材PNGをインポート、Inspector設定を確認してApply
- [ ] Sprite Editorでスライス、Apply
- [ ] Tile Paletteにドラッグ、タイルアセット生成先フォルダを指定
- [ ] Grid + Tilemapをシーンに配置
- [ ] レイヤー(背景 / 地面 / 前景など)を作成
- [ ] 各TilemapのSorting Layer、Order in Layerを設定
- [ ] 当たり判定レイヤーにTilemap Collider 2D + Composite Collider 2D + Rigidbody 2D(Static)
- [ ] Active Tilemapを確認してから描画開始
- [ ] カメラのSizeとPosition調整
- [ ] Playで動作確認、プレイヤーが落下しないこと、引っかからないことを確認

---

## 付録A: レンダーパイプライン(SRP/URP/HDRP)について

アンチエイリアスの設定場所が見つからないなど、描画まわりで混乱したときに読む用のメモです。

### レンダーパイプラインとは

ゲーム画面は、シーン内のモデル・スプライト・光源・カメラなどの情報を最終的にピクセルへ変換することで表示されています。この変換手順をまとめたものをレンダーパイプラインと呼びます。「何をどの順番で描くか」「影をどう計算するか」「ポストエフェクトをいつかけるか」といった描画フロー全体が含まれます。

### Unityの3種類のパイプライン

Unityには歴史的経緯もあり、現在3種類のレンダーパイプラインが存在します。

**1. Built-in Render Pipeline**

Unityに昔から組み込まれているパイプラインです。中身はほぼいじれませんが、ドキュメントやアセットストアの古い資産が最も豊富です。古いチュートリアルはこれが前提になっていることが多いです。

**2. URP(Universal Render Pipeline)**

軽量で、スマホ・PC・コンソールの幅広いプラットフォームに対応します。2Dにも3Dにも使えて、新規2Dプロジェクトのデフォルトはこれになっていることが多いです。

**3. HDRP(High Definition Render Pipeline)**

フォトリアル向けの高品質パイプラインで、PCやハイエンドコンソール専用です。スマホでは重すぎて動きません。2Dゲームで採用するメリットはほぼありません。

### SRPとは

**SRP(Scriptable Render Pipeline)**は、URPとHDRPの共通の土台となる仕組みです。Unityが「パイプライン自体をC#で書き換えられるようにしよう」と導入したフレームワークで、その上にURP(軽量・万能)とHDRP(高品質・重め)という2つの具体的実装が乗っています。

関係を整理すると次のようになります。

- SRP = 仕組みの名前(親)
- URP = SRPを使った軽量実装(子)
- HDRP = SRPを使った高品質実装(子)

Project Settings画面で「A Scriptable Render Pipeline is in use」という警告が出たら、「URPかHDRPを使っていますよ」という意味です。この場合、Built-in用の古い設定項目は使われないため、Unityが自動で非表示にします。

### 使っているパイプラインの確認方法

`Edit > Project Settings > Graphics`を開きます。上部の「Scriptable Render Pipeline Settings」欄に現在設定されているパイプラインアセットが表示されています。

- 欄が空: Built-in Render Pipeline
- `UniversalRenderPipelineAsset`が設定: URP
- `HDRenderPipelineAsset`が設定: HDRP

補助的な判別方法として、左サイドバーに「ShaderGraph」の項目があれば、ほぼ確実にURPかHDRPを使っています(ShaderGraphはSRP系の機能です)。

### URPのアンチエイリアス設定場所

URPでは、アンチエイリアス関連の設定が2か所に分散しています。

**1. URP Asset側(MSAA)**

Projectビューで `t:UniversalRenderPipelineAsset` と検索すると、URP Assetが見つかります(通常は`Assets/Settings/`配下)。このアセットを選択し、Inspectorの「Quality」セクションにある **Anti Aliasing (MSAA)** を設定します。

- Disabled / 2x / 4x / 8x から選択
- ピクセルアートなら「Disabled」

**2. Camera側(ポストプロセス系のAA)**

Main Cameraを選択し、Rendering セクションの **Anti-aliasing** を設定します。

- **No Anti-aliasing**: AAなし
- **FXAA(Fast Approximate)**: 軽量、画面全体を簡易処理
- **SMAA(Subpixel Morphological)**: FXAAより高品質
- **TAA(Temporal)**: 時間軸情報を使う最高品質。ただし動きが少ない画面で弱い場合あり

ピクセルアートなら「No Anti-aliasing」。

### HDRPの場合

Camera個別の Frame Settings でアンチエイリアス方式を指定します。設定階層が深いので、HDRPを使う場面で都度調べた方が早いです。

### ピクセルアートで本当に重要なのは

Quality設定やパイプラインのAA設定以上に効くのは、各スプライトのInspectorで設定する **Filter Mode = Point (no filter)** と **Compression = None** の2つです。これさえ正しく設定されていれば、AAが有効になっていてもドット絵が派手にぼやけることはあまりありません。

ただし以下のケースでは影響が出るので、ピクセルアート前提なら両方を切っておくのが無難です。

- シェーダーで自作ポストエフェクトをかけるとき
- 3D要素をピクセルアートと混在させるとき
- カメラを斜めに構えたり回転させたりするとき

### 2Dタイルマップで使うならどれか

結論としてはURPがおすすめです。理由は以下のとおりです。

- 2D Lights機能で松明・懐中電灯・環境光などをスプライトに対して表現できる
- ShaderGraphでビジュアル的にシェーダーを作れる
- 新しいUnity機能やアセットストアのアセットはURP前提のものが多い
- パフォーマンスが軽い

Built-inに戻す理由があるのは、古いチュートリアルや古いアセットを丸ごと流用したい場合くらいです。新規プロジェクトなら最初からURPで進めた方が将来的にも有利です。

### パイプラインの切り替え方(参考)

既存プロジェクトのパイプラインを切り替えることもできますが、マテリアルやシェーダーの互換性問題が起きやすく、中盤以降の切り替えは手間がかかります。可能なら新規プロジェクト作成時に決め切ってしまうのが安全です。

- 新規プロジェクト作成時: 「2D (URP)」「3D (URP)」「3D (HDRP)」などのテンプレートを選ぶ
- 既存プロジェクトに後から導入: Package ManagerでURPパッケージを追加し、URP Assetを作成して `Project Settings > Graphics` で指定する。マテリアルは `Window > Rendering > Render Pipeline Converter` で一括変換できる

---

## 更新履歴

- 初版作成: 2026/04/22
- 第2版: スクリプト操作、ScriptableTile、Tiled連携を追加
- 第2版: プロシージャル生成とCinemachine詳細を紹介枠として追加
- 第3版: 付録Aとしてレンダーパイプライン(SRP/URP/HDRP)の解説を追加、URP使用時のアンチエイリアス設定場所も整理

## 次に足したい項目(TODO)

- [ ] プロシージャル生成の具体的な実装例(アルゴリズム別のコード付き)
- [ ] Cinemachineの詳細設定ガイド(デッドゾーン、Confiner、State-Driven)
- [ ] 大規模マップの分割ロード手法(Addressables、シーンストリーミング)
- [ ] Tilemap用のシェーダー活用(夜色変更、ライティング、水面揺らぎ)
- [ ] 2D Lightの組み合わせ(URP 2D Renderer)