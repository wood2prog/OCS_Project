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
- Detailed entity model (Job → Room → Wall → Cabinet → Parts)
- Standard cabinet specifications (heights, depths, construction methods)
- CNC machine type and G-code requirements
- Output format specifications (cut lists, labels, QuickBooks data)
- Design rules engine approach
