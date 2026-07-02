# Portfolio Web
Repository that contains a functional/simple API to feed my portfolio web.

## Description
The architecture is base on a modular monolith with independet monoliths (.Domain, .Application, .Infrastructure, .Api), each of them with it responsibilities.

## Database
PostgreSQL is the database choosen to store the data.

## Deployment
The API is dockerized as the database as well. 

## Run With Docker
Create a local `.env` file first. The quickest path is:

```powershell
Copy-Item .env.example .env
```

Then adjust the secret values in `.env` before starting the containers.

From the solution root, run:

```bash
docker compose up --build
```

This starts:

- PostgreSQL on `localhost:5432`
- The API on `http://localhost:8080`
- Scalar on `http://localhost:8080/scalar`

Security note:

- `.env` is local and gitignored.
- Docker secrets such as the PostgreSQL password and JWT signing key should stay there, not in tracked files.
- OpenAPI and Scalar are exposed by default in local development and testing. In other environments they should stay disabled unless explicitly enabled through configuration.

To stop the containers:

```bash
docker compose down
```

If you also want to remove the PostgreSQL volume:

```bash
docker compose down -v
```

## Run Tests
To run the full automated test suite from the solution root:

```powershell
.\scripts\test-all.ps1
```

If you already restored packages and want a faster run:

```powershell
.\scripts\test-all.ps1 -NoRestore
```

## Optional Git Pre-Push Hook
The repository includes a prepared but non-activated Git hook template at:

`scripts/git-hooks/pre-push.sample`

and an installer script at:

`scripts/install-git-hooks.ps1`

Its purpose is to run the automated test suite before allowing a `git push`.

This is not activated automatically because Git hooks live inside `.git/hooks` and are local to each developer machine.

To activate it locally from the solution root, run:

```powershell
.\scripts\install-git-hooks.ps1
```

If you want to overwrite an existing local `pre-push` hook:

```powershell
.\scripts\install-git-hooks.ps1 -Force
```

This will copy the template to:

`.git/hooks/pre-push`

The installed hook will call:

```powershell
.\scripts\test-all.ps1 -NoRestore
```

Recommended approach:

- Use the local `pre-push` hook as a fast safety net.
- Keep CI in the remote repository as the final source of truth.
- Do not rely only on the local hook, because it can be skipped or missing on other machines.

# WIP
