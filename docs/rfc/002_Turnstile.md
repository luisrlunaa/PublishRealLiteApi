# Turnstile.

Alcance: Vamos a utilizar Turnstile para validar los formularios publicos.
- Identifica los flujos que tienen formularios publicos. 
- Describe un plan acerca de los cambios que necesitas realizar para implementar Turnstile.
- Agrega dudas/sugerencias que tengas al respecto.



Nota: 
- Este documento contiene interacciones entre humano/agente. Por eso pueden existir multiple ronda de Respuestas (para los resultados de la IA) y Instruccion (para los comentarios del humano).
- Por tanto, Agrega una session de Respuesta en este documento, para tu respuesta.
- No hagas cambios de codigo hasta que haya una session al final que `## Changes Approved By the User`
- En cualquier ronda de Respuesta puedes agregar dudas/sugerencias que tengas al respecto.

---

## Respuesta 1

### 1. Flujos con formularios públicos

Hay **3 flujos** con formularios públicos identificados en `PublishRealLite/app/(auth)/`:

| Ruta | Archivo | Campos | Destino |
|------|---------|--------|---------|
| `/login` | `(auth)/login/page.tsx` | Email, Password | `POST /api/Auth/login` |
| `/register` | `(auth)/register/page.tsx` | Email, Password, ConfirmPassword, AdminCode (opcional) | `POST /api/Auth/register` |
| `/apply` | `(auth)/apply/page.tsx` | 9 campos (nombre artístico, email, país, Instagram, rol, canciones, derechos) | Formspree (externo) |

La ruta `/invite/[token]` NO es un formulario — ejecuta una acción automáticamente al cargar la página, no tiene inputs del usuario.

---

### 2. Plan de implementación

#### Stack involucrado
- **Frontend**: Next.js (React) — renderiza el widget de Turnstile y obtiene el token
- **Backend**: ASP.NET Core — valida el token contra la API de Cloudflare antes de procesar la request

El flujo es siempre el mismo: el usuario completa el formulario → Turnstile genera un token en el cliente → el token viaja en el body del request → el backend valida el token con Cloudflare → si es válido, procesa; si no, rechaza con `400`.

---

#### Frontend — cambios necesarios

**Paso 1: Instalar el paquete**
```bash
pnpm add @marsidev/react-turnstile
```

**Paso 2: Agregar variable de entorno**
```env
NEXT_PUBLIC_TURNSTILE_SITE_KEY=<site_key_de_cloudflare>
```

**Paso 3: Crear un componente `TurnstileWidget`**

Un wrapper simple sobre `@marsidev/react-turnstile` que expone el token vía `onSuccess`. Opcional pero recomendado para no repetir configuración en los 3 formularios.

**Paso 4: Modificar cada formulario**

En cada uno de los 3 formularios:
- Agregar el widget de Turnstile debajo del último campo
- Guardar el token en estado local (`useState`)
- Incluir `turnstileToken` en el payload del submit
- Deshabilitar el botón de submit hasta que el token esté presente
- Manejar el caso de expiración del token (Turnstile llama `onExpire` — limpiar el estado y el widget se auto-reinicia)

El formulario de `/apply` envía a **Formspree**, no a nuestra API. En este caso hay dos opciones (ver sección de dudas).

---

#### Backend — cambios necesarios

**Paso 1: Agregar configuración**
En `appsettings.json`:
```json
"Turnstile": {
  "SecretKey": "<secret_key_de_cloudflare>",
  "VerifyUrl": "https://challenges.cloudflare.com/turnstile/v0/siteverify"
}
```

**Paso 2: Crear `TurnstileService`** en `Application`

Un servicio que hace `POST` a la URL de verificación con `secret` + `response` (el token). Cloudflare devuelve `{ "success": true/false }`.

```csharp
public interface ITurnstileService
{
    Task<bool> ValidateAsync(string token);
}
```

**Paso 3: Modificar los DTOs de Auth**

Agregar `TurnstileToken` a los request bodies que corresponden:
- `LoginRequest`
- `RegisterRequest`

**Paso 4: Validar en los controllers**

En `AuthController`, antes de llamar al servicio de negocio:
```csharp
if (!await _turnstileService.ValidateAsync(request.TurnstileToken))
    return BadRequest("Invalid CAPTCHA token.");
```

Aplica a: `POST /api/Auth/login` y `POST /api/Auth/register`.

---

### 3. Dudas y sugerencias

**Duda 1 — ¿Qué hacemos con `/apply`?**

El formulario de Apply envía a Formspree (externo), no a nuestro backend. Hay dos caminos:
- **Opción A**: Validar el token de Turnstile en el cliente antes de hacer el submit a Formspree (solo client-side, no hay verificación server-side). Es menor protección pero algo es algo.
- **Opción B**: Migrar `/apply` a un endpoint propio (`POST /api/Applications`) y pasar por la misma validación server-side que Login y Register. Esto también nos daría persistencia de las solicitudes en nuestra DB.

Sugiero la **Opción B** — ya que tenemos el backend, guardar las applications ahí es lo correcto a largo plazo.

**Duda 2 — ¿Environment de desarrollo?**

Cloudflare provee una `siteKey` especial para testing (`1x00000000000000000000AA`) que siempre pasa. Hay que asegurarse de usar esa key en `.env.local` para no bloquear el flujo de desarrollo.

**Sugerencia — No exponer el error de Turnstile al usuario**

Si el token es inválido, el backend debería devolver un error genérico (ej. "Algo salió mal, intentá de nuevo") y no revelar que fue el CAPTCHA lo que falló. Dar esa información facilita el bypass.

**Sugerencia — Dónde poner `TurnstileService`**

El servicio de validación debería vivir en `Application` (capa de negocio), no en el Controller ni en Infrastructure, ya que es lógica de validación de entrada, no acceso a datos. El `HttpClient` que usa internamente se inyecta vía DI.


## Instruccion 1

1. Vamos aplicar la opcion B. Agrega en tu proxima respuesta el plan.
2. Vamos usar ese especial para testing.
3. Aplica ambas sugerencias.

---

## Respuesta 2

### Decisiones incorporadas

- `/apply` → **Opción B**: se migra a `POST /api/Applications` (backend propio, con validación server-side de Turnstile).
- Dev key: `NEXT_PUBLIC_TURNSTILE_SITE_KEY=1x00000000000000000000AA` en `.env.local`.
- Error genérico al usuario cuando falla Turnstile (no revelar causa).
- `TurnstileService` vive en la capa `Application`.

---

### Plan completo actualizado

#### Backend

**1. Nuevo modelo de dominio** — `PublishRealLiteApi.Models`

```csharp
// Models/ArtistApplication.cs
public class ArtistApplication
{
    public int Id { get; set; }
    public string ArtistName { get; set; }
    public string Email { get; set; }
    public string Country { get; set; }
    public string InstagramUrl { get; set; }
    public string Role { get; set; }           // "Compositor" | "Artista" | "Ambos"
    public string? SongAsComposerUrl { get; set; }
    public string? SongAsArtistUrl { get; set; }
    public bool AffiliatedWithPro { get; set; }
    public string OwnershipType { get; set; }  // "Total" | "Parcial"
    public bool InterestedInSigning { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**2. Repository interface** — `PublishRealLiteApi.Application`

```csharp
// Application/Interfaces/IArtistApplicationRepository.cs
public interface IArtistApplicationRepository
{
    Task AddAsync(ArtistApplication application);
}
```

**3. DTO de request** — `PublishRealLiteApi.Application`

```csharp
// Application/DTOs/SubmitApplicationRequest.cs
public record SubmitApplicationRequest(
    string ArtistName,
    string Email,
    string Country,
    string InstagramUrl,
    string Role,
    string? SongAsComposerUrl,
    string? SongAsArtistUrl,
    bool AffiliatedWithPro,
    string OwnershipType,
    bool InterestedInSigning,
    string TurnstileToken  // requerido para todos los formularios
);
```

**4. `TurnstileService`** — `PublishRealLiteApi.Application`

```csharp
// Application/Services/TurnstileService.cs
public interface ITurnstileService
{
    Task<bool> ValidateAsync(string token);
}

public class TurnstileService : ITurnstileService
{
    private readonly HttpClient _http;
    private readonly string _secretKey;

    public TurnstileService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _secretKey = config["Turnstile:SecretKey"]!;
    }

    public async Task<bool> ValidateAsync(string token)
    {
        var response = await _http.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _secretKey,
                ["response"] = token
            })
        );
        var json = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
        return json?.Success ?? false;
    }
}

// record interno
private record TurnstileResponse([property: JsonPropertyName("success")] bool Success);
```

**5. `ApplicationService`** — `PublishRealLiteApi.Application`

```csharp
public class ApplicationService
{
    private readonly IArtistApplicationRepository _repo;
    private readonly ITurnstileService _turnstile;

    public async Task<Result> SubmitAsync(SubmitApplicationRequest request)
    {
        if (!await _turnstile.ValidateAsync(request.TurnstileToken))
            return Result.Fail("validation_error"); // error genérico — no mencionar CAPTCHA

        var application = new ArtistApplication { /* mapear campos */ };
        await _repo.AddAsync(application);
        return Result.Ok();
    }
}
```

> El controller devuelve al cliente un mensaje genérico ("Something went wrong, please try again") — nunca "CAPTCHA failed". Lo mismo aplica para Login y Register.

**6. `ApplicationController`** — `PublishRealLiteApi`

```csharp
[Route("api/[controller]")]
[ApiController]
public class ApplicationsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(SubmitApplicationRequest request)
    {
        var result = await _applicationService.SubmitAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = "Something went wrong, please try again." });
        return Ok();
    }
}
```

**7. Modificar `AuthController`** para Login y Register

Agregar `TurnstileToken` a `LoginRequest` y `RegisterRequest`, y validar antes de procesar:

```csharp
if (!await _turnstileService.ValidateAsync(request.TurnstileToken))
    return BadRequest(new { message = "Something went wrong, please try again." });
```

**8. Configuración**

`appsettings.json`:
```json
"Turnstile": {
  "SecretKey": "<secret_key_produccion>"
}
```

`appsettings.Development.json`:
```json
"Turnstile": {
  "SecretKey": "1x0000000000000000000000000000000AA"
}
```

> La secret key de testing de Cloudflare para el servidor es `1x0000000000000000000000000000000AA` (par de la siteKey `1x00000000000000000000AA`).

**9. Registro de DI** — `Program.cs`

```csharp
builder.Services.AddHttpClient<ITurnstileService, TurnstileService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<IArtistApplicationRepository, ArtistApplicationRepository>();
```

**10. EF Core migration**

```bash
dotnet ef migrations add AddArtistApplication \
  --project PublishRealLiteApi.Infrastructure \
  --startup-project PublishRealLiteApi
```

---

#### Frontend

**1. Variables de entorno**

`.env.local` (desarrollo):
```env
NEXT_PUBLIC_TURNSTILE_SITE_KEY=1x00000000000000000000AA
```

`.env.production` (producción):
```env
NEXT_PUBLIC_TURNSTILE_SITE_KEY=<site_key_real_de_cloudflare>
```

**2. Instalar paquete**

```bash
pnpm add @marsidev/react-turnstile
```

**3. Componente `TurnstileWidget`** — `components/ui/turnstile-widget.tsx`

```tsx
import { Turnstile } from "@marsidev/react-turnstile";

interface TurnstileWidgetProps {
  onSuccess: (token: string) => void;
  onExpire?: () => void;
}

export function TurnstileWidget({ onSuccess, onExpire }: TurnstileWidgetProps) {
  return (
    <Turnstile
      siteKey={process.env.NEXT_PUBLIC_TURNSTILE_SITE_KEY!}
      onSuccess={onSuccess}
      onExpire={onExpire}
    />
  );
}
```

**4. Modificar `/login` y `/register`**

- Agregar `TurnstileWidget` debajo del último campo
- Guardar token en `useState<string | null>(null)`
- Deshabilitar submit si `token === null`
- Incluir `turnstileToken` en el payload del request
- Limpiar token en `onExpire`

**5. Migrar `/apply` de Formspree a `POST /api/Applications`**

- Reemplazar `fetch("https://formspree.io/f/mkokqnej", ...)` por `apiClient.submitApplication(data)`
- Agregar método `submitApplication` en `lib/api/client.ts`
- Agregar tipo `SubmitApplicationRequest` en `lib/api/types.ts`
- Agregar `TurnstileWidget` al formulario igual que Login/Register

---

### Resumen de archivos afectados

| Capa | Archivo | Acción |
|------|---------|--------|
| Models | `Models/ArtistApplication.cs` | Crear |
| Application | `Application/Interfaces/IArtistApplicationRepository.cs` | Crear |
| Application | `Application/DTOs/SubmitApplicationRequest.cs` | Crear |
| Application | `Application/Services/TurnstileService.cs` | Crear |
| Application | `Application/Services/ApplicationService.cs` | Crear |
| Application | `Application/DTOs/LoginRequest.cs` | Modificar (agregar TurnstileToken) |
| Application | `Application/DTOs/RegisterRequest.cs` | Modificar (agregar TurnstileToken) |
| Infrastructure | `Infrastructure/Repositories/ArtistApplicationRepository.cs` | Crear |
| Infrastructure | `Infrastructure/Migrations/` | Nueva migración |
| API | `PublishRealLiteApi/Controllers/ApplicationsController.cs` | Crear |
| API | `PublishRealLiteApi/Controllers/AuthController.cs` | Modificar (validar Turnstile) |
| API | `PublishRealLiteApi/Program.cs` | Modificar (registrar DI) |
| API | `appsettings.json` / `appsettings.Development.json` | Modificar (agregar config) |
| Frontend | `components/ui/turnstile-widget.tsx` | Crear |
| Frontend | `app/(auth)/login/page.tsx` | Modificar |
| Frontend | `app/(auth)/register/page.tsx` | Modificar |
| Frontend | `app/(auth)/apply/page.tsx` | Modificar (migrar de Formspree) |
| Frontend | `lib/api/client.ts` | Modificar (agregar submitApplication) |
| Frontend | `lib/api/types.ts` | Modificar (agregar tipos) |
| Frontend | `.env.local` / `.env.production` | Modificar (agregar siteKey) |


## Changes Approved By the User