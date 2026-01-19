# 关卡编辑器指南

## 概述

BlockEditor 是一个 Unity 编辑器窗口工具，用于在 SceneView 中快速放置和删除方块，支持框选批量操作和 Undo。适用于横版卷轴游戏的关卡搭建。

## 架构设计

### 文件结构

```
Assets/
├── _Scripts/
│   ├── Editor/
│   │   └── BlockEditorWindow.cs    ← 编辑器窗口
│   └── Settings/
│       └── BlockEditorSettings.cs  ← 配置类 (ScriptableObject)
└── Settings/
    └── BlockEditorSettings.asset   ← 配置资产
```

### 场景层级结构

放置的方块按坐标分组组织：

```
Grid                    ← 根容器
├── (0,0)               ← 坐标组 (x,y)
│   ├── Block_z-1       ← 方块 (z轴偏移)
│   ├── Block_z0
│   └── Block_z1
├── (1,0)
│   └── ...
└── (-2,3)
    └── ...
```

## 使用方式

### 打开窗口

菜单：`Window → Block Editor`

### 窗口界面

| 区域 | 说明 |
|------|------|
| **编辑模式** | 开关按钮，启用/禁用 SceneView 编辑功能 |
| **可见 Layer** | 编辑模式下显示的层级 |
| **放置深度** | 沿 z 轴放置的层数 |
| **方块调色板** | 可放置的预制体列表 |

### 编辑模式

**开启时**：
- SceneView 切换到 2D 模式
- 只显示配置的 Layer 物体

**关闭时**：
- 恢复 SceneView 视图模式
- 恢复物体可见性

### 操作方式

| 操作 | 功能 |
|------|------|
| 左键单击 | 在当前位置放置方块组 |
| 左键拖拽 | 框选区域批量放置 |
| 右键单击 | 删除该位置的方块组 |
| 右键拖拽 | 框选区域批量删除 |

### 视觉反馈

- **绿色半透明**：放置预览区域
- **红色半透明**：删除预览区域

## 配置参数

### BlockEditorSettings

| 参数 | 类型 | 说明 |
|------|------|------|
| `blockPrefabs` | `List<GameObject>` | 可放置的方块预制体列表 |
| `visibleLayers` | `LayerMask` | 编辑模式下可见的层级 |
| `depth` | `int` | 沿 z 轴放置的层数 (最小值 1) |
| `placeColor` | `Color` | 放置预览颜色 (默认绿色半透明) |
| `deleteColor` | `Color` | 删除预览颜色 (默认红色半透明) |

### 创建配置

1. 首次打开窗口时，如果配置不存在，点击 "创建配置" 按钮
2. 或手动创建：`Assets → Create → Settings → BlockEditor`
3. 配置保存路径：`Assets/Settings/BlockEditorSettings.asset`

## 核心机制

### 放置逻辑

1. 坐标对齐到整数网格
2. 检查目标位置是否已有方块组
3. 创建坐标组 `(x,y)` 作为父物体
4. 根据深度设置，在 z = -(depth-1) 到 z = (depth-1) 范围放置方块
5. 使用 `PrefabUtility.InstantiatePrefab` 保持预制体连接

```csharp
// 深度 = 2 时，放置 z = -1, 0, 1 共 3 层
var zStart = -(depth - 1);
var zEnd = depth - 1;
```

### 删除逻辑

1. 查找 Grid 根物体
2. 按坐标名称 `(x,y)` 查找目标组
3. 删除整个坐标组及其子物体

### 输入处理

使用 `SceneView.duringSceneGui` 回调处理 SceneView 事件：

```csharp
SceneView.duringSceneGui += OnSceneGUI;
```

坐标转换：屏幕坐标 → 世界坐标 → 网格坐标

```csharp
var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
var worldPos = new Vector3(ray.origin.x, ray.origin.y, 0);
var gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
```

### Undo 支持

所有操作都支持 Ctrl+Z 撤销：

```csharp
Undo.SetCurrentGroupName("Place Blocks");
var undoGroup = Undo.GetCurrentGroup();

// ... 执行操作 ...

Undo.CollapseUndoOperations(undoGroup);
```

## 公共接口

### BlockEditorWindow

```csharp
// 打开编辑器窗口
[MenuItem("Window/Block Editor")]
public static void ShowWindow();
```

### BlockEditorSettings

通过 `[CreateAssetMenu]` 特性支持资产创建菜单。

## 最佳实践

### 预制体准备

- 方块预制体应为 1x1x1 单位大小
- 预制体锚点应在中心
- 建议将方块设置到专用 Layer（如 "Environment"）

### 编辑流程

1. 打开 BlockEditor 窗口
2. 配置方块预制体列表
3. 选择要放置的方块
4. 开启编辑模式
5. 在 SceneView 中放置/删除方块
6. 完成后关闭编辑模式

### 性能考虑

- 大量方块时考虑使用 LOD 或遮挡剔除
- 静态方块可标记为 Static 以启用批处理

## 常见问题

### Q: 为什么放置的方块位置不对?

检查：
- SceneView 是否在 2D 模式
- 鼠标是否在 SceneView 内
- 方块预制体的锚点是否在中心

### Q: 如何修改已放置方块的深度?

删除现有方块组，修改深度设置后重新放置。

### Q: 方块没有保持预制体连接?

确保使用的是 Project 中的预制体资产，而非场景中的实例。

## 相关资源

- [项目概览](./project-overview.md)
- `Assets/_Scripts/Editor/BlockEditorWindow.cs:1` - 编辑器窗口源码
- `Assets/_Scripts/Settings/BlockEditorSettings.cs:1` - 配置类源码