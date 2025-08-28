# Fruit Catch (Unity 2022 LTS / 2D Built-in)

- Android 実機で遊べるミニゲーム。
- 操作: 画面左/右をタップでカゴが左右に動く。
- 仕様:
  - フルーツをキャッチで +1 点、同種3連続で +1 ボーナス
  - キャッチ毎に +0.5 秒（上限 60 秒）
  - 取り逃しで Life 減少、0 でゲームオーバー

## ビルド手順（Android）
1. File > Build Settings > Android > Switch Platform
2. Add Open Scenes（Mainを追加）
3. Player Settings: IL2CPP / ARM64 / Portrait
4. 端末USBデバッグONで Build And Run