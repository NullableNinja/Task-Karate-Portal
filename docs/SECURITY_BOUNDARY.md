# Security boundary

## Implemented

- Staff sign-in uses ASP.NET Core Identity password hashing and a protected, HttpOnly cookie. The API accepts a configured local username or email and requires the staff-use acknowledgment before creating a session.
- Staff mutation/read routes require a server-side role policy: Administrator, Instructor, or Staff.
- Roles Guardian and Student are defined in the platform database. Student portal access is implemented separately against the supplied starter database with a protected `task_karate_student` cookie and one server-verified 4–6 digit student PIN used for both Hub login and supervised class check-in. Guardian login remains schema-ready only.
- Cookie-authenticated mutations require a CSRF double-submit cookie/header token.
- Input validation happens in the API; database uniqueness protects attendance and other relationships.
- Attendance helper status is an API decision, not a client-side option: the server compares the student’s current belt order with the highest belt threshold in the selected class track before writing `helper` status.
- Public content endpoints expose only published schedule/content fields. The deliberate local student-entry endpoint returns only display names and current rank labels for active, provisioned student accounts so the supervised dojo roster can be selected; it never returns usernames, passwords, guardians, attendance, or profile details.
- Audit events record actor, action, entity, entity ID, UTC timestamp, and safe metadata. Passwords, PINs, consent signatures, and raw secrets are not custom fields.
- Default hosting guidance is loopback-only and SQLite is server-side only. The database file is never a browser asset or a public download.
- Student profile data, friends, and messages require the student cookie and a server-recorded profile acknowledgment. Messages require an accepted friendship. Public schedule returns class-session fields only. The roster-based student entry and confirmation-based check-in flow is intended for loopback/local or deliberately supervised LAN use, not unrestricted public internet exposure.
- Staff social moderation can edit or soft-remove posts and comments. Removal is reversible, server-authorized, and audited; it does not destroy the underlying record.

## Explicit non-goals

The student milestone does not implement guardian login, consent signing, media uploads, rate limiting, email delivery, or internet deployment. The friend/message feature is intentionally limited to authenticated students who have accepted a local profile acknowledgment and should receive a separate privacy review before use with real families.

Never add a client-side PIN, hidden admin button, localStorage session, IndexedDB fallback, dynamic SQL, or a production-shared default credential. Student PINs are assigned or replaced by staff and must be issued privately; the local demo PIN is development-only. The legacy portal and Python desk server remain prototype-only and must not be used for real student records.
- Student access is managed through server-side account records: the same student PIN is checked for Hub sign-in and supervised attendance, and students can change it only after supplying the current PIN. Staff PIN assignment requires the Administrator role and is audited. PIN hashes are produced by the ASP.NET password hasher; plaintext PINs are never stored or returned.
