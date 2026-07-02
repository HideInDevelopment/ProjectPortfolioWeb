# Smoke Test

Checklist manual para validar la API desde Scalar en el estado actual del repo.

## Precondiciones

- Levantar PostgreSQL y la API.
- Confirmar que la API arranca sin error y aplica migraciones si existen.
- Abrir Scalar en `/scalar`.
- Partir de una base limpia o, como minimo, usar emails unicos por prueba.

## Datos de prueba sugeridos

Usar dos usuarios para probar ownership:

- Usuario A
  - `email`: `smoke.a@example.com`
  - `password`: `Password123!`
  - `authorName`: `Smoke Author A`
- Usuario B
  - `email`: `smoke.b@example.com`
  - `password`: `Password123!`
  - `authorName`: `Smoke Author B`

Proyecto de prueba:

- `title`: `Smoke Project`
- `description`: `Project created during smoke test.`
- `releaseDate`: `2026-07-01T00:00:00+00:00`
- `version`: `1`
- `isInDevelopment`: `true`

## Orden recomendado

## 1. Disponibilidad basica

- `GET /openapi/v1.json`
  - Esperado: `200`
- Abrir `/scalar`
  - Esperado: carga correcta de la documentacion

## 2. Auth

### Registro

- `POST /api/auth/register` con Usuario A
  - Esperado: `200`
  - Validar que devuelve `accessToken` y `expiresAt`
- `POST /api/auth/register` con Usuario B
  - Esperado: `200`
- `POST /api/auth/register` repitiendo el email de Usuario A
  - Esperado: `409`

### Login

- `POST /api/auth/login` con Usuario A
  - Esperado: `200`
  - Guardar `accessToken`
- `POST /api/auth/login` con password incorrecta
  - Esperado: `401`
- `POST /api/auth/login` con `email` o `password` vacios
  - Esperado: `400`

## 3. Autorizacion en Scalar

- Autorizar con el token del Usuario A
- Repetir luego con el del Usuario B cuando toque validar ownership

## 4. Authors

Nota: no existe `POST /api/Authors`. El `Author` se crea al registrar un `User`.

### Lectura

- `GET /api/Authors`
  - Esperado: `200`
  - Validar que aparecen los authors creados
- `GET /api/Authors/{authorAId}`
  - Esperado: `200`
- `GET /api/Authors/{guidInexistente}`
  - Esperado: `404`
- `GET /api/Authors/{Guid.Empty}`
  - Esperado: `400`

Nota:
- si llamas a `/api/Authors` sin `id`, eso es el endpoint de coleccion y devolvera la lista con `200`
- no se puede hacer que "falta el id" y "quiero la coleccion" compartan la misma URL y devuelvan cosas distintas

### Escritura protegida

- `PUT /api/Authors` con token del Usuario A y body:

```json
{
  "name": "Smoke Author A Updated"
}
```

  - Esperado: `200`
  - Validar que el nombre cambia

- `PUT /api/Authors` sin token
  - Esperado: `401`

- `PUT /api/Authors` con token del Usuario B
  - Esperado: `200`
  - Nota: actualiza su propio author, no el del Usuario A, porque el author objetivo sale del JWT

- `PUT /api/Authors` con body invalido:

```json
{
  "name": ""
}
```

  - Esperado: `400`

### Delete protegido

- `DELETE /api/Authors/{authorAId}` sin token
  - Esperado: `401`
- `DELETE /api/Authors/{authorAId}` con token del Usuario B
  - Esperado: `403`
- `DELETE /api/Authors/{Guid.Empty}` con token del Usuario A
  - Esperado: `400`

No ejecutar el delete correcto hasta el final del smoke test, porque arrastra `User -> Author -> Projects`.

## 5. Projects

### Lectura

- `GET /api/Projects`
  - Esperado: `200`
- `GET /api/Projects/{guidInexistente}`
  - Esperado: `404`
- `GET /api/Projects/{Guid.Empty}`
  - Esperado: `400`

Nota:
- si llamas a `/api/Projects` sin `id`, eso es el endpoint de coleccion y devolvera la lista con `200`

### Create protegido

- `POST /api/Projects` con token del Usuario A

```json
{
  "title": "Smoke Project",
  "description": "Project created during smoke test.",
  "releaseDate": "2026-07-01T00:00:00+00:00",
  "version": 1,
  "isInDevelopment": true
}
```

  - Esperado: `201`
  - Guardar `projectAId`

- `POST /api/Projects` sin token
  - Esperado: `401`

- `POST /api/Projects` con un token cuyo `authorId` ya no exista en base de datos
  - Esperado: `400`

- `POST /api/Projects` con body invalido, por ejemplo `title = ""`
  - Esperado: `400`

### Update protegido

- `PUT /api/Projects/{projectAId}` con token del Usuario A

```json
{
  "title": "Smoke Project Updated",
  "description": "Project updated during smoke test.",
  "version": 2,
  "isInDevelopment": false
}
```

  - Esperado: `200`

- `PUT /api/Projects/{projectAId}` sin token
  - Esperado: `401`

- `PUT /api/Projects/{projectAId}` con token del Usuario B
  - Esperado: `403`

- `PUT /api/Projects/{Guid.Empty}` con token del Usuario A
  - Esperado: `400`

- `PUT /api/Projects/{projectAId}` con body invalido
  - Esperado: `400`

### Delete protegido

- `DELETE /api/Projects/{projectAId}` sin token
  - Esperado: `401`

- `DELETE /api/Projects/{projectAId}` con token del Usuario B
  - Esperado: `403`

- `DELETE /api/Projects/{Guid.Empty}` con token del Usuario A
  - Esperado: `400`

- `DELETE /api/Projects/{projectAId}` con token del Usuario A
  - Esperado: `204`

## 6. Cascada de borrado

Ejecutar solo al final.

- `DELETE /api/Authors/{authorAId}` con token del Usuario A
  - Esperado: `204`
- `GET /api/Authors/{authorAId}`
  - Esperado: `404`
- `GET /api/Projects/{projectAId}`
  - Esperado: `404`

Si se quiere validar la cascada completa desde `User`, hoy no hay endpoint publico para borrar usuarios, asi que este punto queda cubierto indirectamente por la persistencia y los tests automatizados.

## 7. Criterios de salida del smoke test

El smoke test se puede dar por bueno si se cumple todo esto:

- La API arranca y Scalar carga
- Registro y login funcionan
- Los endpoints publicos de lectura responden correctamente
- Los endpoints protegidos exigen JWT
- El ownership entre usuarios funciona con `403`
- Las validaciones principales responden con `400`
- Los `404` aparecen en recursos inexistentes
- El CRUD de `Project` funciona end to end
- El update y delete de `Author` funcionan
- No aparecen errores inesperados `500` en los escenarios anteriores
