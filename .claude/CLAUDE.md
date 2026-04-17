# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project: Chronos for Azure

Chronos for Azure is a lightweight Azure Functions engine that starts and stops Azure Virtual Machines based on CRON schedules supplied via Azure Resource Tags. Users tag VMs with start and/or stop expressions; Chronos discovers those tags, schedules the corresponding lifecycle events, and executes them — no per-VM configuration or code changes required. The goal is zero-friction, tag-driven cost optimization.

The repository was originally written on .NET 6 (C# in-process Durable Functions, Bicep) and is undergoing a full rewrite. Assume nothing from the legacy code — patterns, tag names, and architecture are being redesigned.

## Target stack (authoritative)

- **Runtime**: .NET 10, Azure Functions isolated worker
- **Orchestration**: Azure Durable Functions (choice between Durable Entities vs. orchestrations is **not yet decided** — must be discussed before scaffolding)
- **IaC**: Terraform (AzureRM provider), under `infra/`
- **Deployment**: Azure Developer CLI (`azd`) headless, orchestrating the Terraform deployment. An `azure.yaml` will be added in a later feature
- **Source layout**: `src/` for Functions code, `infra/` for Terraform, `deploy/` currently held empty via `.gitkeep` pending the infra decision

None of the above code exists yet. This initialization feature only establishes conventions and clears legacy code.

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
- **Ask before deciding architecture.** The user wants tag names, Durable Entities vs. orchestrations, timer cadence, identity model (managed identity scope, RBAC boundaries), and VM discovery strategy (subscription-wide ARG query vs. per-RG enumeration) co-designed — not pre-decided.
- **No speculative scaffolding.** Don't generate full Function apps or Terraform modules without an explicit go-ahead for that feature.
