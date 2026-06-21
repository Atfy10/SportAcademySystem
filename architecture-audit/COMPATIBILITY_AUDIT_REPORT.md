# Backend ↔ Frontend Compatibility Audit Report

**Review date:** 2026-06-21  
**Backend:** SportAcademy (.NET 9)  
**Frontend:** `aura-management-system` (React/TypeScript)  
**Governing standard:** `BACKEND_COMPATIBILITY_ARCHITECTURE_GUIDE.md`

## Executive assessment

The system has a broad functional API, but it does not currently operate as one stable contract. The dominant failure mode is contract drift: controllers, application commands, frontend services, and frontend types have evolved independently. This produces hard integration failures in several mutation workflows and silent correctness failures in filtering and search.

The API should not be stabilized by patching individual calls. The source of truth must become a versioned, testable contract covering routes, query models, request models, response models, enum representation, nullability, and error semantics.

### Risk profile

| Severity | Count | Meaning |
|---|---:|---|
| Critical | 5 | Core workflow is non-functional or failure can be misreported as success |
| High | 15 | User-visible feature is broken, incomplete, or materially incorrect |
| Medium | 18 | Inconsistency creates latent defects, scaling limits, or ambiguous behavior |
| Low | 7 | Documentation, naming, or maintainability weakness |

## Severity model

- **Critical:** blocks a primary workflow, corrupts contract semantics, or hides failures system-wide.
- **High:** breaks a feature or causes materially incorrect data/behavior.
- **Medium:** partial failure, silent limitation, or architecture that will predictably create regressions.
- **Low:** inconsistency with limited immediate runtime impact.

## A. API and integration findings

### Critical findings

| ID | Area | Description | Impact | Root cause |
|---|---|---|---|---|
| C-01 | Error handling | Most controllers return `200 OK` for failed `Result` objects, including failures carrying 400, 404, 409, or 500 in `statusCode`. | The frontend fetch wrapper treats failures as successful HTTP calls; monitoring, caches, retries, and clients receive false success signals. | Transport status and application result status are modeled separately but never mapped at the web boundary. |
| C-02 | Attendance | Single attendance sends `{sessionOccurrenceId, traineeId, status, checkInTime}` while the backend requires `{attendanceDate, attendanceStatus, checkInTime, coachNote, enrollmentId, sessionOccurrenceId}`. | Single attendance marking cannot bind to the command correctly. | Frontend models attendance around a trainee; backend models it around an enrollment, with different field names. |
| C-03 | Attendance bulk | Frontend sends a raw array of attendance items; backend requires `{ items: [...] }`. | Bulk attendance—the primary attendance modal workflow—returns model-binding/validation failure. | Wrapper command shape was not shared with the client. |
| C-04 | User creation | Frontend calls `POST /api/auth/users`; backend declares `GET /api/auth/users` and binds `CreateUserCommand` containing a different payload. | User creation returns 405 or cannot bind. Roles, password, and active state requested by the UI are unsupported. | Wrong HTTP method plus two independently designed user-creation contracts. |
| C-05 | Mutation routes | Frontend uses resource routes (`PUT /resource/{id}`) for branch, employee, trainee, trainee group, sport, enrollment, and coach; backend updates are mostly `PUT /resource` and coach has no update endpoint. | Most edit modals fail with 404/405 before command validation. | No enforced route convention; frontend follows resource-oriented routes while backend embeds IDs in command bodies. |

### High findings

| ID | Area | Description | Impact | Root cause |
|---|---|---|---|---|
| H-01 | Branch search | Frontend sends `searchTerm`; backend requires `term`. | Branch search produces a binding/validation failure. | Search parameter vocabulary is inconsistent. |
| H-02 | Enrollment search | Frontend sends `searchTerm`; backend requires `term`. | Enrollment search is broken. | Search parameter vocabulary is inconsistent. |
| H-03 | Session search | Frontend sends `searchTerm`; backend requires `term`. | Session and attendance session search are broken. | Search parameter vocabulary is inconsistent. |
| H-04 | Branch status | Frontend calls `PATCH /api/branch/{id}/deactivate`; backend exposes `PATCH /api/branch/{id}/toggle-status`. | Deactivation action fails; toggle semantics are also unsafe for retry and stale UI state. | Different action vocabulary and non-idempotent toggle design. |
| H-05 | Branch payload | Frontend update omits `id` and `isActive`, and serializes coordinates as numbers; backend requires `Id`, `IsActive`, and string coordinates. Create also sends nullable fields where backend requires strings. | Update cannot bind even if the route is corrected; coordinate serialization is incompatible. | DTO property names, requiredness, and types are not shared. |
| H-06 | Employee payload | Frontend update omits required `id`, `firstName`, `lastName`, and sends `secondNumber`; backend requires `SecondPhoneNumber` plus all fields. | Employee edit cannot bind or overwrites fields ambiguously. | Patch-like frontend model is sent to a full-replacement backend command. |
| H-07 | Enrollment payload | Frontend update sends `expiryDate`, `sessionAllowed`, and `subscriptionDetailsId`; backend expects `Id`, `ExpiryDate`, `SessionRemaining`, and `IsActive`. | Enrollment edit cannot perform the requested changes. | UI and domain update models represent different capabilities. |
| H-08 | Coach update | Frontend exposes `PUT /api/coach/{id}` with sport/skill changes; no backend endpoint or command exists. | Coach edit is entirely unsupported. | Missing business capability. |
| H-09 | Trainee group update | Frontend omits required `id` and `branchId`; backend expects a full update command. | Group edit fails after route correction or loses branch context. | Partial versus full-update contract mismatch. |
| H-10 | Trainee filtering | Frontend sends `sport` and `status` to trainee list/search. Controllers bind only page/pageSize and search term. | UI labels filtering as server-side, but parameters are silently ignored; users see unfiltered data and misleading totals. | Query parameters were added client-side without backend filter models. |
| H-11 | Enrollment filtering | Frontend appends arbitrary list/search filters; backend binds none. | Enrollment filters are silently ignored. | No dedicated enrollment filter contract. |
| H-12 | Subscription scale | Subscriptions and subscription types are fully loaded, cached, searched, filtered, and paginated in the browser. | Counts and pages become stale; memory/network use grows with data; authorization boundaries cannot be expressed in server queries. | Backend lacks paginated/search/filter endpoints and frontend compensates locally. |
| H-13 | Enum wire format | Backend serializes enums as camelCase strings; frontend API types frequently declare PascalCase values (`Present`, `Individual`, `Active`, etc.). | TypeScript contracts are false and case-sensitive display/logic can take wrong branches. | Enum serialization policy is not represented in frontend types or generated contracts. |
| H-14 | Route-less failures | `apiFetch` throws only on non-2xx. Because most backend failures are HTTP 200, calls that do not explicitly inspect `isSuccess` can continue as if successful. | False success toasts, stale cache invalidation, and hidden failures are possible across the UI. | Application-envelope failures and transport failures are handled by different mechanisms. |
| H-15 | Authorization | Most protected endpoints require only authentication; no role/policy authorization is visible for destructive, user-management, pricing, or bulk operations. Some reference endpoints are anonymous. | Any authenticated user may reach privileged operations; future bulk actions would lack required authorization guarantees. | Authorization is applied at controller level as a blanket `[Authorize]`, not as capability policies. |

### Medium findings

| ID | Area | Description | Impact | Root cause |
|---|---|---|---|---|
| M-01 | Coordinates | Frontend list DTO uses numeric coordinates; backend branch DTOs expose strings. | Runtime types disagree and map/form code must coerce values. | Domain/entity schema stores coordinates as strings. |
| M-02 | Trainee nullability | Frontend requires `branchName`; backend card DTO permits null. | UI can receive null outside its declared type. | Nullability is not audited across language boundaries. |
| M-03 | Dates | `EmployeeCardDto.hireDate` is typed as JavaScript `Date`, but JSON returns a string. | Date operations can fail unless every consumer parses implicitly/manually. | Transport DTO and in-memory UI model are conflated. |
| M-04 | Group list DTO | Frontend expects trainee-group `name`; backend `ListTraineeGroupDto` has no `Name` although another card DTO does. | Group names can render undefined depending on handler projection. | Multiple overlapping DTOs represent the same list view. |
| M-05 | Branch grouped response | Frontend expects `{sportId, sportName, branchId, branchName}`; backend DTO has no `sportName`. | Grouped branch UI cannot label sport groups without joining another response. | Response contract omits a consumed field. |
| M-06 | Attendance rate | Frontend sends optional `year`; controller binds only `month`. | Year selection is silently ignored and historical rates may be wrong. | Query contract does not support the UI control. |
| M-07 | Notification commands | Read endpoints use `Result<T>`, while mark-read returns bare `bool`/204 and mark-all returns 204. Frontend signatures claim `ApiResult<null>`. | Service typings are inaccurate; error handling differs inside one service. | Notification handlers bypass the shared result pattern. |
| M-08 | Notification ID | Frontend declares notification IDs as strings; backend routes and commands use integers. | Invalid identifiers can reach routing; generated contracts would disagree. | Type drift. |
| M-09 | Update semantics | Many backend `PUT` actions accept partial nullable fields, while others require full models; IDs alternate between body and route. | Clients cannot infer whether PUT means replace or partial update. | No uniform mutation semantics. |
| M-10 | Delete semantics | Several deletes accept JSON bodies on `DELETE`; others use route IDs. Some controllers block on `.Result`. | Proxies/tooling may not handle bodies consistently; sync-over-async can starve threads. | Commands are exposed directly rather than translated by a stable web contract. |
| M-11 | Search validation | Search validation is duplicated in handlers. `SearchValidationBehavior` is unregistered and constrained to non-generic `Result`, making it unsuitable for current paged searches. | Search rules and error messages can diverge. | Abandoned cross-cutting implementation. |
| M-12 | Query model growth | Employee query already carries page plus six scalar filter/sort parameters. Other resources append ad hoc parameters or ignore them. | Adding filters causes constructor churn and inconsistent repository signatures. | No resource-specific filter/sort request models. |
| M-13 | Sorting vocabulary | Employee uses `sortOrder`; guide standard is `sortDirection`. Other list endpoints have no server sorting. | Shared UI sorting cannot use one contract. | First sorting implementation was local rather than platform-level. |
| M-14 | Search routes | Some resources use `/search`, sports also use `/search-name`, and trainee ID search uses `/search/{id}`. | Discovery and SDK generation are less predictable. | Search use cases were added incrementally. |
| M-15 | Resource naming | Routes mix singular (`branch`, `employee`, `trainee`) and plural (`sports`, `notifications`, `nationalities`, `SubscriptionTypes`). | Contract is harder to predict and version. | `[controller]` derives public routes from class names. |
| M-16 | Coach resource | Coach collection data comes from `/api/employee/coaches`, while coach search/details/mutations come from `/api/coach`. | One frontend resource depends on two backend ownership models. | Coach is modeled both as employee specialization and independent resource. |
| M-17 | API versioning | Swagger is labeled v1, but routes have no version segment or negotiated version. | Contract-breaking changes cannot coexist with existing clients. | Documentation version is not runtime versioning. |
| M-18 | Production integration | CORS permits only localhost 8080/8081 and Swagger is Development-only. | A separately hosted production frontend requires configuration changes and production contract diagnosis is difficult. | Environment-specific client origins and API documentation are not modeled as configuration/contracts. |

### Low findings

| ID | Area | Description | Impact | Root cause |
|---|---|---|---|---|
| L-01 | Controller naming | `ChatBotController.cs` contains `ChatController`, so the route is `/api/chat`, not implied `/api/chatbot`. | Maintenance and discovery confusion. | File/class/resource naming drift. |
| L-02 | Typos | Public routes include `trainnes`; internal names include `Coachs`, `Branchs`, and `SearchEmployeess`. | Misspellings become permanent public contract or generated SDK names. | No API naming review gate. |
| L-03 | Empty controller | `DashboardController` has no endpoints while dashboard data is assembled from many calls. | Extra client orchestration and no coherent dashboard read model. | Placeholder controller/deferred aggregation design. |
| L-04 | Response metadata | Nearly all actions omit `ProducesResponseType`; request/response examples are absent. | OpenAPI does not accurately describe outcomes. | Swagger is enabled but not treated as a contract artifact. |
| L-05 | Pagination normalization | Controllers normalize page values, then a behavior normalizes the immutable page request again. | Redundant infrastructure obscures ownership. | Validation responsibility is duplicated. |
| L-06 | Frontend mocks | Dev-mode mock responses can make unsupported routes appear functional. | Integration defects are hidden during local UI development. | Mock mode is not contract-validated against the real API. |
| L-07 | Client cache | Hand-built 30-second caches for subscription data have manual invalidation and no request-key semantics. | Stale or cross-view inconsistent data. | Missing server query capability led to local caching workarounds. |

## B. Frontend service compatibility matrix

Legend: **Compatible** means route, method, and primary shape align from static inspection. **Partial** means some calls align but another contract dimension does not. **Broken** means the current call cannot execute as intended. This is a static contract audit; it does not certify runtime data or database state.

| Frontend service | Compatible capabilities | Incompatible or risky capabilities | Overall |
|---|---|---|---|
| `attendance.services.ts` | Session lists/by-date; attendance roster route | Search uses wrong parameter; year ignored; single payload incompatible; bulk wrapper incompatible | **Broken** |
| `auth.services.ts` | Sign-up, login, refresh, revoke, roles, profile, password, user toggle | Create-user method and payload incompatible; frontend user DTO assumes `isActive` contract not verified by command | **Broken** |
| `branch.services.ts` | Count, dropdown, list, get, delete, stats, create route, sport-link routes | Search parameter; deactivate route; update route and payload; coordinate types; grouped result misses `sportName` | **Broken** |
| `coaches.service.ts` | List through employee, search, counts, rating, detail, create, delete | Update endpoint absent; split resource ownership | **Broken** |
| `employees.service.ts` | List filters/sort, search, counts, detail, delete, toggle, create route | Update route and payload incompatible; `hireDate` transport type wrong | **Broken** |
| `enrollment.services.ts` | Counts, detail, delete, activate/suspend, payment status, dropdowns, create route | Search parameter; update route and payload; list/search filters ignored | **Broken** |
| `family.services.ts` | Search route and parameter | Anonymous access differs from most API; no pagination for potentially growing results | **Compatible with risk** |
| `nationality.services.ts` | List route and primary DTO | Public access and enum/reference-data duplication risk | **Compatible with risk** |
| `nationalityCategory.services.ts` | List route and primary DTO | Public access differs from protected form dependencies | **Compatible with risk** |
| `notifications.service.ts` | List, unread count, read routes | ID type differs; mutation responses are 204, not `ApiResult<null>` | **Partial** |
| `session.services.ts` | List, by-date, count, generation, group picker | Session/group search parameter differs for session only (`term` vs `searchTerm`); group search aligns | **Partial** |
| `sport.services.ts` | Paginated list, search, count, name search, dropdown, detail, delete, create, skill level | Update route incompatible; enum casing contract differs | **Broken** |
| `sportPrice.service.ts` | List, composite lookup, create, update, delete | DELETE-body convention is fragile; no paging/filtering | **Compatible with risk** |
| `subscription.service.ts` | All/detail/create/update/delete/dropdown/stats/renew/suspend/activate routes | List/search/filter/paging are browser-side; cached data can be stale | **Partial** |
| `subscriptionType.service.ts` | All/detail/create/update/delete routes and payloads | List/search/filter/paging are browser-side | **Partial** |
| `trainee.service.ts` | Counts, detail, create, delete, dropdown through enrollment service | List/search filters ignored; update route incompatible; branch nullability differs | **Broken** |
| `traineeGroup.services.ts` | List, detail, create route, delete, count, dropdowns | Search naming currently aligns; update route/payload incompatible; list name field uncertain/missing | **Broken** |
| `videoAnalysis.services.ts` | Analyze, detail, current-user list and nested DTO shapes | AI call duration can exceed global 30-second timeout; no operation-specific timeout supplied | **Compatible with risk** |

## C. Business capability gaps

| Capability gap | User/business impact | Complexity | Dependencies |
|---|---|---:|---|
| Trainee bulk status update | Bulk action UI explicitly cannot persist changes. | High | Explicit status model; policy authorization; transactional bulk command; old/new audit records; affected count; idempotency decision. |
| Server-side trainee filters | Sport/status filters display but do not affect server results or totals. | Medium | `TraineeFilter`; repository query composition; count semantics; contract tests. |
| Server-side enrollment filters | UI controls cannot reliably narrow enrollment data. | Medium | Enrollment filter model; repository projections; search/filter composition. |
| Server-side coach filters/sort | UI filters only the current page, producing incomplete results. | Medium | Coach resource ownership decision; coach filter/sort model; paged repository query. |
| Subscription search/filter/pagination | Large datasets cannot be managed accurately or securely. | Medium–High | Paginated query endpoints; filter/sort models; cache removal/migration. |
| Subscription-type search/filter/pagination | Same scaling and correctness issue as subscriptions. | Medium | Paginated resource query; status/sport filters. |
| Coach update | Coach sport and skill changes cannot be persisted. | Medium | Update semantics; coach identifier definition; validation; authorization/audit. |
| User role assignment | UI collects roles, but backend create-user command cannot accept or assign them. | High | Role policy model; Identity role assignment transaction; audit; safe password lifecycle. |
| User active state contract | UI exposes activation state; backend/domain uses ban/deletion concepts and toggle action without one documented state model. | Medium | Canonical user lifecycle vocabulary and authorization. |
| Historical attendance rate | UI supplies year but API cannot scope by year. | Low–Medium | Date-range or period filter model; aggregate query update. |
| Dashboard read model | Dashboard makes many independent requests and can show a mixed-time snapshot. | Medium | Dashboard projection/aggregation contract; authorization-aware metrics; caching strategy. |
| General bulk operations | Selection components exist, but no standard bulk contract exists beyond attendance. | High | Bulk envelope, per-item errors, authorization, audit trail, transaction strategy, limits. |

## D. Technical-debt assessment

The compatibility defects originate in five structural weaknesses:

1. **Web contracts are application commands.** Controllers bind MediatR commands directly. This leaks use-case internals into HTTP and makes route IDs, body IDs, update semantics, and versioning difficult to control independently.
2. **No executable contract boundary exists.** OpenAPI is not complete, frontend types are handwritten, and tests stop below the HTTP layer. There are no route/model/serialization integration tests.
3. **Resource query architecture is local.** Employee filtering is implemented one way; other resources ignore parameters or move work to the browser. Search validation is duplicated and the abandoned behavior is dead code.
4. **Result semantics stop before HTTP.** Application failures are represented well enough internally, but controllers discard their status meaning.
5. **Public naming is derived accidentally.** `[controller]`, class names, typos, and singular/plural variation define routes without a reviewed API vocabulary.

## Audit boundaries and confidence

Reviewed: all backend controllers; shared result/pagination/error behaviors; relevant commands and DTOs; repository/query patterns; all frontend service files; shared fetch client; all frontend API types/commands; feature usage; and backend test inventory.

Not executed: live API/database calls, authentication against a running environment, AI provider calls, SignalR runtime behavior, or load tests. These require the validation gates defined in the roadmap and should not be inferred from static compatibility alone.
