# OCS Project — Agent Guide

## Repo type

Documentation / knowledge capture for a cabinet shop automation project. **No code, no build system, no test framework, no package manifests.**

## First reads

1. **`docs/conversation-record.md`** — the central artifact: business context, tool stack, process flow, automation direction.
2. **`CONTEXT.md`** — project-specific vocabulary (Job Folder, Clipboard, Odds and Ends, etc.). Use these terms; avoid their `_Avoid_` alternatives.
3. **`ADR-FORMAT.md`** — lightweight ADR pattern (one paragraph is fine). ADRs live in `docs/adr/` with sequential numbering.
4. **`CONTEXT-FORMAT.md`** — how to write and update glossary/context files.

## Conventions

- **ADRs** go under `docs/adr/` (create lazily). Template in `ADR-FORMAT.md`.
- **Vocabulary** is defined in `CONTEXT.md` and must be respected when writing any content.
- **No automated checks** — no lint, test, typecheck, or format commands exist. Nothing to run.

## Project direction

The automation path explored in `docs/conversation-record.md` targets a **Rhino-based cabinet design system** to replace Cabinet Vision. The repo captures design decisions, terminology, and process analysis toward that goal. Any new content should align with this direction.
