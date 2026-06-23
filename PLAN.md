# PortfolioWeb Plan

## Objetivo
Construir un backend sólido para PortfolioWeb que pueda:

- servir como MVP real
- desplegarse con seguridad razonable
- ser consumido más adelante por un frontend
- evolucionar después hacia un modelo multiusuario

## Estado actual

### Completado
- [x] Estructura base de la solución en proyectos separados:
  - `Domain`
  - `Core.Contracts`
  - `Application.Contract`
  - `Application`
  - `Infrastructure`
  - `Api`
  - proyectos de test por capa
- [x] Modelo de dominio inicial con `Author` y `Project`
- [x] Relación `Author 1:N Project`
- [x] Persistencia con EF Core + PostgreSQL
- [x] Configuración de entidades y migración inicial
- [x] CRUD básico para `Author` y `Project`
- [x] DTOs, mappers manuales y services
- [x] Endpoints CRUD en la API
- [x] OpenAPI + Scalar
- [x] Autoaplicación de migraciones al arrancar la API fuera de entorno `Testing`
- [x] Dockerización de la API y de PostgreSQL
- [x] Manejo inicial de excepciones:
  - validaciones funcionales en servicios
  - excepciones de aplicación
  - excepciones de infraestructura
  - traducción HTTP centralizada en `GlobalExceptionHandler`
- [x] Logging estructurado en `Application` y `Api`
- [x] Suite automatizada inicial:
  - tests de `Application`
  - tests de `Core.Contracts`
  - tests de `Infrastructure` en happy path
  - tests de `Api`
- [x] Script de ejecución secuencial de tests
- [x] Hook opcional de `pre-push` preparado para validar tests antes de subir cambios

### En progreso funcionalmente, pero no cerrado del todo
- [~] Endurecimiento de la API
  - ya existe validación de IDs y varios casos funcionales
  - faltan validaciones de payload más cercanas al borde HTTP
  - algunos límites de longitud siguen descansando en EF / base de datos en vez de validarse antes
- [~] Testing
  - la base está montada y es útil
  - falta completar cobertura de escenarios no felices y más integración real contra infraestructura

## Pendiente para considerar el MVP como cerrado

### 1. Endurecimiento final de la API
- [ ] Revisar todos los DTOs de entrada para que validen explícitamente lo necesario antes de llegar a persistencia
- [ ] Homogeneizar respuestas de error esperadas por tipo de caso de uso
- [ ] Añadir validaciones que hoy dependen solo de EF / base de datos:
  - longitudes máximas
  - campos obligatorios
  - incoherencias entre entidades relacionadas
- [ ] Revisar contratos de request/response para que el frontend tenga una superficie estable

### 2. Test automáticos con foco en fiabilidad
- [ ] Completar tests de escenarios negativos en `Infrastructure`
- [ ] Añadir tests de integración con PostgreSQL real para los puntos críticos de persistencia
- [ ] Cubrir mejor validaciones y errores HTTP visibles desde controladores / handler global
- [ ] Dejar claro qué parte del sistema queda protegida por unit tests y qué parte por integration tests

### 3. Seguridad base
- [ ] Definir y aplicar el mínimo de seguridad exigible para un backend desplegable:
  - secretos fuera del código
  - configuración segura por entorno
  - superficie de error controlada
  - revisión de exposición innecesaria en entornos no locales
- [ ] Decidir y montar el enfoque de autenticación y autorización del MVP extendido

### 4. Gestión de usuarios
Este punto pasa a ser requisito de cierre del MVP.

- [ ] Crear entidad `User`
- [ ] Relacionarla `1:1` con `Author`
- [ ] Separar responsabilidades:
  - `Author`: información de portfolio y relación con proyectos
  - `User`: autenticación, autorización y datos propios de acceso
- [ ] Revisar el impacto en:
  - dominio
  - persistencia
  - DTOs
  - services
  - endpoints
  - tests

### 5. Criterio de salida a despliegue
- [ ] Definir un flujo mínimo de despliegue reproducible
- [ ] Verificar que el contenedor de API funciona con configuración externa limpia
- [ ] Dejar resuelta la estrategia de migraciones para entorno desplegado
- [ ] Añadir automatización remota básica
  - CI de build + test como mínimo

## Orden recomendado de ejecución
1. Cerrar endurecimiento de API
2. Completar batería de tests pendiente
3. Incorporar `User` y la relación `1:1` con `Author`
4. Aplicar seguridad base ligada a autenticación/autorización
5. Cerrar criterios de despliegue

## Fuera de alcance por ahora
- Frontend
- funcionalidades avanzadas de multiusuario más allá de la base de auth/authz
- features no necesarias para exponer portfolio y proyectos

## Definición práctica de MVP cerrado
Consideraremos el MVP cerrado cuando se cumpla todo lo siguiente:

- CRUD de `Author` y `Project` estable
- modelo `User 1:1 Author` implementado
- autenticación/autorización base definidas
- validaciones principales resueltas antes de persistencia
- tests automáticos cubriendo happy path y casos negativos relevantes
- despliegue reproducible con configuración externa razonable
