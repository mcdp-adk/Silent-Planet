using System;
using UnityEngine;

namespace _Scripts
{
    /// <summary>
    /// 玩家运动控制器 - 横板 3D 游戏
    /// 使用 Rigidbody + CapsuleCollider
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMotor : MonoBehaviour
    {
        #region 配置参数

        [Header("LAYERS")] [Tooltip("玩家所在层级，用于碰撞检测排除")] [SerializeField]
        private LayerMask playerLayer;

        [Header("MOVEMENT")] [Tooltip("最大水平移动速度")] [SerializeField]
        private float maxSpeed = 14f;

        [Tooltip("加速度")] [SerializeField] private float acceleration = 120f;

        [Tooltip("地面减速度")] [SerializeField] private float groundDeceleration = 60f;

        [Tooltip("空中减速度")] [SerializeField] private float airDeceleration = 30f;

        [Header("JUMP")] [Tooltip("跳跃初速度")] [SerializeField]
        private float jumpPower = 12f;

        [Tooltip("最大下落速度")] [SerializeField] private float maxFallSpeed = 20f;

        [Tooltip("下落加速度（空中重力）")] [SerializeField]
        private float fallAcceleration = 50f;

        [Tooltip("提前释放跳跃时的重力倍增器")] [SerializeField]
        private float jumpEndEarlyGravityModifier = 3f;

        [Tooltip("土狼时间（离开地面后仍可跳跃的时间）")] [SerializeField]
        private float coyoteTime = 0.15f;

        [Tooltip("跳跃缓冲时间（落地前预输入跳跃的有效时间）")] [SerializeField]
        private float jumpBuffer = 0.2f;

        [Header("GROUND CHECK")] [Tooltip("地面检测距离")] [SerializeField]
        private float grounderDistance = 0.1f;

        [Tooltip("着地力（防止在斜坡上滑动）")] [SerializeField]
        private float groundingForce = -1.5f;

        #endregion

        #region 事件

        /// <summary>
        /// 落地状态变化事件 (isGrounded, impactVelocity)
        /// </summary>
        public event Action<bool, float> GroundedChanged;

        /// <summary>
        /// 跳跃事件
        /// </summary>
        public event Action Jumped;

        #endregion

        #region 组件引用

        private Rigidbody _rb;
        private CapsuleCollider _col;

        #endregion

        #region 输入状态

        private Vector2 _moveInput;

        #endregion

        #region 移动状态

        private Vector3 _frameVelocity;
        private float _time;

        #endregion

        #region 地面检测状态

        private bool _grounded;
        private float _frameLeftGrounded = float.MinValue;

        #endregion

        #region 跳跃状态

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        #endregion

        #region 朝向状态

        private bool _facingRight = true;

        #endregion

        #region 计算属性

        /// <summary>
        /// 是否有缓冲跳跃可用
        /// </summary>
        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + jumpBuffer;

        /// <summary>
        /// 是否可使用土狼时间跳跃
        /// </summary>
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + coyoteTime;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<CapsuleCollider>();

            // 配置 Rigidbody
            _rb.useGravity = false;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private void Update()
        {
            _time += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleJump();
            HandleHorizontalMovement();
            HandleGravity();
            HandleFacing();
            ApplyMovement();
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 设置移动输入
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        /// <summary>
        /// 跳跃按下
        /// </summary>
        public void OnJumpPressed()
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        /// <summary>
        /// 跳跃释放
        /// </summary>
        public void OnJumpReleased()
        {
            // 如果在上升过程中释放跳跃，标记为提前结束
            if (!_grounded && _rb.linearVelocity.y > 0)
            {
                _endedJumpEarly = true;
            }
        }

        #endregion

        #region 碰撞检测

        private void CheckCollisions()
        {
            // 地面检测 - 从胶囊体底部向下投射
            Vector3 origin = GetGroundCheckOrigin();
            float radius = _col.radius * 0.9f;

            bool groundHit = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out _,
                grounderDistance,
                ~playerLayer,
                QueryTriggerInteraction.Ignore
            );

            // 天花板检测
            Vector3 ceilingOrigin = GetCeilingCheckOrigin();
            bool ceilingHit = Physics.SphereCast(
                ceilingOrigin,
                radius,
                Vector3.up,
                out _,
                grounderDistance,
                ~playerLayer,
                QueryTriggerInteraction.Ignore
            );

            // 撞到天花板时清除向上速度
            if (ceilingHit)
            {
                _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            }

            // 着地状态变化处理
            if (!_grounded && groundHit)
            {
                // 刚着地
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            else if (_grounded && !groundHit)
            {
                // 刚离地
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0f);
            }
        }

        private Vector3 GetGroundCheckOrigin()
        {
            // 胶囊体底部球心位置
            return transform.position + Vector3.up * _col.radius;
        }

        private Vector3 GetCeilingCheckOrigin()
        {
            // 胶囊体顶部球心位置
            return transform.position + Vector3.up * (_col.height - _col.radius);
        }

        #endregion

        #region 跳跃系统

        private void HandleJump()
        {
            // 没有跳跃请求且没有缓冲跳跃，直接返回
            if (!_jumpToConsume && !HasBufferedJump) return;

            // 在地面上或土狼时间内可以跳跃
            if (_grounded || CanUseCoyote)
            {
                ExecuteJump();
            }

            // 消费跳跃输入
            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0f;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = jumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region 水平移动

        private void HandleHorizontalMovement()
        {
            // 横板游戏只使用 X 轴移动
            float inputX = _moveInput.x;

            if (Mathf.Approximately(inputX, 0f))
            {
                // 无输入时减速
                float deceleration = _grounded ? groundDeceleration : airDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // 有输入时加速
                float targetSpeed = inputX * maxSpeed;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
            }

            // 横板游戏锁定 Z 轴
            _frameVelocity.z = 0f;
        }

        #endregion

        #region 重力系统

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                // 在地面上时施加着地力（防止在斜坡上滑动）
                _frameVelocity.y = groundingForce;
            }
            else
            {
                // 空中重力
                float gravity = fallAcceleration;

                // 提前释放跳跃时增加重力
                if (_endedJumpEarly && _frameVelocity.y > 0)
                {
                    gravity *= jumpEndEarlyGravityModifier;
                }

                // 应用重力，限制最大下落速度
                _frameVelocity.y = Mathf.MoveTowards(
                    _frameVelocity.y,
                    -maxFallSpeed,
                    gravity * Time.fixedDeltaTime
                );
            }
        }

        #endregion

        #region 朝向处理

        private void HandleFacing()
        {
            // 只有在有水平输入时才更新朝向
            if (Mathf.Approximately(_moveInput.x, 0f)) return;

            bool shouldFaceRight = _moveInput.x > 0f;

            // 朝向没有变化，直接返回
            if (shouldFaceRight == _facingRight) return;

            _facingRight = shouldFaceRight;

            // 目标旋转：右朝向 90°，左朝向 -90°（横板游戏面向 X 轴）
            float targetYRotation = _facingRight ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }

        #endregion

        #region 应用移动

        private void ApplyMovement()
        {
            _rb.linearVelocity = _frameVelocity;
        }

        #endregion

        #region 调试可视化

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_col == null) return;

            float radius = _col.radius * 0.9f;

            // 地面检测范围
            Gizmos.color = _grounded ? Color.green : Color.red;
            Vector3 groundOrigin = GetGroundCheckOrigin();
            Gizmos.DrawWireSphere(groundOrigin + Vector3.down * grounderDistance, radius);

            // 天花板检测范围
            Gizmos.color = Color.cyan;
            Vector3 ceilingOrigin = GetCeilingCheckOrigin();
            Gizmos.DrawWireSphere(ceilingOrigin + Vector3.up * grounderDistance, radius);
        }
#endif

        #endregion
    }
}