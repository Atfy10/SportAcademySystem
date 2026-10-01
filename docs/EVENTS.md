# Events — developer guide

**Events** is the venue-booking module: an academy rents one of its branches to a customer for a party,
wedding, birthday, etc., bills them through the normal finance ledger, and lets their guests in at
the door with per-guest QR tickets that staff scan.

This guide covers **both repos**:

| Repo | Path |
|---|---|
| Backend (.NET 9, Clean Architecture, CQRS/MediatR, EF Core 9 + SQL Server) | `Backend/SportAcademy/SportAcademy` |
| Frontend (React + TypeScript + Vite + shadcn/ui) | `Frontend/aura-management-system` |

> A snapshot as of 2026-10-01 (PRs SportAcademySystem#11/#13, aura-management-system#16/#18). The code
> is the source of truth — most classes below carry a header comment explaining *why* they are the way
> they are; read those before changing behaviour.

---

## 1. Business rules at a glance

| Rule | Where it is enforced |
|---|---|
| Feature `event-management` — **Enterprise plan only** (sold separately in the bundle builder). | `AppDataSeeder` feature catalog; every command/query implements `IRequiresFeature` |
| An event belongs to **one branch**, has a title, a customer, price, optional decoration fee, capacity, start/end. | `Event`, `EventValidators` |
| Customers are **deduplicated by phone** (E.164, unique per tenant). A returning customer is picked up, never overwritten. | `EventCustomer`, `CreateEventCommandHandler.ResolveCustomerAsync` |
| All money goes through the **finance ledger**: one invoice per event (rental line + optional decoration line). | `FinanceLedgerService.IssueEventInvoiceAsync` / `ReviseEventInvoiceAsync` |
| At booking: pay **nothing / deposit / full**. The rest is owed by a collect date (defaults to the event day). | `CreateEventCommand.AmountPaidNow` / `BalanceDueDate` |
| **Overlapping** bookings at a branch only **warn**, they don't block. | `GET api/events/overlaps`, `EventFormModal` |
| **Status** (Upcoming / Ongoing / Completed / Cancelled) is *derived*, never stored. | `EventStatusRules` |
| **Cancel** either refunds every payment or keeps what was collected and waives the rest. | `CancelEventCommandHandler`, `EventCancellationMode` |
| **Delete** only while no money was ever collected (soft delete); otherwise cancel. | `DeleteEventCommandHandler` |
| "Created by" is mandatory (never `Guid.Empty`). | `EventRuleException.NoActingUser` |
| **Entry**: each guest has their own ticket (random 6-digit number + secret code). Staff scan it and **Admit** or **Refuse**. | `EventTicket`, check-in commands, `EventCheckIn.tsx` |
| Once an event **ends or is cancelled, all its tickets are terminated for good.** | `EventEntryRules.TicketsTerminated` |

---

## 2. File map

### Backend

```
SportAcademy.Domain/
  Entities/Events/Event.cs, EventCustomer.cs, EventTicket.cs
  Enums/EventStatus.cs, EventPaymentState.cs, EventCancellationMode.cs,
        EventTicketCheckResult.cs, EventTicketFilter.cs
  Services/EventStatusRules.cs        derived status + day bounds
  Services/EventEntryRules.cs         entry window, ticket termination, token/number generation
  Exceptions/EventExceptions/         EventRuleException (all rule errors), *NotFoundException
  Authorization/Permissions.cs        Permissions.Event.{Manage,View,Report,CheckIn}

SportAcademy.Application/
  Commands/EventCommands/             Create, Update, Cancel, Delete
  Commands/EventCustomerCommands/     Create, Update, Delete
  Commands/EventTicketCommands/       Issue, Update (rename), Reissue, Revoke, Admit
  Queries/EventQueries/               GetEvents, GetEventById, GetEventOverlaps, GetEventsReport
  Queries/EventCustomerQueries/       GetEventCustomers, GetEventCustomerById, GetEventCustomerByPhone
  Queries/EventTicketQueries/         GetEventTickets, CheckEventTicket, GetCheckInEvents, GetPublicEventTicket
  DTOs/EventDtos/EventDtos.cs, EventTicketDtos.cs
  Mappings/Manual/EventMapper.cs, EventTicketMapper.cs   (hand-written, not AutoMapper)
  Services/EventDetailsLoader.cs      one loader for "event + bill + payments" (every command returns it)
  Services/EventTicketCheckService.cs shared find / judge / describe for the door
  Services/FinanceLedgerService.cs    Issue/Revise/CloseInvoiceForCancelledEvent
  Interfaces/IEventRepository.cs, IEventCustomerRepository.cs, IEventTicketStore.cs
  Validators/EventValidators/         EventValidators, EventCustomerValidators, EventTicketValidators

SportAcademy.Infrastructure/
  Persistence/Configurations/Events/  EventConfiguration, EventCustomerConfiguration, EventTicketConfiguration
  Persistence/Repositories/           EventRepository, EventCustomerRepository, EventTicketStore
  Persistence/Migrations/             AddEventsAndEventCustomers, AddEventEntryQrCode, ReplaceEventEntryQrWithTickets
  Localization/Resources/{en,ar}.json "errors.event.*" messages
  Seeders/AppDataSeeder.cs            feature catalog + role permission defaults

SportAcademy.Web/Controllers/
  EventsController.cs                 api/events (+ tickets, check-in)
  EventCustomersController.cs         api/event-customers
  PublicController.cs                 api/public/event-tickets/{token} (anonymous)

SportAcademy.Tests/
  Application/Services/EventBookingTests.cs   handlers + real ledger over EF InMemory
  Domain/EventStatusRulesTests.cs
```

### Frontend

```
src/services/event.service.ts           types mirroring the DTOs + every API call
src/lib/academyClock.ts                 academyNow(tz) - "now" on the academy's clock
src/lib/qrImage.ts                      composeQrImage() - shareable QR picture (org name, caption, "AURA")
src/components/modals/EventFormModal.tsx         book / edit (phone lookup, overlap warning, pay-now)
src/components/modals/EventCustomerFormModal.tsx
src/pages/events/
  Events.tsx               list + EventsTimeline + filters           /events
  EventsTimeline.tsx       week grid: rows = days, hours across, branch switcher
  EventProfile.tsx         one event: booking, customer, bill, tickets, payments, cancel/delete, print
  EventTicketsCard.tsx     tickets on the event page
  EventTicketQrDialog.tsx  one ticket's QR: copy / download / share
  EventCheckIn.tsx         the door scanner                          /events/check-in, /staff/check-in
  EventCustomers.tsx, EventCustomerProfile.tsx                        /events/customers[/:id]
  EventsReport.tsx, EventsReportPrint.tsx                             /events/report
  EventPrintSheet.tsx      printable booking sheet + cut-out ticket sheet
  eventUi.tsx              useWallClock, useEventSpan, badges, EventsTabs, eventsBase
  ticketUi.ts              useTicketCaption, fetchAllTickets
src/pages/EventTicket.tsx  the guest's public ticket page            /ticket/:token
src/i18n/locales/{en,ar}/events.json, eventTicket.json, eventCheckIn.json
```

---

## 3. Data model

| Table | Entity | Scope | Notes |
|---|---|---|---|
| `Events` | `Event` | `ITenantScoped`, `IBranchScoped`, `ISoftDeletable`, `IAuditableEntity` | `RowVersion`; index `(BranchId, StartsAt)`; FK to branch, customer, invoice (all `Restrict`) |
| `EventCustomers` | `EventCustomer` | tenant-scoped, soft-deletable, **not** branch-scoped | unique `(TenantId, PhoneNumber)` among non-deleted rows |
| `EventTickets` | `EventTicket` | tenant-scoped; branch-scoped **through `Event.BranchId`** (navigation filter in `ApplicationDbContext`) | unique `Token` (global), unique `(EventId, Number)`; `RowVersion` |

Finance additions made for events (migration `AddEventsAndEventCustomers`):

- `Invoice.PayerName` / `Invoice.PayerPhone`: copied from the customer when the invoice is issued, because payer names used to be hard-wired to `Invoice.Trainee`.
- `InvoiceLineType.EventFee` / `EventDecoration`, plus `InvoiceLine.EventId`.

Migrations, in order:
1. `AddEventsAndEventCustomers`
2. `AddEventEntryQrCode`: the first entry design (one QR per event, guests self-scan). Superseded.
3. `ReplaceEventEntryQrWithTickets`: drops `EventAdmissions`, `Events.EntryToken` and `Events.AdmittedCount`, and creates `EventTickets`. No backfill.

**What is *not* stored (derived on read):**
- event status (`EventStatusRules`);
- payment state (`EventMapper.ResolvePaymentState`);
- the admitted count (count of tickets with `AdmittedAt`).

---

## 4. Time zones — read this before touching dates

Events are the one module where staff type exact wall-clock times, so getting zones wrong shows up
immediately ("I booked 18:00 and it says 21:00").

- **Storage:** `StartsAt` / `EndsAt` are **UTC instants**, like every stored `DateTime`. `UtcDateTimeConverter` marks every `DateTime` read from the DB as `Kind=Utc`, so wall-clock values must never be stored.
- **Per request:** middleware in `Program.cs` reads the academy's zone (`ITenantClock.GetTimeZoneAsync`, from `TenantSettings.TimeZone`) into `TenantCalendar` (an `AsyncLocal`), plus `TenantCalendar.Today`.
- **In:** handlers convert typed local times with `TenantCalendar.ToUtc(request.StartsAt)`. A value already marked UTC (`...Z`) passes through unchanged.
- **Out:** DTOs carry both the UTC instant (`StartsAt`) and the academy wall clock (`StartsAtLocal`, `Kind=Unspecified`, serialized with **no offset**).
- **Frontend:**
  - Show and edit only the `*Local` fields, formatted with `useWallClock()` / `useEventSpan()`. **Never** use `fmt.dateTime` on them; it would shift them a second time.
  - Send times with `toWallClock()`.
  - Use `academyNow(tz)` for "today/now" comparisons.
- **Status filters** compare stored UTC columns against `EventStatusRules.DayBounds(today)`, so list filters and `Resolve` always agree.
- **Anonymous requests** (the public ticket page) have no signed-in tenant, so the handler impersonates the event's tenant and sets `TenantCalendar` itself.

---

## 5. Money — how events use the finance ledger

Events never write `Invoice`/`Payment` rows directly. Everything goes through `IFinanceLedgerService`,
so event money appears in Payments, Outstanding, Revenue and the Financial statement automatically.

| Action | Flow (one `IUnitOfWork.InTransactionAsync`) |
|---|---|
| **Create** | Resolve or insert the customer, then insert the event, then `IssueEventInvoiceAsync` (EventFee + optional EventDecoration line, payer copied), then optionally `RecordPaymentAsync` for the deposit/full amount. Due date: today if paid in full, else `BalanceDueDate` or the event day. |
| **Update** | `ReviseEventInvoiceAsync` re-prices the lines. It refuses a total below what was collected (`TotalBelowPaid`) and a branch move once money was taken (`BranchLocked`). |
| **Cancel / RefundPayments** | Each payment's share of this invoice is refunded via `RefundPaymentAsync` (recorded in the month it happened), then `CloseInvoiceForCancelledEventAsync` cancels the invoice. A payment that also settles *other* invoices is refused (`PaymentSharedWithOtherInvoices`) and must be refunded by hand. |
| **Cancel / KeepPayments** | `CloseInvoiceForCancelledEventAsync` waives the remainder as `DiscountTotal`, so the invoice closes **Paid at the amount kept**. |
| **Delete** | Only without any allocation. The unpaid invoice is cancelled and deleted, and the event is soft-deleted. |

Where event money shows up elsewhere:
- **Financial statement:** an "Events" income category, keyed by `IncomeKind.Event` in `FinancialStatementReader`.
- **Payment receipt:** shows the event title and date (`PaymentReceiptDto.EventId/EventTitle/EventStartsAt`).
- **Events report:** totals at `GET api/events/report`, capped at 2000 rows with `Truncated`.

---

## 6. Customers

- **Lookup:** `GET api/event-customers/by-phone?phone=` normalizes the number to E.164 with the tenant's regional rules. The booking form looks the customer up by phone first, and asks for name and nationality only for an unknown number.
- **Race on create:** if two people book the same new customer at once, the unique index rejects the second insert. `AddOrReuseCustomerAsync` then clears the change tracker and books the existing row instead.
- **Inactive customers** can't be booked (`CustomerInactive`). A customer **with events can't be deleted** (`CustomerHasEvents`); deactivate them instead.
- **Not branch-scoped**, on purpose: a branch-restricted user must still find a customer by phone to book them at their own branch. The customer's totals (`EventCustomerDto.EventCount`, billed, paid, balance) only count events the caller can see.

---

## 7. Tickets and door check-in

### Model
- **`Number`:** random **6-digit** (100000–999999), unique within the event (`EventEntryRules.NewTicketNumber`). It is short enough to type when a code won't scan, and not sequential, so it can't be guessed by counting.
- **`Token`:** 128 random bits as 32 hex characters (`NewToken`), globally unique. The QR encodes `{origin}/ticket/{token}`.
- **`GuestName`:** optional.
- **Admission:** `AdmittedAt` and `AdmittedByUserId`.

### Lifecycle
| Operation | Rule |
|---|---|
| Issue (`POST api/events/{id}/tickets`) | On demand. At most 1000 per request, never more than `Capacity` in total. The event row is touched in the same save, so its **`RowVersion` guards the capacity check**: concurrent issues, or an issue racing a capacity edit, get a 409 "try again". |
| Rename / Re-issue / Revoke | **Unused tickets only** (a used ticket is the attendance record). Re-issue gives a **new token and a new number**, so the old ticket can't get in by QR or by number. Revoke deletes the row and frees a place. |
| Capacity edit | Can't go below the number of issued tickets (`CapacityBelowIssued`). |

### The door
1. The scanner reads a QR, then calls `POST api/events/check-in/check` with `{ code }`, where `code` is the full URL or the bare token (`EventTicketCheckService.ParseToken`). The check is **read-only**.
2. The verdict is one of `valid`, `alreadyUsed`, `notYetOpen`, `ended`, `cancelled` or `invalid`. Entry opens 15 minutes before the start (`EventEntryRules.OpensBefore`).
3. **Admit:** `POST api/events/check-in/admit` re-runs every check (nothing from the scan is trusted), then `TryAdmitAsync` saves under the ticket's `RowVersion`. Two doormen admitting the same ticket: one gets `admitted`, the other `alreadyUsed` with who and when.
4. **Refuse** records nothing.
5. **Fallback:** `{ eventId, number }` instead of `code`, chosen from `GET api/events/check-in/events` (today's events that haven't ended, with counts).

Tickets are found through the normal tenant **and branch** filters, so a doorman only sees tickets for events at their own branches.

### Termination (by design, not a bug)
`EventEntryRules.TicketsTerminated(isCancelled, endsAt, now)` is true once the event has ended or been cancelled. From then on:
- check and admit return `ended` / `cancelled`;
- the public page shows only the academy and event names, with **no code and no ticket details**;
- issue, rename, re-issue and revoke throw `TicketsTerminated`;
- `UpdateEvent` refuses moving an **ended** event's times (`EndedTimesLocked`; a price or notes correction is still fine);
- `UpdateEvent` also refuses moving a **running** event that has tickets so that it ends in the past (`EndingWouldCloseTickets`), which protects against a mistyped year.

### Public ticket page
`GET api/public/event-tickets/{token}` is anonymous and **read-only; it never admits**. It has its own rate-limit policy, `event-ticket` (150/min/IP), because guests share the venue Wi-Fi. An unknown or revoked code, a suspended academy, or a plan without the feature all answer `invalid` and reveal nothing. On the frontend it is called with a plain `fetch` (not `apiFetch`), so a guest is never redirected to login.

---

## 8. Access control

| Permission | Owner | Admin | Accountant | Employee | Grants |
|---|:-:|:-:|:-:|:-:|---|
| `event.manage` | ✓ | ✓ | | | book, edit, cancel, delete; customers; issue/rename/re-issue/revoke tickets |
| `event.view` | ✓ | ✓ | ✓ | | lists, details, customers, tickets list |
| `event.report` | ✓ | ✓ | ✓ | | events report |
| `event.checkin` | ✓ | ✓ | | ✓ | door scanner only (no bookings, no money) |

- Defaults live in `AppDataSeeder.DefaultRolePermissions`. They are **reconciled on every startup** (`EnsureCoreDataAsync`), so adding a permission there is enough; no SQL grant migration is needed.
- API: `[Authorize(Policy = "Permission:event.x")]`, resolved by `PermissionPolicyProvider`.
- Frontend: `ProtectedRoute requiredFeature="event-management" requiredPermission="event.x"` and `<Can permission="event.x">`.
- **Feature gate:** every command and query implements `IRequiresFeature { FeatureKey = "event-management" }`, so a tenant without the feature gets `FEATURE_DISABLED` (403) whatever its permissions. The anonymous public query checks the feature itself (`ITenantRepository.IsFeatureEnabledAsync`).
- **Branch scope:** `Event` is `IBranchScoped`, filtered automatically. Create and Update also implement `IBranchScopedRequest` and `IRequiresActiveBranch`.

---

## 9. API reference

| Method & route | Permission | Purpose |
|---|---|---|
| `GET api/events` | view | paged list; filters `branchId, from, to, status, customerId, createdByUserId, term` |
| `GET api/events/{id}` | view | `EventDetailsDto` (event + payments) |
| `GET api/events/overlaps` | view | other bookings at the branch in `[startsAt, endsAt)` (warning only) |
| `GET api/events/report` | report | rows + totals, ≤ 2000 rows |
| `POST api/events` · `PUT api/events` | manage | create · full-replace update |
| `POST api/events/{id}/cancel` | manage | `{ reason, mode: refundPayments \| keepPayments }` |
| `DELETE api/events/{id}` | manage | only without payment history |
| `GET api/events/{id}/tickets` | view | counts + page; `filter=all\|unused\|admitted`, `term` (number or name) |
| `POST api/events/{id}/tickets` | manage | `{ count, guestName? }` |
| `PUT api/events/tickets/{ticketId}` | manage | `{ guestName }` |
| `POST api/events/tickets/{ticketId}/reissue` | manage | new token + number |
| `DELETE api/events/tickets/{ticketId}` | manage | revoke unused |
| `GET api/events/check-in/events` | checkin | today's not-ended events with issued/admitted |
| `POST api/events/check-in/check` | checkin | `{ code }` or `{ eventId, number }` (verdict only) |
| `POST api/events/check-in/admit` | checkin | same body; admits |
| `GET/POST/PUT/DELETE api/event-customers[...]` | view / manage | list, by-phone, by id, create, update, delete |
| `GET api/public/event-tickets/{token}` | anonymous | guest's ticket page data |

Every response uses the standard `Result<T>` envelope. `ResultStatusFilter` maps `StatusCode` to the HTTP status.
Rule violations are `EventRuleException`, a `LocalizableException` with an `errors.event.*` key, which shows
up as a localized 400 (`Resources/{en,ar}.json`). `DbUpdateConcurrencyException` becomes 409 `errors.concurrency`.

---

## 10. Frontend notes

- **Routes:**
  - Owner/Admin console (AppLayout): `/events`, `/events/report`, `/events/customers[/:id]`, `/events/check-in`, `/events/:id`.
  - Accountant: the same pages are mounted under `/finance/events...` with `basePath="/finance"` (`eventsBase(basePath)` builds links).
  - Employee: only `/staff/check-in`.
  - Public: `/ticket/:token`.
- **Navigation:** **one** sidebar entry, "Events" (`navConfig.ts`). Bookings, customers and report are in-page tabs (`EventsTabs`). The scanner is reached from the Events and event pages, and from the Employee home card.
- **Booking form** (`EventFormModal`):
  - phone-first customer lookup;
  - debounced overlap check that warns but doesn't block;
  - pay now: none / deposit (must be less than the total) / full, plus payment type and collect date.
  - The edit form always sends every field (the update is a full replacement).
- **Timeline** (`EventsTimeline`): rows are days, hours run across, with a branch switcher. The week starts on Sunday in both languages. A booking that runs past midnight stays on its start day.
- **Printing:** the page renders a hidden `print:block` sheet inside `.report-print-area` and calls `window.print()`. `EventProfile` switches between the booking sheet and the ticket sheet. The ticket list is deliberately **not** reset after `print()`, because mobile browsers print asynchronously.
- **Scanner** (`EventCheckIn.tsx`, npm `qr-scanner`):
  - Needs HTTPS or localhost for the camera.
  - The `QrScanner` instance is created once; callbacks are read through refs, so re-renders never restart the camera.
  - The camera pauses while a verdict is shown and resumes after errors too.
  - A just-read code is ignored for 5 s after resuming, so a refused guest isn't re-offered.
  - Includes camera denied/unavailable states and a 6-digit manual entry.
- **QR images:** `qrcode.react` renders the code; `composeQrImage` turns it into the shareable PNG.
- **i18n:** namespaces `events`, `eventTicket`, `eventCheckIn` (plus `employeeHome.blocks.checkIn`). `src/i18n/catalogs.test.ts` fails if en/ar keys differ.

---

## 11. Conventions this module follows (copy them)

1. **One folder per operation:** `{Op}Command.cs` + `{Op}CommandHandler.cs` (same for queries). Validators live in `Validators/EventValidators/`.
2. **Rule errors** are factory methods on `EventRuleException`, each with an `errors.event.*` key. Add the en and ar text in `Localization/Resources`.
3. **Commands return what the details page shows** (`EventDetailsLoader.LoadAsync`), so the UI can `setDetails(res.data)` with no refetch.
4. **Derive, don't store** anything that follows from dates or other rows (status, payment state, admitted count).
5. **Multi-step writes** (customer + event + invoice + payment) go in one `InTransactionAsync`.
6. **Concurrency** is handled with `RowVersion` on `Event` and `EventTicket`, plus unique indexes (customer phone, ticket token/number). Let `DbUpdateConcurrencyException` surface as a 409 unless you can resolve it, as `TryAdmitAsync` and `AddOrReuseCustomerAsync` do.
7. **Anonymous endpoints** live only in `PublicController`, look up with `IgnoreQueryFilters()`, then `ITenantIdProvider.Impersonate(tenantId)` before touching tenant data.
8. **Header comments explain *why*.** Keep them accurate when you change behaviour.

---

## 12. Testing

- `SportAcademy.Tests/Application/Services/EventBookingTests.cs` runs the real handlers, the real `FinanceLedgerService` and the real repositories over EF InMemory. It covers booking money, re-pricing, cancellation modes, customer dedupe, tickets, the door, termination and the update locks.
- `SportAcademy.Tests/Domain/EventStatusRulesTests.cs` covers derived status.
- InMemory does **not** enforce `RowVersion` or translate SQL. For query-shape or concurrency changes, also run against SQL Server: apply migrations to a throwaway database and drive the API.
- If Visual Studio has the Web `bin` locked, build and test into another folder:
  `dotnet test SportAcademy.Tests -p:OutDir=<some temp dir>`.
- Frontend: `npx tsc -p tsconfig.app.json --noEmit` (the root `tsc` checks nothing), `npx vitest run`, `npm run build`.

---

## 13. Known limits and gotchas

- **Capacity** is informational for the booking itself. It only caps how many tickets can be issued.
- **Report and statement caps:** the events report returns at most 2000 rows (`Truncated` flag). The financial statement caps each kind (income, refunds, expenses, salaries) at 5000 lines, with exact totals.
- **Print / Copy all** load every unused ticket (6 pages of 100 in parallel) and render them all. Fine for hundreds, heavy for tens of thousands.
- **Overlaps** are warnings only; two bookings can share a branch and time.
- **Cancelling with `RefundPayments`** fails if a payment also settled other invoices. Refund that payment from Payments first.
- **Migration order matters** when deploying: `ReplaceEventEntryQrWithTickets` removes the columns the old entry code used, so deploy the backend and frontend together.

---

## 14. Extending the module — checklist

- [ ] Domain change? Add the entity property, its `IEntityTypeConfiguration`, and a migration (`dotnet ef migrations add <Name> -p SportAcademy.Infrastructure -s SportAcademy.Web -o Persistence/Migrations`). Review `Down`.
- [ ] New rule? Add an `EventRuleException` factory, the en/ar resource strings, and a test.
- [ ] New command or query? Implement `IRequiresFeature` (`event-management`); add `[Authorize(Policy = "Permission:event.x")]` on the controller action.
- [ ] New permission? Add it to `Permissions.cs` (+ `All`) and `DefaultRolePermissions`.
- [ ] Touching dates? Store UTC, accept and return `*Local`, use `TenantCalendar`. On the frontend use `useWallClock` / `toWallClock`.
- [ ] Touching money? Go through `IFinanceLedgerService`, never raw invoice/payment rows.
- [ ] Frontend: types and calls in `event.service.ts`, strings in both `en` and `ar`, a `ProtectedRoute` with the feature and permission.
