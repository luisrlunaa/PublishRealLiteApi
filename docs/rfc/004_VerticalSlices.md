# Vertical Slice


Alcance: El proyecto actualmente propone `CleanArchitecture`, propon una version del mismo con Vertical Slices, para valorar si este enfoque funciona para este proyecto.
- Agrega dudas/sugerencias que tengas al respecto.



Nota: 
- Este documento contiene interacciones entre humano/agente. 
- Por eso pueden existir multiple ronda de Respuestas (`Respuesta — Ronda #`) (para los resultados de la IA) y Instruccion(`Instruccion — Ronda #`) (para los comentarios del humano).
- Por tanto, Agrega una session de Respuesta en este documento, para tu respuesta.
- No hagas cambios de codigo hasta que haya una session al final que `## Changes Approved By the User`
- En cualquier ronda de Respuesta puedes agregar dudas/sugerencias que tengas al respecto.

---

## Respuesta — Ronda 1

### Diagnóstico del estado actual

El proyecto hoy usa **Clean Architecture en su forma más literal**: cuatro proyectos, cada entidad tiene su `IService` → `Service` → `IRepository` → `Repository`. El flujo para crear un release es:

```
ReleasesController → IReleaseService → ReleaseService → IReleaseRepository → ReleaseRepository → AppDbContext
```

Esto genera **mucho ceremony para muy poco valor** en operaciones CRUD simples. `ReleaseService.CreateAsync` es básicamente un mapper y un llamado al repositorio. `ReleaseRepository` es un wrapper sobre EF Core. Las interfaces proliferan sin aportar testabilidad real (EF Core ya es testeable con in-memory o SQLite).

### ¿Qué son Vertical Slices?

En lugar de organizar por **capa** (`Controllers/`, `Services/`, `Repositories/`), se organiza por **feature/operación**. Cada slice contiene TODO lo necesario para ejecutar UNA operación:

- El request (Command/Query)
- El handler (la lógica de negocio + acceso a datos)
- El validator
- El response DTO

La premisa: **el acoplamiento dentro de un slice es deseable; el acoplamiento entre slices es lo que hay que evitar**.

### Estructura propuesta

```
src/
├── PublishRealLiteApi/                  ← Proyecto principal (API + Features)
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Register/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── RegisterHandler.cs
│   │   │   │   ├── RegisterValidator.cs
│   │   │   │   └── RegisterResponse.cs
│   │   │   └── Login/
│   │   │       ├── LoginCommand.cs
│   │   │       ├── LoginHandler.cs
│   │   │       └── LoginResponse.cs
│   │   ├── Releases/
│   │   │   ├── GetReleases/
│   │   │   │   ├── GetReleasesQuery.cs
│   │   │   │   ├── GetReleasesHandler.cs
│   │   │   │   └── GetReleasesResponse.cs
│   │   │   ├── GetReleaseById/
│   │   │   ├── CreateRelease/
│   │   │   │   ├── CreateReleaseCommand.cs
│   │   │   │   ├── CreateReleaseHandler.cs
│   │   │   │   ├── CreateReleaseValidator.cs
│   │   │   │   └── CreateReleaseResponse.cs
│   │   │   ├── UpdateRelease/
│   │   │   └── DeleteRelease/
│   │   ├── ArtistProfiles/
│   │   ├── Tracks/
│   │   ├── Videos/
│   │   ├── Teams/
│   │   └── Stats/
│   ├── Controllers/                     ← Controllers delgados, solo routing + MediatR dispatch
│   │   ├── ReleasesController.cs
│   │   └── ...
│   ├── Middleware/
│   └── Program.cs
│
├── PublishRealLiteApi.Infrastructure/   ← DbContext, Migrations, Storage, Workers
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── DbSeeder.cs
│   │   └── Migrations/
│   └── Storage/
│       ├── LocalStorageService.cs
│       └── AzureBlobStorageService.cs
│
└── PublishRealLiteApi.Models/           ← Entidades de dominio (sin cambios)
```

> **Nota**: el proyecto `Application` desaparece. No hay más `IService`/`Service`/`IRepository`/`Repository`. Los handlers acceden a `AppDbContext` directamente.

### Código de ejemplo: el slice `CreateRelease`

**Antes (Clean Architecture):**
- `CreateReleaseDto.cs` en Application/DTOs
- `IReleaseService.cs` en Application/Services/Interfaces
- `ReleaseService.cs` en Application/Services
- `IReleaseRepository.cs` en Application/Repositories/Interfaces
- `ReleaseRepository.cs` en Infrastructure/Repositories
- `ReleasesController.cs` en API/Controllers

**Después (Vertical Slice):**

```csharp
// Features/Releases/CreateRelease/CreateReleaseCommand.cs
public record CreateReleaseCommand(
    int ArtistProfileId,
    string Title,
    DateOnly ReleaseDate,
    string? Genre,
    string? Label,
    string? UPC,
    string? ISRC,
    string? LinksJson
) : IRequest<CreateReleaseResponse>;
```

```csharp
// Features/Releases/CreateRelease/CreateReleaseHandler.cs
public class CreateReleaseHandler : IRequestHandler<CreateReleaseCommand, CreateReleaseResponse>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateReleaseHandler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateReleaseResponse> Handle(CreateReleaseCommand cmd, CancellationToken ct)
    {
        var release = new Release
        {
            ArtistProfileId = cmd.ArtistProfileId,
            Title = cmd.Title,
            ReleaseDate = cmd.ReleaseDate,
            Genre = cmd.Genre,
            Label = cmd.Label,
            UPC = cmd.UPC,
            ISRC = cmd.ISRC,
            LinksJson = cmd.LinksJson,
            CreatedBy = _currentUser.UserId!
        };

        _db.Releases.Add(release);
        await _db.SaveChangesAsync(ct);

        return new CreateReleaseResponse(release.Id, release.ArtistProfileId, release.Title, release.ReleaseDate);
    }
}
```

```csharp
// Features/Releases/CreateRelease/CreateReleaseValidator.cs
public class CreateReleaseValidator : AbstractValidator<CreateReleaseCommand>
{
    public CreateReleaseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ArtistProfileId).GreaterThan(0);
    }
}
```

```csharp
// Controllers/ReleasesController.cs  ← Controller ultra-delgado
[ApiController]
[Route("api/[controller]")]
public class ReleasesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReleasesController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReleaseCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

El controller ya no sabe nada de `IReleaseService`. Solo despacha via MediatR y retorna HTTP.

### Comparación directa

| Aspecto | Clean Architecture (actual) | Vertical Slices |
|---|---|---|
| Proyectos | 4 | 2 (API + Infrastructure) |
| Archivos para crear un Release | ~6 (DTO, IService, Service, IRepository, Repository, Controller) | ~4 (Command, Handler, Validator, Response) |
| Agregar una feature nueva | Tocar 4 proyectos | Agregar 1 carpeta en `Features/` |
| Dónde vive la lógica | Distribuida en capas | Concentrada en el Handler |
| Testear CreateRelease | Mockear `IReleaseService` + `IReleaseRepository` | Testear el Handler directamente con DbContext in-memory |
| Acoplamiento entre features | Bajo (via interfaces) | Bajo (cada slice es independiente) |
| Curva de aprendizaje | Alta (layers + interfaces) | Media (MediatR + un patrón por operación) |

### Dudas y sugerencias

**1. ¿MediatR o no MediatR?**

MediatR es el habilitador clásico de VS en .NET, pero para un proyecto de este tamaño agrega indirección que puede confundir. Una opción intermedia: feature folders SIN MediatR. Los Handlers son clases de servicio simples inyectadas directamente. Perdes el pipeline de behaviors pero ganás simplicidad.

**2. El `AppDbContext` compartido es infraestructura legítima**

Algunos puristas del VS dicen que cada slice debería tener su propio contexto. Para este proyecto eso es sobrediseño total. El `AppDbContext` compartido con las entidades actuales está bien. Lo que desaparece es la capa de `Repository` — los Handlers consultan EF Core directamente.

**3. ¿Qué pasa con el código compartido?**

`ICurrentUserService`, `JwtService`, `IStorageService` son cross-cutting concerns que no pertenecen a un slice. En VS tienen dos lugares válidos:
- En `Infrastructure/` si son servicios de infraestructura
- En una carpeta `Common/` o `Shared/` dentro del proyecto si son abstracciones de dominio

El riesgo real de VS es que el código compartido se convierta en un "segundo Application layer" disfrazado. Hay que ser disciplinado.

**4. Los `Workers` y los `BackgroundServices` no encajan naturalmente en slices**

`StreamStatAggregatorWorker` es un proceso de background que agrega stats. ¿A qué slice pertenece? A ninguno realmente. Sigue viviendo en `Infrastructure/` o directamente en el proyecto API. VS no cambia nada aquí.

**5. ¿Vale la pena migrar el proyecto existente?**

Para un proyecto nuevo: **sí, VS es una excelente elección** — menos boilerplate, más navegabilidad.

Para migrar el proyecto EXISTENTE: el costo-beneficio es cuestionable. Habría que mover ~80 archivos, eliminar el proyecto Application, reescribir los controllers, agregar MediatR. El resultado es más limpio pero el ROI durante la migración es bajo porque la lógica es mayormente CRUD con poca complejidad de dominio.

**Mi recomendación**: si el objetivo es aprender/evaluar VS, hacé un POC con el slice de `Releases` completo (5 operaciones) en un branch separado. Si se ve más claro y navegable que la versión actual → migrar. Si no → Clean Architecture para este proyecto cumple sin problemas.

**6. Relación con CQRS**

VS no requiere CQRS, pero se llevan muy bien. En este proyecto, si se adopta VS, tiene sentido separar Queries (solo lectura, van directo a `_db.Releases.AsNoTracking()`) de Commands (escritura, usan el tracking normal de EF Core). No hace falta una base de datos de lectura separada — alcanza con el mismo SQL Server usando `AsNoTracking()` en las queries.

## Instruccion — Ronda 1

- Respuestas a tus dudas
    1. MediatR no hace falta.
    2. DbContext compartido esta bien.
    3. Hay que tomarlo en cuenta.
    4. Colocarlo debajo de un folder Workers (aunque ojo esto pertenece realmente a `Feature/Stats`)
- A tu propuesta creo que podemos ayudarle con algunas consideraciones
    1.  Todo lo referente a un command/query podrian vivir en un solo archivo, es decir unificar los archivos: 
        - `RegisterCommand.cs` 
        - `RegisterHandler.cs` 
        - `RegisterValidator.cs` 
        - `RegisterResponse.cs`
    2. Como son modelos simples nos podriamos ahorrar completamente el proyecto de Models.
    3. Justificame la existencia del proyecto de `Infrastructure` y como seria la intedependencia de los proyectos (contemplando la eliminacion de `Models` y la no eliminacion)

---

## Respuesta — Ronda 2

### 1. Un slice = un archivo

Sin MediatR, el patrón más limpio es agrupar todo en una **static class contenedora** con nested types. El archivo se llama igual que la operación:

```csharp
// Features/Releases/CreateRelease.cs
public static class CreateRelease
{
    public record Command(
        int ArtistProfileId,
        string Title,
        DateOnly ReleaseDate,
        string? Genre,
        string? Label,
        string? UPC,
        string? ISRC,
        string? LinksJson
    );

    public record Response(Guid Id, int ArtistProfileId, string Title, DateOnly ReleaseDate);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ArtistProfileId).GreaterThan(0);
        }
    }

    public class Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
        {
            var release = new Release
            {
                ArtistProfileId = cmd.ArtistProfileId,
                Title = cmd.Title,
                ReleaseDate = cmd.ReleaseDate,
                Genre = cmd.Genre,
                Label = cmd.Label,
                UPC = cmd.UPC,
                ISRC = cmd.ISRC,
                LinksJson = cmd.LinksJson,
                CreatedBy = currentUser.UserId!
            };

            db.Releases.Add(release);
            await db.SaveChangesAsync(ct);

            return new Response(release.Id, release.ArtistProfileId, release.Title, release.ReleaseDate);
        }
    }
}
```

```csharp
// Controllers/ReleasesController.cs
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateRelease.Command cmd,
    [FromServices] CreateRelease.Handler handler)
{
    var result = await handler.HandleAsync(cmd);
    return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
}
```

Ventajas de este enfoque:
- Un archivo por operación — la carpeta `CreateRelease/` ya no existe, solo `CreateRelease.cs`
- Los tipos `CreateRelease.Command`, `CreateRelease.Response` son auto-documentados
- Sin MediatR, sin `IRequest<T>`, sin infraestructura de dispatching
- El Handler se registra como `services.AddScoped<CreateRelease.Handler>()` en `Program.cs` (o con un scanner automático tipo Scrutor)

La estructura de carpetas queda así:

```
Features/
├── Auth/
│   ├── Login.cs
│   └── Register.cs
├── Releases/
│   ├── GetReleases.cs
│   ├── GetReleaseById.cs
│   ├── CreateRelease.cs
│   ├── UpdateRelease.cs
│   └── DeleteRelease.cs
├── ArtistProfiles/
├── Tracks/
├── Videos/
├── Teams/
└── Stats/
    ├── GetStatsSummary.cs
    └── Workers/
        └── StreamStatAggregatorWorker.cs
```

Un folder = un dominio. Un archivo = una operación. Limpio.

### 2. El problema de eliminar el proyecto `Models`

Si eliminamos `Models`, las entidades tienen que vivir en alguno de los proyectos restantes. Acá aparece un problema concreto de **dependencia circular**:

**Intento: entidades en el proyecto API**
```
PublishRealLiteApi (entidades + Features + Controllers)
    ↑ referenciado por
PublishRealLiteApi.Infrastructure (DbContext necesita las entidades)
    ↑ referenciado por
PublishRealLiteApi ← CIRCULAR. No compila.
```

**Intento: entidades en Infrastructure**
```
PublishRealLiteApi.Infrastructure (AppDbContext + Migrations + Storage + Entidades)
    ↑ referenciado por
PublishRealLiteApi (Features + Controllers)
```
Funciona sin circular, pero las entidades quedan acopladas a la capa de datos. `Release`, `Track`, `Team` son conceptos de dominio — no de infraestructura. Semánticamente incómodo, aunque pragmáticamente válido para un proyecto de este tamaño.

**Las tres opciones reales:**

| Opción | Proyectos | Dependencias | Trade-off |
|---|---|---|---|
| A — Mantener Models | 3 (API + Infra + Models) | `Models` ← `Infra` ← `API` | +1 proyecto liviano, dependencias limpias |
| B — Entidades en Infrastructure | 2 (API + Infra) | `Infra` ← `API` | Entidades acopladas a infra, pero sin overhead |
| C — Todo en un proyecto | 1 | — | Máxima simplicidad, sin separación física |

**Mi recomendación**: para este proyecto, **Opción A** (mantener Models como proyecto ultra-liviano, solo entidades POCO sin ninguna dependencia de framework). El overhead de un `.csproj` extra es mínimo y evita el problema semántico de las entidades en Infrastructure. Si en algún momento el dominio crece y las entidades necesitan lógica, ya tienen su lugar natural.

Opción B es aceptable si el equipo prefiere menos proyectos y acepta el trade-off semántico.

### 3. Justificación del proyecto `Infrastructure`

Sin MediatR y con el proyecto Application eliminado, `Infrastructure` concentra todo lo que tiene **dependencias de frameworks externos**:

**¿Qué vive en Infrastructure?**

```
PublishRealLiteApi.Infrastructure/
├── Data/
│   ├── AppDbContext.cs          ← EF Core + SQL Server (Microsoft.EntityFrameworkCore.SqlServer)
│   ├── DbSeeder.cs              ← lógica de seed inicial
│   └── Migrations/              ← artefactos generados por EF Core
├── Storage/
│   ├── IStorageService.cs       ← interfaz (cross-cutting concern)
│   ├── LocalStorageService.cs   ← System.IO
│   └── AzureBlobStorageService.cs ← Azure.Storage.Blobs
└── DatabaseHealthCheck.cs       ← Microsoft.Extensions.Diagnostics.HealthChecks
```

**¿Por qué no mezclar esto con el proyecto API?**

Porque `PublishRealLiteApi.csproj` no debería referenciar `Microsoft.EntityFrameworkCore.SqlServer` ni `Azure.Storage.Blobs`. Esas son dependencias de infraestructura — si mañana migrás de SQL Server a PostgreSQL, o de Azure Blob a S3, el cambio está aislado en Infrastructure. El proyecto API no sabe qué motor de base de datos ni qué proveedor de storage estás usando.

Esta separación también facilita testing: los tests pueden referenciar solo el proyecto API y usar un `AppDbContext` con SQLite in-memory sin necesidad de tener el driver de SQL Server instalado.

### Grafo de dependencias final

**Con Models (Opción A — recomendada):**
```
PublishRealLiteApi.Models
    ↑
PublishRealLiteApi.Infrastructure  →  (EF Core, SQL Server, Azure Blob)
    ↑
PublishRealLiteApi                 →  (Features, Controllers, Middleware)
```

**Sin Models (Opción B):**
```
PublishRealLiteApi.Infrastructure  →  (EF Core, SQL Server, Azure Blob, Entidades)
    ↑
PublishRealLiteApi                 →  (Features, Controllers, Middleware)
```

En ambos casos, el flujo de dependencias es **unidireccional** — nunca hay ciclos. La diferencia es solo dónde viven las entidades.

## Instruccion — Ronda 2

1. En la simplificacion de la clase de todo en uno. Creo que no estas tomando en cuenta, en que momento vas aplicar la validacion.

2. Si movemos Models al Api, como quedaria las dependencias y ¿sobre todo es posible? y como.

---

## Respuesta — Ronda 3

### 1. La validación — el punto ciego del ejemplo anterior

Tenés razón. Definí el `Validator` pero nunca lo invoqué. El ejemplo compilaría y funcionaría, pero la validación nunca se ejecutaría. Hay tres opciones reales sin MediatR:

---

**Opción A — FluentValidation auto-validation (recomendada)**

Registrar el paquete `FluentValidation.AspNetCore` y activar la integración automática con el pipeline de model binding:

```csharp
// Program.cs
builder.Services
    .AddFluentValidationAutoValidation()
    .AddValidatorsFromAssemblyContaining<CreateRelease.Validator>();
```

Con esto, ASP.NET Core valida el `Command` antes de que el controller action se ejecute. Si la validación falla, devuelve un `400 Bad Request` con los errores automáticamente. El Handler no sabe nada de validación y el controller queda limpio:

```csharp
[Authorize]
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateRelease.Command cmd,        // ← validado automáticamente antes de llegar acá
    [FromServices] CreateRelease.Handler handler)
{
    var result = await handler.HandleAsync(cmd);
    return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
}
```

La validación ocurre en el pipeline de ASP.NET Core, igual que `[Required]` pero con FluentValidation. Sin código extra por slice.

---

**Opción B — Inyectar `IValidator<T>` en el Handler**

Si no querés depender del auto-validation, el Handler puede validar explícitamente:

```csharp
public class Handler(AppDbContext db, ICurrentUserService currentUser, IValidator<Command> validator)
{
    public async Task<Response> HandleAsync(Command cmd, CancellationToken ct = default)
    {
        await validator.ValidateAndThrowAsync(cmd, ct);  // lanza ValidationException si falla
        // ... lógica de negocio
    }
}
```

Más explícito, pero necesitás un exception middleware para convertir `ValidationException` en `400 Bad Request`. Más boilerplate que la Opción A resuelve sola.

---

**Opción C — Validar en el controller (explícito)**

```csharp
public async Task<IActionResult> Create(
    [FromBody] CreateRelease.Command cmd,
    [FromServices] CreateRelease.Handler handler,
    [FromServices] CreateRelease.Validator validator)
{
    var result = await validator.ValidateAsync(cmd);
    if (!result.IsValid)
        return BadRequest(result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

    var response = await handler.HandleAsync(cmd);
    return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
}
```

Demasiado verboso. Repetís el mismo bloque de validación en cada action.

---

**Conclusión**: Opción A. Un `AddFluentValidationAutoValidation()` en `Program.cs` y `AddValidatorsFromAssemblyContaining<T>()` para el scanner automático. Todos los `Validator` del proyecto quedan registrados y ejecutados sin código extra.

---

### 2. Mover Models al proyecto API — ¿es posible?

**Directamente: no.** Si `Release`, `Track`, etc. viven en el proyecto API, Infrastructure necesita referenciar API para poder definir `DbSet<Release>` en el `AppDbContext`. Al mismo tiempo, API necesita referenciar Infrastructure para registrar el `AppDbContext` en el DI. Resultado: referencia circular, no compila.

**Indirectamente: sí, pero requiere un movimiento adicional.**

La pregunta clave es: ¿por qué Infrastructure necesita las entidades? Solo por `AppDbContext` y las `Migrations`. Si `AppDbContext` y `Migrations` también se mueven al proyecto API, Infrastructure deja de necesitar las entidades y la dependencia circular desaparece.

La reestructuración sería:

```
PublishRealLiteApi/
├── Features/          ← slices
├── Controllers/
├── Data/
│   ├── AppDbContext.cs          ← se mueve acá desde Infrastructure
│   ├── DbSeeder.cs              ← se mueve acá
│   └── Migrations/              ← se mueven acá
├── Models/                      ← entidades (antes eran proyecto separado)
│   ├── Release.cs
│   ├── Track.cs
│   └── ...
└── Program.cs

PublishRealLiteApi.Infrastructure/
├── Storage/
│   ├── IStorageService.cs
│   ├── LocalStorageService.cs
│   └── AzureBlobStorageService.cs
└── (Email, otros externos que no necesiten entidades)
```

Infrastructure ahora solo contiene **integraciones externas que no necesitan entidades**: storage (Azure Blob, local filesystem), email, etc. No referencia `Release` ni ninguna entidad. No tiene EF Core.

El grafo de dependencias resultante:

```
PublishRealLiteApi.Infrastructure  →  (Azure.Storage.Blobs, etc.)
    ↑ referenciado por
PublishRealLiteApi                 →  (EF Core SqlServer, Features, Controllers, Entidades, DbContext, Migrations)
```

Sin ciclos. API referencia Infrastructure para obtener los servicios de storage. Infrastructure no sabe nada de las entidades.

**Trade-off de esta opción:**

| | Models separado (Opción A previa) | Entidades + DbContext en API |
|---|---|---|
| Proyectos | 3 | 2 |
| EF Core SqlServer en | Infrastructure | **API** |
| Migrations generadas en | Infrastructure | **API** |
| Cambiar de SQL Server a PostgreSQL | Solo tocar Infrastructure | Tocar API |
| Separación de concerns | Más clara | API más acoplado a infra de datos |

El costo es que el proyecto API pasa a tener `Microsoft.EntityFrameworkCore.SqlServer` como dependencia directa. Para un proyecto de este tamaño, es un trade-off completamente aceptable.

**¿Cuándo elegir cada una?**

- Separación estricta de infra de datos → Opción A (mantener Models, Infrastructure con EF Core)
- Menos proyectos, aceptando que API "sabe" de SQL Server → mover todo al API, Infrastructure solo para external services
- Máxima simplicidad → un solo proyecto


## Instruccion — Ronda 3

- Vamos a dejar los 3 proyectos. me parecen razonable.
- Vamos a crear un nuevo branch para esto cambio, colocale `001-VerticalApproach`.

## Changes Approved By the User