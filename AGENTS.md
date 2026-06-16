# OCS Project — Agent Guide

## Repo type

Documentation / knowledge capture for a cabinet shop automation project. **No code, no build system, no test framework, no package manifests.**

## First reads

1. **`docs/conversation-record.md`** — the central artifact: business context, tool stack, process flow, automation direction.
2. **`CONTEXT.md`** — project-specific vocabulary (Job Folder, Clipboard, Odds and Ends, etc.). Use these terms; avoid their `_Avoid_` alternatives.
3. **`design-component/ARCHITECTURE.md`** — resolved decisions, entity hierarchy, module boundaries.
4. **`data-ingestion/README.md`** — scope and next steps for the ingestion module.
5. **`ADR-FORMAT.md`** — lightweight ADR pattern (one paragraph is fine). ADRs live in `docs/adr/` with sequential numbering.
6. **`CONTEXT-FORMAT.md`** — how to write and update glossary/context files.

## Conventions

- **ADRs** go under `docs/adr/` (create lazily). No opencode.json exists.
- **Vocabulary** is defined in `CONTEXT.md` and must be respected when writing any content.
- **No automated checks** — no lint, test, typecheck, or format commands exist. Nothing to run.

## Architecture

- **Entity hierarchy**: `Job → Room → Run → Cabinet → Part`. A Run is an abstract reference line (type: `wall`, `island`, or `peninsula`).
- **4 modules**: Data Ingestion (web + LLM) → Design Component (Rhino plugin) | Job Tracking (business ops) | Reporting (cut lists, labels, costing).
- **Design Component**: Rhino plugin using RhinoCommon / openNURBS, runs inside Rhino UI + headless. Grasshopper data flow for solver (not UI). Build own components; avoid third-party plugin deps.
- **Material system**: Cascading schedule — Job defaults → Cabinet overrides → Part overrides.
- **CNC**: Thermwood Model 45, standard G-code with machine variations.

## Project direction

Targets a **Rhino-based cabinet design system** to replace Cabinet Vision. The repo captures design decisions, terminology, and process analysis toward that goal. Any new content should align with this direction.
