# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project: Chronos for Azure

Chronos for Azure is a lightweight Azure Functions engine that starts and stops Azure Virtual Machines based on CRON schedules supplied via Azure Resource Tags. Users tag VMs with start and/or stop expressions; Chronos discovers those tags, schedules the corresponding lifecycle events, and executes them — no per-VM configuration or code changes required. The goal is zero-friction, tag-driven cost optimization.

The repository was originally written on .NET 6 (C# in-process Durable Functions, Bicep) and is undergoing a full rewrite. Assume nothing from the legacy code — patterns, tag names, and architecture are being redesigned.

## Target stack (authoritative)

- **Runtime**: .NET 10, Azure Functions isolated worker
- **Hosting**: Flex Consumption plan (Linux) — recommended serverless plan for .NET 10, scales to zero
- **Engine**: single **timer-triggered reconciler** (5-minute tick) — no Durable Functions (decided July 2026; see `README.md` → Architecture → "Why not Durable Functions")
- **CRON evaluation**: [Cronos](https://github.com/HangfireIO/Cronos) library (CRON parsing, timezones, DST)
- **State**: one watermark blob (timestamp of last successful run) — no other persisted state
- **Discovery**: Azure Resource Graph (single KQL query, multi-subscription-ready)
- **IaC**: Terraform (AzureRM provider), under `infra/`
- **Deployment**: Azure Developer CLI (`azd`) headless, orchestrating the Terraform deployment. An `azure.yaml` will be added in a later feature
- **Source layout**: `src/` holds all .NET assets — the solution (`src/AzureChronos.slnx`), the SDK pin (`src/global.json`), and the Functions project; `infra/` for Terraform (directory is created when the Terraform feature lands). Same layout as the sibling az-reaper project. Run `dotnet` commands from within `src/` so the SDK pin applies.

The Functions project under `src/AzureChronos.Functions/` is a minimal timer-trigger scaffold. The reconciler pipeline (discovery, schedule evaluation, executors), the watermark persistence, and the Terraform infrastructure do not exist yet — they land in subsequent features.

## Architecture (decided)

One reconciler — full description, diagram, and rationale live in [`README.md`](../README.md) (`## Architecture`). Summary:

- **`ChronosReconciler`** (5-min Timer trigger, the only trigger) — load watermark → single ARG KQL query for tagged VMs including power state (pause-tagged VMs filtered out in KQL) → per-VM CRON evaluation → idempotent start/deallocate via `Azure.ResourceManager.Compute` (user-assigned managed identity) → advance watermark on success.
- **Schedule evaluation is a pure function**: `(tags, watermark, now) → actions`, using Cronos for CRON/timezone/DST. Occurrences in `(watermark, now]` fire; if both start and stop occurred, the most recent wins. Keep it side-effect-free and unit-tested.
- **Edge-triggered semantics**: Chronos acts only on occurrences that fell due since the watermark — it does not enforce a desired power state between occurrences, so manual operations are respected. The watermark only advances on success, so outages catch up.
- **Tag mutation flow**: none needed — every tick re-reads reality from ARG. Worst-case reaction latency to any tag change is one tick (5 minutes).

Durable Functions (Durable Entity per VM + eternal per-VM orchestrator) was the previous design; it was evaluated and **rejected in July 2026** on cost (always-ready instance requirement on Flex Consumption breaks scale-to-zero), unusable precision, and maintenance grounds — the full evaluation and revisit criteria live in the README. The standalone Durable Task SDK was rejected for the same reasons plus the mandatory paid backend.

## Scheduling scenarios the engine must support

The exact tag names are **not yet defined**. Refer to these scenarios by behavior:

1. **Stop-only**: VM has a stop/deallocate schedule but no start schedule → Chronos stops it on schedule and never starts it.
2. **Start-only**: VM has a start schedule but no stop schedule → Chronos starts it on schedule.
3. **Start + stop**: VM has both schedules → Chronos runs both.
4. **Pause**: VM has a pause tag → Chronos ignores the VM entirely, even if start/stop tags are present. Users must not be forced to remove and reapply schedule tags to suspend the schedule; removing the pause tag resumes it.
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

- **Use current Microsoft documentation.** Patterns for Azure Functions have shifted substantially since .NET 6 (isolated worker model, DI conventions). Do not replicate legacy in-process patterns. When in doubt, fetch current Microsoft Learn docs for Azure Functions, .NET 10, and the Terraform AzureRM provider.
- **Architecture is fixed; do not relitigate.** The decisions in "Target stack" and "Architecture (decided)" — a single timer-triggered reconciler on Flex Consumption, edge-triggered, watermark blob as the only state — were re-evaluated deliberately in July 2026 (Durable Functions was the previous design and was rejected; see `README.md` → "Why not Durable Functions" for the rationale and revisit criteria). Do not propose alternatives (Durable Entities/orchestrators, standalone Durable Task SDK, Logic Apps, Azure Automation, external state stores) without a concrete new reason such as sub-minute precision or sequenced-workflow requirements.
- **Open questions remain — ask before deciding these.** Concrete Chronos tag *names*, the manual-override surface (HTTP API shape), and Event Grid integration timing are all undecided. When implementing a feature that touches these, ask first.
- **No speculative scaffolding.** Don't generate full Function apps or Terraform modules without an explicit go-ahead for that feature.
