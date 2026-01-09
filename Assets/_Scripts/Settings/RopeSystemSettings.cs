using UnityEngine;

namespace _Scripts.Settings
{
    [CreateAssetMenu(fileName = "RopeSystemSettings", menuName = "Settings/RopeSystem")]
    public class RopeSystemSettings : ScriptableObject
    {
        #region 绳索结构

        [Header("绳索结构")]
        [Tooltip("绳索节点数量，不包括玩家连接点")]
        public int nodeCount = 20;

        [Tooltip("节点之间的最大间距")]
        public float nodeSpacing = 0.5f;

        #endregion

        #region 物理参数

        [Header("物理参数")]
        [Tooltip("单个节点的质量")]
        public float nodeMass = 0.1f;

        [Tooltip("节点的线性阻尼，减少晃动")]
        public float nodeDamping = 1f;

        [Tooltip("节点间弹簧的弹力强度")]
        public float springStrength = 1000f;

        [Tooltip("节点间弹簧的阻尼")]
        public float springDamper = 0.2f;

        #endregion

        #region 碰撞参数

        [Header("碰撞参数")]
        [Tooltip("节点碰撞体的半径")]
        public float colliderRadius = 0.2f;

        [Tooltip("绳索节点所属的物理层级")]
        public LayerMask ropeLayer;

        #endregion

        #region 玩家连接

        [Header("玩家连接")]
        [Tooltip("绳索连接点相对于玩家的偏移")]
        public Vector3 playerAnchorOffset = new Vector3(0, 1, 0);

        [Tooltip("玩家与绳索末端连接的弹力强度")]
        public float playerSpringStrength = 100f;

        [Tooltip("玩家与绳索末端连接的弹簧阻尼")]
        public float playerSpringDamper = 0.2f;

        [Tooltip("玩家侧的质量缩放系数")]
        public float playerMassScale = 1f;

        [Tooltip("绳索侧的质量缩放系数")]
        public float connectedMassScale = 1f;

        #endregion

        #region 可视化

        [Header("可视化")]
        [Tooltip("编辑器中 Gizmos 的绘制颜色")]
        public Color gizmoColor = Color.yellow;

        [Tooltip("绳索线条的宽度")]
        public float lineWidth = 0.1f;

        [Tooltip("绳索线条使用的材质")]
        public Material lineMaterial;

        #endregion
    }
}
