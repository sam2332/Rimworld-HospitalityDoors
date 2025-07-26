---
applyTo: '**'
---
# RimWorld Mod Development Guidelines - HospitalityDoors

## Core Principles

**ALWAYS Use Decompiled Game Files as Source of Truth**

When working on this RimWorld mod, you MUST rely on the decompiled game files as your primary source of knowledge about RimWorld's code structure, APIs, and implementation details. Never rely on training data or memory for RimWorld-specific information.

**Research First, Code Second**

Before making any changes, thoroughly research the relevant game systems using the available tools and decompiled code.

## Available Resources

**File Locations:**
- Game Source Code: `/Game Source/`
- Game XML Definitions: `/Game XML DEFS/`
- 0Harmony Code: `/0Harmony Source/`
- Hospitality Mod Code: `/Hospitality Source/`
- Installed Workshop Mods: `C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\`


**Project Documentation:**
- `Mod_Base.md` - Original development directions and requirements
- `MOD_MEMORY.md` - Recent changes and fixes (update during development)
- `IMPLEMENTATION_SUMMARY.md` - Feature implementation details
- `SCRATCH_PAD.md` - Session notes and findings (keep clean and organized)

**Decompiled Mod Generation:**
To analyze other mods, create decompiled source folders using:
```bash
ilspycmd -p --nested-directories -o "{ModName} Source" "{ModPath}/Assemblies/{ModName}.dll"
```

## Development Workflow

### 1. Research Phase
- **Always ask clarifying questions first** to understand context and requirements
- Use `semantic_search` and `grep_search` to explore decompiled game code
- Look for similar implementations in the base game before creating new solutions
- Check existing mod patterns (especially Hospitality mod) for compatibility

### 2. Implementation Standards
- **Follow RimWorld Patterns**: Examine how the base game implements similar features
- **Use Standard Serialization**: Follow patterns from `CompAssignableToPawn` and similar components for `PostExposeData()`
- **Handle Loading Gracefully**: Properly implement `LoadSaveMode.PostLoadInit` and `LoadSaveMode.Saving`
- **Prefer Reflection**: Use reflection for mod compatibility when accessing private/internal members

### 3. Code Quality
- **No Direct Memory Dependencies**: Always verify against decompiled source
- **Error Handling**: Follow game patterns, avoid wrapping `Scribe_*` calls in try-catch
- **Documentation**: Include clear comments explaining mod-specific logic
- **Compatibility**: Consider impact on other mods, especially Hospitality

### 4. Communication
- **Mod Author**: ProgrammerLily is the primary developer - ask for guidance on design decisions
- **No Assumptions**: If uncertain about requirements, ask before implementing
- **Document Changes**: Update project documentation files with significant changes

## Why This Approach Matters

**Version Compatibility**: RimWorld 1.6+ introduced significant changes that may not be reflected in training data.

**Accuracy**: Decompiled code shows actual current implementations, not outdated documentation.

**Compatibility**: Understanding actual game patterns ensures mods work correctly with the current version.

## Technical Guidelines

### Serialization (Critical for Save/Load)
- Use `Scribe_Collections.Look(ref list, "identifier", LookMode.Reference)` directly
- **Never wrap Scribe calls in try-catch** - this breaks RimWorld's load ID system
- Handle null references in `LoadSaveMode.PostLoadInit` only
- Sync data structures in `LoadSaveMode.Saving` phase
- Clean up dead/invalid references during PostLoadInit

### Component Development
- Inherit from `ThingComp` for building components
- Override `PostSpawnSetup()` for initialization
- Use `CompProperties` for configuration
- Follow naming conventions: `Comp{FeatureName}` and `CompProperties_{FeatureName}`

### Mod Compatibility
- Use reflection to access private members when needed
- Check for mod presence before using mod-specific features
- Follow established patterns from successful mods like Hospitality
- Test interactions with common mods

### Tools and Utilities
- Use available Python search tools for content analysis
- Leverage VS Code tasks for building (prefer `run_task` over `run_in_terminal`)
- Use `grep_search` with regex for efficient code exploration
- Use `semantic_search` for concept-based discovery
