# Silent Planet 项目概述

## 基本信息
- **项目类型**: Unity 3D 横向卷轴游戏
- **开发阶段**: 早期原型开发
- **平台**: Windows

## 核心架构: Module-Glue 模式

### 模块 (Module)
- `InputManager` - 输入处理
- `PlayerMotor` - 玩家移动逻辑
- `RopeSystem` - 绳索物理系统

### 粘合层 (Glue)
- `PlayerGlue` - 连接输入与运动模块

### 编辑器工具
- `BlockEditorWindow` - 关卡方块编辑器

## 目录结构
```
Assets/
├── _Scripts/           # 核心脚本
│   ├── Editor/         # 编辑器工具
│   └── Settings/       # ScriptableObject 配置
├── Scenes/             # 游戏场景
├── Settings/           # 配置资产
├── Prefabs/            # 预制体
└── Plugins/            # 第三方插件
```

## 文档位置
`/Documents` 目录，使用中文编写
