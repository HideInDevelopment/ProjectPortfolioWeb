# Portfolio Web
Repository that contains a functional/simple API to feed my portfolio web.

## Description
The architecture is base on a modular monolith with independet monoliths (.Domain, .Application, .Infrastructure, .Api), each of them with it responsibilities.

## Database
PostgreSQL is the database choosen to store the data.

## Deployment
The API is dockerized as the database as well. 

## Run With Docker
From the solution root, run:

```bash
docker compose up --build
```

This starts:

- PostgreSQL on `localhost:5432`
- The API on `http://localhost:8080`
- Scalar on `http://localhost:8080/scalar`

To stop the containers:

```bash
docker compose down
```

If you also want to remove the PostgreSQL volume:

```bash
docker compose down -v
```

# WIP
