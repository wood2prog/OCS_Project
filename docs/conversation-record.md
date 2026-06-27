# Conversation Record — Session 1

## Goal
Automate the user's role at a 5-person residential cabinet shop to the point where only a few hours/week of "information translation" is needed. The user plans to leave the business eventually, and without automation the shop cannot function without them.

## The Business
- 5 people: Owner, User, Finisher/assembler (x1), Production/manufacturing (x2)
- Residential custom cabinetry. Standardized internal designs, handles fully custom requests.
- The user's role spans: design, engineering, production planning, quality/load check, change order management, and shop floor production (40hrs/week cutting parts)

## Tools
| Tool | Use |
|------|-----|
| Cabinet Vision Ultimate + S2M Ultimate | Design (source of truth), job costing, CNC output |
| RhinoCAD | One-off custom pieces (hoods, furniture) |
| Cabinet Vision UCS | Part-level automation (toekick cutouts, vents, etc.) |
| Excel | Employee hours / payroll |
| QuickBooks | Estimates, billing |
| Task3 | Job scheduling |
| Gmail (shared account) | Business email, customer communication |
| Google Contacts | Customer management |
| Pen & paper | Scratch notes |

## Process Flow
1. Owner meets customer → creates **Job Folder** (manilla envelope with sketches, dimensions, notes, appliance specs, photos, paint chips)
2. User gets Job Folder → designs in Cabinet Vision (45min simple, 40-50hrs complex)
3. User refines with customer via email (same shared account, typically 50+ emails on complex jobs)
4. User generates machine files (CV → CNC, plus CSV → RhinoCAD plugin for custom doors)
5. User prints **Clipboard** sheets (drawings, odds & ends list, color/finish sheet, machining sheet)
6. User schedules in Task3 (~1hr/week)
7. User cuts parts on shop floor (~40hrs/week)
8. User does final assembly check and **load check** (1.5-2hrs)
9. Owner + one worker deliver

## Tacit Knowledge
- Standard cabinet heights and depths; only width varies to fit the space
- ~90% of jobs use standard-width cabinets from a catalog; ~10% fully custom
- Color/door style trends change but construction standards are stable
- Design rules encoded partly in CV UCS scripts and partly in the user's head
- Contractor expectations have been standardized over years by the user

## Automation Path — Key Findings
- Cabinet Vision's `.ORD` text format can be imported to populate walls and cabinets, but it requires a visual design tool to generate the ORD — it's a transfer format, not a spec-to-design bridge
- CV's internal database isn't writable for design manipulation
- CV's UCS can modify parts but not create full assemblies from scratch
- RhinoCAD + Grasshopper have existing infrastructure that could replace CV entirely:
  - **Bark Beetle** (open-source): generates CNC G-code from Grasshopper
  - **Biber**: parametric woodworking with joints
  - **D2P Components** (MIT): component hierarchy management
  - **ShopPrentice**: AI-driven parametric furniture (Fusion 360)
  - Also: PaletteCAD Wood Technology, CabiCAD, Cubinets (FreeCAD)

## User's Preferred Direction
Build a novel Rhino-based cabinet design system that:
- Replaces Cabinet Vision as the design platform
- Accepts structured job specs (entered by owner via guided interface)
- Applies design rules automatically
- Generates renderings for customer approval
- Generates all outputs: CNC G-code, cut lists, labels, job costing data
- One-click data path to the shop floor
- User handles final tweaks and exception cases (a few hours/week)

## Next Areas to Explore
- Detailed entity model (Job → Room → Run → Cabinet → Parts)
- Standard cabinet specifications (heights, depths, construction methods)
- CNC machine type and G-code requirements
- Output format specifications (cut lists, labels, QuickBooks data)
- Design rules engine approach

---

# Conversation Record — Session 2: Application Architecture

## Repo Restructure
Split the repo into domain knowledge (stays at root) and application design (new folders):
- `design-component/` — the Rhino plugin module
- `data-ingestion/` — LLM-based job spec ingestion

## Architecture: 4 Modules

```
┌─────────────────┐     JSON      ┌──────────────────┐
│  Data Ingestion  │ ───────────▶  │  Design Component │
│  (web app, LLM,  │               │  (Rhino plugin)   │
│   validation)     │               │  + Grasshopper    │
└─────────────────┘               └────────┬─────────┘
                                            │
                  ┌─────────────────┐       │ updates
                  │  Job Tracking    │ ◀────┘
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

### Module Boundaries
- **Data Ingestion**: Owner uploads scanned Job Folder notes → LLM parses into structured JSON → human reviews/confirms → validated data to Design Component. JSON schema is complex and needs its own session.
- **Design Component**: Rhino plugin (runs inside Rhino UI + headless). Custom woodworking logic with Grasshopper data flow supplements. Generates 3D model, parts, and G-code.
- **Job Tracking**: Business operations — emails, change orders, scheduling, cost tracking.
- **Reporting**: Queries other modules for cut lists, labels, costing, part delegation.

## Entity Model

```
Job → Room → Run → Cabinet → Part
```

- **Run** (new term, added to CONTEXT.md): Abstract reference line that cabinets mount to. Types: wall, island, peninsula. Replaces the earlier "Wall" in the hierarchy.
- **Cabinet positioning**: Reference-point constraints — center on window, end of wall, corner, arbitrary point, space-on-left, center-from-left. These match Cabinet Vision's constraint properties.
- **Part**: Arbitrary-depth sub-assembly tree. Drawer is an assembly of sides/front/back/bottom. Cabinet is an assembly of box + doors + drawers + etc. Parts have named identities, materials, and parametric relationships.

## Design Component Decisions

| Topic | Decision |
|-------|----------|
| Geometry kernel | Rhino (RhinoCommon / openNURBS) |
| Plugin runtime | Inside Rhino UI (design) + headless (batch) |
| Parametrics | Custom woodworking logic; Grasshopper data flow for solver (not UI) |
| Build vs. buy | Build own cabinet components; avoid third-party plugin dependencies |
| Material system | Cascading schedule: Job defaults → Cabinet overrides → Part overrides |
| Cabinet types | Face frame (~95%) and frameless (~5%, growing) |
| CNC | Thermwood Model 45, standard G-code with machine variations |
| Standards role | Defaults, not rules — every value can be overridden per job/room/run/cabinet |

## Open / Deferred

- Part generation and joinery specifics (deferred to next session)
- Data Ingestion JSON schema (needs its own dedicated session)
- Design rules engine approach
- Output format specifications (Reporting module territory)

---

# Conversation Record — Session 3: JSON Schema Architecture

## Three-Document Architecture

The system is built around **three independent JSON documents** that the Engine evaluates together:

| Document | Role | Analogy |
|----------|------|---------|
| **Job Document** | Layout structure: rooms, runs, cabinet positions, one-off overrides, customer info | HTML |
| **Construction Document** | Assembly templates, joinery rules, standard profiles for a product line/style | CSS |
| **Material Schedule** | Binds material references to actual material specs (species, thickness, finish) | CSS variables |

The Engine is a **pure evaluator** — it knows *how* to evaluate (resolve dimensions, apply profiles, follow the cascade) but knows *nothing* about cabinets. New shapes, construction styles, and profile types never touch engine code.

The JSON is declarative only. All math lives in the Engine.

## Cascade (CSS-like specificity)

```
global defaults  →  room overrides  →  cabinet overrides
   (lowest)             (medium)           (highest)
```

- `constructionStyle` and `materialRef` appear at global, room, and cabinet levels
- A cabinet inherits from its room; a room inherits from the global header
- Job Document overrides take precedence over Construction Document defaults

## Part Types (4 types)

| JSON type | Code treatment | Human meaning |
|-----------|---------------|---------------|
| `rectangular` | Sugar → extrusion | Box parts, shelves, panels (shorthand for rect cross-section extrusion) |
| `extrusion` | Native geometry | Molding, trim, edge banding — constant cross-section along a path |
| `user-defined` | Imported Rhino geo | Screws, hinges, decorative elements, complex brackets |
| `auxiliary` | Listed, not rendered | Purchased hardware, fasteners, consumables — BOM only |

## Assembly Tree: Nested

- Assembly templates are **fully nested** in the Construction Document (not a flat ID pool)
- Every cabinet instantiation creates its own tree — a 36" cabinet's left side is not shared with a 30" cabinet's left side
- Override system can target any node in the tree by its local path

## Profiles

- **Construction Document**: defines profiles that apply to a whole style (e.g. toe kick notch for every base cabinet in face-frame line)
- **Job Document**: one-off profiles (customer-specific pipe notch, odd corner cut)
- Engine resolves all profiles at evaluation time regardless of source

## Resolved

- Named reference for construction/material lookup (not file paths)
- Three separate source files, optional bundled transport file
- `auxiliary` kept as distinct part type (not a boolean flag)
- Engine is a pure evaluator, not domain-aware

## Open / Deferred (from this session)

- Parameterization: how cabinet dimensions (width, height, depth) drive template resolution — more discussion needed
- Profile format: 2D cross-section definition (line/arc segments in local coords)
- Construction document JSON schema (detailed)
- User-defined part tracking across jobs

---

# Conversation Record — Session 4: Python-Based Construction Scripts

## Paradigm Shift

Moved away from the pure-JSON declarative construction document model. Instead, the **Construction Document becomes a Python script** that defines all geometry, relationships, and connection types programmatically.

| Web stack | OCS stack |
|-----------|-----------|
| HTML | Job Document (JSON) — rooms, runs, cabinet positions, overrides |
| CSS | Material Schedule (JSON) — material bindings |
| JavaScript | Construction scripts (Python) — part definitions, relationships, joinery |

## New Engine Role

The engine is a **generic interpreter** — it knows nothing about cabinets, left sides, or base cabinets. It exposes a minimal API of primitives, and the Python script defines everything domain-specific. Anyone could add definitions for whatever they want to build.

## Geometry Model (reduced from Session 3)

Two fundamental operations, both mapping directly to Rhino:

| Concept | Definition | Rhino function |
|---------|-----------|----------------|
| **Panel** | Closed 2D perimeter shape extruded by thickness | `ExtrudeCurve` |
| **Sweep** | Closed 2D cross-section extruded along a drawn 3D path | `Sweep1` / `Sweep2` |

A square panel's perimeter is 4 lines. Any other shape uses as many lines as needed. Custom user shapes are native Rhino curves.

Part types `rectangular` and `extrusion` from Session 3 collapse into this single model: `rectangular` is sugar for a 4-line panel, `extrusion` is a sweep.

## Minimal Engine API (tentative)

- `Panel(shape, thickness, material, quantity?)`
- `Sweep(profile, path, material)`
- Cutout / notch operations (not yet defined)
- Assembly composition (not yet defined)

## Open / Deferred (from this session)

- How the engine discovers and invokes the right construction script (registration pattern — decorator? class? convention?)
- Whether Panel and Sweep are separate constructors or a single duck-typed constructor
- Cutout/notch/modeling operations API
- Assembly and part tree composition in Python
- Auxiliary/hardware part representation in Python

---

# Conversation Record — Session 5: Job Tracking Module

## Goal
Define the scope, data model, and architecture for the Job Tracking module — a Blazor web app that tracks jobs from lead to closed.

## Resolved Decisions

| Area | Decision |
|------|----------|
| Platform | ASP.NET Core Blazor web app (.NET ecosystem) |
| Progress model | Flat milestone checklist (12 items), single default template |
| Milestone template | 1. Designed / 2. Sent for approval / 3. Approved to build / 4. Production started / 5. Components machined and assembled / 6. Components finished / 7. Final assembly done / 8. Loaded / 9. Delivered / 10. Billed / 11. Paid / 12. Closed |
| Change orders | Sub-milestone groups that cycle through redesign → requote → reapprove → re-enter production |
| Phases | Deferred to future (Notes field for now) |
| Documents | 3 typed buckets: Customer (emails, specs, paint chips, customer photos), Design (drawings, renderings, site photos), Production (cut lists, machining sheets, labels) — manual upload |
| Email handling | Manual PDF export and upload to Customer bucket; Gmail API integration deferred |
| Scheduling | Lead date, start date, delivery date only (stage-level estimates too difficult, deferred) |
| Access roles | Admin (Owner + User) — full access; Shop (production workers + finisher) — read-only |
| Cost tracking | Quote amount + change order amounts only; QuickBooks handles detailed financials |
| QuickBooks | CSV export (estimates + invoices) for manual import; live API integration deferred |
| CNC files | Live at the machines, not in Job Tracking |
| Gmail integration | Deferred — manual PDF upload to Customer bucket |

## Created Files

- `docs/future-improvements.md` — tracks deferred items (graphical workflow editor, Gmail integration, photo bucket)
- `docs/adr/0001-milestone-checklist-over-states.md` — rationale for milestone model over state machine
- `CONTEXT.md` updated — added **Milestone** and **Change Order** terms

## Known Issues

- **NU1903 — CVE-2025-6965**: `SQLitePCLRaw.lib.e_sqlite3` 2.1.11 (transitive via `Microsoft.EntityFrameworkCore.Sqlite` 10.0.9) has a critical memory-corruption vulnerability in bundled SQLite < 3.50.2. No patched 2.x exists (package line deprecated). Risk is low for local/embedded DB usage. Suppressed via `<NoWarn>NU1903</NoWarn>` in `JobTracking.Api.csproj` and `JobTracking.Api.Tests.csproj`. Track upstream for a real fix — watch `Microsoft.Data.Sqlite` / `SQLitePCLRaw.bundle_e_sqlite3` v3 line.

## Open / Deferred (for future sessions)

- Scaffold the Blazor project and implement the data model
- JSON schema for Job Tracking data
- Document upload UI
- Milestone checklist UI
- Role-based access enforcement
- QuickBooks CSV export format
- Phases / parallel job tracking
- Graphical workflow/milestone designer
