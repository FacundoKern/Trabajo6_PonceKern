# Tienda API — Trabajo Práctico: Programación de Aplicaciones Móviles

API REST desarrollada en **.NET 8** con **Entity Framework Core** y **SQL Server**, para la gestión de
productos, stock, compras a proveedores y ventas a clientes, con autenticación **JWT** y carga de imágenes.

> Se usó SQL Server (en vez de PostgreSQL/MySQL) y Somee.com como hosting gratuito porque es el stack
> con el que ya vengo trabajando en otras materias (ver `RRHH2026`), lo que facilita reutilizar el
> flujo de despliegue. Si preferís otro motor, en la sección "Cambiar de motor de base de datos" están
> los pasos para migrar a PostgreSQL o MySQL.

## Contenido de la entrega

- `/Models` — Entidades del modelo de datos (EF Core Code First)
- `/Data/ApplicationDbContext.cs` — DbContext con la configuración de relaciones
- `/Controllers` — Controladores REST (Auth, Categorías, Productos, Proveedores, Clientes, Ingresos, Salidas)
- `/DTOs` — Objetos de transferencia de datos (entrada/salida de cada endpoint)
- `/Services/TokenService.cs` — Generación de tokens JWT
- `/Helpers` — Paginado genérico (`PagedResult<T>`, `PaginacionParams`)
- `/docs/DER.mmd` — Diagrama entidad-relación (formato Mermaid, se puede abrir en https://mermaid.live)
- `/docs/TiendaAPI.postman_collection.json` — Colección de Postman con todos los endpoints
- `/wwwroot/uploads` — Carpeta donde se guardan las imágenes de productos

## Modelo de datos

Tablas: `Usuarios`, `Categorias`, `Productos`, `Proveedores`, `Clientes`, `Ingresos` + `IngresoDetalles`
(compras), `Salidas` + `SalidaDetalles` (ventas).

Se usó un patrón **cabecera/detalle** para Ingresos y Salidas (igual que una factura con renglones),
porque una sola operación de compra o venta puede incluir varios productos. El precio unitario se
guarda en el detalle (no se toma del producto en tiempo real) para conservar el precio histórico de
cada operación, aunque el precio del producto cambie después.

El diagrama completo está en `docs/DER.mmd`. Para visualizarlo: pegar el contenido en
https://mermaid.live o usar la extensión "Markdown Preview Mermaid Support" en VS Code.

## Cómo afecta el stock

- **Ingreso** (compra a proveedor): por cada línea del detalle, `Producto.Stock += Cantidad`.
- **Salida** (venta a cliente): antes de descontar nada, se valida que **todos** los productos
  del pedido tengan stock suficiente. Si falta stock de alguno, se rechaza toda la operación con
  `400 Bad Request` y el detalle de qué producto no alcanza — no se permite una venta parcial.
- Ambas operaciones corren dentro de una transacción de base de datos (`BeginTransactionAsync`),
  así que si algo falla a mitad de camino, no queda stock desincronizado.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express o el motor que prefieras) o una cadena de conexión a un SQL Server remoto
- (Opcional) [Postman](https://www.postman.com/downloads/) para importar la colección de pruebas

## Puesta en marcha local

```bash
# 1) Restaurar paquetes
dotnet restore

# 2) Instalar la herramienta de EF Core (si no la tenés)
dotnet tool install --global dotnet-ef

# 3) Configurar la cadena de conexión en appsettings.Development.json
#    (ya viene apuntando a localhost\SQLEXPRESS con autenticación integrada)

# 4) Crear la migración inicial y la base de datos
dotnet ef migrations add InicialTiendaAPI
dotnet ef database update

# 5) Ejecutar la API
dotnet run
```

La API queda disponible en `https://localhost:7000` (o el puerto que asigne `dotnet run`) con
Swagger en `/swagger`.

Al arrancar por primera vez, `Program.cs` aplica las migraciones automáticamente y crea un
usuario administrador inicial si la tabla `Usuarios` está vacía:

```
Usuario: admin
Password: Admin123$
```

**Importante:** cambiar esta contraseña (o crear un nuevo Admin y desactivar este) antes de
dejar la API expuesta públicamente.

## Autenticación (JWT)

1. `POST /api/auth/login` con `{ "nombreUsuario": "admin", "password": "Admin123$" }` devuelve un token.
2. En Postman/Swagger, agregar el header `Authorization: Bearer {token}` a las siguientes requests.
3. Los endpoints de **creación/edición/borrado** (POST/PUT/DELETE) de Categorías, Productos,
   Proveedores e Ingresos requieren rol `Admin`. Los `GET` solo requieren estar autenticado
   (cualquier rol). Clientes y Salidas están abiertas a `Admin` y `Empleado` incluso para crear,
   ya que en un local de venta cualquier empleado debe poder cargar una venta.
4. `POST /api/auth/registro` (requiere `Admin`) crea nuevos usuarios con rol `Admin` o `Empleado`.

CORS está configurado en modo permisivo (`AllowAnyOrigin`, `AllowAnyMethod`, `AllowAnyHeader`),
tal como pide la consigna, para simplificar las pruebas desde cualquier cliente móvil o Postman.

## Paginado

Todos los listados (`GET /api/productos`, `/api/clientes`, `/api/proveedores`, `/api/ingresos`,
`/api/salidas`, `/api/categorias`) aceptan los parámetros de query:

- `pagina` (default 1)
- `tamanioPagina` (default 10, máximo 50)
- `buscar` (texto libre, opcional, según la entidad)

La respuesta trae `items`, `paginaActual`, `tamanioPagina`, `totalRegistros`, `totalPaginas`,
`tienePaginaAnterior` y `tienePaginaSiguiente`.

## Carga de imagen de producto

`POST /api/productos/{id}/imagen` con `multipart/form-data`, campo `archivo`. Acepta jpg, jpeg,
png y webp, hasta 5 MB. La imagen se guarda en `wwwroot/uploads` con un nombre único
(`producto_{id}_{guid}.ext`) y la ruta relativa queda en `Producto.ImagenUrl`. Al subir una nueva
imagen, se borra la anterior del disco. Se puede acceder a la imagen directamente desde
`https://<host>/uploads/<nombre-archivo>` gracias a `app.UseStaticFiles()`.

## Pruebas con Postman

Importar `docs/TiendaAPI.postman_collection.json`. La colección:

- Tiene una variable `baseUrl` (cambiarla por la URL local o la del hosting).
- El request **Auth → Login (admin)** guarda automáticamente el token en la variable de colección
  `token` (vía un script de test), así el resto de los requests ya quedan autenticados.
- Incluye un caso de prueba de venta **sin stock suficiente**, para validar que la API responde
  `400` y no descuenta stock.
- Trae ejemplos de creación de cada entidad y de las operaciones de ingreso/salida con detalle.

Sugerencia de orden para una demo completa: Login → Crear categoría → Crear producto → Crear
proveedor → Registrar ingreso (sube el stock) → Crear cliente → Registrar salida (baja el stock) →
Intentar una salida sin stock (para mostrar el rechazo).

## Publicación en hosting gratuito

### Opción usada: Somee.com (SQL Server + ASP.NET, gratuito)

1. Crear cuenta en https://somee.com y una base de datos MSSQL gratuita desde el panel.
2. Copiar la cadena de conexión que provee Somee y pegarla en un `appsettings.Production.json`
   (no versionado, ver `.gitignore`) o como variable de entorno `ConnectionStrings__DefaultConnection`.
3. Publicar con Visual Studio (`Publish` → perfil FTP con los datos de Somee) o generar el paquete
   con `dotnet publish -c Release` y subirlo por FTP (FileZilla) a la carpeta que indique Somee.
4. Ejecutar las migraciones contra la base de Somee antes o durante el primer arranque:
   - Opción simple: dejar que `db.Database.Migrate()` en `Program.cs` las aplique solas al iniciar
     (ya está así de fábrica en este proyecto).
   - Opción alternativa: generar el script SQL con `dotnet ef migrations script` y ejecutarlo desde
     el panel de administración de la base en Somee.
5. Revisar que el plan gratuito de Somee soporte `wwwroot/uploads` con permisos de escritura; si no,
   considerar guardar las imágenes en un servicio externo (ver nota abajo).

### Alternativas si Somee da problemas

- **Railway** o **Render** (planes free/hobby) + PostgreSQL gestionado — requiere cambiar el
  proveedor de EF Core (ver sección siguiente) pero el despliegue vía Docker/GitHub es más simple
  que FTP.
- **Azure App Service (Free F1)** + Azure SQL Free tier — más pasos de configuración pero con más
  garantías de disponibilidad para una demo.

> Nota sobre imágenes en hosting gratuito: muchos planes gratuitos no persisten el sistema de
> archivos entre reinicios/despliegues. Para la entrega académica alcanza con `wwwroot/uploads`
> tal como está armado; si se quiere una solución más robusta, se puede reemplazar el guardado en
> disco por un bucket externo (Cloudinary tiene un plan gratuito simple de integrar).

## Cambiar de motor de base de datos (PostgreSQL o MySQL)

El proyecto usa `Microsoft.EntityFrameworkCore.SqlServer`. Para cambiar de motor:

**PostgreSQL:**
```bash
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```
En `Program.cs`, reemplazar `options.UseSqlServer(...)` por `options.UseNpgsql(...)`.

**MySQL:**
```bash
dotnet remove package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Pomelo.EntityFrameworkCore.MySql
```
En `Program.cs`, reemplazar por `options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))`.

En ambos casos hay que borrar la carpeta `Migrations` (si ya existiera) y volver a correr
`dotnet ef migrations add InicialTiendaAPI` para que EF genere el SQL específico del nuevo motor.

## Seguridad — resumen de decisiones

- Contraseñas de usuario hasheadas con **BCrypt** (nunca se guardan en texto plano).
- Tokens JWT firmados con HMAC-SHA256, clave en `appsettings.json` (`Jwt:Key`) — **cambiar esta
  clave** antes de publicar, y no versionarla en un repositorio público.
- Autorización por rol (`[Authorize(Roles = "Admin")]`) en las operaciones sensibles (alta/baja/
  modificación de catálogo, ingresos de mercadería).
- Baja lógica (`Activo = false`) en Proveedores, Clientes y Productos en lugar de `DELETE` físico,
  para no romper el historial de Ingresos/Salidas ya registrados.

---

# Cambios del TP6 (backend para la app Blazor Hybrid)

## Configuración

Los datos sensibles (cadena de conexión de Somee y `Jwt:Key`) **no** están en el repositorio. Van en
`appsettings.Production.json`, que está en `.gitignore`. Hay una plantilla en `appsettings.Production.example.json`.

## Roles y gestión de usuarios

- Nueva tabla `Roles` (`Usuarios.RolId` → `Roles.Id`). Roles base: **Admin** y **Empleado** (no se pueden borrar
  ni renombrar). La migración `TablaRoles` convierte los usuarios existentes sin perder su rol.
- `GET/PUT api/usuarios`, `PUT api/usuarios/{id}/password` y `GET/POST/PUT/DELETE api/roles` (solo Admin).
- Un Admin no puede quitarse su propio rol ni desactivarse.

## Catálogo y ventas

- `GET api/productos?soloActivos=true` devuelve solo productos activos (lo usa el catálogo de la app).
- `POST api/salidas`: el **precio lo pone la API** con el precio actual del producto (ya no se recibe del cliente).
- **Pago simulado**: la venta recibe `medioPago` (`Efectivo`, `Tarjeta`, `Transferencia`) y, si es tarjeta,
  `numeroTarjeta` (ficticio, 13 a 19 dígitos). Las tarjetas que terminan en `0000` se **rechazan** para poder
  mostrarlo en la demo. Se guarda `EstadoPago`, `CodigoOperacion` y solo los últimos 4 dígitos de la tarjeta.
- Al arrancar se crea el cliente **Consumidor Final** para ventas de mostrador.

## Auditoría y log de errores

- Tabla `Auditorias`: se registran login (y login fallido), ventas, pagos rechazados, ingresos, altas/bajas/
  modificaciones de productos, categorías, proveedores, clientes, usuarios y roles, y la generación de reportes.
- Tabla `LogErrores`: el middleware `ManejoErroresMiddleware` atrapa cualquier error no controlado, lo guarda y
  responde `500` con un `errorId`, sin mostrar detalles internos.
- Consulta (solo Admin): `GET api/auditoria`, `GET api/auditoria/errores`, `GET api/auditoria/errores/{id}`.
  `POST api/auditoria/probar-error` provoca un error a propósito para la demo.

## Reportes PDF (QuestPDF)

- `GET api/reportes/stock` (cualquier usuario logueado)
- `GET api/reportes/ingresos?desde=&hasta=` y `GET api/reportes/ventas?desde=&hasta=` (solo Admin;
  sin fechas toma los últimos 30 días)

Devuelven un archivo `application/pdf`. La app lo descarga, lo abre y lo comparte por mail desde el celular.

## Normalización

Ver [`docs/Normalizacion.md`](docs/Normalizacion.md) (1FN, 2FN y 3FN) y el DER actualizado en `docs/DER.mmd`.
