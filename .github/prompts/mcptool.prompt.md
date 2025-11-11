---
description: Defining PowerShell 7.0 MCP tool
mode: agent
---

# User input
You must consider the following inputs which could be provided by user or agent:
- `tool name`: The name of the MCP tool to create or edit. If not provided suggest suitable name.
- `tool form` (`script` or `function`): The form of the MCP tool to create, either as a standalone script or as a function within a module. Use `script` by default if not provided.

# Instructions
You are creating a **PowerShell MCP tool** in a form of script (`.ps1` files) or in a for of function (in `.psm1` modules) depending on the `tool form`. Follow [these guidelines](../../GUIDE.md) to create the tool.
