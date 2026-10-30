# Tienda — TP6 Programación de Aplicaciones Móviles

Sistema de gestión de una tienda: una **app Android** hecha con .NET MAUI Blazor Hybrid que consume una
**API REST** publicada en Somee. Tiene catálogo y carrito de compras, ventas con pago simulado, ingresos de
mercadería con control de stock, reportes en PDF, roles de usuario, auditoría y log de errores.

**Integrantes**

- Manuel Ponce — API
- Facundo Kern — app móvil

## Estructura del repositorio

```
├── TiendaAPI/              API REST (.NET 8, EF Core, SQL Server, JWT, QuestPDF)
│   └── docs/               DER, normalización, colección de Postman y cambios del TP6
├── TiendaApp/              App .NET MAUI Blazor Hybrid (.NET 10)
│   ├── Components/         Pantallas, layouts y componentes Razor
│   ├── Models/             Clases que reflejan los DTO de la API
│   ├── Services/           Acceso a la API, sesión, carrito y formato
│   └── Platforms/Android/  Manifiesto y configuración de red
├── TrabajoFinal.slnx       Solución con los dos proyectos
└── PLAN.md                 Plan de trabajo
```

## Funcionalidades

| Función | Admin | Empleado |
|---|---|---|
| Catálogo con búsqueda, categorías y stock | Sí | Sí |
| Carrito y compra con pago simulado | Sí | Sí |
| Historial de ventas y comprobante | Sí | Sí |
| Consultar ingresos de mercadería | Sí | Sí |
| Registrar ingresos de mercadería (suman stock) | Sí | No |
| Reporte PDF de stock | Sí | Sí |
| Reportes PDF de ventas e ingresos por fechas | Sí | No |
| Usuarios y roles | Sí | No |
| Auditoría y log de errores | Sí | No |

Los permisos se controlan en la API con `[Authorize(Roles = ...)]`. La app además oculta las opciones y
bloquea las pantallas que el rol no puede usar.

### Compra y pago simulado

- El precio lo pone la API según el producto, nunca la app.
- La API valida el stock de todo el pedido: si falta un producto, no se registra nada.
- Las compras desde la app se registran a nombre del cliente **Consumidor Final**.
- Medios de pago: Efectivo, Tarjeta y Transferencia. No se hace ningún cobro real.
- Con tarjeta se pide un número ficticio de 13 a 19 dígitos: uno terminado en `0000` sale rechazado y
  cualquier otro sale aprobado (por ejemplo `4111 1111 1111 1111`).

### Reportes

La API arma los PDF con QuestPDF. La app los descarga, los abre con el visor del celular y los envía por
correo con la app de mail del teléfono, con el PDF adjunto. Si no hay ninguna configurada, usa el menú de
compartir de Android.

## Probar la aplicación

La API ya está publicada en <http://tp5poncekern.somee.com/>, y la app viene configurada para usarla.

### Usuario de prueba

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123$` | Admin |

Para probar el rol Empleado, crear un usuario desde **Administración → Usuarios**. La contraseña del `admin`
es la inicial del sistema, solo para esta entrega: en un sistema real habría que cambiarla.

### Instalar en un celular Android

Requisitos: Visual Studio con la carga de trabajo **.NET MAUI** y el SDK de .NET 10, y un celular con
Android 7.0 o superior, con la depuración USB activada.

1. Abrir `TrabajoFinal.slnx` y elegir `TiendaApp` como proyecto de inicio.
2. Elegir la configuración **Release** y el celular como dispositivo.
3. Ejecutar sin depurar (**Ctrl+F5**).

También se puede generar el APK:

```bash
dotnet publish TiendaApp/TiendaApp.csproj -f net10.0-android -c Release
```

Queda en `TiendaApp/bin/Release/net10.0-android/publish/` como `com.poncekern.tiendaapp-Signed.apk`.

## Correr la API en forma local (opcional)

1. En `TiendaAPI/appsettings.Development.json`, ajustar la cadena de conexión a un SQL Server propio.
2. Crear la base y levantar la API:

   ```bash
   cd TiendaAPI
   dotnet tool restore
   dotnet ef database update
   dotnet run
   ```

3. Para que la app use esa API, cambiar `BaseUrl` en `TiendaApp/Services/ApiConfig.cs`. Desde el emulador,
   la PC se ve como `http://10.0.2.2:<puerto>/`, que ya está habilitada. Para usar un celular real, además
   hay que agregar la dirección de la PC en `network_security_config.xml`.

Los datos sensibles (cadena de conexión de Somee y clave del JWT) no están en el repositorio: van en
`appsettings.Production.json`, que está en `.gitignore`. Hay una plantilla en
`appsettings.Production.example.json`.

## Decisiones de la app

- **Sesión:** el token JWT se guarda en `SecureStorage`, cifrado por el sistema. Si la API responde 401, la
  sesión se cierra y se vuelve al login.
- **Carrito:** se guarda en el dispositivo, sobrevive a cerrar la app y se vacía al cerrar sesión. Al abrirlo
  se actualizan precio y stock desde la API.
- **HTTP en Android:** Somee publica por `http`, así que se habilitó el tráfico sin cifrar solo para ese
  dominio y para el emulador (`network_security_config.xml`), y el contenido mixto para las imágenes.
- **Pantallas esqueleto:** el catálogo, el detalle y los listados muestran la forma del contenido mientras
  cargan, con las clases `placeholder` de Bootstrap.

## Documentación

- [Documentación de la API](TiendaAPI/README.md)
- [Cambios del TP6 en la API](TiendaAPI/docs/README.md)
- [Normalización 1FN, 2FN y 3FN](TiendaAPI/docs/Normalizacion.md)
- [Diagrama entidad-relación](TiendaAPI/docs/DER.mmd) (se puede abrir en <https://mermaid.live>)
- [Colección de Postman](TiendaAPI/docs/TiendaAPI.postman_collection.json)
