# Database schema

Identity is supplied by ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, claims, logins, and tokens). Application identifiers are GUIDs. Audit and tracked timestamps are UTC.

| Area | Tables and important relationships |
| --- | --- |
| People | `Students` has active status, age group, join date, sizes, and current belt FK. `Guardians` has contact data. `GuardianStudents` is a composite-key many-to-many link, so a guardian can link to many students and a student can have many guardians. |
| Classes | `Programs` → `ClassTemplates` → dated `ClassSessions`. `Enrollments` links students to recurring templates. `AttendanceRecords` links a real student and session with a unique `(ClassSessionId, StudentId)` index. |
| Rank progress | `BeltRanks`, append-only `StudentRankHistory`, `RankRequirements`, and unique `StudentRequirementProgress`. Current rank is a convenience FK; history is never overwritten. |
| Public content | `Announcements` and `NewsPosts` use `Draft`/`Published` plus `PublishedAtUtc`; `MediaAssets` is metadata-only in this milestone. Public queries filter to published rows. |
| Compliance readiness | `ConsentDocuments`, `ConsentAcceptances`, and `AuditEvents` are schema-ready. No signature or password plaintext is stored. Consent workflow is not exposed yet. |

Useful indexes include active student/name, guardian email, class-session date/template, active enrollment, attendance session/student and student, rank history, published content status/date, and audit entity/date. Deactivation is preferred over deleting people or templates. Foreign keys are explicit and required relationships use cascading behavior only for join rows.

The initial EF migration is in `server/TaskKarate.Api/Data/Migrations`. Apply it with `dotnet ef database update` or let the API apply pending migrations at startup in local development. The active portal workflow also maintains its canonical operational tables (`portal_programs`, `portal_class_templates`, `portal_enrollments`, and `portal_content`) in the same runtime file; these are deliberately separate from the `platform_*` Identity compatibility namespace.
