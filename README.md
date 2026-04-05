# 🎵 PublishRealLiteApi  
### La infraestructura que empodera a artistas independientes con datos, control y autonomía.

PublishRealLiteApi es la base tecnológica de una plataforma moderna diseñada para resolver uno de los mayores problemas de los artistas independientes:  
**la falta de un panel centralizado donde puedan ver su música, estadísticas, ingresos y equipo en un solo lugar.**

Inspirado en plataformas como **DistroKid**, **Spotify for Artists** y **UnitedMasters**, este backend ofrece una arquitectura sólida, escalable y lista para integrarse con un frontend profesional.

---

# 🌟 Visión del Proyecto

Crear un ecosistema donde cualquier artista —sin importar su tamaño o presupuesto— pueda:

- Controlar su identidad artística  
- Administrar su catálogo musical  
- Subir contenido promocional  
- Analizar su crecimiento con métricas reales  
- Gestionar su equipo de trabajo  
- Tomar decisiones basadas en datos  

Todo desde una sola plataforma.

La misión es **democratizar el acceso a herramientas profesionales**, permitiendo que artistas emergentes tengan el mismo nivel de control que un artista firmado con un sello discográfico.

---

# 💼 ¿Qué problema resuelve?

Hoy en día, los artistas deben usar múltiples plataformas:

- Una para distribuir música  
- Otra para ver estadísticas  
- Otra para manejar su equipo  
- Otra para contenido promocional  
- Otra para almacenar archivos  

Esto fragmenta su operación, complica su crecimiento y limita su capacidad de tomar decisiones.

**PublishRealLiteApi unifica todo en un solo backend.**

---

# 🚀 ¿Qué ofrece la plataforma?

## ✔ Gestión de Perfil Artístico
Cada artista tiene un perfil profesional que centraliza:

- Identidad  
- Lanzamientos  
- Videos  
- Equipo  
- Estadísticas  

---

## ✔ Módulo de Música (Releases & Tracks)
Permite a los artistas:

- Crear y administrar lanzamientos  
- Subir portadas  
- Organizar canciones  
- Mantener un catálogo limpio y profesional  

Ideal para integrarse con un frontend tipo **DistroKid MyMusic**.

---

## ✔ Módulo de Videos Promocionales
Los artistas pueden:

- Subir videos  
- Organizar contenido promocional  
- Mostrar clips, trailers o reels  

Perfecto para dashboards modernos.

---

## ✔ Módulo de Estadísticas (StreamStats)
Un sistema tipo **Spotify for Artists**, donde el artista puede ver:

- Streams por plataforma  
- Streams por país  
- Tendencias por fecha  
- Métricas clave  

Esto permite decisiones basadas en datos reales.

---

## ✔ Módulo de Equipos (Teams)
Los artistas pueden:

- Crear su equipo  
- Invitar managers, editores o colaboradores  
- Asignar roles y permisos  
- Gestionar participación y acceso  

Un sistema similar al de **DistroKid Teams**.

---

# 🧩 Arquitectura Técnica

- **ASP.NET Core 10**  
- **Entity Framework Core (Code First)**  
- **Identity + JWT** para autenticación segura  
- **SQL Server** como base de datos  
- **Controladores REST** para cada módulo  
- **Servicios desacoplados** para lógica de negocio  
- **DTOs** para transporte limpio de datos  
- **Swagger / Scalar** para documentación interactiva  

La arquitectura está diseñada para escalar y soportar:

- Miles de artistas  
- Millones de estadísticas  
- Integraciones con proveedores externos  

---

# 📈 Oportunidad de Negocio

PublishRealLiteApi puede convertirse en la base de:

### ✔ Un SaaS para artistas independientes  
### ✔ Una plataforma de distribución musical  
### ✔ Un dashboard de estadísticas multi-plataforma  
### ✔ Un sistema de gestión de equipos creativos  
### ✔ Un hub de contenido promocional  

El mercado de herramientas para artistas independientes está creciendo rápidamente, impulsado por:

- La explosión del streaming  
- La independencia creativa  
- La necesidad de datos  
- La descentralización de la industria  

Este backend está diseñado para capturar esa oportunidad.

---

# 🧱 Componentes del Sistema (Modelos)

## **AppUser**
Usuario autenticado del sistema (Identity).

## **ArtistProfile**
Perfil profesional del artista.  
Es el núcleo del ecosistema.

## **Release**
Lanzamiento musical (álbum, EP, single).

## **Track**
Canción individual dentro de un release.

## **ArtistVideo**
Videos promocionales del artista.

## **Team**
Equipo del artista (managers, colaboradores, etc.).

## **TeamMember**
Miembro del equipo con rol y permisos.

## **TeamInvite**
Invitaciones enviadas a nuevos miembros.

## **StreamStat**
Estadísticas de streams por plataforma, país y fecha.

---

# 🎮 Componentes del Sistema (Controladores)

## **AuthController**
Autenticación y generación de tokens JWT.

## **ArtistProfilesController**
Gestión del perfil del artista.

## **ReleasesController**
Administración de lanzamientos musicales.

## **TracksController**
Gestión de canciones dentro de un release.

## **VideosController**
Administración de videos promocionales.

## **TeamsController**
Gestión del equipo del artista.

## **TeamInvitesController**
Invitaciones y acceso colaborativo.

## **StatsController**
Estadísticas de streams y métricas.

---

# 🏁 Estado del Proyecto

El backend está en desarrollo activo y ya cuenta con:

- Autenticación completa  
- Modelos funcionales  
- Controladores REST  
- Relaciones EF Core  
- Arquitectura escalable  

Próximos pasos:

- Integración con proveedores de streaming  
- Dashboard avanzado  
- Sistema de notificaciones  
- Integración con almacenamiento en la nube  

---

# 📬 Contacto

Proyecto desarrollado por **Luis Luna**.  
Ideal para inversionistas, colaboradores y equipos interesados en construir la próxima gran plataforma para artistas independientes.
