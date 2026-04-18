# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project: Chronos for Azure

Chronos for Azure is a lightweight Azure Functions engine that starts and stops Azure Virtual Machines based on CRON schedules supplied via Azure Resource Tags. Users tag VMs with start and/or stop expressions; Chronos discovers those tags, schedules the corresponding lifecycle events, and executes them — no per-VM configuration or code changes required. The goal is zero-friction, tag-driven cost optimization.

The repository was originally written on .NET 6 (C# in-process Durable Functions, Bicep) and is undergoing a full rewrite. Assume nothing from the legacy code — patterns, tag names, and architecture are being redesigned.

## Target stack (authoritative)

- **Runtime**: .NET 10, Azure Functions isolated worker
- **Hosting**: Flex Consumption plan (Linux) — required for .NET 10 + Durable on Linux, scales to zero
- **Orchestration**: Azure Durable Functions — **Durable Entity per VM + eternal per-VM orchestrator + activities** (decided; see `README.md` → Architecture)
- **Durable backend**: Azure Storage (default provider) for v1; Durable Task Scheduler is a future migration if scale demands it
- **Discovery**: Azure Resource Graph (single KQL query, multi-subscription-ready)
- **IaC**: Terraform (AzureRM provider), under `infra/`
- **Deployment**: Azure Developer CLI (`azd`) headless, orchestrating the Terraform deployment. An `azure.yaml` will be added in a later feature
- **Source layout**: `src/` for Functions code, `infra/` for Terraform, `deploy/` currently held empty via `.gitkeep` pending the infra decision

None of the above code exists yet. Project initialization establishes conventions and architecture; implementation lands in subsequent features.

## Architecture (decided)

Four components — full description, diagram, and rationale live in [`README.md`](../README.md) (`## Architecture`). Summary:

- **`DiscoveryTimer`** (5-min Timer trigger) — ARG query for tagged VMs → signals each `VmScheduleEntity` with the materialized schedule, or `Clear()` for vanished tags. Spawns the orchestrator for actionable entities that don't yet have one.
- **`VmScheduleEntity`** — Durable Entity keyed by ARM resource ID. Holds start CRON, stop CRON, timezone, exclusion flag, last action. Source of truth for per-VM intent.
- **`VmScheduler`** — eternal Durable Orchestrator, one per VM. Reads entity, arms a durable timer to the next CRON occurrence, races it against the `ScheduleChanged` external event. On timer: calls start/stop activity. On event: recomputes. Always `ContinueAsNew`.
- **Start / Stop activities** — idempotent wrappers over `Azure.ResourceManager.Compute` via user-assigned managed identity.

**Tag mutation mechanism (every case): discovery signals entity → entity raises `ScheduleChanged` → orchestrator reacts.** Edits update the schedule and re-arm the timer; full removal makes the orchestrator exit cleanly. No special cases.

The standalone Durable Task SDK was evaluated and rejected — see the README section. Do not propose it again without a concrete reason.

## Scheduling scenarios the engine must support

The exact tag names are **not yet defined**. Refer to these scenarios by behavior:

1. **Stop-only**: VM has a stop/deallocate schedule but no start schedule → Chronos stops it on schedule and never starts it.
2. **Start-only**: VM has a start schedule but no stop schedule → Chronos starts it on schedule.
3. **Start + stop**: VM has both schedules → Chronos runs both.
4. **Exclusion**: VM has an exclusion tag → Chronos ignores the VM entirely, even if start/stop tags are present. Users must not be forced to remove schedule tags to opt out.
5. **Timezone override**: VM has a timezone tag → Chronos parses it and interprets the CRON expressions in that timezone. Invalid/unparseable timezones must be handled gracefully.

Do **not** invent concrete tag names. When implementing a feature that needs them, ask the user first.

## Workflow & conventions

### Branching
- `main` is protected — **never** commit or push to it directly.
- Every change (code, infra, docs) happens on a feature branch and lands via pull request.

### Commit message prefixes
- `feat:` — new feature
- `bug:` — hotfix
- `docs:` — documentation
- `refactor:` — refactoring of existing functionality

### Pull request title prefixes
- `Feature: …`
- `Documentation: …`
- `Refactoring: …`

(No dedicated PR prefix for hotfixes — use `Feature:` or ask the user.)

### Commit approval

**Always ask before creating a commit.** Before running `git commit`, present the proposed commit message to the user and wait for explicit confirmation. Never commit unilaterally, even when changes are obviously ready. This applies to every commit — the user's approval of a prior commit does not roll forward to the next.

Do **not** add Claude attribution, co-author trailers, or "Generated with Claude Code" footers to commit messages or PR bodies. The harness is configured (`.claude/settings.json` → `attribution`) to suppress these; don't reintroduce them manually.

## Working-style notes for Claude

- **Use current Microsoft documentation.** Patterns for Azure Functions have shifted substantially since .NET 6 (isolated worker model, new Durable Functions APIs, DI conventions). Do not replicate legacy in-process patterns. When in doubt, fetch current Microsoft Learn docs for Azure Functions, Durable Functions, .NET 10, and the Terraform AzureRM provider.
- **Architecture is fixed; do not relitigate.** The decisions in "Target stack" and "Architecture (decided)" — Durable Entities + eternal per-VM orchestrator on Flex Consumption with Azure Storage backend — were made deliberately and are documented in `README.md`. Do not propose alternatives (pure Timer trigger, ad-hoc orchestrations per event, standalone Durable Task SDK, external state store) without a concrete new reason.
- **Open questions remain — ask before deciding these.** Concrete Chronos tag *names*, the manual-override surface (HTTP API shape), and Event Grid integration timing are all undecided. When implementing a feature that touches these, ask first.
- **No speculative scaffolding.** Don't generate full Function apps or Terraform modules without an explicit go-ahead for that feature.
