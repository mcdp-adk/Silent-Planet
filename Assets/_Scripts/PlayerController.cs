using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region 字段

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private float groundedGravity = -2f;
        [SerializeField] private float turnSpeed = 20f;

        private CharacterController _characterController;
        private InputSystem_Actions _actions;
        private Vector2 _moveInput;
        private float _verticalVelocity;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _actions = new InputSystem_Actions();
            _actions.Player.AddCallbacks(this);
        }

        private void OnDestroy()
        {
            _actions.Dispose();
        }

        private void OnEnable()
        {
            _actions.Player.Enable();
        }

        private void OnDisable()
        {
            _actions.Player.Disable();
        }

        private void Update()
        {
            ApplyGravity();
            MoveCharacter();
        }

        #endregion

        #region 私有方法

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

        #region IPlayerActions

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context) { }
        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }

        #endregion
    }
}