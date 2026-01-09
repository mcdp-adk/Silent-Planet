using System.Collections.Generic;
using _Scripts.Settings;
using UnityEngine;

namespace _Scripts
{
    /// <summary>
    /// 动态生成弹簧绳索系统
    /// Player 长按 Interact 时在当前位置部署绳索并连接到 Player
    /// </summary>
    public class RopeSystem : MonoBehaviour
    {
        #region 配置

        private RopeSystemSettings _settings;

        public void Initialize(RopeSystemSettings settings)
        {
            _settings = settings;
        }

        #endregion

        #region 运行时数据

        private SpringJoint _playerSpringJoint;
        private GameObject _ropeContainer;
        private readonly List<Rigidbody> _nodes = new();
        private LineRenderer _lineRenderer;
        private bool _nodesCreated;

        #endregion

        #region 公共属性

        public bool IsDeployed { get; private set; }

        #endregion

        #region Mono 生命周期

        private void Start()
        {
            IsDeployed = false;
        }

        private void LateUpdate()
        {
            UpdateLineRenderer();
        }

        private void OnDrawGizmos()
        {
            if (!IsDeployed || _nodes.Count == 0 || _settings == null) return;

            Gizmos.color = _settings.gizmoColor;
            foreach (var node in _nodes)
            {
                if (node != null)
                {
                    Gizmos.DrawWireSphere(node.position, _settings.colliderRadius);
                }
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 切换绳索状态（部署/收回）
        /// </summary>
        public void ToggleRope()
        {
            if (IsDeployed) RetractRope();
            else DeployRope();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 在 Player 当前位置部署绳索
        /// </summary>
        private void DeployRope()
        {
            if (IsDeployed) return;

            var anchorPosition = transform.position + _settings.playerAnchorOffset;

            if (!_nodesCreated)
            {
                CreateRopeNodes(anchorPosition);
                _nodesCreated = true;
            }
            else
            {
                ReactivateRope(anchorPosition);
            }

            ConfigurePlayerSpringJoint();
            IsDeployed = true;
        }

        /// <summary>
        /// 收回绳索
        /// </summary>
        private void RetractRope()
        {
            if (!IsDeployed) return;

            // 销毁 Player 的 SpringJoint
            if (_playerSpringJoint != null)
            {
                Destroy(_playerSpringJoint);
                _playerSpringJoint = null;
            }

            _ropeContainer.SetActive(false);
            IsDeployed = false;
        }

        /// <summary>
        /// 获取锚点节点
        /// </summary>
        private Rigidbody GetAnchorNode()
        {
            return _nodes.Count > 0 ? _nodes[0] : null;
        }

        /// <summary>
        /// 获取末端节点
        /// </summary>
        private Rigidbody GetLastNode()
        {
            return _nodes.Count > 0 ? _nodes[^1] : null;
        }

        /// <summary>
        /// 创建绳索节点（首次部署）
        /// </summary>
        private void CreateRopeNodes(Vector3 anchorPosition)
        {
            // 创建容器
            _ropeContainer = new GameObject("RuntimeStringRope");

            // 添加 LineRenderer
            _lineRenderer = _ropeContainer.AddComponent<LineRenderer>();
            ConfigureLineRenderer();

            // 创建节点
            for (int i = 0; i < _settings.nodeCount; i++)
            {
                var node = CreateNode(i, anchorPosition);
                _nodes.Add(node);

                // 非锚点节点添加 SpringJoint 连接到上一个节点
                if (i > 0)
                {
                    ConfigureNodeSpringJoint(node.gameObject, _nodes[i - 1]);
                }
            }
        }

        /// <summary>
        /// 创建单个节点
        /// </summary>
        private Rigidbody CreateNode(int index, Vector3 position)
        {
            var nodeObj = new GameObject($"Node_{index}");
            nodeObj.transform.SetParent(_ropeContainer.transform);
            nodeObj.transform.position = position;
            nodeObj.layer = GetLayerFromMask(_settings.ropeLayer);

            // 添加 Rigidbody
            var rb = nodeObj.AddComponent<Rigidbody>();
            rb.mass = _settings.nodeMass;
            rb.linearDamping = _settings.nodeDamping;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // 锚点节点设为 Kinematic
            if (index == 0)
            {
                rb.isKinematic = true;
            }

            // 添加 SphereCollider
            var col = nodeObj.AddComponent<SphereCollider>();
            col.radius = _settings.colliderRadius;

            return rb;
        }

        /// <summary>
        /// 配置节点之间的 SpringJoint
        /// </summary>
        private void ConfigureNodeSpringJoint(GameObject node, Rigidbody connectedNode)
        {
            var joint = node.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = connectedNode;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            joint.spring = _settings.springStrength;
            joint.damper = _settings.springDamper;
            joint.minDistance = 0f;
            joint.maxDistance = _settings.nodeSpacing;
        }

        /// <summary>
        /// 创建并配置 Player 的 SpringJoint
        /// </summary>
        private void ConfigurePlayerSpringJoint()
        {
            // 创建新的 SpringJoint
            _playerSpringJoint = gameObject.AddComponent<SpringJoint>();
            _playerSpringJoint.autoConfigureConnectedAnchor = false;
            _playerSpringJoint.connectedBody = GetLastNode();
            _playerSpringJoint.anchor = _settings.playerAnchorOffset;
            _playerSpringJoint.connectedAnchor = Vector3.zero;
            _playerSpringJoint.spring = _settings.playerSpringStrength;
            _playerSpringJoint.damper = _settings.playerSpringDamper;
            _playerSpringJoint.minDistance = 0f;
            _playerSpringJoint.maxDistance = 0f;
            _playerSpringJoint.massScale = _settings.playerMassScale;
            _playerSpringJoint.connectedMassScale = _settings.connectedMassScale;
        }

        /// <summary>
        /// 重新激活绳索（复用节点）
        /// </summary>
        private void ReactivateRope(Vector3 anchorPosition)
        {
            _ropeContainer.SetActive(true);

            // 重新定位所有节点
            foreach (var node in _nodes)
            {
                node.position = anchorPosition;

                if (node.isKinematic) continue;
                node.linearVelocity = Vector3.zero;
                node.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 配置 LineRenderer
        /// </summary>
        private void ConfigureLineRenderer()
        {
            _lineRenderer.startWidth = _settings.lineWidth;
            _lineRenderer.endWidth = _settings.lineWidth;
            _lineRenderer.positionCount = _settings.nodeCount + 1;
            _lineRenderer.useWorldSpace = true;

            if (_settings.lineMaterial != null)
            {
                _lineRenderer.material = _settings.lineMaterial;
            }
        }

        /// <summary>
        /// 更新 LineRenderer 位置
        /// </summary>
        private void UpdateLineRenderer()
        {
            if (!IsDeployed || !_lineRenderer)
            {
                if (_lineRenderer)
                {
                    _lineRenderer.enabled = false;
                }

                return;
            }

            _lineRenderer.enabled = true;

            // 设置节点位置
            for (int i = 0; i < _nodes.Count; i++)
            {
                _lineRenderer.SetPosition(i, _nodes[i].position);
            }

            // 最后一点为 Player 连接点位置
            _lineRenderer.SetPosition(_settings.nodeCount, transform.position + _settings.playerAnchorOffset);
        }

        /// <summary>
        /// 从 LayerMask 获取 Layer 索引
        /// </summary>
        private static int GetLayerFromMask(LayerMask mask)
        {
            int layerNumber = 0;
            int layer = mask.value;
            while (layer > 1)
            {
                layer >>= 1;
                layerNumber++;
            }

            return layerNumber;
        }

        #endregion
    }
}