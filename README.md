# 🎬 MovieApi

A RESTful Web API built with **ASP.NET Core (.NET 10)** and **Entity Framework Core**, using **SQL Server** as the database. Also include testing with xUnit.

---

## 🧱 Tech Stack

| Technology | Version |
|---|---|
| .NET | 10 |
| ASP.NET Core Web API | 10.0 |
| Entity Framework Core | 10.0.9 |
| SQL Server LocalDB | 17.0.4 |
| xunit | 2.9.3 |
| Moq | 4.20.72 |

---

## 📁 Project Structure

```text
MovieApi/
├── Controllers/
│   ├── ActorsController.cs
│   ├── MoviesController.cs
│   └── ReviewsController.cs
├── Data/
│   └── MovieDbContext.cs
├── DTOs/
│   ├── ActorDto.cs
│   ├── MovieCreateDto.cs
│   ├── MovieDetailDto.cs
│   ├── MovieDetailsDto.cs
│   ├── MovieDto.cs
│   ├── MovieUpdateDto.cs
│   └── ReviewDto.cs
├── Extensions/
│   └── SeedDataExtensions.cs
├── Interfaces/
│   └── IMovieDbContext.cs
├── Migrations/
├── Models/
│   ├── Actor.cs
│   ├── Genre.cs
│   ├── Movie.cs
│   ├── MovieDetails.cs
│   └── Review.cs
├── Services/
│   ├── IMovieService.cs
│   └── MovieService.cs
├── appsettings.json
└── Program.cs
│
└── TestingMovieWebApi/             
    ├── TestingMovieWebApi.csproj
    └── MoviesControllerTests.cs
```

## 🗂️ Data Model

### Entities

| Entity | Key Fields |
|---|---|
| `Movie` | Id, Title, Year, Duration, GenreId |
| `Genre` | Id, Name |
| `MovieDetails` | Id, Synopsis, Language, Budget, MovieId |
| `Review` | Id, ReviewerName, Comment, Rating, MovieId |
| `Actor` | Id, Name, BirthYear |

### Relationships

| Type | Description |
|---|---|
| **N:1** | A `Movie` belongs to one `Genre` |
| **1:1** | A `Movie` has one `MovieDetails` |
| **1:M** | A `Movie` has many `Reviews` |
| **N:M** | `Movie` ↔ `Actor` via the `MovieActor` join table |

---

## 🧪 Testing

The `TestingMovieWebApi` project contains unit tests for `MoviesController` using **xUnit** and **Moq**.

###  Endpoints

| Method | Endpoint | Query params |
|--------|----------|--------------|
| `GET` | `/api/movies` | `?title=` `?year=` `?genre=` |
| `GET` | `/api/movies/{id}` | `?withActors=` `?withReviews=` `?withDetails=` |
| `GET` | `/api/movies/{id}/details` |
| `POST` | `/api/movies` |
| `PUT` | `/api/movies/{id}` |
| `DELETE` | `/api/movies/{id}` |

#### Reviews

| Method | Endpoint | Query params |
|--------|----------|-------------- |
| `GET` | `/api/movies/{movieId}/reviews` | `?minRating=` `?maxRating=` 
| `GET` | `/api/movies/{movieId}/reviews/{id}` | 
#### Actors

| Method | Endpoint |
|--------|----------|
| `GET` | `/api/movies/{movieId}/actors` |
| `POST` | `/api/movies/{movieId}/actors/{actorId}` |
| `DELETE` | `/api/movies/{movieId}/actors/{actorId}` |

---

### 📊 HTTP Status Codes Reference

| Code | Meaning |
|---|---|
| `200 OK` | Successful GET |
| `201 Created` | Resource successfully created (POST) |
| `204 No Content` | Successful PUT / DELETE |
| `400 Bad Request` | Validation failed or invalid reference |
| `404 Not Found` | Resource does not exist |
| `409 Conflict` | Duplicate relationship |
