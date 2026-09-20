# Student portal safety and consent boundary

## What this version is

The rebuilt portal is a frontend product prototype for a Task Karate student community. It demonstrates the intended information architecture and interaction model:

- a student-first community feed with instructor notes and school announcements;
- private-looking message threads for instructor and training-partner conversations;
- Gold Stars, achievements, rank progression, assignments, attendance, and a journey timeline;
- a family workflow for reviewing participation, community, and future communication policies;
- a staff workspace for local moderation, recognition, notes, consent status, and synthetic student data.

The demo uses `data/portal-demo.json`, `sessionStorage`, and `localStorage`. It does not authenticate a person, protect a record, send a message, or publish a staff action.

## Roles for a production system

The smallest useful role model is:

- **Student** — can view the records and conversations allowed for that student and create content within community rules.
- **Parent/guardian** — can review verified dependent accounts, consent versions, communication permissions, and safety notices.
- **Instructor/staff** — can write notes, record recognition, manage training information, respond to messages, and moderate within assigned scope.
- **Administrator** — can manage staff access, policy versions, retention, reports, and audit review.

The role must be enforced by the backend on every read and write. A hidden button, local role flag, or client-side route is not authorization.

## Community and messaging boundaries

The social layer is intentionally a training community, not a general social network. Production should provide:

- reporting on every post, comment, and message;
- staff moderation queues with reason codes and audit history;
- block/mute controls and safe escalation for students and families;
- clear rules about personal contact information, photos, bullying, threats, and off-platform communication;
- age-appropriate defaults and guardian visibility where required;
- server-side access checks so one student cannot read another student’s private thread;
- rate limits, abuse detection, and retention/deletion rules;
- an emergency/safety escalation path that does not depend on the feed.

Staff notes should be distinguishable from peer posts. Recognition should reward training behavior—effort, courage, consistency, and care for partners—not popularity or volume of posting.

## Waivers, disclaimers, and family consent

The portal includes a versioned-form interaction to make the product boundary visible. The current wording is a placeholder and is deliberately labeled as such.

This file and the demo form are not legal advice. A checkbox, typed name, or disclaimer in this repository is not a legally sufficient waiver, does not guarantee enforceability, and is not a substitute for safe supervision, appropriate policies, or professional counsel. Before production use, the school should have a qualified attorney review the participation waiver, photo/media policy, community rules, communication policy, privacy notice, retention policy, and guardian-consent process for the applicable jurisdiction and student ages.

A production consent workflow should record:

1. the verified guardian identity and relationship to the student;
2. the exact policy/waiver version presented;
3. what each consent covers and whether it is optional or required;
4. the timestamp, actor, method, and applicable jurisdiction;
5. withdrawal/revocation behavior and its effect on account access;
6. who can view, export, correct, or delete the record;
7. an audit trail that staff cannot silently rewrite.

## Production replacement checklist

- Replace demo login with server-managed sessions or a trusted identity provider.
- Enforce authentication, authorization, CSRF protection, validation, and rate limits server-side.
- Store messages, posts, notes, achievements, attendance, and consent records in a controlled database.
- Add moderation/reporting tools before enabling student-to-student posting.
- Verify guardians for minors and define parent/student communication visibility.
- Define retention, deletion, export, correction, and incident-response procedures.
- Add secure media handling and explicit consent before publishing student images.
- Keep secrets, real student records, passwords, and tokens out of this repository.

## Demo-only behaviors

- Login accepts any values and creates a browser-only session.
- Admin edits are saved only in the current browser.
- Messages and community posts are local synthetic data.
- Hiding a post is not moderation or deletion.
- The family acknowledgment is not signed, transmitted, witnessed, or legally executed.
- The download snapshot contains demo state only and must not be populated with real student data.
