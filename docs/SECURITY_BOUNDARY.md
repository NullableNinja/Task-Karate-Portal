# Security boundary

## Implemented

- Staff sign-in uses ASP.NET Core Identity password hashing and a protected, HttpOnly cookie.
- Staff mutation/read routes require a server-side role policy: Administrator, Instructor, or Staff.
- Roles Guardian and Student are defined but their portal access is intentionally not implemented.
- Cookie-authenticated mutations require a CSRF double-submit cookie/header token.
- Input validation happens in the API; database uniqueness protects attendance and other relationships.
- Public endpoints expose only published schedule/content fields and never return roster, guardian, or attendance data.
- Audit events record actor, action, entity, entity ID, UTC timestamp, and safe metadata. Passwords, PINs, consent signatures, and raw secrets are not custom fields.
- Default hosting guidance is loopback-only and SQLite is server-side only.

## Explicit non-goals

The milestone does not implement guardian/student login, consent signing, messaging, social feeds, uploads, rate limiting, email delivery, or internet deployment. Those require additional threat modeling and review before use with real families.

Never add a client-side PIN, hidden admin button, localStorage session, IndexedDB fallback, dynamic SQL, or a default password. The legacy portal and Python desk server remain prototype-only and must not be used for real student records.
