# 玩家运动系统指南

## 概述

PlayerMotor 是玩家角色的核心运动控制器，适用于横板 3D 游戏。使用 Rigidbody + CapsuleCollider 实现物理驱动的移动、跳跃、喷气背包和蹲伏功能。

## 架构设计

### 组件依赖

```
Player GameObject
├── Rigidbody           → 物理驱动
├── CapsuleCollider     → 碰撞体
├── InputManager        → 输入处理
├── PlayerMotor         → 运动控制
├── RopeSystem          → 绳索系统
└── PlayerGlue          → 模块粘合 (持有配置)
```

### 配置系统

运动参数通过 ScriptableObject 进行配置：

| 文件 | 作用 |
|------|------|
| `Assets/_Scripts/Settings/PlayerMotorSettings.cs` | 配置类定义 |
| `Assets/_Scripts/Settings/DefaultPlayerMotorSettings.asset` | 默认配置资产 |

配置由 `PlayerGlue` 在 Awake 中注入：

```csharp
// PlayerGlue.cs
_playerMotor.Initialize(motorSettings);
```

## 运动机制

### 水平移动

使用增量速度修改方式，支持加速和减速：

```csharp
// 有输入时加速
float targetSpeed = inputX * _settings.maxSpeed;
_frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, targetSpeed, _settings.acceleration * Time.fixedDeltaTime);

// 无输入时减速 (地面/空中减速度不同)
float deceleration = _grounded ? _settings.groundDeceleration : _settings.airDeceleration;
_frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
```

### 跳跃机制

#### 基础跳跃

按下跳跃键时，如果在地面或土狼时间内，设置垂直速度为跳跃力：

```csharp
_frameVelocity.y = _settings.jumpPower;
```

#### 土狼时间 (Coyote Time)

离开地面后的短暂缓冲时间，允许玩家在落下瞬间仍可跳跃：

```csharp
private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _settings.coyoteTime;
```

#### 可变跳跃高度

提前释放跳跃键时，增加重力倍增器实现短跳：

```csharp
if (_endedJumpEarly && _frameVelocity.y > 0)
{
    gravity *= _settings.jumpEndEarlyGravityModifier;
}
```

### 喷气背包

空中且有燃料时，长按跳跃键激活喷气背包：

```csharp
// 应用推力
_frameVelocity.y += _settings.jetpackForce * Time.fixedDeltaTime;

// 限制最大上升速度
_frameVelocity.y = Mathf.Min(_frameVelocity.y, _settings.maxRiseSpeed);

// 消耗燃料
_jetpackFuel -= Time.fixedDeltaTime;
```

燃料仅在地面时恢复：

```csharp
if (_grounded && !_jetpackActive && _jetpackFuel < _settings.jetpackMaxFuel)
{
    _jetpackFuel += _settings.jetpackFuelRecovery * Time.fixedDeltaTime;
}
```

### 地面检测

使用 SphereCast 从胶囊体底部向下检测：

```csharp
Vector3 origin = transform.position + Vector3.up * _col.radius;
float radius = _col.radius * 0.9f;

bool groundHit = Physics.SphereCast(
    origin, radius, Vector3.down, out _,
    _settings.grounderDistance,
    _settings.environmentLayer,
    QueryTriggerInteraction.Ignore
);
```

### 着地力 (Grounding Force)

站在地面时施加向下力，防止在斜坡上滑动：

```csharp
if (_grounded && _frameVelocity.y <= 0f && !_jetpackActive)
{
    _frameVelocity.y = _settings.groundingForce;  // 负值向下
}
```

### 蹲伏

切换蹲伏状态时，动态调整碰撞体高度和中心位置：

```csharp
// 蹲伏
_col.height = _settings.crouchHeight;
_col.center = new Vector3(0f, _settings.crouchHeight / 2f, 0f);

// 站立（需先检测头顶无障碍）
_col.height = _standingHeight;
_col.center = new Vector3(0f, _standingCenterY, 0f);
```

蹲伏时移动速度降低：

```csharp
float speedMultiplier = _isCrouching ? _settings.crouchSpeedMultiplier : 1f;
float targetSpeed = inputX * _settings.maxSpeed * speedMultiplier;
```

蹲伏状态下按跳跃会先尝试站起，站起成功后需再次按跳跃才能跳跃。

## 配置参数

### 层级

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `environmentLayer` | - | 环境层级，用于地面检测 |

### 移动

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `maxSpeed` | 14 | 最大水平移动速度 |
| `acceleration` | 120 | 水平加速度 |
| `groundDeceleration` | 60 | 地面上无输入时的减速度 |
| `airDeceleration` | 30 | 空中无输入时的减速度 |

### 跳跃

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `jumpPower` | 12 | 跳跃时的初始垂直速度 |
| `maxFallSpeed` | 20 | 下落时的最大速度限制 |
| `fallAcceleration` | 50 | 空中下落时的重力加速度 |
| `jumpEndEarlyGravityModifier` | 3 | 提前释放跳跃键时的重力倍增 |
| `coyoteTime` | 0.15 | 离开地面后仍可跳跃的缓冲时间 |

### 地面检测

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `grounderDistance` | 0.1 | 向下检测地面的射线距离 |
| `groundingForce` | -1.5 | 站在地面时施加的向下力 |

### 喷气背包

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `jetpackForce` | 160 | 喷气背包向上的推力 |
| `maxRiseSpeed` | 15 | 使用喷气背包时的最大上升速度 |
| `jetpackMaxFuel` | 2 | 喷气背包的最大燃料容量 |
| `jetpackFuelRecovery` | 0.5 | 站在地面时燃料的恢复速率 |

### 蹲伏

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `crouchHeight` | 1 | 蹲伏时碰撞体高度 |
| `crouchSpeedMultiplier` | 0.5 | 蹲伏移动速度倍率 |

## 事件

| 事件 | 参数 | 说明 |
|------|------|------|
| `GroundedChanged` | `(bool isGrounded, float impactVelocity)` | 落地状态变化 |
| `Jumped` | - | 跳跃触发 |

## 公共接口

```csharp
// 初始化配置 (由 PlayerGlue 调用)
public void Initialize(PlayerMotorSettings settings);

// 设置移动输入 (由 InputManager 事件触发)
public void SetMoveInput(Vector2 input);

// 跳跃按下
public void OnJumpPressed();

// 跳跃释放
public void OnJumpReleased();

// 切换蹲伏状态
public void ToggleCrouch();
```

## 生命周期

| 阶段 | 操作 |
|------|------|
| `Awake` | (无操作，等待配置注入) |
| `Start` | 获取组件引用，配置 Rigidbody，初始化燃料 |
| `Update` | 更新时间计数 |
| `FixedUpdate` | 物理计算：碰撞检测 → 跳跃 → 水平移动 → 重力 → 喷气背包 → 朝向 → 燃料恢复 → 应用速度 |

## 调试可视化

在编辑器中选中 Player 时，会显示检测范围的 Gizmo：
- 红色球体：地面检测范围
- 青色球体：头顶检测范围（仅蹲伏状态显示）

## 相关资源

- [输入系统指南](./input-system-guide.md)
- [绳索系统指南](./rope-system-guide.md)
- `Assets/_Scripts/PlayerMotor.cs:1` - 运动控制器源码
- `Assets/_Scripts/Settings/PlayerMotorSettings.cs:1` - 配置类源码
