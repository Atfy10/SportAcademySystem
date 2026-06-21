# API Standardization Proposal

## 1. Contract governance

Adopt a versioned, resource-oriented HTTP contract as the sole public API definition. Application commands and entities must not be the public contract by default.

The approved contract vocabulary should be represented in OpenAPI and validated in CI. Frontend API types should be generated from that contract or checked against it automatically. Handwritten UI view models may remain, but transport models should not be duplicated manually.

## 2. Route standard

Use lowercase plural resource names under an explicit version:

| Operation | Standard |
|---|---|
| Collection | `GET /api/v1/trainees` |
| Detail | `GET /api/v1/trainees/{id}` |
| Create | `POST /api/v1/trainees` |
| Replace | `PUT /api/v1/trainees/{id}` |
| Partial update | `PATCH /api/v1/trainees/{id}` |
| Delete | `DELETE /api/v1/trainees/{id}` |
| Search | Prefer `GET /api/v1/trainees?search=...`; retain `/search` only if contract migration requires it |
| Subresource | `GET /api/v1/sports/{sportId}/enrollments` |
| Explicit transition | `POST /api/v1/enrollments/{id}/actions/suspend` or a documented status PATCH |
| Bulk operation | `POST /api/v1/trainees/bulk-actions/status` |

Rules:

- Route IDs are authoritative. If an ID is also present in a body during migration, disagreement is a 400 error.
- Never use a request body on DELETE. Composite resources use a fully identified route.
- Avoid `toggle` actions. Use an explicit desired state so retries are idempotent.
- Resolve coach ownership: `/coaches` owns coach collection, search, detail, update, and delete. Employee-based candidate selection is a separate relationship endpoint.
- Correct misspellings only in a versioned route or through a documented compatibility alias with removal date.

## 3. Naming standard

- JSON properties: `camelCase`.
- Query parameters: `camelCase`.
- Resource names: plural nouns.
- Search term: `search` everywhere.
- State filter: `status` only when the domain has a named lifecycle; otherwise use the explicit boolean (`isActive`, `isSubscribed`). Do not expose both for the same meaning.
- Sort direction: `sortDirection`, values `asc` or `desc`.
- Coordinates: numeric latitude/longitude-style values with one documented coordinate system; do not expose numeric data as strings.
- Dates: ISO 8601. Use date-only (`YYYY-MM-DD`) where time has no meaning and UTC timestamps with `Z` where it does.

## 4. Pagination standard

Every growing collection accepts:

- `page`: one-based integer, default 1.
- `pageSize`: integer, default 10, maximum 100.

Every paginated response uses one shape:

```text
items, totalCount, page, pageSize
```

Recommended additions are `totalPages`, `hasNextPage`, and `hasPreviousPage`, but they should be introduced once and applied uniformly.

Invalid explicit values should not be silently changed without documentation. Choose and document one policy: normalize with response metadata/warnings, or reject with 400. The current normalization policy may remain for compatibility, but must be contract-tested.

Dropdown/reference endpoints may be unpaginated only when the domain enforces a safe bounded cardinality. Otherwise they use search plus a small page size.

## 5. Search standard

All searchable collections support:

```text
?search=ahmed&page=1&pageSize=10
```

The standard defines:

- whitespace trimming;
- minimum length (recommended two characters for full-text search);
- matching fields per resource;
- case/accent behavior;
- empty-search behavior;
- deterministic secondary sorting when relevance scores tie.

Search validation belongs in shared query infrastructure, not repeated handlers. ID lookup is detail retrieval, not search; remove `/search/{id}` from the future contract.

## 6. Filtering standard

Each resource gets a dedicated web filter model and corresponding application filter object. Filters compose with search, sorting, and pagination in one query.

Examples:

```text
GET /api/v1/trainees?search=ahmed&sportId=1&isSubscribed=true&branchId=3
GET /api/v1/coaches?sportId=1&branchId=3&isActive=true
GET /api/v1/enrollments?status=active&paymentStatus=paid&createdFrom=2026-01-01
```

Standards:

- Foreign keys use `{resource}Id`.
- Date ranges use `{field}From` and `{field}To`.
- Multi-value filters use repeated keys or one documented comma-separated form, never both.
- Unknown filter/sort fields return 400 rather than being ignored.
- Filter values and enum values are documented in OpenAPI.
- Server filtering occurs before count, sort, and pagination.

## 7. Sorting standard

Every paged collection accepts:

```text
?sortBy=name&sortDirection=asc
```

Each resource publishes an allow-list of stable sort fields. Unknown fields or directions return 400. Every sort includes a deterministic ID tie-breaker to prevent duplicate/missing records across pages.

Do not expose database column names directly. Public sort keys are contract names mapped to query expressions.

## 8. Mutation semantics

- POST creates and returns 201 plus the created representation or identifier and a `Location` header.
- PUT replaces the complete mutable representation and requires all replaceable fields.
- PATCH changes only supplied fields. Null means an explicit clear only where the contract permits it.
- DELETE returns 204 after deletion; deleting a missing resource follows one documented policy (404 recommended).
- Business transitions either use an explicit status PATCH or action endpoint and return the resulting state.
- Commands that affect several records use a bulk envelope with operation, target IDs, optional concurrency token, and a result containing affected count and per-item failures where partial success is allowed.

## 9. Response and error standard

Select one transport strategy and apply it globally. The recommended incremental choice is to retain the result envelope for successful and failed business outcomes while making HTTP status truthful.

Success envelope:

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "...",
  "data": {}
}
```

Error envelope:

```json
{
  "isSuccess": false,
  "statusCode": 404,
  "message": "Trainee not found",
  "errors": null,
  "traceId": "..."
}
```

Required mapping:

| Outcome | HTTP status |
|---|---:|
| Successful read/update | 200 |
| Successful create | 201 |
| Successful delete/no body | 204 |
| Validation/model binding | 400 |
| Unauthenticated | 401 |
| Unauthorized | 403 |
| Not found | 404 |
| Conflict/business invariant | 409 |
| Unexpected failure | 500 |

Use a single web-boundary adapter (base controller, result extension, or result-aware action filter). Framework model-binding errors, FluentValidation errors, domain failures, and unhandled exceptions must converge on the same error schema. Never expose exception internals in production.

## 10. Enum and nullability standard

- Choose one enum casing; because the API already emits camelCase, make camelCase the documented wire format.
- Generate frontend enum unions from OpenAPI.
- Reject unknown enum values with a structured 400.
- Model nullable fields identically in OpenAPI, C#, and TypeScript.
- Separate transport date strings from parsed UI date objects.
- Avoid `unknown` response types in frontend services for established endpoints.

## 11. Authorization and audit standard

Define capability policies such as trainee read/write, attendance manage, pricing manage, identity administer, and bulk operations. Apply policies per operation, not only authentication per controller.

Bulk and high-impact mutations must record:

- actor user ID;
- timestamp;
- entity and IDs;
- old and new values;
- affected count;
- correlation/trace ID;
- outcome and per-item failures.

Existing Created/Updated/Deleted metadata is useful but is not a change history and does not satisfy bulk-operation audit requirements.

## 12. Versioning and migration

Introduce `/api/v1` as the stabilized contract. Where current clients must remain operational:

1. Publish the canonical v1 endpoint.
2. Keep old routes as adapters only when needed.
3. Add deprecation headers and a removal date.
4. Instrument old-route usage.
5. Remove aliases after frontend migration and an agreed support window.

Do not maintain two independent handler implementations; compatibility routes must delegate to the same application use case.
