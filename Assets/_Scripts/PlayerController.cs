using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        #region 字段

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private float groundedGravity = -2f;
        [SerializeField] private float turnSpeed = 20f;

        private CharacterController _characterController;
        private PlayerInputHandler _inputHandler;
        private Vector2 _moveInput;
        private float _verticalVelocity;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputHandler = GetComponent<PlayerInputHandler>();

            // 订阅输入事件
            if (_inputHandler != null)
            {
                _inputHandler.OnMoveInput += HandleMoveInput;
            }
        }

        private void OnDestroy()
        {
            // 取消订阅输入事件
            if (_inputHandler != null)
            {
                _inputHandler.OnMoveInput -= HandleMoveInput;
            }
        }

        private void Update()
        {
            ApplyGravity();
            MoveCharacter();
        }

        #endregion

        #region 私有方法

        private void HandleMoveInput(Vector2 moveInput)
        {
            _moveInput = moveInput;
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = groundedGravity;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void MoveCharacter()
        {
            var horizontalVelocity = _moveInput.x * moveSpeed;
            var motion = new Vector3(horizontalVelocity, _verticalVelocity, 0f);
            _characterController.Move(motion * Time.deltaTime);

            if (Mathf.Abs(_moveInput.x) > 0.01f)
            {
                var targetForward = _moveInput.x > 0f ? Vector3.right : Vector3.left;
                transform.forward = Vector3.Slerp(transform.forward, targetForward, turnSpeed * Time.deltaTime);
            }
        }

        #endregion
    }
}