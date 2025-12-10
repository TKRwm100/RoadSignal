# BveEx.Toukaitetudou.RoadSignal

車両用・歩行者用の交通信号機の制御をするプラグインです.

## 利用方法

マップ構文, 設定ファイルを適切に記述してください.
尚, 動作には [BveEx](https://bveex.okaoka-depot.com/) が別途必要です.[![BveEx](https://www.okaoka-depot.com/contents/bve/banner_AtsEX.svg)](https://bveex.okaoka-depot.com/)

尚, 構文解説については[__こちら__](Reference/Reference.pdf)をご参照ください.

## サンプルシナリオについて

サンプルシナリオでは内部でBveExサンプルシナリオを参照しています.
BveExサンプルシナリオと同階層に配置する等, 参照依存を適切に解決してください.

## よくあるであろうお問い合わせ

### 読み込んだ際 "ハンドルされていない例外" のウインドウが出る

ゾーン識別子の削除ができていない可能性があります.

エクスプローラにてファイルを右クリック->プロパティ->許可するにチェックを入れて適用又はOKを押下してください.

### 読み込んだ際 "ファイル ～ が見つかりません." のウィンドウが出る

モデルへのパスが間違っているかファイルが存在しません.

### 読み込んだ際 "D3DXFERR_BADFILE: Bad file (-2005531761)" のエラーが出る

指定されたモデルが読み込めません. パスが間違っているか読み込み可能な形式のファイルではありません.

### ～ができません

殆どの場合仕様です.

気が向いたら作るかもですが, それが待てないならば是非ご自身で実装してください.

## ライセンス

[拡張PolyForm準拠ライセンス](LICENSE.md)

## 使用ライブラリ等

### [BveEx.CoreExtensions](https://github.com/automatic9045/BveEX)(PolyForm NonCommercial 1.0.0)

Copyright (c) 2022 automatic9045

### [BveEx.PluginHost](https://github.com/automatic9045/BveEX) (PolyForm NonCommercial 1.0.0)

Copyright (c) 2022 automatic9045

### [BveEx.Diagnostics](https://bveex.okaoka-depot.com/) (PolyForm Noncommercial License 1.0.0)

Copyright (c) 2024 automatic9045

### [Harmony](https://github.com/pardeike/Harmony) (MIT)

Copyright (c) 2017 Andreas Pardeike

### [ObjectiveHarmonyPatch](https://github.com/automatic9045/ObjectiveHarmonyPatch) (MIT)

Copyright (c) 2022 automatic9045

### [SlimDX](https://www.nuget.org/packages/SlimDX/) (MIT)

Copyright (c) 2013  exDreamDuck
