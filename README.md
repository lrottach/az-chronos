# Chronos for Azure

Chronos for Azure is a lightweight Azure Functions engine that starts and stops Azure Virtual Machines based on CRON schedules supplied via Azure Resource Tags. Tag a VM with a start and/or stop expression and Chronos does the rest — it discovers the tags, schedules the corresponding lifecycle events, and executes them. No per-VM configuration, no code changes: zero-friction, tag-driven cost optimization.

> **Note**: Chronos is undergoing a full rewrite on .NET 10 (Azure Functions isolated worker, Durable Functions). The chapter below describes the target architecture; implementation lands feature by feature. The concrete Chronos tag names are not yet finalized and will be documented here once they are.

## Architecture

### Operational model

Chronos is an Azure Functions app on the **Flex Consumption** plan, **.NET 10 isolated worker**, using **Durable Functions** with the **Azure Storage** backend. The hosting plan gives us scale-to-zero between ticks; the Durable extension gives us per-VM stateful scheduling without a second state store.

### Components

```
                ┌──────────────────────────┐
Timer (5 min)──▶│ DiscoveryTimer           │──ARG query──▶ Tagged VMs
                └────────┬─────────────────┘
                         │ signal per VM
                         ▼
                 ┌───────────────────┐
                 │ VmScheduleEntity  │  (one Durable Entity per ARM resource ID)
                 │  - start CRON     │
                 │  - stop CRON      │
                 │  - timezone       │
                 │  - exclusion flag │
                 │  - last action    │
                 └────────┬──────────┘
                          │ external event on change
                          ▼
                 ┌───────────────────┐
                 │ VmScheduler       │   eternal orchestrator, one per VM:
                 │ (orchestrator)    │    read entity → compute next event
                 │                   │    Task.WhenAny(durableTimer, changeEvent)
                 │                   │    on timer  : CallActivity(start|stop)
                 │                   │    on event  : recompute
                 │                   │    ContinueAsNew
                 └────────┬──────────┘
                          │
                 ┌────────▼──────────┐
                 │ Start / Stop      │   Azure.ResourceManager.Compute
                 │ activities        │   via user-assigned managed identity
                 └───────────────────┘
```

- **`DiscoveryTimer`** — runs every 5 minutes. Single Azure Resource Graph (ARG) KQL query across all in-scope subscriptions returns VMs carrying Chronos tags. For each VM: signals `VmScheduleEntity.UpdateSchedule(...)`. For VMs that no longer carry Chronos tags: signals `Clear()`. Spawns a `VmScheduler` orchestrator for any actionable entity that doesn't already have one.
- **`VmScheduleEntity`** — a Durable Entity keyed by the VM's ARM resource ID. Holds the materialized schedule (start CRON, stop CRON, timezone, exclusion flag, last executed action). Operations: `UpdateSchedule`, `MarkExecuted`, `Clear`, `Get`. On any state-changing operation, raises a `ScheduleChanged` external event on the running orchestrator.
- **`VmScheduler`** — an eternal orchestrator, one per VM. Reads its entity, computes the next CRON occurrence (with timezone), arms a durable timer to that instant, and races it against the `ScheduleChanged` external event. Whichever fires first wins; the loop continues via `ContinueAsNew`.
- **Start / Stop activities** — thin idempotent wrappers around `Azure.ResourceManager.Compute`; check current power state before acting. Authenticate via the Function App's user-assigned managed identity. Durable handles retry/backoff.

### Tag mutation flow

Every kind of tag change reduces to one mechanism: **discovery signals the entity → entity raises `ScheduleChanged` → orchestrator reacts**.

- **Tag value edited** — discovery sees the new value within 5 minutes, signals `UpdateSchedule`. Entity diffs, persists, raises the change event. The orchestrator's `Task.WhenAny(timer, changeEvent)` returns the change event; the pending durable timer is abandoned. Orchestrator recomputes the next occurrence and `ContinueAsNew`s with the new timer.
- **Tag removed entirely** — discovery signals `Clear()`. Entity wipes state and raises the change event. Orchestrator wakes, sees an empty schedule, and exits cleanly (no `ContinueAsNew`). Discovery only re-spawns the orchestrator when the entity becomes actionable again.
- **Partial removal (e.g. start removed, stop kept)** — same path as "edited"; entity now reflects a stop-only schedule and the orchestrator arms a timer for the next stop event. No special case.

### Discovery

A single Azure Resource Graph KQL query enumerates VMs with Chronos tags. Development runs against one subscription; the query and identity model are already shaped for **multi-subscription support** (v-next), which expands by adding additional subscription scopes to the managed identity's role assignments — no code change.

### Identity and RBAC

The Function App runs under a **user-assigned managed identity** with the principle of least privilege:

- `Reader` on the target scope (for ARG queries and VM reads)
- `Microsoft.Compute/virtualMachines/start/action`
- `Microsoft.Compute/virtualMachines/deallocate/action`

Implemented as a custom role rather than the broad built-in `Virtual Machine Contributor`.

### Why this shape

1. **Future-proof.** Manual overrides, additional resource types (VMSS, SQL, AKS node pools), and Event-Grid-driven reconciliation all land on existing seams (entity operations, additional signal sources, new activity types) — not as redesigns.
2. **Clear separation of concerns.** Tags are the *intent*. The entity is the *materialized schedule*. The orchestrator is *control flow*. Each piece has one job.
3. **Operationally cheap.** Flex Consumption scales to zero between ticks; Durable timers avoid polling loops; Azure Storage backend has effectively no fixed cost at this scale.
4. **Failure-tolerant.** Durable retries activities; orchestrator history prevents double-fires or silently-dropped events across Function App restarts.
5. **Idempotent execution.** Start/stop activities check current power state before acting, so re-runs are safe.

### Out of scope for v1

- Event Grid subscription on tag changes (sub-second reconciliation) — additive enhancement, slots in as a second signal source on the same entity.
- Manual override HTTP API (e.g. "skip next start").
- Resource types other than VMs.
- Durable Task Scheduler backend — unnecessary at this scale.
- Concrete Chronos tag names (still to be decided).

### Why Durable Functions, not the standalone Durable Task SDK

The portable [Durable Task SDKs](https://learn.microsoft.com/en-us/azure/durable-task/sdks/durable-task-overview) (GA for .NET, Python, Java) offer the *same* programming model as Durable Functions — orchestrations, entities, activities, timers, external events — but run on any compute (Container Apps, AKS, VMs) and require the **Durable Task Scheduler** (a paid managed Azure resource) as their backend.

For Chronos's profile they are the wrong trade:

- **Backend cost.** SDK requires Durable Task Scheduler; Durable Functions can use Azure Storage at near-zero cost.
- **Built-in triggers.** Durable Functions ships `[TimerTrigger]`, HTTP, and Event Grid triggers; with the SDK we'd hand-roll a scheduled job and ingress.
- **Scale to zero.** Flex Consumption gives this for free; SDK on Container Apps/AKS needs KEDA configuration.
- **Operational footprint.** Durable Functions = Function App + Storage account. SDK = container host + DTS resource + custom monitoring + custom HTTP plumbing.
- **Microsoft's own guidance** (["Choose your orchestration framework"](https://learn.microsoft.com/en-us/azure/durable-task/common/choose-orchestration-framework)) says: choose Durable Functions for serverless, scale-to-zero, pay-per-execution, and quick prototyping — all of which fit Chronos.

The hedge: the programming model is identical, so if Chronos ever needs to leave the Functions host (e.g. mandated AKS deployment), the orchestrator/entity/activity code carries over with mostly cosmetic changes. No design accommodation needed today.
