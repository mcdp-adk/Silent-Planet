using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class PlayerInputHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region 事件

        public Action<Vector2> OnMoveInput;
        public Action<Vector2> OnLookInput;
        public Action OnAttackInput;
        public Action OnInteractInput;
        public Action OnCrouchInput;
        public Action OnJumpInput;
        public Action OnPreviousInput;
        public Action OnNextInput;
        public Action OnSprintInput;

        #endregion

        #region 字段

        private InputSystem_Actions _actions;

        #endregion

        #region Mono 生命周期

        private void Awake()
        {
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

        #endregion

        #region IPlayerActions

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveInput?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            OnLookInput?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnAttackInput?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnInteractInput?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnCrouchInput?.Invoke();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnJumpInput?.Invoke();
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnPreviousInput?.Invoke();
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnNextInput?.Invoke();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSprintInput?.Invoke();
            }
        }

        #endregion
    }
}