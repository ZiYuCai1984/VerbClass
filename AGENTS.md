# Repository Rules

## General Coding Rules

- Do not add HTML `aria` attributes in new code unless the user explicitly asks for them.
- Do not add `sealed` in new code unless the user explicitly asks for it.
- Prefer minimal, explicit changes over broad speculative refactors.

## Core Engineering Rules

- Prefer fail-fast behavior over permissive fallback.
- Prefer explicit errors over silent recovery.
- Do not weaken domain constraints just to make code run.
- Do not trade correctness for demoability.
- If required information is missing, fail explicitly and state what is missing.
- Keep invariants explicit instead of hiding them behind nullable or default-heavy code.

## Forbidden Without Explicit Request

- Do not add silent fallback behavior.
- Do not swallow exceptions.
- Do not use catch-all exception handlers unless they rethrow or add clear diagnostic context.
- Do not replace missing required values with defaults just to keep execution going.
- Do not return empty or success-shaped results to hide upstream failure.
- Do not broaden nullability or optionality purely to avoid errors.
- Do not auto-create default tenant, user, config, or placeholder entities unless explicitly required.
- Do not add retry or fallback chains unless they are part of the requested design.

## Before Coding

For any non-trivial change:
- first summarize the task,
- list assumptions,
- list invariants that must remain true,
- list likely failure modes,
- state whether any fallback is being introduced,
- outline a short plan before editing when the change crosses layers or has multiple steps.

If fallback behavior is truly required:
- explain why fail-fast is insufficient,
- explain how the fallback is observable,
- explain which correctness guarantees remain preserved.

## Layering Rules

- Domain layer stays strict and explicit. Invalid state should fail explicitly.
- Application layer coordinates use cases but must not hide domain or infrastructure failures.
- HttpApi and Web layers may translate errors into user-facing messages, but must not invent fake success states.
- Infrastructure resilience logic must be explicit, bounded, and observable.
- Do not move business rules into UI, controllers, or infrastructure helpers just to make flows pass.

## Verification Rules

Before considering a task complete:
- run the relevant build command,
- run the relevant tests for the changed area when tests exist,
- run format, lint, or type checks when the touched stack has them configured,
- add or update regression tests when behavior changes,
- review the diff for hidden fallback paths, swallowed exceptions, weakened invariants, and accidental cross-layer leakage.

## Build / Test Commands

- Full build: `dotnet build src/ZYC.VerbClass.slnx`
- When the repository or changed area contains a test project: `dotnet test src/ZYC.VerbClass.slnx`
- Do not claim automated tests passed when no test project exists for the changed area. State that explicitly and describe the manual verification performed.

## Review

- Follow the repository review checklist in `code_review.md` when reviewing or self-reviewing changes.

## Directory-Specific Rules

- UI, CSS, layout, and page-pattern rules belong in local `AGENTS.md` files under the relevant Web projects.
- When working in `src/ZYC.VerbClass.Web` or `src/ZYC.VerbClass.Web.Modules.Mock`, follow the local UI rules there in addition to this root file.

## Definition of Done

A task is not done unless:
- the relevant build succeeds,
- relevant tests pass or their absence is explicitly called out,
- invariants remain explicit,
- no hidden fallback path was introduced,
- failure behavior is intentional and observable,
- the final diff has been reviewed for risky patterns.
