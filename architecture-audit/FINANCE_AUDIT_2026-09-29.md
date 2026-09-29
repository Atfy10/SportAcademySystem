# Finance & Subscriptions Audit — 2026-09-29

Scope: payments ↔ invoices ↔ subscriptions (backend + console), plus the trainee CSV
import/export. Branch `feature/finance-overhaul` in both repos.

**Verdict:** the ledger core (`FinanceLedgerService`: invoice → payment → allocation, status
resolution) was sound, but it was surrounded by gaps that made the system unreliable in
practice. Every finding below is fixed on the branch unless marked *open*.

## Findings

| # | Severity | Finding | Fix |
|---|---|---|---|
| F1 | **Critical** | "Partial refund refunds the whole receipt": the receipt's **Void** button sat next to Refund with no confirmation, labelled **"إلغاء"** in Arabic — the same word as the dialog's "Cancel". One click reversed the whole payment (status Voided, refunded = total, invoice reopened). The refund path itself honoured the typed amount. | Void moved into a menu, renamed "Void payment / إبطال الدفعة", confirmation dialog stating it reverses the full remaining amount; both actions require a reason and show their consequence. |
| F2 | High | Reversal always drained allocations from the first one and never tracked what was already reversed → repeated partial refunds on a multi-invoice payment drove an invoice's `AmountPaid` negative. Void had the same bug. | `PaymentAllocation.ReversedAmount`; reversal caps at what each allocation still holds. Backfilled by the migration. |
| F3 | High | Record/refund/void ran without a transaction (each repository call saves) and without concurrency tokens → partial writes; two concurrent collections could both pass the overpayment check. | Handlers run in one transaction; `rowversion` on `Invoice` and `Payment`; conflicts return a localized 409. |
| F4 | High | No refund history (date, reason, who). Reports netted refunds into the original payment's month. | `PaymentRefund` entity; receipt shows history; revenue reports place refunds in the month they happened. Existing refunds backfilled. |
| F5 | High | A 100 % discount (total 0) always failed: the ledger rejects a zero allocation, so approval rolled back forever. | Zero-total invoice is born Paid; no payment recorded. |
| F6 | Medium | Discount approval was marked Approved in a save outside the creation transaction → a failed save left the subscription created but the request re-approvable (duplicate). | `beforeCommit` hook: Approved is saved in the same transaction. |
| F7 | Medium | Deleting a subscription left its invoice owed; wrong exception type. | Delete refused while money is held (refund/void first); unpaid invoice cancelled. |
| F8 | Medium | Updating a subscription could change trainee/sport/branch/plan with no re-billing. | Billing fields locked after billing (dates still editable). |
| F9 | Medium | Subscription "Price" was the live list price (ignored discount and later price changes); "latest payment" included voided ones. | Price = invoice total; voided payments excluded; balance/due date/payment state exposed. |
| F10 | Medium | "Overdue" meant two things: enrollment = subscription expired; finance = invoice past due. Invoice due date hard-coded to issue + 7 even when paid at the till. | Enrollment payment status is now invoice-based; due date = today (paid) or the chosen collect date (deposit). |
| F11 | Medium | Reports disagreed: payment-method report summed gross incl. voided/refunded; date-only `to` filters dropped the whole last day (payments, salaries). | Net of refunds/voids; `ReportDateRange.EndExclusive`. |
| F12 | Medium | A renewal sold ahead of time expired the current subscription and reset its sessions immediately, even if it started weeks later. | Hand-over deferred to the renewal's start date (`SubscriptionLifecycleService`). |
| F13 | Medium | Subscription expiry was lazy (only when someone opened stats) and used server-local `DateTime.Today`. | Daily lifecycle sweep; all reads derive status from dates in UTC. |
| F14 | Medium | `PATCH api/Enrollment/{id}/payment-status` records real money with no permission policy. | Requires `payment.record`. *Open:* subscription GET endpoints still only `[Authorize]` — no `subscription.view` permission exists yet. |
| F15 | Low | Voided/refunded payments could still be re-dated/edited; payment type not checked; no refund validator; record-payment allowed another branch's invoice / another currency / the same invoice twice. | All rejected with specific localized messages (`FinanceRuleException`). |
| F16 | Low | Create-subscription modal showed prices with `toFixed(2)` (KWD has 3 decimals). | Tenant currency formatting. |
| F17 | Low | A discounted subscription's invoice has two lines, so every payment appeared twice in the trainee's payment history. | History reads the fee line only. |
| F18 | — | Zero test coverage of the ledger, creation, import. | 10 ledger tests, 13 import-validation tests, 2 model-coverage tests (805 backend tests passing). |

## New behaviour (user requests)

- **Deposit at creation** (create + renew + discount request): pay part now, pick a collect date; the rest stays owed. "Completed" = created & active; payment state is shown separately (Paid / Deposit paid / Unpaid / Overdue).
- **Overdue warnings (warn only):** `OverdueBalanceService` notifies Owners/Admins/Accountants once when a balance is due within 3 days and once when overdue (link → Outstanding filtered to overdue); a banner on every console page; Overdue stat card and filter on Subscriptions; badges on the trainee profile.
- **Any future start date;** future subscriptions show as **Upcoming** (derived, never stored).
- **Refund semantics:** refunded money is owed again on the invoice ("reopen the balance"), with a new collect date chosen in the dialog.
- **Payments list** leads with the trainee, searchable by name/phone/number; **printable customer receipt**.

## Trainee CSV

The downloaded template could never import (headers merged by a `join("")`, camelCase vs the backend's case-sensitive PascalCase, ids not names), one bad value 500'd the whole file, and duplicates/limits surfaced as generic errors. Now: validate-then-commit with a per-row/column/value report; every database constraint pre-checked (a test fails if a new unique index/length limit appears on `Trainee` without a pre-check); names in English or Arabic; export round-trips.

## Open issues found (not fixed — need a decision)

1. **Trainee ID overflow for Arabic names.** `CreateTraineeCommandHandler.CreateTraineeId` builds `{branchId}{yy}{MM}{char code of first letter}{counter}` as an `int`. An Arabic first letter has a 4-digit code (e.g. `أ` = 1571), so any Arabic first name at **branch id ≥ 2** overflows `int` → the create fails with a generic error. The import now catches it up front with a clear message; the create form still hits it. Fixing it means changing the business ID scheme (e.g. transliterate, or use the letter's position in the alphabet) — a product decision.
2. Subscription GET endpoints have no dedicated view permission (F14).
3. `InvoiceStatus.Refunded`/`Draft` are still never set (refunds reopen the balance by design, so an invoice goes back to Issued/PartiallyPaid).
