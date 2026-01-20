# 绳索系统指南

## 概述

RopeSystem 是动态生成的弹簧绳索系统。玩家长按交互键时在当前位置部署绳索锚点，绳索末端连接到玩家，形成物理约束。再次长按交互键收回绳索。

## 架构设计

### 运行时结构

```
Scene
├── Player
│   ├── Rigidbody
│   ├── SpringJoint        ← 部署时动态创建，连接到绳索末端
│   ├── InputManager
│   ├── PlayerMotor
│   ├── RopeSystem         ← 管理绳索生命周期
│   └── PlayerGlue         ← 持有配置，连接输入事件
│
└── RuntimeStringRope      ← 部署时动态创建的容器
    ├── LineRenderer       ← 绳索可视化
    ├── Node_0             ← 锚点节点 (isKinematic=true)
    │   ├── Rigidbody
    │   └── SphereCollider
    ├── Node_1             ← 中间节点
    │   ├── Rigidbody
    │   ├── SphereCollider
    │   └── SpringJoint    ← 连接到 Node_0
    ├── Node_2
    │   └── SpringJoint    ← 连接到 Node_1
    └── ...
```

### 配置系统

绳索参数通过 ScriptableObject 进行配置：

| 文件 | 作用 |
|------|------|
| `Assets/_Scripts/Settings/RopeSystemSettings.cs` | 配置类定义 |
| `Assets/_Scripts/Settings/DefaultRopeSystemSettings.asset` | 默认配置资产 |

配置由 `PlayerGlue` 在 OnEnable 中注入：

```csharp
// PlayerGlue.cs
_ropeSystem.Initialize(ropeSettings);
```

## 核心机制

### 绳索部署

长按交互键触发 `ToggleRope()`：

```csharp
// InputManager.OnInteractHold → RopeSystem.ToggleRope
public void ToggleRope()
{
    if (IsDeployed) RetractRope();
    else DeployRope();
}
```

首次部署时创建节点，后续部署复用节点：

```csharp
if (!_nodesCreated)
{
    CreateRopeNodes(anchorPosition);  // 首次：创建节点
    _nodesCreated = true;
}
else
{
    ReactivateRope(anchorPosition);   // 复用：重新定位节点
}
```

### 节点链

所有节点初始生成在锚点位置，通过 SpringJoint 连接，靠重力自然下垂：

```csharp
for (int i = 0; i < _settings.nodeCount; i++)
{
    var node = CreateNode(i, anchorPosition);
    _nodes.Add(node);

    if (i > 0)
    {
        ConfigureNodeSpringJoint(node.gameObject, _nodes[i - 1]);
    }
}
```

### SpringJoint 配置

节点间 SpringJoint：

```csharp
joint.autoConfigureConnectedAnchor = false;
joint.anchor = Vector3.zero;
joint.connectedAnchor = Vector3.zero;
joint.spring = _settings.springStrength;
joint.damper = _settings.springDamper;
joint.minDistance = 0f;
joint.maxDistance = _settings.nodeSpacing;  // 最大伸展距离
```

玩家与末端节点 SpringJoint：

```csharp
_playerSpringJoint.spring = _settings.playerSpringStrength;
_playerSpringJoint.damper = _settings.playerSpringDamper;
_playerSpringJoint.minDistance = 0f;
_playerSpringJoint.maxDistance = 0f;  // 紧密连接
_playerSpringJoint.massScale = _settings.playerMassScale;
_playerSpringJoint.connectedMassScale = _settings.connectedMassScale;
```

### 绳索收回

收回时销毁玩家的 SpringJoint 并禁用容器：

```csharp
private void RetractRope()
{
    if (_playerSpringJoint != null)
    {
        Destroy(_playerSpringJoint);  // 销毁而非 disable
        _playerSpringJoint = null;
    }
    _ropeContainer.SetActive(false);
    IsDeployed = false;
}
```

**注意**：Unity Joint 组件没有 `enabled` 属性，设置 `connectedBody = null` 会连接到世界空间而非断开连接，因此必须销毁组件。

### 节点复用

为减少 GC，节点在首次创建后不销毁，复用时重置位置和速度：

```csharp
private void ReactivateRope(Vector3 anchorPosition)
{
    _ropeContainer.SetActive(true);

    foreach (var node in _nodes)
    {
        node.position = anchorPosition;
        if (node.isKinematic) continue;
        node.linearVelocity = Vector3.zero;
        node.angularVelocity = Vector3.zero;
    }
}
```

## 配置参数

### 绳索结构

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `nodeCount` | 20 | 绳索节点数量，不包括玩家连接点 |
| `nodeSpacing` | 0.5 | 节点之间的最大间距 |

### 物理参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `nodeMass` | 0.1 | 单个节点的质量 |
| `nodeDamping` | 1 | 节点的线性阻尼，减少晃动 |
| `springStrength` | 1000 | 节点间弹簧的弹力强度 |
| `springDamper` | 0.2 | 节点间弹簧的阻尼 |

### 碰撞参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `colliderRadius` | 0.2 | 节点碰撞体的半径 |
| `ropeLayer` | - | 绳索节点所属的物理层级 |

### 玩家连接

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `playerAnchorOffset` | (0, 1, 0) | 绳索连接点相对于玩家的偏移 |
| `playerSpringStrength` | 100 | 玩家与绳索末端连接的弹力强度 |
| `playerSpringDamper` | 0.2 | 玩家与绳索末端连接的弹簧阻尼 |
| `playerMassScale` | 1 | 玩家侧的质量缩放系数 |
| `connectedMassScale` | 1 | 绳索侧的质量缩放系数 |

### 可视化

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `gizmoColor` | Yellow | 编辑器中 Gizmos 的绘制颜色 |
| `lineWidth` | 0.1 | 绳索线条的宽度 |
| `lineMaterial` | - | 绳索线条使用的材质 |

## 公共接口

```csharp
// 初始化配置 (由 PlayerGlue 调用)
public void Initialize(RopeSystemSettings settings);

// 切换绳索状态 (由 InputManager.OnInteractHold 触发)
public void ToggleRope();

// 当前是否已部署
public bool IsDeployed { get; }
```

## 生命周期

| 阶段 | 操作 |
|------|------|
| `Awake` | (无操作，等待配置注入) |
| `Start` | 初始化 IsDeployed = false |
| `LateUpdate` | 更新 LineRenderer 位置 |
| `OnDrawGizmos` | 绘制节点调试球体 |

## 使用方式

1. 在 Player GameObject 上添加 `RopeSystem` 组件
2. 创建 `RopeSystemSettings` 资产并配置参数
3. 在 `PlayerGlue` 的 Inspector 中拖入配置资产
4. 运行时长按交互键 (默认 E) 部署/收回绳索

## 调试可视化

- **LineRenderer**: 运行时显示绳索线条
- **Gizmos**: 编辑器中显示节点碰撞体球体

## 注意事项

1. 所有 SpringJoint 的 `autoConfigureConnectedAnchor = false`
2. 节点全部生成在同一位置，靠物理自然下垂
3. 节点复用而非销毁，减少 GC 压力
4. 玩家与末端节点 `minDistance = maxDistance = 0` 实现紧密连接
5. Joint 组件无法禁用，只能销毁重建

## 相关资源

- [输入系统指南](./input-system-guide.md)
- [玩家运动系统指南](./player-motor-guide.md)
- `Assets/_Scripts/RopeSystem.cs:1` - 绳索系统源码
- `Assets/_Scripts/Settings/RopeSystemSettings.cs:1` - 配置类源码
