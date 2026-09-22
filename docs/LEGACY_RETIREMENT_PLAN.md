# Legacy retirement plan

The following paths are preserved because they support the current public site or contain historical design/content reference:

| Legacy area | Current treatment | Retirement condition |
| --- | --- | --- |
| `portal/` and old `checkin.html` routes | Preserved, labeled prototype in documentation | Remove after the new staff/public experience is approved and a content owner signs off. |
| `server/desk_server.py` | Preserved but deprecated; it is not extended | Remove after the new API has completed supervised attendance verification and any needed data export is archived. |
| `data/schedules.json` and `data/portal-students.json` | Preserved as source archive; optional development importer only | Retire after staff verifies the new database content and no public page depends on the files. |
| old news JSON/social/message/waiver demo data | Not imported; retained for historical reference | Archive outside the application after editorial and privacy review. |

No blind deletion was performed. Before retirement, search for references, verify the replacement route, export any approved public content, remove only unused code, and run the legacy public-page smoke test. This milestone keeps old pages functioning while the new platform becomes the future source of truth.
