# Backend ↔ Frontend Compatibility Implementation Roadmap

## Roadmap principles

- Contract decisions precede code changes.
- Stabilize transport behavior before adding business features.
- Migrate by architectural capability and resource contract, not by file.
- Every phase has measurable exit criteria.
- Compatibility aliases are temporary migration tools, not a second API.

## Phase 0 — Contract baseline and decisions

**Goal:** Freeze an agreed target before changing either implementation.

### Outcomes

- Approve the API vocabulary, versioning approach, enum casing, date/null semantics, update semantics, and error strategy.
- Produce the current and target OpenAPI contracts.
- Define resource ownership, especially coach versus employee.
- Define role/capability and lifecycle-state matrices.
- Classify each current route as retain, adapt temporarily, replace, or retire.

### Exit criteria

- Architecture decisions are approved.
- Every frontend service call has an owner and target contract.
- Breaking changes and compatibility windows are published.

## Phase 1 — Contract fixes

**Goal:** Restore primary workflows against one coherent v1 contract.

### Workstreams

1. **Mutation identity and semantics**
   - Align route IDs and bodies for branch, employee, trainee, trainee group, sport, enrollment, and coach.
   - Select PUT versus PATCH behavior per operation.
   - Remove DELETE bodies from the target contract.

2. **Payload alignment**
   - Resolve branch coordinate, active-state, and requiredness mismatches.
   - Align employee full/partial update fields and second-phone naming.
   - Align enrollment session/subscription update capabilities.
   - Align attendance single and bulk envelopes around enrollment identity.
   - Align user creation with password, role assignment, and active-state requirements.

3. **Search vocabulary**
   - Standardize `search` for branch, enrollment, session, employee, coach, sport, trainee, group, and family.

4. **Response alignment**
   - Resolve grouped branch `sportName`, group list `name`, notification ID, branch nullability/coordinate types, date transport types, and enum casing.

### Dependencies

- Phase 0 decisions.
- OpenAPI and frontend transport-type generation/checking available early in the phase.

### Exit criteria

- Create/edit/delete/status/search flows pass HTTP integration and frontend service tests.
- Attendance single and bulk workflows pass end to end.
- No frontend transport type contradicts the published contract.

## Phase 2 — Standardization

**Goal:** Make the entire API predictable, not merely compatible.

### Workstreams

1. Normalize plural, versioned resource routes and explicit transition semantics.
2. Apply one pagination request/response standard to every growing collection.
3. Apply `search`, resource filters, `sortBy`, and `sortDirection` consistently.
4. Make unknown query fields and invalid sort keys fail clearly.
5. Apply one enum/date/nullability serialization policy.
6. Complete OpenAPI response status, error, and example documentation.

### Migration strategy

- Introduce canonical routes first.
- Add thin legacy adapters only for calls that cannot move in the same release.
- Capture telemetry for legacy calls and remove them by a dated milestone.

### Exit criteria

- A shared API convention checklist passes for every controller/resource.
- OpenAPI contains no undocumented public endpoint or outcome.
- Legacy route use is measurable and declining.

## Phase 3 — Infrastructure

**Goal:** Prevent recurrence through shared platform capabilities.

### Workstreams

1. **Result-to-HTTP adapter**
   - One translation path for application results, model binding, validation, authorization, and unhandled errors.

2. **Query infrastructure**
   - Shared page/search/sort primitives.
   - Resource-specific filter objects and typed query composition.
   - Deterministic sorting and filter-before-count guarantees.

3. **Contract toolchain**
   - Versioned OpenAPI artifact.
   - Frontend type generation or compatibility check.
   - CI breaking-change detection.

4. **Security and audit**
   - Named authorization policies.
   - Audit events for privileged and bulk changes.
   - Correlation/trace IDs in errors and audit records.

5. **Test platform**
   - ASP.NET HTTP integration harness.
   - Contract fixtures and frontend API-client test harness.

### Exit criteria

- Business failures produce correct HTTP statuses and one error schema.
- Combined query behavior is reusable across at least three resources.
- CI blocks route/DTO/enum breaking changes without explicit approval.

## Phase 4 — Business features

**Goal:** Add currently missing capabilities on top of the standardized platform.

### Priority order

1. Trainee server-side sport/subscription-status/branch filters.
2. Enrollment status/payment/sport/branch/date filters.
3. Subscription and subscription-type server pagination, search, filters, and sort.
4. Coach filters, sort, and update capability under the canonical coach resource.
5. User creation with authorized role assignment and documented lifecycle state.
6. Historical attendance period filtering.
7. Trainee bulk status operation with authorization, transaction policy, affected count, and old/new audit records.
8. Dashboard read model if consistency and request volume justify it.

### Complexity and dependency notes

- Bulk status and role assignment are high complexity because audit and authorization are part of the feature definition.
- Subscription query work must precede removal of browser-side pagination/cache.
- Coach update depends on the coach identity/ownership decision.

### Exit criteria

- UI controls have corresponding documented server capabilities.
- No business collection relies on loading the entire dataset for filtering or pagination.
- Bulk-operation audit records answer who, what, when, old/new state, and affected count.

## Phase 5 — Frontend integration

**Goal:** Consume the canonical contract consistently and remove compatibility workarounds.

### Workstreams

1. Replace handwritten transport types with generated/verified types.
2. Centralize query-string construction for page/search/filter/sort.
3. Update the API client for truthful non-2xx errors and the standard error envelope.
4. Migrate services resource-by-resource to canonical routes.
5. Replace full-dataset caches and client pagination with server queries.
6. Keep UI view models separate from wire DTOs; parse date strings explicitly.
7. Replace mock responses with contract-derived fixtures.

### Recommended migration slices

1. Authentication/users and shared API client.
2. Attendance/session occurrences.
3. Branch/employee/coach.
4. Trainee/group/enrollment.
5. Sports/pricing/subscriptions.
6. Notifications/video analysis/reference data.

### Exit criteria

- No frontend service calls legacy routes.
- Every list uses server totals and deterministic pages.
- Frontend service and end-to-end suites cover all primary workflows.

## Phase 6 — Refactoring and retirement

**Goal:** Remove the duplicate paths and debt retained for safe migration.

### Workstreams

- Remove legacy route adapters after telemetry reaches zero and the support window closes.
- Remove dead search behavior and duplicated handler validation.
- Remove redundant pagination normalization ownership.
- Consolidate overlapping DTOs for the same projections.
- Remove client-side subscription caches and business filtering.
- Rename misspelled internal types once public compatibility is detached from class names.
- Remove empty/placeholder controllers or give them a deliberate read-model responsibility.
- Align repository abstractions with typed query specifications without leaking EF/Dapper concerns into Application.

### Exit criteria

- One production contract and one implementation path exist for each capability.
- No deprecated endpoints or compatibility-only DTOs remain.
- Architecture fitness tests and contract checks are mandatory in CI.

## Delivery gates

| Gate | Required evidence |
|---|---|
| Contract gate | Approved OpenAPI diff, DTO/nullability/enum review, migration classification |
| Functional gate | HTTP integration tests and frontend service tests for route/method/body/response |
| Error gate | Verified 400/401/403/404/409/500 behavior and shared error parsing |
| Query gate | Search/filter/sort/pagination combination tests with deterministic ordering |
| Security gate | Policy matrix tests and audit-event verification |
| Performance gate | Representative dataset tests; no unbounded collection fetches |
| Migration gate | Legacy telemetry, rollback plan, deprecation date, frontend release coordination |

## Suggested release sequence

1. **Foundation release:** contract artifacts, shared error handling, frontend client readiness.
2. **Compatibility release:** primary mutation/search fixes with temporary adapters where necessary.
3. **Query release:** standardized server-side filters/sort/pagination.
4. **Capability release:** coach update, user roles, bulk trainee status, historical attendance.
5. **Retirement release:** remove legacy routes and frontend workarounds.

This sequence keeps architectural consistency ahead of local fixes while allowing production risk to be controlled through versioning, telemetry, and explicit exit gates.
