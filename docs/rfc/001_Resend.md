# Resend email.

Alcance: Vamos a utilizar Resend (https://resend.com/) para el envio de correos. 
- Identifica los flujos que generan email actualmente. 
- Describe un plan acerca de los cambios que necesitas realizar para implementar Resend.
- Agrega dudas/sugerencias que tengas al respecto.


Nota: 
- Este documento contiene interacciones entre humano/agente. Por eso pueden existir multiple ronda de Respuestas (para los resultados de la IA) y Instruccion (para los comentarios del humano).
- Por tanto, Agrega una session de Respuesta en este documento, para tu respuesta.

---

## Respuesta 1

### Flujos que actualmente generan email

Encontré **dos flujos**, uno activo y uno incompleto:

#### 1. Invitación de artista (ACTIVO)
`ArtistProfileService.CreateWithAdminCodeAsync` — cuando un admin crea un perfil de artista para otro usuario:
1. Crea el `IdentityUser` con el email recibido
2. Crea el `ArtistProfile` vinculado
3. Genera un token de reset de password con `UserManager`
4. Construye el invite link: `{AppUrl}/auth/set-password?userId={id}&token={encodedToken}`
5. Llama a `IEmailService.SendInvitationEmailAsync(email, artistName, inviteLink)`

**Archivo relevante**: `src/PublishRealLiteApi.Application/Services/ArtistProfileService.cs` (líneas 98–108)

#### 2. Invitación a equipo (INCOMPLETO)
`TeamService.InviteAsync` genera el token y lo guarda en la base de datos, pero hay un comentario explícito:
```
// envío de email queda en capa API o servicio de infraestructura
```
El email **nunca se envía**. El `TeamsController.Invite` tampoco lo envía.

**Archivo relevante**: `src/PublishRealLiteApi.Application/Services/TeamService.cs` (líneas 27–33)

---

### Implementación actual

- Clase: `EmailService.cs` en capa `Application` (incorrecto arquitectónicamente — debería estar en `Infrastructure`)
- Usa `System.Net.Mail.SmtpClient` con credenciales de `appsettings.json`
- Config keys: `EmailSettings:Host`, `EmailSettings:Port`, `EmailSettings:Username`, `EmailSettings:Password`, `EmailSettings:FromEmail`
- El `appsettings.json` solo tiene valores placeholder — nunca fue configurado para producción

---

### Plan de implementación

#### Paso 1 — Agregar el SDK de Resend

```bash
dotnet add src/PublishRealLiteApi.Infrastructure package Resend
```

El SDK oficial de Resend para .NET expone `IResend` y se registra con `services.AddResend(...)`.

#### Paso 2 — Crear `ResendEmailService` en Infrastructure

Reemplazar la implementación concreta. La interfaz `IEmailService` se queda en `Application` (correcto). La implementación se mueve a `Infrastructure` porque es una dependencia de infraestructura externa.

```
src/PublishRealLiteApi.Infrastructure/
  Services/
    ResendEmailService.cs   ← nueva clase
```

Implementación:
```csharp
public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromEmail;

    public ResendEmailService(IResend resend, IConfiguration config)
    {
        _resend = resend;
        _fromEmail = config["Resend:FromEmail"] ?? "noreply@publishreal.com";
    }

    public async Task SendInvitationEmailAsync(string email, string artistName, string inviteLink)
    {
        var message = new EmailMessage
        {
            From = _fromEmail,
            To = { email },
            Subject = "Welcome to PublishReal - Complete Your Profile",
            HtmlBody = $"""
                <h1>Hello, {artistName}!</h1>
                <p>An administrator has created an artist profile for you on PublishReal.</p>
                <p><a href='{inviteLink}'>Set Your Password & Claim Account</a></p>
                <p>If you did not expect this invitation, please ignore this email.</p>
                """
        };

        await _resend.EmailSendAsync(message);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new EmailMessage
        {
            From = _fromEmail,
            To = { email },
            Subject = subject,
            HtmlBody = htmlMessage
        };

        await _resend.EmailSendAsync(message);
    }
}
```

#### Paso 3 — Completar el flujo de Team Invite

`TeamService.InviteAsync` necesita:
- Recibir `IEmailService` por constructor
- Construir el invite link: `{AppUrl}/invite/{token}`
- Llamar a `SendEmailAsync` con el link

Requiere inyectar `IConfiguration` (o un `AppSettings` tipado) en `TeamService` para construir la URL.

#### Paso 4 — Actualizar `DependencyInjection.cs`

```csharp
// Reemplazar:
services.AddScoped<IEmailService, EmailService>();

// Por:
services.AddResend(options =>
{
    options.ApiToken = config["Resend:ApiKey"]!;
});
services.AddScoped<IEmailService, ResendEmailService>();
```

#### Paso 5 — Actualizar `appsettings.json`

Eliminar el bloque `EmailSettings` y agregar:
```json
"Resend": {
  "ApiKey": "",
  "FromEmail": "noreply@publishreal.com"
}
```

La `ApiKey` **no va en appsettings** — va en `appsettings.Development.json` (gitignored) o en User Secrets / variables de entorno para producción.

#### Paso 6 — Eliminar `EmailService.cs` (el antiguo)

Una vez que `ResendEmailService` esté operativo, borrar `src/PublishRealLiteApi.Application/Services/EmailService.cs`.

---

### Archivos a modificar/crear

| Acción | Archivo |
|--------|---------|
| CREAR | `src/PublishRealLiteApi.Infrastructure/Services/ResendEmailService.cs` |
| MODIFICAR | `src/PublishRealLiteApi.Infrastructure/DependencyInjection.cs` |
| MODIFICAR | `src/PublishRealLiteApi.Application/Services/TeamService.cs` |
| MODIFICAR | `src/PublishRealLiteApi/appsettings.json` |
| ELIMINAR | `src/PublishRealLiteApi.Application/Services/EmailService.cs` |

---

### Dudas y sugerencias

1. **Dominio verificado en Resend**: El `from` de los emails debe ser un dominio verificado en la plataforma de Resend. ¿El dominio `publishreal.com` ya existe y está verificado en la cuenta de Resend? Si no, durante desarrollo se puede usar el dominio de prueba de Resend (`onboarding@resend.dev`).

2. **Team Invite — formato del link**: Para completar el flujo de equipo, necesito saber cuál es la URL del frontend para aceptar invitaciones. Actualmente el frontend tiene `(auth)/invite/[token]/page.tsx`, entonces el link sería `{AppUrl}/auth/invite/{token}` — ¿es correcto?

3. **Mover `EmailService` a Infrastructure**: Técnicamente, la clase concreta de email debería estar en `Infrastructure` (depende de un servicio externo). ¿Confirmamos que movemos ahí `ResendEmailService`? El código actual tiene la implementación SMTP en `Application`, lo cual viola la arquitectura limpia.

4. **Templates HTML**: Los emails actuales son HTML inline básico. ¿Queremos mantener ese approach o aprovechamos que Resend soporta templates? Para esta iteración recomiendo mantenerlo simple (HTML inline) y dejar los templates para después.

## Instruccion 1

1. utiliza el de prueba de resend.
2. correcto.
3. Muevelo a `Infrastructure`
4. Dejalo simple (HTML Inline).

Si tienes mas duda/sugerencia, dejame saber.

---

## Respuesta 2

Implementación completa. Build: **0 errores**.

### Cambios realizados

#### CREADO — `src/PublishRealLiteApi.Infrastructure/Services/ResendEmailService.cs`
Implementación concreta usando el SDK de Resend (v0.5.1). Usa `onboarding@resend.dev` como `From` (dominio de prueba de Resend). Implementa ambos métodos de `IEmailService`.

#### MODIFICADO — `src/PublishRealLiteApi.Infrastructure/DependencyInjection.cs`
Reemplazado `services.AddScoped<IEmailService, EmailService>()` por:
```csharp
services.AddResend(options => { options.ApiToken = config["Resend:ApiKey"] ?? string.Empty; });
services.AddScoped<IEmailService, ResendEmailService>();
```

#### MODIFICADO — `src/PublishRealLiteApi.Application/Services/TeamService.cs`
- El repositorio de invites dejó de ser opcional (era `ITeamInviteRepository?`) — ahora es requerido
- Inyectado `IEmailService` y `IConfiguration` por constructor
- `InviteAsync` ahora envía el email con el link `{AppUrl}/invite/{encodedToken}`
- Eliminado el guard `if (_teamInviteRepository == null)` redundante en `AcceptInviteAsync`

#### MODIFICADO — `src/PublishRealLiteApi/appsettings.json`
Eliminado el bloque `EmailSettings` (SMTP). Reemplazado por:
```json
"Resend": {
  "ApiKey": "",
  "FromEmail": "onboarding@resend.dev"
}
```
⚠️ La `ApiKey` real **no va en este archivo** — va en `appsettings.Development.json` (gitignored) o en variables de entorno en producción.

#### ELIMINADO — `src/PublishRealLiteApi.Application/Services/EmailService.cs`
Implementación SMTP antigua eliminada.

---

### Nota sobre la URL del team invite

En el RFC mencioné `/auth/invite/{token}` pero en Next.js App Router, los route groups entre paréntesis como `(auth)` **no aparecen en la URL**. El archivo `(auth)/invite/[token]/page.tsx` corresponde a la ruta `/invite/{token}`. Por eso implementé `{AppUrl}/invite/{encodedToken}`.

---

### Próximo paso

Para usar en desarrollo: agregar la API key real de Resend en `appsettings.Development.json`:
```json
{
  "Resend": {
    "ApiKey": "re_xxxxxxxxxxxx"
  }
}
```