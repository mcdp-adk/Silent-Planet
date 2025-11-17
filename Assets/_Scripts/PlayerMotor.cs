using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        #region 常量

        private const float InputThreshold = 0.01f; // 过滤极小输入，防止抖动

        #endregion

        #region 字段

        [Header("移动参数")] [SerializeField] private float moveSpeed;
        [SerializeField] private float gravity;
        [SerializeField] private float groundedGravity;
        [SerializeField] private float turnSpeed;
        [Header("跳跃参数")] [SerializeField] private float jumpHeight;
        [Header("喷气背包参数")] [SerializeField] private float jetpackVerticalForce;
        [SerializeField] private float jetpackHorizontalForce;
        [SerializeField] private float maxFallSpeed;
        [SerializeField] private float maxRiseSpeed;
        [Header("空中控制参数")] [SerializeField] private float airControlMultiplier;

        private CharacterController _characterController;
        private Vector2 _moveInput;
        private float _verticalVelocity;
        private float _horizontalVelocity;
        private bool _isUsingJetpack;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            ProcessHorizontalVelocity();
            ProcessVerticalVelocity();
            MovePlayer();
            RotatePlayer();
        }

        #endregion

        #region 公共方法

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = input;
        }

        public void OnJumpTap()
        {
            if (_characterController.isGrounded)
            {
                ExecuteJump();
            }
        }

        public void OnJumpHold()
        {
            _isUsingJetpack = true;
        }

        public void OnJumpReleased()
        {
            _isUsingJetpack = false;
        }

        #endregion

        #region 私有方法

        private void ProcessHorizontalVelocity()
        {
            var controlMultiplier = _characterController.isGrounded ? 1f : airControlMultiplier;
            _horizontalVelocity = _moveInput.x * moveSpeed * controlMultiplier;

            if (_isUsingJetpack && Mathf.Abs(_moveInput.x) > InputThreshold)
                _horizontalVelocity += _moveInput.x * jetpackHorizontalForce; // 喷气附加水平推力
        }

        private void ProcessVerticalVelocity()
        {
            var grounded = _characterController.isGrounded;
            var verticalAcceleration = gravity;

            if (grounded)
            {
                if (_verticalVelocity < 0f) _verticalVelocity = groundedGravity; // 贴地时保持轻微向下，避免悬空
                return;
            }

            if (_isUsingJetpack) verticalAcceleration += jetpackVerticalForce; // 仅在空中叠加喷气推力

            _verticalVelocity += verticalAcceleration * Time.deltaTime;
            _verticalVelocity = Mathf.Clamp(_verticalVelocity, maxFallSpeed, maxRiseSpeed);
        }

        private void MovePlayer()
        {
            var motion = new Vector3(_horizontalVelocity, _verticalVelocity, 0f);
            _characterController.Move(motion * Time.deltaTime);
        }

        private void RotatePlayer()
        {
            if (Mathf.Abs(_moveInput.x) <= InputThreshold) return;

            var targetForward = _moveInput.x > 0f ? Vector3.right : Vector3.left;
            transform.forward = Vector3.Slerp(transform.forward, targetForward, turnSpeed * Time.deltaTime);
        }

        private void ExecuteJump()
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // 保持简单的地面起跳实现
        }

        #endregion
    }
}