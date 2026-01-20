# 代码规范

## 命名约定 (JetBrains Rider 风格)

| 类型 | 规范 | 示例 |
|------|------|------|
| 类/结构体 | PascalCase | `PlayerController` |
| 方法 | PascalCase | `OnMove()` |
| 私有字段 | _camelCase | `_characterController` |
| 序列化字段 | camelCase | `moveSpeed` |
| 常量 | PascalCase | `GridName` |
| 事件 | PascalCase | `OnHealthChanged` |

## 代码组织

使用 `#region` 按以下顺序组织：
1. Constants
2. Serialized Fields
3. Private Fields
4. Unity Callbacks (Awake, OnEnable, Start, Update, OnDisable)
5. Public Methods
6. Private Methods

## Inspector 最佳实践
- 使用 `[Header("分组名")]` 分组字段
- 使用 `[Tooltip("说明")]` 添加提示
- 使用 `[Range(min, max)]` 限制数值范围

## 架构原则
- **Module-Glue 模式**: 模块单一职责，Glue 负责协调
- **事件驱动**: 模块间通过 C# Action 通信
- **配置分离**: 使用 ScriptableObject 存储配置
