# Architecture Recommendations

## Target architecture

Establish an explicit anti-corruption boundary between HTTP and application use cases:

```text
React transport client
  ↕ generated/verified OpenAPI contracts
Versioned Web API request/response models
  ↕ explicit mapping
Application commands, queries, filters, and results
  ↕
Domain and repository/query infrastructure
```

This preserves Clean Architecture while preventing MediatR command refactors from accidentally breaking the public API.

## Priority recommendations

### 1. Make the contract executable

Treat OpenAPI as a build artifact, not only a Swagger UI. Complete response declarations, enum values, nullability, parameter names, and examples. Generate or validate frontend transport types from the specification. Add a CI diff gate for breaking contract changes.

This directly addresses the root cause behind route, payload, enum, and nullability drift.

### 2. Centralize result-to-HTTP translation

Adopt one result adapter at the web boundary. It must map application status to actual HTTP status and preserve the common error envelope. Framework model-binding and authentication errors must use the same error family.

Avoid controller-by-controller `if (!result.IsSuccess)` logic; the current partial adoption proves that local handling diverges quickly.

### 3. Separate web models from application commands

Define versioned request DTOs for create, replace, patch, action, and bulk operations. Controllers should combine route values and bodies into application commands explicitly. This makes route IDs authoritative and allows the application model to evolve without silently changing HTTP.

### 4. Build composable query infrastructure

Create shared abstractions for page, search, sort, and date range, then resource-specific filters. Keep public sort/filter allow-lists near the web/application boundary and map them to typed query expressions. Apply filtering and search before count and pagination.

Do not create one universal dynamic query object. Shared mechanics should be common; valid fields and domain semantics should remain resource-specific.

### 5. Consolidate resource ownership

Make coach operations coherent under `/coaches`. Keep employee-to-coach conversion and coach-candidate selection as explicit relationship/use-case endpoints. Likewise, distinguish subscription definitions (`subscription-types`) from trainee subscriptions (`subscriptions`) in naming.

### 6. Define lifecycle state models

Replace ambiguous combinations such as `status`, `isActive`, `isWork`, `isBanned`, `isSubscribed`, suspend/activate, and toggle endpoints with documented state machines per resource. State transitions should be explicit, authorized, validated, auditable, and idempotent where possible.

### 7. Move data management to the server

Remove browser-side full-dataset pagination, search, and business filtering once equivalent server queries exist. The frontend should send query state and render returned pages. Client-side filtering remains appropriate only for already bounded presentation data, not business collections.

### 8. Add policy authorization and meaningful audit history

Use named authorization policies at use-case level. Build an audit event mechanism for high-impact and bulk mutations; EF audit columns alone cannot answer what changed or provide per-operation affected counts.

### 9. Standardize identifiers and value types

Document which identifier a coach route uses, align notification ID types, use numeric coordinates, and represent dates consistently. Normalize enum casing through generated contracts. These are contract decisions, not mapper details.

### 10. Remove compatibility-obscuring code

After migration, remove dead search behavior, redundant pagination normalization, obsolete route aliases, duplicate DTOs for the same view, local subscription caches, client pagination workarounds, and dev mocks that are not generated from the contract.

## Breaking-change risk assessment

| Risk | Probability | Impact | Mitigation |
|---|---:|---:|---|
| Route normalization breaks the current frontend | High | High | Introduce v1 routes or temporary adapters; migrate services by resource; instrument legacy use. |
| Truthful non-2xx responses change frontend control flow | High | High | Update shared client first; add error-envelope parsing tests; migrate feature error handling before backend switch. |
| Enum casing correction breaks comparisons | High | Medium | Generate enum types; add serialization contract tests; normalize at one migration boundary only. |
| Server pagination changes visible totals/order | Medium | High | Define deterministic sorting and filter-before-count semantics; acceptance-test representative datasets. |
| PUT/PATCH cleanup changes null behavior | High | High | Publish per-field patch semantics; test omitted versus explicit null; avoid implicit full replacements. |
| Authorization policies expose hidden dependency on broad access | Medium | High | Build role-capability matrix; run permission acceptance tests; deploy with audit-only telemetry if needed. |
| Removing client caches increases request volume | Medium | Medium | Measure endpoints, add query caching/ETags where justified, and use a standard frontend query cache. |
| Compatibility aliases become permanent | Medium | Medium | Require owner, telemetry, deprecation date, and removal criterion for every alias. |

## Validation strategy

### Contract validation

- Snapshot/version the OpenAPI document.
- Detect breaking OpenAPI changes in CI.
- Generate or compile-check frontend transport types against the specification.
- Verify every frontend service route, method, query, body, and response against OpenAPI.
- Add serialization tests for every public enum, nullable field, date type, and coordinate type.

### Backend integration validation

- Use HTTP-level tests with the real ASP.NET Core pipeline.
- Cover success plus 400/401/403/404/409/500 status mapping.
- Cover model binding, validation error schema, route/body ID disagreement, unknown filters, and invalid sorts.
- Cover pagination boundaries, filter-before-count, deterministic sorting, combined search/filter/sort, and authorization policies.
- Test bulk transaction behavior, affected counts, per-item failures, and emitted audit events.

### Frontend integration validation

- Test the shared API client against 200, 201, 204, 400, 401 refresh/retry, 403, 404, 409, malformed JSON, timeout, and network failure.
- Replace freehand service mocks with contract-derived fixtures.
- Add service-level tests for URL construction and request bodies.
- Add end-to-end tests for create/edit/delete/search/filter/sort/paginate workflows by resource.

### Runtime validation

- Correlate frontend and backend requests with trace IDs.
- Measure 4xx/5xx rates by endpoint and deprecated-route usage.
- Canary the new contract and compare key list counts and detail projections.
- Run representative data-volume tests before removing client-side workarounds.

## Architecture decision records required

Before implementation, record decisions for:

1. API versioning mechanism and compatibility window.
2. Result envelope versus Problem Details, including HTTP mapping.
3. PUT/PATCH and explicit-null semantics.
4. Enum casing and contract generation toolchain.
5. Search implementation and relevance behavior.
6. Filter/sort abstraction boundaries.
7. Bulk-operation atomicity and audit-event storage.
8. Coach resource identity and ownership.
9. Resource lifecycle/state vocabularies.

These decisions prevent the roadmap from degrading into disconnected endpoint fixes.
