using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Settings
{
    [CreateAssetMenu(fileName = "BlockEditorSettings", menuName = "Settings/BlockEditor")]
    public class BlockEditorSettings : ScriptableObject
    {
        #region 方块调色板

        [Header("方块调色板")] [Tooltip("可放置的方块预制体列表")]
        public List<GameObject> blockPrefabs = new();

        #endregion

        #region 编辑模式

        [Header("编辑模式")] [Tooltip("编辑模式下可见的 Layer")]
        public LayerMask visibleLayers;

        #endregion

        #region 放置参数

        [Header("放置参数")] [Tooltip("沿 z 轴放置的深度（层数）")] [Min(1)]
        public int depth = 2;

        #endregion

        #region 视觉反馈

        [Header("视觉反馈")] [Tooltip("放置区域的颜色")] public Color placeColor = new(0f, 1f, 0f, 0.3f);

        [Tooltip("删除区域的颜色")] public Color deleteColor = new(1f, 0f, 0f, 0.3f);

        #endregion
    }
}