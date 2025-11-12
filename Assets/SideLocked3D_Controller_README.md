# 侧向锁定视角的 3D 玩家控制器

## 目标
在 3D 场景中构建一个行为类似 2D 横版平台的角色控制器。角色主要在 X 轴移动，Y 轴处理重力与跳跃，Z 轴被固定以保持单一轨道。此模式适用于“2.5D” 关卡：环境与摄像机是 3D 的，但游戏性保持在同一平面。

## 推荐架构

| 系统 | 职责 | 备注 |
| --- | --- | --- |
| `PlayerMotor` (MonoBehaviour) | 持有刚体，处理输入，更新速度，并强制平面限制。 | 使用 3D `Rigidbody` + `CapsuleCollider`，通过约束或手动矫正锁定 Z 轴，并冻结所有旋转。 |
| `GroundSensor` | 通过射线或球形射线检测地面。 | 可复用组件，负责切换落地状态并输出地面法线。 |
| `PlayerStats` (ScriptableObject) | 存储运动调参：最大速度、加速度、减速度、重力、跳跃力度、土狼/缓冲时间、空中控制、轨道位置。 | 参考 Tarodev 套件中的 `ScriptableStats`，将数据调整为 3D 物理适配值。 |
| `PlayerAnimator3D` | 根据 `PlayerMotor` 事件驱动 Mecanim 动画、VFX 与音效。 | 将速度映射到 Blend Tree，触发跳跃/落地状态，可选倾斜动画。 |
| `InputReader` | 将 Unity Input System 的输入映射到简洁的 `FrameInput` 结构。 | 保持输入层解耦，以便复用在多角色或 AI。 |
| 摄像机装备 | 保持正交或窄透视的侧向视角。 | Cinemachine Framing Transposer 并锁定 Z 偏移效果较好。 |

## 核心机制

1. **输入采集**
   - 读取水平轴（A/D、左摇杆）和跳跃按键。
   - 如规划梯子/穿透平台，可预留垂直输入。
   - 输出与 Tarodev 控制器类似的 `FrameInput` 供其他系统使用。

2. **平面移动**
   - 将水平输入转换为沿 `transform.right`（X 轴）的目标速度。
   - 使用不同的地面/空中加速度与减速度趋近 `targetSpeed = input.x * MaxSpeed`。
   - 当输入低于死区时施加阻尼，防止手柄轻微漂移。

3. **跳跃与重力**
   - 通过计时器维护土狼时间与跳跃缓冲（可复用 `PlayerController.cs:126-138` 的逻辑）。
   - 执行跳跃时施加竖直冲量：`velocity.y = JumpPower`。
   - 自定义重力：`velocity.y = Mathf.MoveTowards(velocity.y, -MaxFallSpeed, FallAcceleration * dt)`，并在提前松跳时施加倍率以缩短跳跃。

4. **地面检测**
   - 在脚部使用短距离 `Physics.SphereCast` 或 `Physics.Raycast`，忽略玩家图层，距离可配置。
   - 记录接触法线以调整贴地力并支持小坡度。
   - 发出 `GroundedChanged(bool grounded, float impactVelocity)` 事件供动画与特效使用。

5. **轨道锁定（Z 轴约束）**
   - 最简单方式：在 Rigidbody 约束中冻结 Z 位置（`RigidbodyConstraints.FreezePositionZ`）。
   - 或在物理步骤后手动重置 Z，便于自定义偏移。
   - 防止外力推动角色出轨：`velocity = Vector3.ProjectOnPlane(velocity, Vector3.forward) + Vector3.forward * laneZ`。

6. **斜坡处理**
   - 落地时将目标速度投影到地面平面：`Vector3.OrthoNormalize(ref groundNormal, ref moveDirection)`，平滑跟随坡道。
   - 落地状态施加小幅向下作用力，避免在缓坡上悬空。

7. **摄像机与表现**
   - 使用 Cinemachine 虚拟摄像机，`Follow` 对准玩家，`LookAt` 保持人物方向。
   - 设置正交或低视场透视镜头，使镜头面对轨道。
   - 可通过 Z 轴摆放背景道具实现视差，同时将玩法物体固定在轨道中心。

## 实现步骤

1. **创建统计数据资产**
   - ScriptableObject，序列化速度、加速度、重力、跳跃、土狼/缓冲时间、轨道宽度、地面检测半径等字段。
   - 建议放置在 `Assets/Data/PlayerStats`，并挂接到控制器组件。

2. **编写 `PlayerMotor` 脚本**
   - 在 `Awake` 中缓存 Rigidbody、Collider、Stats 资产。
   - 在 `Update` 中收集输入（使用 Input System 或 `InputReader`）。
   - 在 `FixedUpdate` 中依次执行：`UpdateTimers`、`CheckGrounded`、`HandleJump`、`HandleHorizontal`、`HandleGravity`、`ApplyMovement`、`ClampLane`。
   - 同 Tarodev 控制器一样发出 `Jumped`、`GroundedChanged` 事件供复用。

3. **搭建角色对象**
   - 添加 `Rigidbody`（将 `Interpolation` 设为 Interpolate，`Collision Detection` 设为 Continuous）。
   - 添加竖直方向的 `CapsuleCollider`。
   - 挂载 `PlayerMotor`、`GroundSensor`（或内置检测逻辑），并链接统计数据资产。
   - 在 Rigidbody 约束中冻结 X/Y/Z 旋转与 Z 位移。

4. **动画与特效**
   - 配置 3D Mecanim 状态机：行走 Blend Tree、跳跃/落地状态、可选下落状态。
   - 利用 `PlayerMotor` 事件触发动画参数、粒子和音频。

5. **测试清单**
   - 验证平地速度一致、坡道行走无滑动、土狼/缓冲反应及时、受到平台碰撞时仍保持轨道、摄像机对齐、跳跃高度符合预期。
   - 通过调试 Gizmo 显示地面检测半径与轨道平面，便于迭代。

## 扩展思路
- 通过检测侧向接触实现贴墙滑行与墙跳，复用轨道投影逻辑。
- 支持穿透平台：检测下压 + 跳跃按键组合，暂时禁用平台碰撞体。
- 暴露与 Tarodev 控制器一致的接口，便于整合现有动画器。
- 在脚本化序列中允许临时改变 Z，以制作攀爬或过场动作。

## 从 Tarodev 2D 控制器迁移注意事项
- 复用 `FrameInput`、土狼/缓冲计时器、事件接口等概念。
- 将 `Physics2D` 改为 3D 版本（`Physics.SphereCast`、`Physics.Raycast`）。
- 使用 `Rigidbody` 替代 `Rigidbody2D`，并调整重力常量以适应 3D 单位。
- 仍可通过检测命中对象的 `Renderer.material.color` 实现基于地面颜色的粒子染色。