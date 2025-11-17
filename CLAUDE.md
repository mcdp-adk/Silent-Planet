# 🎮 Project Instructions

**Unity-Specific Rules**: This project-specific CLAUDE.md overrides or supplements the global CLAUDE.md where applicable. Focus on 3D Unity development norms, emphasizing consistency, readability, and best practices without dictating specific architectures.

## 🎯 Core Principles
- **Unity First**: Prioritize Unity's ecosystem and established workflows for game development
- **Official Documentation**: Use Unity's documentation and official guidelines as primary references
- **C# Default**: Assume all code is in C# unless specified otherwise
- **Asset Integrity**: Handle assets (prefabs, scripts, scenes) with care to maintain project stability
- **Unity MCP First**: Prefer Unity MCP so AI assistants (Claude, Cursor, etc.) can bridge directly into Unity via a local Model Context Protocol client, letting the LLM manipulate assets, control scenes, edit scripts, and automate editor tasks.

## 💻 Coding Guidelines

### 📝 Naming Conventions
Follow JetBrains Rider's default C# naming rules:
- **Types** (classes, interfaces, structs, enums): `PascalCase` → `PlayerController`, `IPlayerActions`
- **Methods**: `PascalCase` → `Awake()`, `OnMove()`, `OnDestroy()`
- **Properties, Events**: `PascalCase` → `OnMoveInput`, `OnAttackInput`
- **Private Fields**: `_camelCase` → `_actions`, `_characterController`
- **Serialized Fields**: `camelCase` → `moveSpeed`, `gravity`, `turnSpeed`
- **Local Variables, Parameters**: `camelCase` → `context`
- **Constants**: `UPPER_CASE` → `MAX_HEALTH`, `DEFAULT_SPEED`
- **Avoid abbreviations** unless standard (e.g., `UI` for User Interface)

### 🏗️ Code Structure
- **Folder Organization**: `/Assets/_Scripts/Core`, `/Assets/_Scripts/UI`, etc.
- **Region Usage**: Use `#region` sparingly for large classes to group related methods
- **Access Modifiers**: Prefer explicit modifiers; default to private unless serialized (`[SerializeField]`)
- **Unity Callbacks**: Implement (Awake, Start, Update) only when necessary; keep concise

### ⚡ Best Practices
- **Editor Attributes**: Use `[Header]`, `[Tooltip]` for Inspector clarity
- **Avoid Magic Numbers**: Use constants or configurable fields instead
- **Performance First**: Profile with Unity Profiler before optimizing
- **Test-Driven**: Test with Unity Test Framework; include unit tests for core logic

### 🏛️ Architecture Style: Module-Glue Pattern
- **Focused Modules**: Create small, single-responsibility modules
- **Glue Code**: Use coordination code between modules
- **Interface Design**: Make modules independent and reusable through interfaces
- **Separation of Concerns**: Glue handles orchestration, not core logic

## 📤 Response Format
- **Code Presentation**: When suggesting code changes, provide diffs or full snippets in ````csharp` blocks
- **Structured Analysis**: Structure responses with sections like "Analysis", "Proposed Changes", and "Rationale"
- **Unity-Specific Steps**: For Unity-specific outputs (e.g., scene setups), describe steps clearly

## 🔧 Error Handling
- **Unity Diagnostics**: Diagnose Unity-specific errors (e.g., NullReferenceException) by checking common causes like missing references in Inspector
- **Debugging Strategy**: Suggest logging with Debug.Log for debugging
- **Error Reference**: Reference Unity's error codes or console messages in fixes

## 📚 Key Documentation References

### 📁 Documentation Guidelines
- **Central Location**: Place all reference documents in the /Documents directory at the project root
- **Real-time Updates**: Timely add, remove, or modify documents in /Documents to keep them aligned with evolving project needs
- **Verification Process**: Always verify document contents against actual codebase using Read/Grep tools before writing
- **Language Requirement**: Write all documents in Chinese for consistency and team preference
- **AI-Optimized Format**: Structure documents for AI readability with clear hierarchies and formatting
- **Concise Content**: Keep documents concise with minimal context overhead; focus only on key information and core concepts
- **Clear Structure**: Use headings, bullet points, and code blocks to organize content with well-defined information hierarchy

### 🚨 ALWAYS ADD OR UPDATE IMPORTANT DOCS HERE!
- Project overview → Documents/project-overview.md
- Input system guide → Documents/input-system-guide.md
