# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-07-31

### Added

#### Core System
- **Custom Lifecycle Management**: 確実な初期化順序を保証するカスタムライフサイクルシステム
    - `ILifecycleTarget`インターフェース
    - `CustomBehaviour`基底クラス
    - `LifecycleController`による自動管理
    - UniTask対応の非同期ライフサイクル

#### Service Locator Pattern
- **Global/Local Service Management**: グローバルとローカルのサービス分離管理
    - `ServiceLocator`クラス
    - `ServiceType`列挙型（Global/Local）
    - 型安全なサービス登録・取得システム
    - 自動的なライフサイクル管理

#### Comprehensive Logging System
- **Multi-Level Logging**: 6段階のログレベル（Verbose〜Fatal）
- **Category-Based Organization**: 9つのログカテゴリ（System, Gameplay, UI, Audio等）
- **Customizable Appearance**: カスタマイズ可能な色設定とフォーマット
- **Performance Monitoring**: 組み込みパフォーマンス測定機能
- **File Output**: オプションのファイルログ出力
- **ScriptableObject Settings**: `LogSettings`、エディター拡張の専用パネルによる設定管理

#### Utility Extensions
- **DOTween Integration**: DOTweenのUniTask統合拡張メソッド
- **UnityEvent Extensions**: 安全なUnityEvent操作メソッド
- **Sequence Extensions**: より便利なシーケンス操作メソッド

#### Additional Utilities
- **PrefabInstantiatorUtility**: 型安全なプレハブインスタンス化
- **LogColorSettings**: ログ色設定の一元管理
- **Performance Scope**: using文を使用したパフォーマンス測定

### Technical Features
- **Async/Await Support**: 全システムでUniTask対応
- **Type Safety**: ジェネリクスを活用した型安全性

### Dependencies
- Unity 2022.3+
- UniTask 2.3.3+
- DOTween 1.2.765+

### Documentation
- 包括的なREADME
- APIドキュメント
- サンプルコード
- ベストプラクティスガイド

### Notes
- 初回リリース版