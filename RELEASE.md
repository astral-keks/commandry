# Commandry MCP Version 0.0.1-preview.5

**Release Date**: June 12, 2025
**Status**: 🚧 **Preview** - Stability improvements and feature enhancements

## 🔧 Improvements

- **PowerShell module reloading**: Added automatic reloading of PowerShell modules on changes

## 🐛 Bug Fixes

- **Schema generation**: Fixed empty parameter descriptions in tool schema

## Getting Started

1. Download and extract the release package
2. Prepare PowerShell scripts with comment-based help including `.ROLE` set to "MCP tool"
3. Run with directory scanning: `commandry-mcp-stdio --scan-directory /path/to/scripts`
4. Or scan PowerShell modules: `commandry-mcp-stdio --scan-module MyModule`
5. Connect your AI assistant using MCP client configuration

## Usage Examples

### STDIO Transport

```bash
# Scan a single directory
commandry-mcp-stdio --scan-directory <path-to-scripts>

# Scan multiple directories
commandry-mcp-stdio --scan-directory <path1> --scan-directory <path2>

# Scan PowerShell modules
commandry-mcp-stdio --scan-module <module-name-or-path>

# Combine directory and module scanning
commandry-mcp-stdio --scan-directory <path-to-scripts> --scan-module <module-name-or-path>
```

See [README.md](README.md) for detailed documentation.

## Important Notes

⚠️ **Preview Software**: Not intended for production use. Future releases may include breaking changes.