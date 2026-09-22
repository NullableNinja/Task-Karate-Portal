# Security boundary

## Implemented

- Staff sign-in uses ASP.NET Core Identity password hashing and a protected, HttpOnly cookie.
- Staff mutation/read routes require a server-side role policy: Administrator, Instructor, or Staff.
- Roles Guardian and Student are defined in the platform database. Student portal access is implemented separately against the supplied starter database with a protected `task_karate_student` cookie and strong password hashing; guardian login remains schema-ready only.
- Cookie-authenticated mutations require a CSRF double-submit cookie/header token.
- Input validation happens in the API; database uniqueness protects attendance and other relationships.
- Public endpoints expose only published schedule/content fields and never return roster, guardian, or attendance data.
- Audit events record actor, action, entity, entity ID, UTC timestamp, and safe metadata. Passwords, PINs, consent signatures, and raw secrets are not custom fields.
- Default hosting guidance is loopback-only and SQLite is server-side only.
- Student profile data, friends, and messages require the student cookie and a server-recorded profile acknowledgment. Messages require an accepted friendship. Public schedule returns class-session fields only; public endpoints do not return a roster.

## Explicit non-goals

The student milestone does not implement guardian login, consent signing, media uploads, rate limiting, email delivery, moderation tooling, or internet deployment. The friend/message feature is intentionally limited to authenticated students who have accepted a local profile acknowledgment and should receive a separate privacy/moderation review before use with real families.

Never add a client-side PIN, hidden admin button, localStorage session, IndexedDB fallback, dynamic SQL, or a default password. The legacy portal and Python desk server remain prototype-only and must not be used for real student records.
