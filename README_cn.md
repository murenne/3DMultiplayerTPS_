# 3D Multiplayer TPS

[English](./README.md)| [中文](./README_cn.md) | [日本語](./README_jp.md)

这是一个基于 **Photon Quantum 3.0 确定性引擎** 开发的多人3D第三人称射击游戏项目  
以实际项目经验为基础，移植并重构核心玩法系统

<!--ts-->
* [3D Multiplayer TPS](#3d-multiplayer-tps)
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
<!--te-->

## Getting Started
1. 该项目基于 Unity 2022.3.36f1 开发，请确保你的 Unity 版本为 **2022.3.36f1** 及以上
2. 确保已安装 Photon Quantum SDK 3.0 并正确配置 AppID
3. 从 GitHub 克隆该项目至指定文件夹，使用 Unity 打开
4. 点击 Play 按钮即可运行项目

---

## Control
该项目支持键鼠和手柄双模式操作

| 按键/操作      | 功能说明                     |
| ----------| ------------------------ |
| `WASD` / `左摇杆`       | 玩家移动                |
| `鼠标左键` / `RT`       | 射击/攻击              |
| `空格` / `A (South)`       | 跳跃                |
| `Shift` / `X (West)`       | 冲刺                |
| `Tab` / `RB`       | 切换锁定目标                |

---

## Software Architecture
本项目使用 Photon Quantum 3.0 确定性网络框架，采用逻辑与视图分离的架构设计

### Quantum ECS
摒弃传统的 MonoBehaviour 单例模式，核心逻辑采用 ECS (Entity Component System) 架构：

- **Entity（实体）**: 游戏世界中的对象（玩家、子弹等）
- **Component（组件）**: 存储实体的数据状态
- **System（系统）**: 处理游戏逻辑和数据更新
- **Simulation（模拟）**: 确定性模拟核心，保证多端同步

### AssetObject
项目使用 Quantum 的 AssetObject 系统管理静态配置数据

| 资源类型     | 说明                     |
| ----------| ------------------------ |
| `Player Action Data`       | 玩家动作配置（跳跃、冲刺、射击等）              |
| `Player Parts Data`       | 玩家换装与部件数据配置                |

所有配置数据通过 AssetObject 管理，方便调整和版本控制

---

## Core Systems
项目已实现以下核心游戏系统：

### Game System
负责游戏生命周期管理
- 游戏开始前的倒计时显示
- 游戏状态切换（准备、进行中、结束）
- 倒计时动画效果展示
- 游戏流程控制

### Spawn System
处理玩家生成逻辑
- 游戏开始时为每个玩家随机分配初始位置
- 基于地图预定义的生成点进行随机分配
- 确保玩家生成位置不重叠
- 支持重生机制

### Movement System
玩家移动与朝向控制
- 支持键盘/手柄输入的移动控制
- 自动面向锁定目标
- 手动切换攻击目标
- 平滑的移动和旋转插值

### Action System
统一管理玩家所有动作行为
- 动作类型：跳跃、冲刺、射击
- 冷却时间（Cooldown）管理
- 动作持续时间（Duration）控制
- 输入缓冲（Input Buffer）机制
- 动作优先级和打断逻辑

### Data Processing
装备数据注入框架
- 自动读取装备配置数据
- 动态注入到 Quantum 模拟系统
- 支持不同装备的属性差异
- 热更新装备数据

### Status and Damage System
玩家状态与伤害计算
- 生命值（HP）管理
- 伤害计算与应用
- 受击反馈
- 死亡判定与处理

### Animation System
视图层动画表现
- 基于 Event 机制连接逻辑与视图
- 根据玩家状态播放对应动画
- 支持动作混合与过渡
- 同步网络状态到本地表现

### Camera
相机控制与镜头管理
- 游戏初始化时激活正确的队伍相机
- 自动调整相机距离以包含所有玩家
- 动态跟随目标玩家
- 平滑的相机过渡效果

---

## Statement
该项目基于 **Photon Quantum 3.0** 确定性网络引擎开发  
目前版本已完成核心玩法系统的移植与重构  
