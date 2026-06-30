# ADR 0003: Sequential milestone stepper

Status: accepted | supersedes clause of ADR 0001

The main milestone checklist now enforces sequential order: only the next incomplete milestone is clickable, and completing it unlocks the next. This replaces ADR 0001's "independent check-off" clause but preserves the rest of that decision — this is still a checklist with a guarded UX, not a formal state machine with guarded transitions, and change orders still use the independent sub-milestone model. Sequential enforcement was deferred to now because the initial priority was validating the milestone abstraction itself; with that validated and timestamps in place, the next concern is guiding the worker through the correct sequence rather than just recording whatever gets checked.
