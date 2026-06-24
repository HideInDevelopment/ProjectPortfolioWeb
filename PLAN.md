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
- [x] Slice tecnica `User + Authentication + Authorization` cerrada:
  - registro y login con credenciales locales
  - hashing de password
  - JWT bearer
  - relacion `User 1:1 Author`
  - ownership sobre escritura de `Author` y `Project`
  - revalidacion de usuario activo en endpoints protegidos
  - tests unitarios, de API e integracion sobre auth/authz

### En progreso funcionalmente, pero no cerrado del todo
- [~] Endurecimiento de la API
  - ya existe validacion de IDs y varios casos funcionales
  - faltan validaciones de payload mas cercanas al borde HTTP
  - algunos limites de longitud siguen descansando en EF / base de datos en vez de validarse antes
- [~] Testing
  - la base esta montada y es util
  - ya hay escenarios no felices relevantes en `Api` e `Infrastructure`
  - ya hay integracion real contra PostgreSQL en `Infrastructure`
  - falta decidir hasta donde compensa empujar mas cobertura en plumbing y glue code

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
- [x] Completar tests de escenarios negativos asumibles en `Infrastructure`
- [x] Anadir tests de integracion con PostgreSQL real para los puntos criticos de persistencia
- [x] Cubrir mejor validaciones y errores HTTP visibles desde controladores / handler global
- [x] Cubrir auth/authz con:
  - roundtrip real de JWT en endpoints protegidos
  - ownership `403`
  - usuario inactivo tras emitir token
  - duplicado real de persistencia en registro
- [x] Ejecutar revision destructiva manual de la suite completa para buscar falsos verdes y huecos de regresion
- [ ] Cubrir rechazo de JWT invalidos o expirados en endpoints protegidos
- [ ] Endurecer tests de validacion HTTP para comprobar errores por campo y no solo `400`
- [ ] Decidir si compensa anadir un happy path autenticado full-stack con servicios y repositorios reales bajo HTTP
- [ ] Dejar claro que parte del sistema queda protegida por unit tests y que parte por integration tests

### 3. Seguridad base
- [ ] Definir y aplicar el minimo de seguridad exigible para un backend desplegable:
  - secretos fuera del codigo
  - configuracion segura por entorno
  - superficie de error controlada
  - revision de exposicion innecesaria en entornos no locales

### 4. Slice tecnica `User + Authentication + Authorization`
- [x] Completada

#### Alcance entregado
- [x] Entidad `User` con relacion `1:1` hacia `Author`
- [x] Registro y login con `Email + PasswordHash`
- [x] JWT bearer para autenticacion
- [x] Alta conjunta `User + Author`
- [x] Ownership sobre escritura de `Author` y `Project`
- [x] Revalidacion de `IsActive` en endpoints protegidos
- [x] Traduccion de duplicado de email tanto en pre-check como en persistencia real
- [x] Tests unitarios, de API e integracion sobre auth/authz

#### Decisiones conscientes mantenidas
- [x] Sin refresh tokens en esta fase
- [x] Sin endpoint `Me`
- [x] Sin roles complejos
- [x] `Role` simple con valor inicial `User`

### 5. Criterio de salida a despliegue
- [ ] Definir un flujo minimo de despliegue reproducible
- [ ] Verificar que el contenedor de API funciona con configuracion externa limpia
- [ ] Dejar resuelta la estrategia de migraciones para entorno desplegado
- [ ] Anadir automatizacion remota basica
  - CI de build + test como minimo

## Orden recomendado de ejecucion
1. Cerrar endurecimiento de API
2. Completar bateria de tests pendiente
3. Aplicar seguridad base ligada a la autenticacion/autorizacion ya implementada
4. Cerrar criterios de despliegue

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
