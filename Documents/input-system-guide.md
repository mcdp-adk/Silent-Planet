# 输入系统指南

## 概述

本项目使用 Unity Input System (新版输入系统) 实现跨平台输入处理。输入系统采用模块化设计，通过事件驱动的方式与游戏逻辑解耦。

## 架构设计

### 三层架构

```
Unity Input System          → 原始输入设备
    ↓
InputManager (输入层)       → 输入事件处理和转换
    ↓ (C# Action 事件)
PlayerGlue (粘合层)         → 模块间连接协调
    ↓
PlayerMotor (逻辑层)        → 实际游戏逻辑
```

### 核心文件

| 文件路径 | 作用 | 说明 |
|---------|------|------|
| `Assets/Settings/InputSystem_Actions.inputactions` | 输入配置 | Unity 可视化配置，定义所有输入动作和绑定 |
| `Assets/_Scripts/InputSystem_Actions.cs` | 自动生成代码 | 由 Unity 根据 .inputactions 文件自动生成 |
| `Assets/_Scripts/InputManager.cs` | 输入管理器 | 实现输入回调接口，发布 C# Action 事件 |
| `Assets/_Scripts/PlayerGlue.cs` | 粘合层 | 连接输入事件到玩家控制器 |

## InputManager 详解

### 职责

- 实现 `InputSystem_Actions.IPlayerActions` 接口
- 处理所有输入回调
- 通过 C# Action 发布输入事件
- 管理输入系统生命周期

### 事件列表

```csharp
// 持续输入事件 (Vector2)
public Action<Vector2> OnMoveInput;      // 移动输入 (WASD/摇杆)
public Action<Vector2> OnLookInput;      // 视角输入 (鼠标/右摇杆)

// 按钮事件 (单次触发)
public Action OnAttackInput;             // 攻击
public Action OnCrouchInput;             // 蹲伏
public Action OnSprintInput;             // 冲刺
public Action OnPreviousInput;           // 上一个
public Action OnNextInput;               // 下一个

// 交互事件 (区分 Tap/Hold)
public Action OnInteractTap;             // 短按交互
public Action OnInteractHold;            // 长按交互

// 跳跃事件 (区分 Tap/Hold/Release)
public Action OnJumpTap;                 // 短按跳跃
public Action OnJumpHold;                // 长按跳跃 (喷气背包)
public Action OnJumpReleased;            // 跳跃释放
```

### 使用示例

```csharp
// 1. 在需要响应输入的类中订阅事件
private void Awake()
{
    var inputManager = GetComponent<InputManager>();
    inputManager.OnMoveInput += HandleMove;
    inputManager.OnJumpTap += HandleJump;
}

// 2. 实现事件处理方法
private void HandleMove(Vector2 input)
{
    // 处理移动输入
}

private void HandleJump()
{
    // 处理跳跃
}

// 3. 清理时取消订阅
private void OnDestroy()
{
    inputManager.OnMoveInput -= HandleMove;
    inputManager.OnJumpTap -= HandleJump;
}
```

## 输入动作映射

### Player 动作映射

所有玩家相关的输入动作都在 `Player` ActionMap 中定义：

| 动作名称 | 类型 | 键盘/鼠标 | 手柄 | 交互方式 |
|---------|------|----------|------|----------|
| Move | Value (Vector2) | WASD/方向键 | 左摇杆 | 持续 |
| Look | Value (Vector2) | 鼠标移动 | 右摇杆 | 持续 |
| Attack | Button | 鼠标左键/Enter | 西键 (X) | 按下 |
| Jump | Button | 空格 | 南键 (A) | Tap/Hold |
| Sprint | Button | 左 Shift | 左摇杆按下 | 按下 |
| Interact | Button | E 键 | 北键 (Y) | Tap/Hold |
| Crouch | Button | C 键 | 东键 (B) | 按下 |
| Previous | Button | 1 键 | 方向键左 | 按下 |
| Next | Button | 2 键 | 方向键右 | 按下 |

### UI 动作映射

UI 相关输入在 `UI` ActionMap 中 (已配置但未实现逻辑)：

- Navigate (方向导航)
- Submit (确认)
- Cancel (取消)
- Point (鼠标位置)
- Click/RightClick/MiddleClick (鼠标点击)
- ScrollWheel (滚轮)

### 控制方案

系统支持多种输入设备：

- **Keyboard&Mouse**: 键盘 + 鼠标
- **Gamepad**: Xbox/PlayStation 手柄
- **Touch**: 触摸屏
- **Joystick**: 通用游戏杆
- **XR**: VR 控制器

## Interaction 系统

### Tap vs Hold 区分

`InputManager` 通过检查 `context.interaction` 类型来区分用户意图：

```csharp
public void OnJump(InputAction.CallbackContext context)
{
    if (context.performed)
    {
        // Tap: 短按跳跃
        if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
        {
            OnJumpTap?.Invoke();
        }
        // Hold: 长按启动喷气背包
        else if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            OnJumpHold?.Invoke();
        }
    }
    else if (context.canceled)
    {
        // 释放按键
        OnJumpReleased?.Invoke();
    }
}
```

### 输入阶段

Unity Input System 的输入回调有三个阶段：

- **started**: 输入开始时触发
- **performed**: 输入满足条件时触发 (如 Hold 达到时间阈值)
- **canceled**: 输入取消时触发 (如释放按键)

## 修改输入配置

### 可视化编辑

1. 在 Unity 编辑器中打开 `Assets/Settings/InputSystem_Actions.inputactions`
2. 使用 Input Actions 窗口修改：
   - Action Maps (动作映射组)
   - Actions (具体动作)
   - Bindings (按键绑定)
   - Interactions (交互方式)
   - Processors (输入处理器)

### 添加新输入

**步骤**:

1. 在 `.inputactions` 文件中添加新 Action
2. Unity 自动重新生成 `InputSystem_Actions.cs`
3. 在 `InputManager.cs` 中添加对应的事件和回调实现
4. 在 `PlayerGlue.cs` 中连接新事件到目标模块

**示例**: 添加 "冲刺" 输入

```csharp
// InputManager.cs
public Action OnSprintInput;  // 1. 添加事件

public void OnSprint(InputAction.CallbackContext context)  // 2. 实现回调
{
    if (context.performed)
    {
        OnSprintInput?.Invoke();
    }
}

// PlayerGlue.cs
private void ConnectInputToMovement()
{
    _inputManager.OnSprintInput += _playerMotor.HandleSprint;  // 3. 连接事件
}
```

## PlayerGlue 粘合层

### 职责

- 获取 `InputManager` 和 `PlayerMotor` 的组件引用
- 在 `Awake()` 中连接输入事件到移动方法
- 在 `OnDestroy()` 中断开所有事件连接

### 代码示例

```csharp
private void ConnectInputToMovement()
{
    if (_inputManager != null && _playerMotor != null)
    {
        _inputManager.OnMoveInput += _playerMotor.SetMoveInput;
        _inputManager.OnJumpTap += _playerMotor.OnJumpTap;
        _inputManager.OnJumpHold += _playerMotor.OnJumpHold;
        _inputManager.OnJumpReleased += _playerMotor.OnJumpReleased;
    }
}

private void DisconnectInputFromMovement()
{
    if (_inputManager != null && _playerMotor != null)
    {
        _inputManager.OnMoveInput -= _playerMotor.SetMoveInput;
        _inputManager.OnJumpTap -= _playerMotor.OnJumpTap;
        _inputManager.OnJumpHold -= _playerMotor.OnJumpHold;
        _inputManager.OnJumpReleased -= _playerMotor.OnJumpReleased;
    }
}
```

## 最佳实践

### 事件订阅

- ✅ 在 `Awake()` 或 `Start()` 中订阅事件
- ✅ 在 `OnDestroy()` 或 `OnDisable()` 中取消订阅
- ✅ 使用 `?.Invoke()` 安全调用事件
- ❌ 避免在 `Update()` 中订阅/取消订阅

### 输入检测

- ✅ 使用 `InputThreshold` 过滤极小输入值，防止摇杆抖动
- ✅ 检查 `context.performed` 而非 `context.started` 用于按钮动作
- ✅ 对于持续输入 (如移动)，每帧读取最新值
- ❌ 避免直接访问 Input System 的原始数据

### 模块解耦

- ✅ 通过事件通信，避免模块间直接引用
- ✅ 输入逻辑与游戏逻辑分离
- ✅ 使用 `PlayerGlue` 管理模块连接
- ❌ 避免在 `PlayerMotor` 中直接访问 `InputManager`

## 调试技巧

### 输入日志

在 `InputManager` 的回调中添加调试日志：

```csharp
public void OnMove(InputAction.CallbackContext context)
{
    var input = context.ReadValue<Vector2>();
    Debug.Log($"Move Input: {input}");
    OnMoveInput?.Invoke(input);
}
```

### Input Debugger

使用 Unity Input System Debugger 查看实时输入状态：

1. Window → Analysis → Input Debugger
2. 查看当前激活的 Action Maps
3. 监控各个 Action 的状态

## 常见问题

### Q: 为什么输入没有响应?

**检查清单**:
- [ ] InputManager 组件是否附加到 GameObject 上?
- [ ] PlayerGlue 是否正确连接了事件?
- [ ] Player ActionMap 是否已启用? (`_actions.Player.Enable()`)
- [ ] 输入设备是否正确连接?

### Q: 如何支持新的输入设备?

在 `.inputactions` 文件的 Control Schemes 中添加新设备方案，并为每个 Action 添加对应的 Binding。

### Q: 如何禁用某个输入?

```csharp
// 临时禁用整个 Player ActionMap
_actions.Player.Disable();

// 禁用特定 Action
_actions.Player.Move.Disable();
```

### Q: 如何在运行时更改按键绑定?

使用 Input System 的 Rebinding API:

```csharp
var rebindOperation = _actions.Player.Move
    .PerformInteractiveRebinding()
    .OnComplete(operation => { /* 绑定完成 */ })
    .Start();
```

## 相关资源

- [Unity Input System 官方文档](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
- [项目概览文档](./project-overview.md)
- `Assets/_Scripts/InputManager.cs:1` - 输入管理器源码
- `Assets/_Scripts/PlayerGlue.cs:1` - 粘合层源码
