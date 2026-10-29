# Normalización de la base de datos (1FN, 2FN y 3FN)

El diagrama entidad-relación está en [`DER.mmd`](DER.mmd) (se puede ver pegándolo en https://mermaid.live).

Tablas: `Roles`, `Usuarios`, `Categorias`, `Productos`, `Proveedores`, `Clientes`, `Ingresos`, `IngresoDetalles`,
`Salidas`, `SalidaDetalles`, `Auditorias`, `LogErrores`.

---

## Primera forma normal (1FN)

> Cada columna guarda un único valor atómico, no hay grupos repetidos y cada fila se identifica con una clave primaria.

- Todas las tablas tienen clave primaria `Id` (entera, autoincremental).
- No hay listas dentro de una columna. El caso típico sería una venta con varios productos: en vez de guardar
  "producto1, producto2, ..." dentro de `Salidas`, cada producto vendido es una fila de `SalidaDetalles`
  (patrón **cabecera / detalle**). Lo mismo para las compras con `Ingresos` / `IngresoDetalles`.
- El nombre del cliente está separado en `Nombre` y `Apellido`; los datos de contacto (teléfono, email,
  dirección) son columnas separadas.

## Segunda forma normal (2FN)

> Está en 1FN y todos los atributos que no son clave dependen de la clave primaria **completa**
> (sin dependencias parciales).

- Todas las claves primarias son simples (una sola columna `Id`), así que no puede haber dependencias parciales.
- En `SalidaDetalles` e `IngresoDetalles`, la cantidad y el precio dependen de la línea completa
  (qué venta + qué producto), no solo del producto ni solo de la venta.
- El nombre del producto **no** se repite en los detalles: solo se guarda `ProductoId`.

## Tercera forma normal (3FN)

> Está en 2FN y ningún atributo no clave depende de otro atributo no clave (sin dependencias transitivas).

Decisiones tomadas para cumplirla:

| Situación | Problema que se evita | Cómo quedó |
|---|---|---|
| Rol del usuario | Antes era un texto (`Usuarios.Rol = "Admin"`): la descripción y permisos del rol dependían del texto, no del usuario, y un error de tipeo creaba un "rol nuevo" | Tabla `Roles` y `Usuarios.RolId` (FK) |
| Categoría del producto | Repetir el nombre/descripción de la categoría en cada producto | `Productos.CategoriaId` (FK) a `Categorias` |
| Proveedor de una compra / cliente de una venta | Repetir razón social, CUIT, dirección en cada operación | `Ingresos.ProveedorId` y `Salidas.ClienteId` (FK) |
| Usuario en auditoría y errores | Guardar el nombre de usuario (que depende del usuario, no del registro de auditoría) | Solo `UsuarioId` (FK); el nombre se obtiene con un JOIN |
| Total de una venta o compra | El total depende de los detalles (cantidad × precio), no de la cabecera | **No se guarda**: se calcula sumando los detalles |
| Datos de la tarjeta | El número completo no depende de la venta y además es un dato sensible | Solo `TarjetaUltimos4` |

### Excepciones justificadas

- **`SalidaDetalles.PrecioUnitario` e `IngresoDetalles.PrecioUnitario`**: parecen repetir `Productos.Precio`, pero
  **no** es una dependencia transitiva: es el precio **histórico** al momento de la operación. Si mañana cambia el
  precio del producto, las ventas anteriores tienen que seguir mostrando lo que se cobró.
- **`Productos.Stock`**: se podría calcular como (suma de ingresos − suma de ventas), pero se guarda y se actualiza
  dentro de una transacción en cada ingreso/venta, para poder validar rápido "no vender sin stock" y para que el
  catálogo no tenga que recorrer todo el historial. Es una desnormalización controlada y documentada.
- **`MedioPago` y `EstadoPago` en `Salidas`**: son valores de una lista fija (Efectivo / Tarjeta / Transferencia,
  Aprobado) que dependen directamente de la venta, por eso no se separaron en otra tabla.
