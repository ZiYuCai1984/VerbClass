# Code Review Checklist

## Fail-Fast and Error Handling

- Any new fallback, retry chain, or compatibility path is explicit, requested, and observable.
- Exceptions are not swallowed. Catch blocks either recover intentionally or add context and rethrow or return an error result that preserves failure semantics.
- Missing required configuration, tenant, user, or input fails explicitly instead of silently defaulting.

## Defaults and Nullability

- Required values are not replaced with placeholder defaults just to keep execution going.
- Nullability was not broadened only to suppress warnings or runtime errors.
- Success results are not returned when the operation actually failed or partially failed.

## Layering and Invariants

- Domain invariants remain enforced in the domain layer.
- Application services coordinate work but do not hide domain or infrastructure failures.
- Web and HttpApi code may translate errors for users, but must not invent fake success states.
- Business rules were not moved into UI, controllers, or infrastructure helpers to bypass stricter layers.

## Regression Risk

- State transitions still reject invalid input explicitly.
- New branches do not create hidden no-op behavior or partial writes.
- Cross-layer changes preserve transaction boundaries and observable failure behavior.

## Verification

- Relevant build, test, and stack-specific checks were run for the changed area.
- Regression tests were added or updated when behavior changed.
- If no automated tests exist, the manual verification steps are stated explicitly.
