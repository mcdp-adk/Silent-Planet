# Tarodev 2D 控制器概览

## 项目简介
**Tarodev 2D Controller** 是一个平台动作游戏的可复用角色控制器，位于 `Assets/Plugins/Tarodev 2D Controller`。该套件提供基于物理的角色控制、可脚本化的运动参数，以及随玩家状态变化的示例动画器，能够实现灵敏的水平移动、跳跃缓冲、土狼时间（Coyote Time）和落地反馈等特性。

## 核心脚本

### ScriptableStats（`Assets/Plugins/Tarodev 2D Controller/_Scripts/ScriptableStats.cs`）
- ScriptableObject，用于集中配置控制器的关键参数（移动、重力、跳跃行为、输入死区、图层遮罩等）。
- 主要参数分组：
  - **输入（Input）：** `SnapInput`、`HorizontalDeadZoneThreshold`、`VerticalDeadZoneThreshold`。
  - **移动（Movement）：** `MaxSpeed`、`Acceleration`、`GroundDeceleration`、`AirDeceleration`、`GroundingForce`、`GrounderDistance`。
  - **跳跃（Jump）：** `JumpPower`、`MaxFallSpeed`、`FallAcceleration`、`JumpEndEarlyGravityModifier`、`CoyoteTime`、`JumpBuffer`。
- 通过 *Create ▶ Tarodev Controller ▶ ScriptableStats* 创建资产，调整参数后赋给 `PlayerController`。

### PlayerController（`Assets/Plugins/Tarodev 2D Controller/_Scripts/PlayerController.cs`）
- `MonoBehaviour` + `IPlayerController` 的实现，负责所有角色物理行为。
- **输入采集（`GatherInput`）：** 每帧读取移动向量与跳跃状态，可选输入离散化以兼顾手柄与键盘。
- **碰撞检测（`CheckCollisions`）：** 使用 `CapsuleCast` 侦测地面与顶面，更新落地状态，启用土狼时间和跳跃缓冲，并触发 `GroundedChanged` 事件。
- **跳跃系统（`HandleJump` / `ExecuteJump`）：** 支持跳跃缓冲、土狼时间、提前松跳快速下落，并在正式起跳时触发 `Jumped` 事件。
- **水平移动（`HandleDirection`）：** 根据输入加速/减速，区分地面与空中的阻尼系数。
- **重力处理（`HandleGravity`）：** 地面施加持续向下的“贴地力”，空中则按可配置的下落加速度处理，并支持提前松键加重下坠。
- **运动应用（`ApplyMovement`）：** 在每次 `FixedUpdate` 中将计算得到的帧速度写入 `Rigidbody2D`。
- 对外暴露 `FrameInput`、`GroundedChanged`、`Jumped` 等事件供动画器等组件使用（参见 `PlayerController.cs:25-28`）。

### PlayerAnimator（`Assets/Plugins/Tarodev 2D Controller/_Scripts/PlayerAnimator.cs`）
- 示例动画组件，订阅 `IPlayerController.Jumped` 与 `GroundedChanged`。
- 根据 `FrameInput` 驱动动画参数（`Grounded`、`IdleSpeed`、`Jump`），并控制精灵翻转与倾斜。
- 触发跳跃、起步、移动尘埃、落地等粒子效果以及脚步音。
- 通过 `Physics2D.Raycast` 采样地面颜色，动态调节粒子系统色调。

## 事件流与整合方式
- `PlayerController` 通过 `FrameInput`、`GroundedChanged`、`Jumped` 输出角色运动状态。
- `PlayerAnimator` 以及其他系统订阅上述事件，实现动画、特效、音频反馈。
- 创建 `ScriptableStats` 资产并挂载到 `PlayerController`，即可微调手感。
- 玩家对象需包含 `Rigidbody2D`、`Collider2D`（建议胶囊碰撞体）、`PlayerController`，以及可选的 `PlayerAnimator` 和对应的粒子/音频引用。

## 调参与扩展建议
- 调整 **加速度/减速度** 以在灵活性和漂浮感之间取得平衡。
- 利用 **GroundingForce** 保持角色在斜坡上稳定贴地，避免颤动。
- 增大 **JumpBuffer** 或 **CoyoteTime** 以获得更宽容的跳跃判定窗口。
- 基于 `IPlayerController` 事件扩展战斗、技能或 UI 反馈等功能模块。