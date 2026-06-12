# Design Component — Architecture

## Entity Hierarchy

```
Job → Room → Run → Cabinet → Part
```

- **Run**: Abstract reference line for cabinets. Can be a physical wall, an island reference, or a peninsula reference. Each Run has a name and a type (`wall`, `island`, `peninsula`).
- **Cabinet positioning**: Uses reference-point constraints (center on window, end of wall, corner, arbitrary point, space-on-left, center-from-left, etc.).
- **Part**: Supports an arbitrary-depth sub-assembly tree (drawer is an assembly of sides/front/back/bottom, cabinet is an assembly of box + doors + drawers, etc.). Parts have named identities, materials, and parametric relationships to other parts.

## Architecture Modules

```
┌─────────────────┐     structured      ┌──────────────────┐
│  Data Ingestion  │ ──── JSON ──────▶  │  Design Component │
│  (web app, LLM,  │                     │  (Rhino plugin)   │
│   validation)     │                     │  + Grasshopper    │
└─────────────────┘                     └────────┬─────────┘
                                                  │
                    ┌─────────────────┐           │ updates
                    │  Job Tracking    │ ◀────────┘
                    │  (business ops)  │
                    └────────┬─────────┘
                             │ queries
                    ┌────────▼─────────┐
                    │    Reporting      │
                    │ (cut lists,       │
                    │  labels, costing, │
                    │  part delegation) │
                    └──────────────────┘
```

## Resolved Decisions

| Topic | Decision |
|-------|----------|
| Geometry kernel | Rhino (RhinoCommon / openNURBS) |
| Plugin runs | Inside Rhino UI + headless (Grasshopper supplements) |
| Parametric engine | Custom woodworking logic; Grasshopper data flow for solver |
| Material system | Cascading schedule: Job → Cabinet → Part, with per-part override |
| Cabinet types | Face frame (95%) and frameless, both supported |
| CNC machine | Thermwood Model 45, standard G-code with machine variations |
| Constraint types | Center on reference, end of wall, corner, arbitrary point, space-on-left, center-from-left |

## Open

- Design rules engine approach (to be explored)
- JSON schema between Data Ingestion and Design Component (separate session needed)
- Part generation / joinery specifics
- Output format specs (Reporting module territory)
