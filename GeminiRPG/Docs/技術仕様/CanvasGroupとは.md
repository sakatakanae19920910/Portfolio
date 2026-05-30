# Canvas Groupとは

**作成日**: 2026-05-12

---

## 一言で言うと

**複数のUI部品をまとめて透明度・操作可否を制御するコンポーネント。**

---

## 何が便利なのか

DialoguePanelをフェードインさせたいとき、Canvas Groupなしだと：

```
MainText の alpha を変える
SpeakerPanel の alpha を変える
SpeakerText の alpha を変える
DialoguePanel の alpha を変える
...（子の数だけ繰り返す）
```

DialoguePanelにCanvas Groupをつけると：

```
DialoguePanel の Canvas Group の alpha を変えるだけ
  → 子のMainText・SpeakerPanel・SpeakerTextが全部一緒に変わる
```

---

## 3つの設定項目

| 項目 | 役割 |
|------|------|
| **Alpha** | 0=完全透明 / 1=完全不透明。子全体の透明度をまとめて変える |
| **Interactable** | falseにすると子のボタンが全部押せなくなる |
| **Blocks Raycasts** | falseにするとクリック判定を無視する |

### Interactable と Blocks Raycasts はセットで使う

Alpha=0（見えない）でも Interactable=true だと**見えないのにボタンが押せる**状態になる。
フェードアウト時は必ず両方 false にする。

```csharp
// 非表示にするとき
canvasGroup.alpha = 0f;
canvasGroup.interactable = false;    // ボタン押せない
canvasGroup.blocksRaycasts = false;  // クリック無視

// 表示するとき
canvasGroup.interactable = true;
canvasGroup.blocksRaycasts = true;
canvasGroup.DOFade(1f, 0.5f);        // alpha 0→1（DOTweenでフェードイン）
```

---

## つけ方

1. Canvas GroupをつけたいGameObjectをHierarchyで選択
2. Inspector → **Add Component → Canvas Group**

---

## このプロジェクトでの使用箇所

| GameObjectの名前 | 役割 |
|----------------|------|
| **MainText** | セリフのフェードイン・アウト（ScenarioExecutorのmainTextCanvasGroupに登録） |
| **ContentPanel**（DemoEndScene） | デモ終了画面のフェードイン（実装済み） |

### MainTextへのつけ方（会話UIセットアップ時）

1. HierarchyでMainTextを選択
2. Add Component → Canvas Group を追加
3. ScenarioExecutorの **Main Text Canvas Group** フィールドにMainTextをドラッグ

---

## 親子関係とCanvas Groupの影響範囲

Canvas Groupは**自分自身＋すべての子**に効く。

```
DialoguePanel（Canvas Group）← alphaを変えるとここから下が全部変わる
├── SpeakerPanel
│   └── SpeakerText
└── MainText（Canvas Group）← MainTextだけのalphaはここで制御
```

上の例のようにネストすることもできる。
DialoguePanelのalphaを0にしたらMainTextのCanvas Groupの値に関わらず消える。
