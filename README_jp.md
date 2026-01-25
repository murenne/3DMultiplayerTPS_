# 3D Multiplayer TPS

[English](./README.md)| [中文](./README_cn.md) | [日本語](./README_jp.md)

**Photon Quantum 3.0 決定論的エンジンに基づいて開発された、マルチプレイヤー3Dサードパーソン・シューティングゲームプロジェクトです。**

実際のプロジェクト経験に基づき、コアとなるゲームプレイシステムを移植およびリファクタリングしました。

* [Getting Started](#getting-started)
* [Control](#control)
* [Software Architecture](#software-architecture)
    * [Quantum ECS](#quantum-ecs)
    * [AssetObject](#assetobject)
* [Core Systems](#core-systems)
    * [Game System](#game-system)
    * [Spawn System](#spawn-system)
    * [Player Movement System](#player-movement-system)
    * [Action System](#action-system)
    * [Data Processing](#data-processing)
    * [Status and Damage System](#status-and-damage-system)
    * [Animation System](#animation-system)
    * [Camera System](#camera-system)
* [Statement](#statement)
## Getting Started

本プロジェクトは **Unity 2022.3.36f1** をベースに開発されています。Unityのバージョンが **2022.3.36f1** 以上であることを確認してください。

1.  **Photon Quantum SDK 3.0** がインストールされ、**AppID** が正しく設定されていることを確認してください。
2.  GitHubからプロジェクトを指定のフォルダにクローンし、Unityで開きます。
3.  Playボタンをクリックしてプロジェクトを実行します。

## Control

本プロジェクトは、キーボード・マウスおよびゲームパッド（コントローラー）の双方に対応しています。

| キー / 操作 | 機能説明 |
| :--- | :--- |
| **WASD** / **左スティック** | プレイヤー移動 |
| **マウス左クリック** / **RT** | 射撃 / 攻撃 |
| **Space** / **A (South)** | ジャンプ |
| **Shift** / **X (West)** | ダッシュ / スプリント |
| **Tab** / **RB** | ターゲットロック切り替え |

## Software Architecture

本プロジェクトは **Photon Quantum 3.0** 決定論的ネットワークフレームワークを使用し、ロジックとビュー（描画）を分離したアーキテクチャ設計を採用しています。

### Quantum ECS
従来の MonoBehaviour シングルトンパターンを廃止し、コアロジックには **ECS (Entity Component System)** アーキテクチャを採用しています：

* **Entity（エンティティ）:** ゲーム世界内のオブジェクト（プレイヤー、弾丸など）。
* **Component（コンポーネント）:** エンティティのデータ状態を保持。
* **System（システム）:** ゲームロジックとデータ更新を処理。
* **Simulation（シミュレーション）:** 決定論的シミュレーションのコアであり、多端末間の同期を保証します。

### AssetObject
静的な設定データの管理には Quantum の **AssetObject** システムを使用しています。

* **Player Action Data:** プレイヤーアクションの設定（ジャンプ、ダッシュ、射撃など）。
* **Player Parts Data:** プレイヤーの着せ替えやパーツデータの設定。

すべての設定データは AssetObject を通じて管理され、調整やバージョン管理が容易になっています。

## Core Systems

プロジェクトでは以下のコアゲームシステムを実装済みです：

### Game System
ゲームのライフサイクル管理を担当します。
* ゲーム開始前のカウントダウン表示。
* ゲームステートの切り替え（準備中、進行中、終了）。
* カウントダウンアニメーションの演出。
* ゲーム進行のフロー制御。

### Spawn System
プレイヤーの生成ロジックを処理します。
* ゲーム開始時に各プレイヤーへ初期位置をランダムに割り当て。
* マップで事前定義されたスポーン地点に基づいてランダム配置。
* プレイヤーの生成位置が重複しないように制御。
* リスポーン（復活）メカニズムのサポート。

### Movement System
プレイヤーの移動と向きの制御を行います。
* キーボード/ゲームパッド入力による移動制御。
* ロックしたターゲットへの自動向き調整。
* 攻撃ターゲットの手動切り替え。
* スムーズな移動と回転の補間処理。

### Action System
プレイヤーのすべてのアクション挙動を統一管理します。
* **アクションタイプ:** ジャンプ、ダッシュ、射撃。
* **クールダウン（Cooldown）:** 再使用待機時間の管理。
* **持続時間（Duration）:** アクション継続時間の制御。
* **入力バッファ（Input Buffer）:** 先行入力メカニズム。
* **優先順位:** アクションの優先度と中断ロジック。

### Data Processing
装備データの注入フレームワークです。
* 装備設定データの自動読み込み。
* Quantum シミュレーションシステムへの動的注入。
* 装備ごとの属性差分のサポート。
* 装備データのホットアップデート（動的更新）対応。

### Status and Damage System
プレイヤーのステータスとダメージ計算を行います。
* **HP管理:** 体力の増減管理。
* **ダメージ計算:** ダメージの算出と適用。
* **被撃フィードバック:** ダメージを受けた際のリアクション。
* **死亡判定:** 死亡時の判定と処理。

### Animation System
ビュー（描画）層のアニメーション表現を担当します。
* **Event** メカニズムに基づき、ロジックとビューを接続。
* プレイヤーの状態に応じて適切なアニメーションを再生。
* アクションのブレンド（混合）とトランジション（遷移）をサポート。
* ネットワーク状態をローカルの表現に同期。

### Camera
カメラ制御とレンズ管理を行います。
* ゲーム初期化時に正しいチームカメラを有効化。
* 全プレイヤーが収まるようにカメラ距離を自動調整。
* ターゲットプレイヤーへの動的追従。
* スムーズなカメラ遷移エフェクト。

## Statement

本プロジェクトは **Photon Quantum 3.0** 決定論的ネットワークエンジンに基づいて開発されています。
現在のバージョンでは、コアとなるゲームプレイシステムの移植とリファクタリングが完了しています。