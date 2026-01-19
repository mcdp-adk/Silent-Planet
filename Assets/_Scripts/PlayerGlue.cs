using _Scripts.Settings;
using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(InputManager), typeof(PlayerMotor), typeof(RopeSystem))]
    public class PlayerGlue : MonoBehaviour
    {
        #region 配置

        [Header("配置")] [Tooltip("玩家运动配置")] [SerializeField]
        private PlayerMotorSettings motorSettings;

        [Tooltip("绳索系统配置")] [SerializeField] private RopeSystemSettings ropeSettings;

        #endregion

        #region 字段

        private InputManager _inputManager;
        private PlayerMotor _playerMotor;
        private RopeSystem _ropeSystem;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            // 获取组件引用
            _inputManager = GetComponent<InputManager>();
            _playerMotor = GetComponent<PlayerMotor>();
            _ropeSystem = GetComponent<RopeSystem>();
        }

        private void OnEnable()
        {
            // 注入配置
            _playerMotor.Initialize(motorSettings);
            _ropeSystem.Initialize(ropeSettings);

            // 连接输入系统和各个模块
            ConnectInputToMovement();
            ConnectInputToRope();
        }

        private void OnDisable()
        {
            // 断开连接
            DisconnectInputFromMovement();
            DisconnectInputFromRope();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 连接输入事件到移动系统
        /// </summary>
        private void ConnectInputToMovement()
        {
            if (_inputManager != null && _playerMotor != null)
            {
                _inputManager.OnMoveInput += _playerMotor.SetMoveInput;
                _inputManager.OnJumpPressed += _playerMotor.OnJumpPressed;
                _inputManager.OnJumpReleased += _playerMotor.OnJumpReleased;
                _inputManager.OnCrouchInput += _playerMotor.ToggleCrouch;
            }
        }

        /// <summary>
        /// 断开输入事件连接
        /// </summary>
        private void DisconnectInputFromMovement()
        {
            if (_inputManager != null && _playerMotor != null)
            {
                _inputManager.OnMoveInput -= _playerMotor.SetMoveInput;
                _inputManager.OnJumpPressed -= _playerMotor.OnJumpPressed;
                _inputManager.OnJumpReleased -= _playerMotor.OnJumpReleased;
                _inputManager.OnCrouchInput -= _playerMotor.ToggleCrouch;
            }
        }

        /// <summary>
        /// 连接输入事件到绳索系统
        /// </summary>
        private void ConnectInputToRope()
        {
            if (_inputManager != null && _ropeSystem != null)
            {
                _inputManager.OnInteractHold += _ropeSystem.ToggleRope;
            }
        }

        /// <summary>
        /// 断开绳索系统输入连接
        /// </summary>
        private void DisconnectInputFromRope()
        {
            if (_inputManager != null && _ropeSystem != null)
            {
                _inputManager.OnInteractHold -= _ropeSystem.ToggleRope;
            }
        }

        #endregion
    }
}