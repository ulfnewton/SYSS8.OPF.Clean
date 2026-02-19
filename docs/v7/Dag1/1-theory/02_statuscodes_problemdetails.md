
# Statuskoder & ProblemDetails

## Vanliga koder
- 201 Created (skapad resurs)
- 200 OK (hämtat/returnerat)
- 204 No Content (ingen kropp)
- 400 Bad Request (valideringsfel)
- 404 Not Found (saknas)
- 409 Conflict (krock, t.ex. unikt värde)
- 401 Unauthorized (ej autentiserad)
- 403 Forbidden (autentiserad men saknar behörighet)

## ProblemDetails-fält (problem+json)
- `type`, `title`, `status`, `detail`, `instance`

## Rekommendation
- Låt **401/403** returnera **ProblemDetails** konsekvent.
- Låt UI:n läsa `status` och `detail` för att visa användbar feedback.
