# Fixtures de contrato — migración .NET

Origen C++: `src/sync/*` (parsers), `src/track/recognition*`, `src/media/library/queue.cpp`, `src/base/settings.cpp`.

## `fixtures/parsers/`

- `anilist_media_list.json` — (pendiente) respuesta `MediaList` con estado, progreso, score; cubre `sync/anilist.cpp`, `anilist_util.cpp`, `anilist_ratings.cpp`.
- `kitsu_library_entry.json` — (pendiente) `libraryEntry` con `status/progress/rating`; cubre `sync/kitsu*.cpp`.
- `myanimelist_list_status.json` — (pendiente) `list_status` OAuth2/PKCE; cubre `sync/myanimelist*.cpp`.
- `error_rate_limit.json` — (pendiente) 429 + `Retry-After`; cubre `service.cpp` + `Polly`.

## `fixtures/recognition/`

- `filenames.csv` — (pendiente) `filename,anime_id,episode`: casos `recognition*` (normalize, path, relations, validate, cache) + corpus anime-relations/anitomy.
- Incluye títulos con `[ Fansub ]`, `S02E05`, absolutos, multi-episodio y edge cases Unicode.

## `fixtures/queue/`

- `offline_replay.jsonl` — (pendiente) secuencia detectar → sin red → encolar → backoff → confirmar sin duplicados (`sync.cpp`, `queue.cpp`).

## Uso en tests

`tests/Taiga.Sync.Tests` y `tests/Taiga.Track.Tests` cargan estos ficheros como `TheoryData`/`MemberData` (xUnit).
Sin red: todo test usa fixtures locales; E2E real solo en smoke manual.
