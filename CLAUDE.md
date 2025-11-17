# Project Instructions

This project-specific CLAUDE.md overrides or supplements the global CLAUDE.md where applicable. Focus on Unity development norms, emphasizing consistency, readability, and best practices without dictating specific architectures.

## Core Principles
- Adhere to global principles, prioritizing Unity's ecosystem for game development.
- Use Unity's documentation and official guidelines as primary references for engine-specific behaviors.
- Assume all code is in C# unless specified otherwise.
- Handle assets (e.g., prefabs, scripts, scenes) with care to maintain project integrity.

## Coding Guidelines
- **Naming Conventions**: Follow JetBrains Rider's default C# naming rules:
  - Types (classes, interfaces, structs, enums): PascalCase (e.g., `PlayerController`, `IPlayerActions`).
  - Methods: PascalCase (e.g., `Awake()`, `OnMove()`, `OnDestroy()`).
  - Properties, events: PascalCase (e.g., `OnMoveInput`, `OnAttackInput`).
  - Private fields: _camelCase (e.g., `_actions`, `_characterController`).
  - Serialized private fields: camelCase (e.g., `moveSpeed`, `gravity`, `turnSpeed`).
  - Local variables, parameters: camelCase (e.g., `context`).
  - Constants: UPPER_CASE (e.g., `MAX_HEALTH`, `DEFAULT_SPEED`).
  - Avoid abbreviations unless standard (e.g., `UI` for User Interface).
- **Code Structure**: 
  - Organize scripts into logical folders (e.g., /Assets/_Scripts/Core, /Assets/_Scripts/UI).
  - Use regions (#region) sparingly for large classes to group related methods.
  - Prefer explicit access modifiers; default to private for fields unless serialized ([SerializeField]).
  - Implement Unity callbacks (e.g., Awake, Start, Update) only when necessary; keep them concise.
- **Best Practices**:
  - Use Unity's built-in attributes (e.g., [Header], [Tooltip]) for editor clarity.
  - Avoid magic numbers; use constants or configurable fields.
  - Ensure code is performant; profile with Unity Profiler before optimizing.
  - Test code with Unity Test Framework; include unit tests for core logic.
- **Encoding and File Handling**: All scripts and assets in UTF-8 without BOM. Recommend Rider for editing to enforce norms.

## Response Format
- When suggesting code changes, provide diffs or full snippets in ```csharp:disable-run
- Structure responses with sections like "Analysis", "Proposed Changes", and "Rationale".
- For Unity-specific outputs (e.g., scene setups), describe steps clearly.

## Error Handling
- Diagnose Unity-specific errors (e.g., NullReferenceException) by checking common causes like missing references in Inspector.
- Suggest logging with Debug.Log for debugging.
- Reference Unity's error codes or console messages in fixes.

## Key Documentation References
- **Documentation Guidelines**:
  - Place all reference documents in the /Documents directory at the project root.
  - Timely add, remove, or modify documents in /Documents to keep them aligned with evolving project needs.
- **ALWAYS ADD OR UPDATE IMPORTANT DOCS HERE!**:
  - Architecture diagrams → Add reference path here
  - Problem solutions → Add reference path here
  - Setup guides → Add reference path here

This CLAUDE.md ensures consistent norms across the Unity project. If conflicts arise with global instructions, prioritize project-specific ones.
```