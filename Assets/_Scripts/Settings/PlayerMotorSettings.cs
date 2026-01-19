using UnityEngine;

namespace _Scripts.Settings
{
    [CreateAssetMenu(fileName = "PlayerMotorSettings", menuName = "Settings/PlayerMotor")]
    public class PlayerMotorSettings : ScriptableObject
    {
        #region 层级

        [Header("层级")]
        [Tooltip("环境层级，用于地面检测")]
        public LayerMask environmentLayer;

        #endregion

        #region 移动

        [Header("移动")]
        [Tooltip("最大水平移动速度")]
        public float maxSpeed = 14f;

        [Tooltip("水平加速度")]
        public float acceleration = 120f;

        [Tooltip("地面上无输入时的减速度")]
        public float groundDeceleration = 60f;

        [Tooltip("空中无输入时的减速度")]
        public float airDeceleration = 30f;

        #endregion

        #region 跳跃

        [Header("跳跃")]
        [Tooltip("跳跃时的初始垂直速度")]
        public float jumpPower = 12f;

        [Tooltip("下落时的最大速度限制")]
        public float maxFallSpeed = 20f;

        [Tooltip("空中下落时的重力加速度")]
        public float fallAcceleration = 50f;

        [Tooltip("提前释放跳跃键时的重力倍增，实现可变跳跃高度")]
        public float jumpEndEarlyGravityModifier = 3f;

        [Tooltip("离开地面后仍可跳跃的缓冲时间")]
        public float coyoteTime = 0.15f;

        #endregion

        #region 地面检测

        [Header("地面检测")]
        [Tooltip("向下检测地面的射线距离")]
        public float grounderDistance = 0.1f;

        [Tooltip("站在地面时施加的向下力，防止斜坡滑动")]
        public float groundingForce = -1.5f;

        #endregion

        #region 喷气背包

        [Header("喷气背包")]
        [Tooltip("喷气背包向上的推力")]
        public float jetpackForce = 160f;

        [Tooltip("使用喷气背包时的最大上升速度")]
        public float maxRiseSpeed = 15f;

        [Tooltip("喷气背包的最大燃料容量")]
        public float jetpackMaxFuel = 2f;

        [Tooltip("站在地面时燃料的恢复速率")]
        public float jetpackFuelRecovery = 0.5f;

        #endregion

        #region 蹲伏

        [Header("蹲伏")]
        [Tooltip("蹲伏时碰撞体高度")]
        public float crouchHeight = 1f;

        [Tooltip("蹲伏移动速度倍率")]
        [Range(0f, 1f)]
        public float crouchSpeedMultiplier = 0.5f;

        #endregion
    }
}
