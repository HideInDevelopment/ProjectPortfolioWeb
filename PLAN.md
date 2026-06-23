# PortfolioWeb Plan

## Objetivo
Construir un backend solido para PortfolioWeb que pueda:

- servir como MVP real
- desplegarse con seguridad razonable
- ser consumido mas adelante por un frontend
- evolucionar despues hacia un modelo multiusuario

## Estado actual

### Completado
- [x] Estructura base de la solucion en proyectos separados:
  - `Domain`
  - `Core.Contracts`
  - `Application.Contract`
  - `Application`
  - `Infrastructure`
  - `Api`
  - proyectos de test por capa
- [x] Modelo de dominio inicial con `Author` y `Project`
- [x] Relacion `Author 1:N Project`
- [x] Persistencia con EF Core + PostgreSQL
- [x] Configuracion de entidades y migracion inicial
- [x] CRUD basico para `Author` y `Project`
- [x] DTOs, mappers manuales y services
- [x] Endpoints CRUD en la API
- [x] OpenAPI + Scalar
- [x] Autoaplicacion de migraciones al arrancar la API fuera de entorno `Testing`
- [x] Dockerizacion de la API y de PostgreSQL
- [x] Manejo inicial de excepciones:
  - validaciones funcionales en servicios
  - excepciones de aplicacion
  - excepciones de infraestructura
  - traduccion HTTP centralizada en `GlobalExceptionHandler`
- [x] Logging estructurado en `Application` y `Api`
- [x] Suite automatizada inicial:
  - tests de `Application`
  - tests de `Core.Contracts`
  - tests de `Infrastructure` en happy path
  - tests de `Api`
- [x] Script de ejecucion secuencial de tests
- [x] Hook opcional de `pre-push` preparado para validar tests antes de subir cambios

### En progreso funcionalmente, pero no cerrado del todo
- [~] Endurecimiento de la API
  - ya existe validacion de IDs y varios casos funcionales
  - faltan validaciones de payload mas cercanas al borde HTTP
  - algunos limites de longitud siguen descansando en EF / base de datos en vez de validarse antes
- [~] Testing
  - la base esta montada y es util
  - falta completar cobertura de escenarios no felices y mas integracion real contra infraestructura

## Pendiente para considerar el MVP como cerrado

### 1. Endurecimiento final de la API
- [ ] Revisar todos los DTOs de entrada para que validen explicitamente lo necesario antes de llegar a persistencia
- [ ] Homogeneizar respuestas de error esperadas por tipo de caso de uso
- [ ] Anadir validaciones que hoy dependen solo de EF / base de datos:
  - longitudes maximas
  - campos obligatorios
  - incoherencias entre entidades relacionadas
- [ ] Revisar contratos de request/response para que el frontend tenga una superficie estable

### 2. Test automaticos con foco en fiabilidad
- [ ] Completar tests de escenarios negativos en `Infrastructure`
- [ ] Anadir tests de integracion con PostgreSQL real para los puntos criticos de persistencia
- [ ] Cubrir mejor validaciones y errores HTTP visibles desde controladores / handler global
- [ ] Dejar claro que parte del sistema queda protegida por unit tests y que parte por integration tests

### 3. Seguridad base
- [ ] Definir y aplicar el minimo de seguridad exigible para un backend desplegable:
  - secretos fuera del codigo
  - configuracion segura por entorno
  - superficie de error controlada
  - revision de exposicion innecesaria en entornos no locales

### 4. Slice tecnica: User + Authentication + Authorization
Este punto pasa a ser requisito de cierre del MVP.

#### Objetivo funcional
- [ ] Un usuario puede registrarse y hacer login con credenciales locales
- [ ] Un usuario solo puede editar su propio `Author`
- [ ] Un usuario solo puede editar sus propios `Projects`
- [ ] La relacion entre `User` y `Author` es `1:1`

#### Reglas de negocio ya decididas
- [ ] Autenticacion local con `Email + PasswordHash`
- [ ] Autenticacion basada en JWT bearer
- [ ] Alta unica: al registrarse se crean `User` y `Author` en la misma operacion
- [ ] Borrado en cascada: `User -> Author -> Projects`
- [ ] `Email` unico
- [ ] `AuthorId` unico
- [ ] Base de datos actual prescindible: se asume `drop database` y nueva migracion

#### Modelo de dominio esperado
- [ ] Crear entidad `User` con estas propiedades:
  - `Id`
  - `Email`
  - `PasswordHash`
  - `AuthorId`
  - `CreatedDate`
  - `Role`
  - `IsActive`
- [ ] Relacionar `User` con `Author` en `1:1`
- [ ] Mantener `Author` centrado en portfolio y proyectos
- [ ] Mantener `User` centrado en acceso, identidad y permisos

#### Persistencia
- [ ] Crear configuracion EF Core para `User`
- [ ] Configurar indices unicos para:
  - `Email`
  - `AuthorId`
- [ ] Configurar la relacion `User 1:1 Author`
- [ ] Configurar delete cascade completo
- [ ] Generar nueva migracion de esquema tras recrear la base

#### DTOs pragmaticos
Evitar repetir el problema que ya hubo con `Author` y `Project`.

- [ ] No mezclar DTOs de auth con DTOs de author/project
- [ ] Mantener DTOs pequenos y orientados al caso de uso
- [ ] Primera propuesta minima:
  - `RegisterUserDTO`
  - `LoginUserDTO`
  - `AuthResponseDTO`
- [ ] `RegisterUserDTO` deberia contener solo:
  - `Email`
  - `Password`
  - `AuthorName`
- [ ] `LoginUserDTO` deberia contener solo:
  - `Email`
  - `Password`
- [ ] `AuthResponseDTO` deberia contener solo:
  - `AccessToken`
  - `ExpiresAt`
- [ ] No crear `UserDTO` general si no hay un caso de uso real que lo necesite
- [ ] No crear endpoint `Me` por ahora, porque todavia no hay necesidad funcional clara

#### Application layer
- [ ] Crear service de autenticacion / usuarios con estos casos de uso minimos:
  - `Register`
  - `Login`
- [ ] En `Register`:
  - validar email no vacio
  - validar password no vacia
  - validar author name no vacio
  - comprobar email no existente
  - crear `Author`
  - hashear password
  - crear `User`
  - devolver JWT
- [ ] En `Login`:
  - buscar `User` por email
  - comprobar `IsActive`
  - verificar password
  - devolver JWT

#### Infraestructura tecnica minima
- [ ] Elegir una implementacion simple y conocida para hash de password
- [ ] Elegir una implementacion simple para generar JWT
- [ ] No introducir capas nuevas "por si acaso"
- [ ] Reutilizar el estilo actual de services, repositories y excepciones

#### Repositorios
- [ ] Exponer solo lo minimo necesario para la slice:
  - consulta por email de `User`
  - alta de `User`
  - alta de `Author` ligada al registro
- [ ] Revisar si conviene un `IUserRepository` y nada mas
- [ ] No tocar `AuthorRepository` ni `ProjectRepository` mas de lo necesario

#### API
- [ ] Crear endpoints minimos:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
- [ ] Mantener `AuthorsController` y `ProjectsController` separados de auth
- [ ] Anadir autorizacion sobre endpoints de modificacion de `Author` y `Project`
- [ ] Definir como se obtiene el `UserId` o `AuthorId` desde claims JWT

#### Excepciones y respuestas
- [ ] Definir excepciones de aplicacion especificas solo si aportan valor claro:
  - email duplicado
  - credenciales invalidas
  - usuario inactivo
- [ ] Traducir esos casos a HTTP desde el handler global
- [ ] No crear una jerarquia compleja de excepciones para auth si tres o cuatro casos cubren el MVP

#### Testing
- [ ] Tests unitarios del service de auth:
  - register correcto
  - login correcto
  - email duplicado
  - password invalida
  - usuario inactivo
- [ ] Tests de API para register y login
- [ ] Tests de autorizacion para evitar editar authors/projects ajenos

#### Riesgos y decisiones conscientes
- [ ] No introducir refresh tokens en esta fase
- [ ] No introducir `Me` en esta fase
- [ ] No introducir roles complejos en esta fase
- [ ] Primera opcion de `Role`: un valor simple tipo `User`
- [ ] Si en el futuro aparece admin, se extiende desde aqui sin rehacer el modelo

### 5. Criterio de salida a despliegue
- [ ] Definir un flujo minimo de despliegue reproducible
- [ ] Verificar que el contenedor de API funciona con configuracion externa limpia
- [ ] Dejar resuelta la estrategia de migraciones para entorno desplegado
- [ ] Anadir automatizacion remota basica
  - CI de build + test como minimo

## Orden recomendado de ejecucion
1. Cerrar endurecimiento de API
2. Completar bateria de tests pendiente
3. Implementar la slice `User + Authentication + Authorization`
4. Aplicar seguridad base ligada a la nueva autenticacion/autorizacion
5. Cerrar criterios de despliegue

## Fuera de alcance por ahora
- Frontend
- endpoint `Me`
- refresh tokens
- roles avanzados
- funcionalidades avanzadas de multiusuario mas alla de la base de auth/authz
- features no necesarias para exponer portfolio y proyectos

## Definicion practica de MVP cerrado
Consideraremos el MVP cerrado cuando se cumpla todo lo siguiente:

- CRUD de `Author` y `Project` estable
- modelo `User 1:1 Author` implementado
- registro y login operativos con JWT
- autorizacion minima para proteger authors y projects propios
- validaciones principales resueltas antes de persistencia
- tests automaticos cubriendo happy path y casos negativos relevantes
- despliegue reproducible con configuracion externa razonable
