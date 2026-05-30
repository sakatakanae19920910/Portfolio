# 会話UI（Mizukagami）設定マニュアル

**作成日**: 2026-05-12
**対象**: 会話ウィンドウ・話者名・選択肢ボタンのUnity設定
**デザインシステム**: Mizukagami C1「夜明け」

Step 1（PNG変換）・Step 2（9-Slice設定）は別途実施済みの前提。

---

## Step 3: Canvas階層を作成する

### 3-1. Canvasを作る

Hierarchyウィンドウで右クリック → **UI → Canvas**

作成したCanvasを選択して、Inspectorで以下を設定：

**Canvas Scalerコンポーネント**
| 項目 | 値 |
|------|----|
| UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | **X: 1920 / Y: 1080** |
| Screen Match Mode | Match Width Or Height |
| Match | **0.5** |

> Match=0.5 にすると、横長・縦長どちらの画面にも均等にスケールする。

---

### 3-2. 階層構造を作る

以下の順番でGameObjectを追加していく。
**上から順に**Hierarchy上に積むこと（描画順 = Hierarchy順）。

```
Canvas
├── BackgroundImage       右クリック → UI → Image
├── CharacterImage        右クリック → UI → Image
├── DialoguePanel         右クリック → UI → Image
│   ├── SpeakerPanel      右クリック → UI → Image
│   │   └── SpeakerText   右クリック → UI → Text - TextMeshPro
│   └── MainText          右クリック → UI → Text - TextMeshPro
├── ChoiceContainer       右クリック → Create Empty（Imageは不要）
└── FadeImage             右クリック → UI → Image
```

> ChoiceContainerはDialoguePanelの**外・上**に置く。ScenarioExecutorがここに選択肢ボタンを動的に生成する。FadeImageより下に置くとボタンが隠れるので順番に注意。

> DialoguePanelを選択した状態で右クリックすると、その子として追加できる。

---

### 3-3. 各オブジェクトのRectTransformを設定する

**BackgroundImage**（背景・全画面）
| 項目 | 値 |
|------|----|
| Anchor | 四隅すべて（Stretch × Stretch） |
| Left / Right / Top / Bottom | 0 |

> Anchorの設定方法：RectTransformの左上にある四角いアイコンをクリック → Alt+Shift押しながら右下の「四隅」を選ぶと、位置・サイズも同時にリセットできる。

---

**CharacterImage**（バストアップ絵・正方形・右下配置）
| 項目 | 値 |
|------|----|
| Anchor | 右下（Bottom Right） |
| Pivot X / Y | 1 / 0 |
| Width | 400 |
| Height | 400 |
| Pos X | -60 |
| Pos Y | 200 |

> Pivot を (1, 0) にすると「右端から何px・下から何px」と直感的に読める。
> Pos X=-60 = 右端から60px余白、Pos Y=200 = 会話ウィンドウ上端のすぐ上。
> Preserve Aspect を ONにすると、画像の縦横比が崩れない。

---

**DialoguePanel**（会話ウィンドウ本体）
| 項目 | 値 |
|------|----|
| Anchor | 下中央（Bottom Center） |
| Width | 1800 |
| Height | 280 |
| Pos X | 0 |
| Pos Y | 60 |

Imageコンポーネント：
| 項目 | 値 |
|------|----|
| Source Image | `glass-window` |
| Image Type | **Sliced** |
| Fill Center | ON |

---

**SpeakerPanel**（話者名タグ）
| 項目 | 値 |
|------|----|
| Anchor | 左上（Top Left）※DialoguePanelの左上が基準 |
| Width | 360 |
| Height | 44 |
| Pos X | 48 |
| Pos Y | 44（パネルの上にはみ出す形になる） |

Imageコンポーネント：
| 項目 | 値 |
|------|----|
| Source Image | `speaker-chip` |
| Image Type | Simple |

---

**SpeakerText**（話者名テキスト）
| 項目 | 値 |
|------|----|
| Anchor | Stretch × Stretch（SpeakerPanel全体に広げる） |
| Left / Right | 18 |
| Top / Bottom | 8 |

TextMeshProコンポーネント：
| 項目 | 値 |
|------|----|
| Font Size | 14 |
| Font Style | Bold |
| Color | `#f4f7fb` |
| Alignment | 左寄せ・縦中央 |

---

**MainText**（セリフテキスト）
| 項目 | 値 |
|------|----|
| Anchor | Stretch × Stretch（DialoguePanel全体に広げる） |
| Left / Right | 48 |
| Top | 24 |
| Bottom | 16 |

TextMeshProコンポーネント：
| 項目 | 値 |
|------|----|
| Font Size | 30 |
| Line Spacing | 1.85（`Extra Settings > Line Spacing` で設定） |
| Color | `#f4f7fb` |
| Overflow | Overflow（テキストが枠を超えても切れない） |

---

**FadeImage**（フェード用・全画面黒）
| 項目 | 値 |
|------|----|
| Anchor | Stretch × Stretch |
| Left / Right / Top / Bottom | 0 |
| Color | `#000000` alpha=255 |
| Raycast Target | **OFF**（スクリプトから操作するまでは無効） |

---

### 3-4. ScenarioExecutorに参照を登録する

ScenarioExecutorがアタッチされているGameObjectを選択して、Inspectorの各フィールドに作ったオブジェクトをドラッグ：

| フィールド | 対応するGameObject / アセット |
|------------|-------------------------------|
| Main Text | MainText |
| Main Text Canvas Group | MainText（※事前にCanvas Groupを追加しておく） |
| Speaker Text | SpeakerText |
| Speaker Panel | SpeakerPanel |
| Character Image | CharacterImage |
| Background Image | BackgroundImage |
| Fade Image | FadeImage |
| Canvas Transform | ChoiceContainer |
| Choice Button Prefab | `Assets/_Prefabs/ChoiceButton_Prefab.prefab` |

> **Main Text Canvas Groupの準備**: HierarchyでMainTextを選択 → Inspector → Add Component → Canvas Group を追加。追加後にMainText自身をこのフィールドにドラッグする。セリフのフェードイン・アウト演出に使われる。

> **Choice Button Prefabについて**: HierarchyではなくProjectウィンドウから `Assets/_Prefabs/ChoiceButton_Prefab.prefab` をドラッグする。テキスト内容とOnClickはScenarioExecutorが実行時に自動でセットするので、Prefab側の設定は触らなくてよい。

**ChoiceContainerのRectTransform設定**
| 項目 | 値 |
|------|----|
| Anchor | 中央（Center） |
| Pos X / Y | 0 / 0 |
| Width / Height | 0 / 0 |

> ChoiceContainerはImageコンポーネント不要。ボタンのサイズ・位置はScenarioExecutorが自動計算する。

---

## Step 4: C1「夜明け」の色値

全色値は `Assets/UI/Mizukagami/unity-export/tokens.json` にもある。

| 役割 | Hex | 使う場所 |
|------|-----|---------|
| bgA（背景・暗部） | `#0a0f1a` | BackgroundImage |
| bgB（中間色） | `#172238` | SpeakerPanel（ブレンド用） |
| bgC（地平線） | `#384a6e` | 背景グラデ下端（将来） |
| ink（メインテキスト） | `#f4f7fb` | MainText / SpeakerText |
| sub（サブテキスト） | `#f4f7fb` alpha=70% | 選択肢の説明文など |
| accent（ミントグリーン） | `#9ed8d3` | カーソル・選択ハイライト |
| accent2（ラベンダー） | `#b8a6e0` | 感情アイコンなど |

### Unityでの色入力方法

Inspectorのカラーフィールドをクリック → Color Pickerが開く → 下部の **Hex入力欄**に直接貼り付けでOK。

---

## Step 5: glass-windowのカラー設定

glass-window.pngはパネル色がすでにSVGに焼き込まれている。UnityのImage Colorは**必ずWhiteにすること**。

> ⚠️ **絶対にやってはいけない**: Image ColorにパネルのHex値を設定しない。
> UnityはImage Color × スプライトのピクセルを乗算するため、同じ暗い色を2回かけると真っ黒になる。

> **ノート（blur非対応調整）**: Mizukagami C1のglass設計は backdrop-filter:blur(24px) 前提。
> Unityではblur不使用のため、SVGのfill色をトークンのbgB(#172238)より明るい **#1e2e50** に変更済み。

### DialoguePanelのImageカラーを設定

| 項目 | 値 |
|------|----|
| Color Hex | `ffffff` |
| Alpha | **255**（= 100%） |

> **Whiteにする理由**: SVGのfillに `rgba(30,46,80,0.98)` = #1e2e50が直接焼き込まれており、  
> Image Color=White（1,1,1,1）にすることで White×#1e2e50 = #1e2e50 が正しく表示される。

### SpeakerPanelのImageカラーを設定

| 項目 | 値 |
|------|----|
| Color Hex | `ffffff` |
| Alpha | **255**（= 100%） |

> DialoguePanelと同じ理由でWhiteにすること。SVGにbgA色(#0a0f1a)が焼き込まれている。

---

## 完成イメージ（C1 夜明け）

```
┌─────────────────────────────────────────────┐
│ 暗いネイビーの背景（#0a0f1a）                 │
│                                             │
│            [立ち絵]                          │
│                                             │
│ [カイリ]  ←話者名タグ                        │
│┌───────────────────────────────────────────┐│
││ セリフテキストがここに表示される。           ││
││ フォントカラーは #f4f7fb の柔らかい白。      ││
│└───────────────────────────────────────────┘│
└─────────────────────────────────────────────┘
```

---

## よくある問題

| 症状 | 原因 | 対処 |
|------|------|------|
| パネルがほぼ見えない・真っ黒になる | Image ColorにHex値（#172238等）を設定している（二重乗算） | Image Color を **White (#ffffff, alpha=255)** に変更する |
| ウィンドウを広げると角が歪む | Image TypeがSimpleになっている | SlicedにしてFill Center ON |
| テキストが見えない | FadeImageがMainTextの上にある | HierarchyでFadeImageをDialoguePanelより下に移動 |
| 話者名が枠の中に収まらない | SpeakerPanelのPos Yがマイナス | Pos Yを+44など正の値に |
| 立ち絵が縦横比崩れる | Preserve AspectがOFF | CharacterImageのInspectorでON |
