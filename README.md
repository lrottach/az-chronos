# Chronos for Azure

Chronos for Azure is a lightweight Azure Functions engine that starts and stops Azure Virtual Machines based on CRON schedules supplied via Azure Resource Tags. Tag a VM with a start and/or stop expression and Chronos does the rest — it discovers the tags, schedules the corresponding lifecycle events, and executes them. No per-VM configuration, no code changes: zero-friction, tag-driven cost optimization.

> **Note**: Chronos is undergoing a full rewrite on .NET 10 (Azure Functions isolated worker). The chapter below describes the target architecture; implementation lands feature by feature. The concrete Chronos tag names are not yet finalized and will be documented here once they are.

## Architecture

### Operational model

Chronos is an Azure Functions app on the **Flex Consumption** plan, **.NET 10 isolated worker**. The engine is a single **timer-triggered reconciler**: every 5 minutes it discovers tagged VMs through Azure Resource Graph, evaluates their CRON schedules against the window since the previous successful run, and executes the start/stop actions that came due. The only persisted state is a watermark — the timestamp of the last successful run — in a single blob. The app runs entirely on-demand and scales to zero between ticks.

### Components

```
                ┌────────────────────────────┐
Timer (5 min)──▶│ ChronosReconciler          │
                └─────┬──────────────────────┘
                      │ 1 · discover
                      ▼
                ┌────────────────────────────┐
                │ Azure Resource Graph       │   single KQL query: VMs carrying
                │ (tags + power state)       │   Chronos tags; paused VMs are
                └─────┬──────────────────────┘   filtered out in the query
                      │ 2 · evaluate
                      ▼
                ┌────────────────────────────┐
                │ Schedule evaluation        │   per VM: parse start/stop CRON
                │ (Cronos + TimeZoneInfo)    │   → occurrences in (watermark, now]
                └─────┬──────────────────────┘   → desired action, if any
                      │ 3 · act
                      ▼
                ┌────────────────────────────┐
                │ Start / Deallocate         │   Azure.ResourceManager.Compute
                │ executors (idempotent)     │   via user-assigned managed identity
                └────────────────────────────┘

         Watermark (last successful run) persisted in a single blob
```

- **`ChronosReconciler`** — the only trigger in the system. Runs every 5 minutes: load the watermark, discover, evaluate, act, then advance the watermark on success.
- **Discovery** — one Azure Resource Graph (ARG) KQL query returns every VM carrying Chronos tags together with its current power state. VMs carrying the pause tag are excluded directly in KQL. The query is multi-subscription-ready.
- **Schedule evaluation** — a pure function: `(tags, watermark, now) → actions`. CRON parsing, timezone interpretation, and DST handling via the [Cronos](https://github.com/HangfireIO/Cronos) library. If both a start and a stop occurrence fell within the window, the most recent one wins.
- **Start / Deallocate executors** — thin idempotent wrappers around `Azure.ResourceManager.Compute`: check the current power state, skip no-ops, and issue the ARM operation without awaiting completion — the next tick observes the outcome.

### Execution semantics: edge-triggered

Chronos acts only when a CRON occurrence fell inside the window since the last successful run — it does not continuously enforce a desired power state. Both consequences are deliberate:

- **Manual operations are respected.** If someone manually starts a stopped VM in the evening, Chronos leaves it alone until the next scheduled occurrence — it never fights the user in between.
- **Outages catch up.** The watermark only advances on a successful run, so occurrences missed while the app was down are evaluated by the next run and the VM converges to the intended state.

### Tag mutation flow

There is no per-VM state to migrate and no event plumbing: every tick re-reads reality from ARG.

- **Tag added or edited** — the next tick evaluates the new expression.
- **Tag removed** — the VM drops out of the ARG result; nothing to clean up.
- **Pause tag added / removed** — the VM disappears from / reappears in the query; the schedule resumes exactly where the remaining tags say, no re-tagging required.

Worst-case reaction latency to any tag change is one tick (5 minutes).

### Discovery

A single Azure Resource Graph KQL query enumerates VMs with Chronos tags. Development runs against one subscription; the query and identity model are already shaped for **multi-subscription support** (v-next), which expands by adding additional subscription scopes to the managed identity's role assignments — no code change.

### Identity and RBAC

The Function App runs under a **user-assigned managed identity** with the principle of least privilege:

- `Reader` on the target scope (for ARG queries and VM reads)
- `Microsoft.Compute/virtualMachines/start/action`
- `Microsoft.Compute/virtualMachines/deallocate/action`

Implemented as a custom role rather than the broad built-in `Virtual Machine Contributor`.

### Why this shape

1. **Cheapest possible to run.** A few-second execution every 5 minutes stays in Flex Consumption's on-demand mode, comfortably inside the monthly free grants — no always-ready instances, no continuous queue polling, no per-VM state infrastructure. For an engine whose entire purpose is saving money, it costs effectively nothing itself.
2. **Easy to develop and maintain.** The heart of the engine is a pure function (`tags + watermark + now → actions`) plus two ARM calls. It is unit-testable without emulators, has no replay semantics or deterministic-code constraints, and a contributor can understand the whole engine in one sitting.
3. **Self-healing by construction.** Every tick is a full pass over reality. A failed tick is simply retried by the next one; there are no orphaned state machines or drift between stores to reconcile.
4. **Extensible to other resource types.** New deallocatable services (VM Scale Sets, Azure Firewall, App Service plans, …) plug in as drivers — a query predicate plus type-specific start/stop semantics — without touching the engine.
5. **Precision matched to the input.** Tag discovery is a 5-minute poll regardless of the execution machinery, so the system's effective granularity is 5 minutes end to end. The reconciler delivers exactly that; nothing is spent on precision the input side can't use.

### Why not Durable Functions (evaluated and rejected)

An earlier iteration of this design used Durable Functions: a Durable Entity per VM holding the materialized schedule, plus an eternal per-VM orchestrator racing a durable timer against a `ScheduleChanged` event. It was rejected in July 2026 after reassessment:

- **It breaks scale-to-zero on Flex Consumption.** Microsoft's [guidance for Durable Functions on the Flex Consumption plan](https://learn.microsoft.com/azure/durable-task/durable-functions/durable-functions-azure-storage-provider#flex-consumption-plan) recommends one always-ready instance for the `durable` group plus a queue polling interval of 10 seconds or less. Always-ready instances are [billed at a continuous baseline rate with no free grants](https://learn.microsoft.com/azure/azure-functions/flex-consumption-plan#billing) — a fixed monthly cost plus round-the-clock storage transactions, versus effectively zero for the on-demand reconciler. Skipping the always-ready instance degrades exactly the timer reliability Durable was chosen for.
- **Its precision can't be used.** Durable timers fire to the second, but tag discovery is a 5-minute poll — the whole system is minute-granular regardless. The eternal orchestrator paid for precision the input side cannot deliver and the use case does not need.
- **It is the maintenance-heavy option.** Replay semantics and deterministic-code constraints for contributors, the eternal-orchestrator versioning problem on every deploy that touches scheduling logic, and orphaned-orchestrator/entity-drift failure modes that required the discovery loop as a watchdog anyway — at which point the watchdog might as well do the work.

**Revisit criteria.** Durable Functions (or the standalone Durable Task SDK, rejected for the same reasons plus a mandatory paid Durable Task Scheduler backend) becomes worth its cost if requirements grow toward sub-minute execution precision, sequenced multi-step workflows (ordered tier start-up, pre/post-actions), or long-running operations that must be tracked across restarts. The executor-level code (ARM calls, CRON evaluation, tag parsing) is identical either way, so the migration path stays open — no design accommodation is needed today.

### Out of scope for v1

- Event Grid subscription on tag changes (sub-second reconciliation) — additive enhancement; slots in as a second trigger on the same evaluation logic.
- Manual override HTTP API (e.g. "skip next start").
- Resource types other than VMs. App Service plans and Azure Firewall are the first candidates — note that stopping an App Service app alone saves nothing (the plan keeps billing), so per-type drivers must model real deallocation semantics.
- Concrete Chronos tag names (v1 ships three tags — start CRON, stop CRON, pause — names still to be decided).
