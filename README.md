> [!WARNING]  
> Preliminär dokumentation som kommer förändras under projektets gång.

# 🎬 MovieApi

Ett RESTful Web API byggt med **ASP.NET Core (.NET 10)** och **Entity Framework Core**, som använder **SQL Server** som databas. Stödjer **JWT-autentisering**, **API-versionering** och **Swagger UI**. Inkluderar även enhetstester med **xUnit**.

---

## 🧱 Teknikstack

| Teknik | Version |
|---|---|
| .NET | 10 |
| ASP.NET Core Web API | 10.0 |
| Entity Framework Core | 10.0.9 |
| SQL Server LocalDB | 17.0.4 |
| Asp.Versioning.Mvc | 10.0.0 |
| Swashbuckle.AspNetCore | 10.2.3 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.9 |
| System.IdentityModel.Tokens.Jwt | 8.19.1 |
| xUnit | 2.9.3 |
| Moq | 4.20.72 |

---

## 🗂️ Datamodell

### Entiteter

| Entitet | Nyckelfält |
|---|---|
| `Movie` | Id, Title, Year, Duration, GenreId |
| `Genre` | Id, Name |
| `MovieDetails` | Id, Synopsis, Language, Budget, MovieId |
| `Review` | Id, ReviewerName, Comment, Rating, MovieId |
| `Actor` | Id, Name, BirthYear |

### Relationer

| Typ | Beskrivning |
|---|---|
| **N:1** | En `Movie` tillhör en `Genre` |
| **1:1** | En `Movie` har en `MovieDetails` |
| **1:M** | En `Movie` kan ha flera `Reviews` |
| **N:M** | `Movie` ↔ `Actor` via kopplingstabellen `MovieActor` |

---

## 🔐 Autentisering

API:t använder **JWT Bearer**-tokens. En token hämtas via login-endpointen och skickas sedan som en **Bearer token** i `Authorization`-headern.

> Demo-inloggning: användarnamn `admin`, lösenord `hemligt`.

Swagger UI innehåller en inbyggd **Authorize**-knapp för att testa skyddade endpoints.

---

## 🌐 API-endpoints

Alla endpoints är versionshanterade under `/api/v1/`.

### Filmer

| Metod | Endpoint | Query-parametrar |
|--------|----------|--------------|
| `GET` | `/api/v1/movies` | `?title=` `?year=` `?genre=` |
| `GET` | `/api/v1/movies/{id}` | `?withActors=` `?withReviews=` `?withDetails=` |
| `GET` | `/api/v1/movies/{id}/details` | |
| `POST` | `/api/v1/movies` | |
| `PUT` | `/api/v1/movies/{id}` | |
| `DELETE` | `/api/v1/movies/{id}` | |

### Recensioner

| Metod | Endpoint | Query-parametrar |
|--------|----------|--------------|
| `GET` | `/api/v1/movies/{movieId}/reviews` | `?minRating=` `?maxRating=` |
| `GET` | `/api/v1/movies/{movieId}/reviews/{id}` | |

### Skådespelare

| Metod | Endpoint |
|--------|----------|
| `GET` | `/api/v1/movies/{movieId}/actors` |
| `POST` | `/api/v1/movies/{movieId}/actors/{actorId}` |
| `DELETE` | `/api/v1/movies/{movieId}/actors/{actorId}` |

### Autentisering

| Metod | Endpoint | Beskrivning |
|--------|----------|-------------|
| `POST` | `/api/v1/auth/login` | Returnerar en JWT-token |

---

## 🧪 Testning

Projektet `TestingMovieWebApi` innehåller enhetstester för `MoviesController` med hjälp av **xUnit** och **Moq**.

---

## 📊 Referens för HTTP-statuskoder

| Kod | Betydelse |
|---|---|
| `200 OK` | Lyckad GET-förfrågan |
| `201 Created` | Resurs skapades framgångsrikt (POST) |
| `204 No Content` | Lyckad PUT / DELETE |
| `400 Bad Request` | Validering misslyckades eller ogiltig referens |
| `401 Unauthorized` | Saknad eller ogiltig JWT-token |
| `404 Not Found` | Resursen finns inte |
| `409 Conflict` | Dubblett av relation |
