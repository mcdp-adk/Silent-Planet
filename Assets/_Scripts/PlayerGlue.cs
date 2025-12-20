using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(InputManager), typeof(PlayerMotor))]
    public class PlayerGlue : MonoBehaviour
    {
        #region 字段

        private InputManager _inputManager;
        private PlayerMotor _playerMotor;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            // 获取组件引用
            _inputManager = GetComponent<InputManager>();
            _playerMotor = GetComponent<PlayerMotor>();

            // 连接输入系统和移动系统
            ConnectInputToMovement();
        }

        private void OnDestroy()
        {
            // 断开连接
            DisconnectInputFromMovement();
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
            }
        }

        #endregion
    }
}