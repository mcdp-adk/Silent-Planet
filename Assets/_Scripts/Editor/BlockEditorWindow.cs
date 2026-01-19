using _Scripts.Settings;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    public class BlockEditorWindow : EditorWindow
    {
        #region 配置

        private BlockEditorSettings _settings;
        private SerializedObject _serializedSettings;

        #endregion

        #region 运行时状态

        private bool _editMode;
        private int _selectedBlockIndex = -1;
        private SceneView.CameraMode _previousCameraMode;
        private int _previousVisibleLayers;

        // 框选状态
        private bool _isDragging;
        private Vector2Int _dragStart;
        private Vector2Int _dragEnd;
        private bool _isRightClick;

        #endregion

        #region 常量

        private const string GridName = "Grid";
        private const string SettingsPath = "Assets/Settings/BlockEditorSettings.asset";

        #endregion

        #region EditorWindow 生命周期

        [MenuItem("Window/Block Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlockEditorWindow>("Block Editor");
            window.titleContent = new GUIContent("Block Editor", EditorGUIUtility.IconContent("Grid.BoxTool").image);
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            if (_editMode) ExitEditMode();
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            if (!_settings)
            {
                EditorGUILayout.HelpBox("请先创建 BlockEditorSettings 配置资产。", MessageType.Warning);
                if (GUILayout.Button("创建配置"))
                {
                    CreateSettings();
                }

                return;
            }

            _serializedSettings.Update();

            DrawEditModeToggle();
            EditorGUILayout.Space();
            DrawSettingsSection();
            EditorGUILayout.Space();
            DrawBlockPalette();

            _serializedSettings.ApplyModifiedProperties();
        }

        #endregion

        #region UI 绘制

        private void DrawEditModeToggle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("编辑模式", GUILayout.Width(60));

            var newEditMode = GUILayout.Toggle(_editMode, _editMode ? "开启" : "关闭", "Button");
            if (newEditMode != _editMode)
            {
                if (newEditMode) EnterEditMode();
                else ExitEditMode();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("设置", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("visibleLayers"),
                new GUIContent("可见 Layer")
            );

            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("depth"),
                new GUIContent("放置深度")
            );
        }

        private void DrawBlockPalette()
        {
            EditorGUILayout.LabelField("方块调色板", EditorStyles.boldLabel);

            var prefabsProperty = _serializedSettings.FindProperty("blockPrefabs");

            // 绘制预制体列表
            for (int i = 0; i < prefabsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                var element = prefabsProperty.GetArrayElementAtIndex(i);
                var isSelected = _selectedBlockIndex == i;

                // 选择按钮
                var newSelected = GUILayout.Toggle(isSelected, "", GUILayout.Width(20));
                if (newSelected && !isSelected)
                {
                    _selectedBlockIndex = i;
                }

                EditorGUILayout.PropertyField(element, GUIContent.none);

                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    prefabsProperty.DeleteArrayElementAtIndex(i);
                    if (_selectedBlockIndex >= prefabsProperty.arraySize)
                    {
                        _selectedBlockIndex = prefabsProperty.arraySize - 1;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            // 添加按钮
            if (GUILayout.Button("添加方块"))
            {
                prefabsProperty.InsertArrayElementAtIndex(prefabsProperty.arraySize);
            }
        }

        #endregion

        #region 编辑模式

        private void EnterEditMode()
        {
            _editMode = true;

            // 保存当前状态
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView)
            {
                _previousCameraMode = sceneView.cameraMode;
                sceneView.in2DMode = true;
            }

            // 保存并修改可见性
            _previousVisibleLayers = Tools.visibleLayers;
            Tools.visibleLayers = _settings.visibleLayers;

            SceneView.RepaintAll();
        }

        private void ExitEditMode()
        {
            _editMode = false;

            // 恢复视图模式
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView)
            {
                sceneView.in2DMode = false;
                sceneView.cameraMode = _previousCameraMode;
            }

            // 恢复可见性
            Tools.visibleLayers = _previousVisibleLayers;

            SceneView.RepaintAll();
        }

        #endregion

        #region SceneView 交互

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_editMode || _settings == null) return;

            // 获取事件控制权
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlId);

            var e = Event.current;
            var mousePosition = e.mousePosition;

            // 转换为世界坐标
            var worldPos = GetWorldPosition(mousePosition);
            var gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

            // 绘制视觉反馈（在 Repaint 事件中）
            if (e.type == EventType.Repaint)
            {
                DrawVisualFeedback(gridPos);
                return;
            }

            // Layout 事件不处理
            if (e.type == EventType.Layout)
            {
                return;
            }

            // 处理鼠标事件
            HandleMouseEvents(e, gridPos);
        }

        private void HandleMouseEvents(Event e, Vector2Int gridPos)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 || e.button == 1)
                    {
                        _isDragging = true;
                        _dragStart = gridPos;
                        _dragEnd = gridPos;
                        _isRightClick = e.button == 1;
                        e.Use();
                    }

                    break;

                case EventType.MouseDrag:
                    if (_isDragging)
                    {
                        _dragEnd = gridPos;
                        SceneView.RepaintAll();
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    if (_isDragging && (e.button == 0 || e.button == 1))
                    {
                        _dragEnd = gridPos;
                        ExecuteAction();
                        _isDragging = false;
                        e.Use();
                    }

                    break;
            }
        }

        private void ExecuteAction()
        {
            var min = Vector2Int.Min(_dragStart, _dragEnd);
            var max = Vector2Int.Max(_dragStart, _dragEnd);

            if (_isRightClick)
            {
                DeleteBlocks(min, max);
            }
            else
            {
                PlaceBlocks(min, max);
            }
        }

        private void DrawVisualFeedback(Vector2Int currentPos)
        {
            Vector2Int min, max;
            bool isDelete;

            if (!_isDragging)
            {
                // 单格预览
                min = currentPos;
                max = currentPos;
                isDelete = false;
            }
            else
            {
                // 框选预览
                min = Vector2Int.Min(_dragStart, _dragEnd);
                max = Vector2Int.Max(_dragStart, _dragEnd);
                isDelete = _isRightClick;
            }

            DrawSelectionRect(min, max, isDelete);
        }

        private void DrawSelectionRect(Vector2Int min, Vector2Int max, bool isDelete)
        {
            var color = isDelete ? _settings.deleteColor : _settings.placeColor;

            // 计算四个角的世界坐标
            var v0 = new Vector3(min.x - 0.5f, min.y - 0.5f, 0);
            var v1 = new Vector3(max.x + 0.5f, min.y - 0.5f, 0);
            var v2 = new Vector3(max.x + 0.5f, max.y + 0.5f, 0);
            var v3 = new Vector3(min.x - 0.5f, max.y + 0.5f, 0);

            // 绘制填充矩形
            Handles.DrawSolidRectangleWithOutline(
                new[] { v0, v1, v2, v3 },
                color,
                new Color(color.r, color.g, color.b, 1f)
            );
        }

        #endregion

        #region 放置/删除逻辑

        private void PlaceBlocks(Vector2Int min, Vector2Int max)
        {
            if (_selectedBlockIndex < 0 || _selectedBlockIndex >= _settings.blockPrefabs.Count)
            {
                Debug.LogWarning("请先选择一个方块预制体。");
                return;
            }

            var prefab = _settings.blockPrefabs[_selectedBlockIndex];
            if (prefab == null)
            {
                Debug.LogWarning("选中的预制体为空。");
                return;
            }

            var grid = GetOrCreateGrid();
            var depth = _settings.depth;
            var zStart = -(depth - 1);
            var zEnd = depth - 1;

            Undo.SetCurrentGroupName("Place Blocks");
            var undoGroup = Undo.GetCurrentGroup();

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var groupName = $"({x},{y})";
                    var existingGroup = grid.transform.Find(groupName);

                    if (existingGroup != null) continue; // 跳过已存在的位置

                    // 创建坐标组
                    var group = new GameObject(groupName);
                    group.transform.SetParent(grid.transform);
                    group.transform.position = new Vector3(x, y, 0);
                    Undo.RegisterCreatedObjectUndo(group, "Create Block Group");

                    // 放置方块
                    for (int z = zStart; z <= zEnd; z++)
                    {
                        var block = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group.transform);
                        block.name = $"Block_z{z}";
                        block.transform.localPosition = new Vector3(0, 0, z);
                        Undo.RegisterCreatedObjectUndo(block, "Create Block");
                    }
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        private void DeleteBlocks(Vector2Int min, Vector2Int max)
        {
            var grid = GameObject.Find(GridName);
            if (grid == null) return;

            Undo.SetCurrentGroupName("Delete Blocks");
            var undoGroup = Undo.GetCurrentGroup();

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var groupName = $"({x},{y})";
                    var group = grid.transform.Find(groupName);

                    if (group != null)
                    {
                        Undo.DestroyObjectImmediate(group.gameObject);
                    }
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        private GameObject GetOrCreateGrid()
        {
            var grid = GameObject.Find(GridName);
            if (grid == null)
            {
                grid = new GameObject(GridName);
                Undo.RegisterCreatedObjectUndo(grid, "Create Grid");
            }

            return grid;
        }

        #endregion

        #region 配置管理

        private void LoadOrCreateSettings()
        {
            _settings = AssetDatabase.LoadAssetAtPath<BlockEditorSettings>(SettingsPath);
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        private void CreateSettings()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            _settings = CreateInstance<BlockEditorSettings>();
            AssetDatabase.CreateAsset(_settings, SettingsPath);
            AssetDatabase.SaveAssets();
            _serializedSettings = new SerializedObject(_settings);
        }

        #endregion

        #region 工具方法

        private static Vector3 GetWorldPosition(Vector2 mousePosition)
        {
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            return new Vector3(ray.origin.x, ray.origin.y, 0);
        }

        #endregion
    }
}