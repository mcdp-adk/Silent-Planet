# Silent Planet 项目概览

## 项目基本信息

- **项目名称**: Silent Planet
- **项目类型**: Unity 3D 游戏开发
- **Unity 版本**: 6000.0.60f1 (Unity 6)
- **开发阶段**: 早期原型开发

## 项目简介

Silent Planet 是一个 3D 横向卷轴游戏项目，采用模块化架构设计，专注于玩家控制和移动机制的实现。

## 核心架构

### Module-Glue 模式

项目采用 Module-Glue 架构模式，确保代码的模块化和可维护性：

- **模块 (Module)**: 专注的单一职责组件
  - `InputManager`: 输入处理模块
  - `PlayerMotor`: 玩家移动逻辑模块
  - `RopeSystem`: 绳索系统模块

- **粘合代码 (Glue)**: 模块间的协调层
  - `PlayerGlue`: 连接输入系统和移动系统

- **编辑器工具 (Editor)**
  - `BlockEditorWindow`: 关卡方块编辑器

### 关键特性

- **分离关注点**: 输入、移动、协调逻辑完全分离
- **事件驱动**: 使用 C# Action 实现松耦合的模块通信
- **易于扩展**: 新功能可独立添加而不影响现有模块

## 目录结构

```
Assets/
├── _Scripts/                      # 核心游戏脚本
│   ├── InputManager.cs            # 输入管理器
│   ├── PlayerMotor.cs             # 玩家移动控制器
│   ├── PlayerGlue.cs              # 输入与移动的粘合层
│   ├── RopeSystem.cs              # 绳索系统
│   ├── InputSystem_Actions.cs     # Unity Input System 自动生成代码
│   ├── Editor/                    # 编辑器工具
│   │   └── BlockEditorWindow.cs   # 关卡编辑器窗口
│   └── Settings/                  # ScriptableObject 配置类
│       ├── PlayerMotorSettings.cs
│       ├── RopeSystemSettings.cs
│       └── BlockEditorSettings.cs
├── Scenes/                        # 游戏场景
│   ├── Level 0.unity              # 主关卡
│   └── TestPlayer.unity           # 玩家测试场景
├── Settings/                      # 配置资产
│   ├── InputSystem_Actions.inputactions
│   ├── DefaultPlayerMotorSettings.asset
│   ├── DefaultRopeSystemSettings.asset
│   └── BlockEditorSettings.asset
└── Plugins/                       # 第三方插件
    ├── Tarodev 2D Controller/     # 2D 控制器参考插件
    ├── Gridbox Prototype Materials/  # 原型材质
    └── Lean GUI 相关插件          # UI 工具
```

## 技术栈

### 核心系统

- **Unity Input System**: 新版输入系统，支持多平台输入设备
- **CharacterController**: Unity 内置角色控制器组件
- **事件系统**: C# Action 用于模块间通信

### 第三方资源

- **Tarodev 2D Controller**: 2D 控制器参考实现
- **Gridbox Prototype Materials**: 快速原型材质
- **Lean GUI 系列**: UI 辅助工具

## 玩家控制系统

### 输入映射

支持键盘、手柄、触摸等多种输入方式：

- **移动**: WASD/方向键/左摇杆
- **跳跃**: 空格/手柄南键（支持短按和长按）
- **冲刺**: 左 Shift/左摇杆按下
- **视角**: 鼠标/右摇杆
- **攻击**: 鼠标左键/手柄西键/触摸
- **交互**: E 键/手柄北键
- **蹲伏**: C 键/手柄东键
- **切换**: 1/2 键/手柄方向键

### 移动机制

- **基础移动**: 平滑的水平移动
- **跳跃系统**:
  - 短按跳跃: 固定高度跳跃
  - 长按跳跃: 启动喷气背包
- **喷气背包**: 提供垂直和水平额外推力
- **空中控制**: 可配置的空中移动能力
- **重力系统**: 自定义重力和最大下落/上升速度

## 开发规范

### 命名约定

遵循 JetBrains Rider 的 C# 命名规范：

- **类型**: `PascalCase` (如 `PlayerController`)
- **方法**: `PascalCase` (如 `OnMove()`)
- **私有字段**: `_camelCase` (如 `_characterController`)
- **序列化字段**: `camelCase` (如 `moveSpeed`)
- **常量**: `UPPER_CASE` (如 `MAX_HEALTH`)

### 代码组织

- 使用 `#region` 组织代码块
- 按功能分组：字段、生命周期、公共方法、私有方法
- 添加 `[Header]` 和 `[Tooltip]` 提升 Inspector 可读性

## 相关文档

- [输入系统指南](./input-system-guide.md) - 输入系统架构和使用说明
- [玩家运动系统指南](./player-motor-guide.md) - PlayerMotor 运动机制详解
- [绳索系统指南](./rope-system-guide.md) - 动态弹簧绳索系统
- [关卡编辑器指南](./block-editor-guide.md) - BlockEditor 编辑器工具
