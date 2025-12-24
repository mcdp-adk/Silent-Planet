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

        [Header("LAYERS")] [Tooltip("环境层级，用于地面和天花板检测")] [SerializeField]
        private LayerMask environmentLayer;

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

        [Header("GROUND CHECK")] [Tooltip("地面检测距离")] [SerializeField]
        private float grounderDistance = 0.1f;

        [Tooltip("着地力（防止在斜坡上滑动）")] [SerializeField]
        private float groundingForce = -1.5f;

        [Header("JETPACK")] [Tooltip("喷气背包推力")] [SerializeField]
        private float jetpackForce = 160f;

        [Tooltip("最大上升速度")] [SerializeField] private float maxRiseSpeed = 15f;

        [Tooltip("最大燃料时长（秒）")] [SerializeField]
        private float jetpackMaxFuel = 2f;

        [Tooltip("燃料恢复速率（每秒）")] [SerializeField]
        private float jetpackFuelRecovery = 0.5f;

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

        #region 私有字段

        // 组件引用
        private Rigidbody _rb;
        private CapsuleCollider _col;

        // 输入
        private Vector2 _moveInput;

        // 移动
        private Vector3 _frameVelocity;
        private float _time;

        // 地面检测
        private bool _grounded;
        private float _frameLeftGrounded = float.MinValue;

        // 跳跃
        private bool _jumpToConsume;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;

        // 喷气背包
        private bool _jetpackActive;
        private float _jetpackFuel;

        // 朝向
        private bool _facingRight = true;

        #endregion

        #region 计算属性

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

            // 初始化喷气背包燃料
            _jetpackFuel = jetpackMaxFuel;
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
            HandleJetpack();
            HandleFacing();
            HandleFuelRecovery();
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
            if (_grounded || CanUseCoyote)
            {
                // 地面或土狼时间内：跳跃
                _jumpToConsume = true;
            }
            else if (_jetpackFuel > 0)
            {
                // 空中且有燃料：启动喷气背包
                _jetpackActive = true;
            }
        }

        /// <summary>
        /// 跳跃释放
        /// </summary>
        public void OnJumpReleased()
        {
            if (_jetpackActive)
            {
                // 关闭喷气背包
                _jetpackActive = false;
            }
            else if (!_grounded && _rb.linearVelocity.y > 0)
            {
                // 非喷气状态下的提前释放惩罚
                _endedJumpEarly = true;
            }
        }

        #endregion

        #region 私有方法

        private void CheckCollisions()
        {
            // 地面检测 - 从胶囊体底部向下投射
            Vector3 origin = transform.position + Vector3.up * _col.radius;
            float radius = _col.radius * 0.9f;

            bool groundHit = Physics.SphereCast(
                origin,
                radius,
                Vector3.down,
                out _,
                grounderDistance,
                environmentLayer,
                QueryTriggerInteraction.Ignore
            );

            // 天花板检测
            Vector3 ceilingOrigin = transform.position + Vector3.up * (_col.height - _col.radius);
            bool ceilingHit = Physics.SphereCast(
                ceilingOrigin,
                radius,
                Vector3.up,
                out _,
                grounderDistance,
                environmentLayer,
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

        private void HandleJump()
        {
            if (!_jumpToConsume) return;

            if (_grounded || CanUseCoyote)
            {
                _endedJumpEarly = false;
                _coyoteUsable = false;
                _frameVelocity.y = jumpPower;
                Jumped?.Invoke();
            }

            _jumpToConsume = false;
        }

        private void HandleHorizontalMovement()
        {
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

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f && !_jetpackActive)
            {
                // 在地面上时施加着地力（防止在斜坡上滑动）
                _frameVelocity.y = groundingForce;
            }
            else
            {
                float gravity = fallAcceleration;

                // 提前释放跳跃时增加重力
                if (_endedJumpEarly && _frameVelocity.y > 0)
                {
                    gravity *= jumpEndEarlyGravityModifier;
                }

                _frameVelocity.y = Mathf.MoveTowards(
                    _frameVelocity.y,
                    -maxFallSpeed,
                    gravity * Time.fixedDeltaTime
                );
            }
        }

        private void HandleJetpack()
        {
            if (_jetpackActive && _jetpackFuel > 0)
            {
                // 应用推力（惯性抵抗方式）
                _frameVelocity.y += jetpackForce * Time.fixedDeltaTime;

                // 限制最大上升速度
                _frameVelocity.y = Mathf.Min(_frameVelocity.y, maxRiseSpeed);

                // 消耗燃料
                _jetpackFuel -= Time.fixedDeltaTime;

                if (_jetpackFuel <= 0)
                {
                    _jetpackFuel = 0;
                    _jetpackActive = false;
                }
            }
        }

        private void HandleFuelRecovery()
        {
            if (_grounded && !_jetpackActive && _jetpackFuel < jetpackMaxFuel)
            {
                _jetpackFuel += jetpackFuelRecovery * Time.fixedDeltaTime;
                _jetpackFuel = Mathf.Min(_jetpackFuel, jetpackMaxFuel);
            }
        }

        private void HandleFacing()
        {
            if (Mathf.Approximately(_moveInput.x, 0f)) return;

            bool shouldFaceRight = _moveInput.x > 0f;
            if (shouldFaceRight == _facingRight) return;

            _facingRight = shouldFaceRight;
            float targetYRotation = _facingRight ? 90f : -90f;
            transform.rotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }

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
            Vector3 groundOrigin = transform.position + Vector3.up * _col.radius;
            Gizmos.DrawWireSphere(groundOrigin + Vector3.down * grounderDistance, radius);

            // 天花板检测范围
            Gizmos.color = Color.cyan;
            Vector3 ceilingOrigin = transform.position + Vector3.up * (_col.height - _col.radius);
            Gizmos.DrawWireSphere(ceilingOrigin + Vector3.up * grounderDistance, radius);
        }
#endif

        #endregion
    }
}