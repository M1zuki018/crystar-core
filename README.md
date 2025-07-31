# CryStar Core

カスタムライフサイクル、サービスロケーター、ログユーティリティを含む包括的なゲーム開発フレームワークです。

## 主な機能
- **カスタムライフサイクル管理**: Unity標準のライフサイクルの不確実性を解決
- **サービスロケーター**: シーン固有のローカルサービスとグローバルサービスの分離
- **ログシステム**: ログカテゴリ/ログレベルなどの機能を備えたログシステム
- **拡張メソッド**: UnityEventの拡張を用意
-
## インストール

### Package Manager経由（推奨）

1. Unity Package Managerを開く
2. `+` → `Add package from git URL...`
3. 以下のURLを入力：

```
https://github.com/M1zuki018/crystar-core.git
```

### 特定のバージョンをインストール

```
https://github.com/M1zuki018/crystar-core.git#v1.0.0
```

### manifest.jsonに直接追加

`Packages/manifest.json`に以下を追加：

```json
{
  "dependencies": {
    "com.crystar.core": "https://github.com/M1zuki018/crystar-core.git",
    "com.crystar.custom-attributes": "https://github.com/M1zuki018/crystar-custom-attributes.git",
    "com.cysharp.unitask": "2.3.3",
    "com.demigiant.dotween": "1.2.765"
  }
}
```

## 使用方法

### 1. CustomBehaviourを継承したコンポーネントの作成

```csharp
using CryStar.Core;
using Cysharp.Threading.Tasks;

public class GameManager : CustomBehaviour
{
    public override async UniTask OnAwake()
    {
        await base.OnAwake();
        // 他に依存しない初期化処理
    }

    public override async UniTask OnUIInitialize()
    {
        await base.OnUIInitialize();
        // UI要素の初期化
    }

    public override async UniTask OnBind()
    {
        await base.OnBind();
        // イベントバインディング
    }

    public override async UniTask OnStart()
    {
        await base.OnStart();
        // ゲーム開始処理
    }
}
```

### 2. LifecycleControllerの配置

シーンに`LifecycleController`をアタッチしたGameObjectを配置し、自動インスタンス化したいプレハブを登録します。

### 3. サービスロケーターの使用

```csharp
// サービスの登録
ServiceLocator.Register<IPlayerService>(new PlayerService());

// サービスの取得
var playerService = ServiceLocator.Get<IPlayerService>();
```

### 4. ログシステムの使用（ベータ版）

```csharp
using CryStar.Utility;
using CryStar.Utility.Enum;

// 基本的なログ出力
LogUtility.Info("ゲームが開始されました", LogCategory.Gameplay);
LogUtility.Warning("リソースが不足しています", LogCategory.System);

// パフォーマンス測定
using (LogUtility.MeasurePerformance("Heavy Calculation"))
{
    // 重い処理
}

// ゲームプレイイベントログ
LogUtility.LogGameplayEvent("PlayerDeath", "Level1", 15.5f);
```

## ⚙️ 設定

### LogSettingsの作成

1. Project Window で右クリック
2. `Create > CryStar > Create ScriptableObject > Utility > Log Settings`
3. `Resources`フォルダに`LogSettings`として保存

### ログ設定のカスタマイズ

設定ウィンドウは、Unityのメニューバーから `CryStar > Utility > Log Utility Settings` を選択することで開けます。

```csharp
// 最小ログレベルの設定
LogUtility.MinLogLevel = LogLevel.Info;

// カテゴリの有効/無効
LogUtility.SetCategoryEnabled(LogCategory.Debug, false);

// リリースビルド用設定
LogUtility.ConfigureForRelease();
```

## 要件

- **Unity 2022.3+**
- **UniTask 2.3.3+**
- **DOTween 1.2.765+**
- **CryStar.Custom Attribute 1.0.0+**

## ライセンス

MIT License

## サポート

Issue報告やフィードバックは[GitHubリポジトリ](https://github.com/M1zuki018/crystar-core)までお願いします。

## 変更履歴

詳細な変更履歴は[CHANGELOG.md](CHANGELOG.md)をご覧ください。