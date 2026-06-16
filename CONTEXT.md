# Cabinet Shop

A custom residential cabinetry business. The shop specializes in a core set of designs with internal standards, but handles fully custom requests by adapting designs and execution to any customer idea.

## Language

**Job Folder**:
The physical manilla envelope containing all artifacts from the initial customer meeting and throughout the job lifecycle: sketches, dimensions, notes, appliance specs, customer idea photos, paint chips, emails, and other materials.
_Avoid_: File, project folder

**Clipboard**:
A set of stapled paper sheets that follows a job through the shop. Contains drawings, an odds-and-ends list, a cabinet color/finish sheet, and a cabinet machining sheet.
_Avoid_: Job packet, traveler

**Odds and Ends**:
Non-cabinet items that must be delivered to a job site: shelf pins, hardware, hood inserts, etc. Once the automated system is in place, this is a transitional concept — moldings and trim are derived from the design file, and remaining items will be captured as structured lists (hardware list, trim list, etc.).
_Avoid_: Accessories, miscellaneous

**Clipboard Sheets**:
The individual paper sheets on a clipboard: drawings, odds-and-ends list, cabinet color/finish sheet, cabinet machining sheet, and occasionally a solid-wood panels/tops sheet.
_Avoid_: N/A

**Milestone**:
A checkable item in a job's progress checklist (e.g. "components machined and assembled", "loaded"). Milestones follow an ordered template that applies across all jobs. Each milestone tracks completion date and the user who marked it.
_Avoid_: Stage, status, step

**Change Order**:
An unplanned revision to a job that occurs during production, triggering a sub-milestone group that cycles through redesign, requote, and reapprove before re-entering production.
_Avoid_: Revision, modification, alteration

**Load Check**:
The final verification that all cabinets and parts are on the trailer before delivery, done against the printout lists.

**S2M Center**:
Screen-to-Machine — Cabinet Vision's module that loads a job and generates CNC toolpaths. Currently produces G-code for the shop's CNC router.

**Cabinet Vision UCS**:
User Created Standards — scripts (UCS:M legacy or UCS:JS JavaScript) that automate part-level modifications in Cabinet Vision, such as toe kick cutouts and vent placement.
_Avoid_: N/A

**Run**:
An abstract reference line that cabinets mount to. A Run can be any contiguous line of cabinets — a physical wall, an island, or a peninsula. Each Run has a name and a type (`wall`, `island`, `peninsula`).
_Avoid_: Wall (when referring to the abstract reference, not the physical wall)

**Job Document**:
The JSON file that describes a specific job's layout: rooms, runs, cabinet positions, customer info, and one-off overrides. Analogous to HTML in a web stack.
_Avoid_: N/A

**Construction Document**:
A JSON file defining assembly templates, joinery rules, and standard profiles for a product line or construction style (e.g. face-frame 5/8 box). Applies across many jobs. Analogous to CSS.
_Avoid_: N/A

**Material Schedule**:
A JSON file binding material references (e.g. `default-plywood`) to actual material specs (species, thickness, finish, supplier). Can be swapped independently of the job and construction documents.
_Avoid_: Material list, cut list

