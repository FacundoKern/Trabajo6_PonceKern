# Plan de trabajo — TP6 Programación Móvil (Blazor Hybrid + TiendaAPI)

Entrega: **viernes 30/10**. Grupo de 2: **A** (backend + reportes + administración) y **B** (app móvil: base, catálogo, carrito, ingresos).
Decidan quién es A y quién es B y pongan los nombres acá.

- A: Manuel Ponce
- B: Facundo Kern

---

## 1. Estructura del repositorio (un solo repo para todo)

```
TrabajoFinal_trani/
├── TiendaAPI/        ← API del TP5 (backend)
├── TiendaApp/        ← App .NET MAUI Blazor Hybrid (Android)
├── docs/             ← DER, normalización, Postman, capturas
├── PLAN.md
└── README.md
```

---

## 2. Reglas de git (simples, trabajando en `main`)

1. **Siempre antes de empezar:** `git pull`
2. **Commit chico apenas algo funciona** (que compile). Mejor 5 commits chicos que 1 gigante.
3. **Push enseguida después del commit:** `git push`
4. **Cada uno toca sus archivos.** Archivos compartidos (`MauiProgram.cs`, `NavMenu.razor`, `Program.cs`, `ApplicationDbContext.cs`):
   avisar por WhatsApp antes de tocarlos, hacer pull, cambiar, commit y push rápido.
5. **Migraciones de EF Core: solo las crea A.** Si B necesita un cambio en la base, se lo pide a A.
6. **Nunca subir contraseñas.** La cadena de conexión de Somee va en `appsettings.Production.json` (ya está en `.gitignore`).
7. Formato de mensajes:
   - `feat: ...` algo nuevo — `feat: pantalla de catálogo con stock`
   - `fix: ...` corrección — `fix: no se mostraba la imagen del producto`
   - `docs: ...` documentación — `docs: justificación de 3FN`
   - `refactor: ...` / `chore: ...` reorganizar / configuración

Si hay conflicto al hacer pull: no borrar nada a ciegas, abrir el archivo, quedarse con ambos cambios y avisar al compañero.

---

## 3. Qué falta respecto a la consigna

| Requisito TP6 | Estado actual en TiendaAPI | Qué hay que hacer |
|---|---|---|
| Catálogo de productos (stock / disponibilidad) | `GET /api/productos` con paginado, imagen y stock | App: pantalla catálogo. API: filtrar solo activos para el catálogo |
| Carrito + compra + pago simulado | `POST /api/salidas` valida stock | **API: el precio lo pone el servidor** (hoy lo manda el cliente). Agregar datos de pago simulado. App: carrito + checkout |
| Entradas de mercadería | `POST /api/ingresos` (Admin) | App: pantalla de ingreso |
| Reportes PDF + envío por correo | No existe | API: `GET /api/reportes/...` que devuelva PDF (QuestPDF). App: descargar, abrir y compartir por mail |
| Control de acceso y gestión de roles | Login JWT, roles Admin/Empleado como texto | API: tabla `Roles` + endpoints para listar usuarios, cambiar rol y activar/desactivar. App: menú según rol |
| Auditoría y log de errores | No existe | API: tablas `Auditoria` y `LogErrores` + middleware de excepciones. App: pantalla de consulta (Admin) |
| Normalización 3FN | Modelo ya bastante normalizado | Rol como tabla, actualizar DER y escribir justificación 1FN/2FN/3FN |
| Repositorio git | Sin repo | Crear repo y subir progresivamente |

---

## 4. Cronograma día por día

### Lunes 26 — Puesta en marcha
**Antes que nada (cualquiera de los dos, juntos):**
- [ ] Instalar Git (`winget install Git.Git`) o GitHub Desktop
- [ ] Sacar la contraseña de Somee de `appsettings.json` → pasarla a `appsettings.Production.json`
- [ ] Crear repo en GitHub, invitar al compañero como colaborador
- [ ] Commit 1: `chore: API del TP5 como base del proyecto`
- [ ] Commit 2: `docs: plan de trabajo del TP6`

**A**
- [ ] `fix: catálogo devuelve solo productos activos` (parámetro `soloActivos`)
- [ ] `fix: el precio de la venta se toma del producto en el servidor`

**B**
- [ ] Crear proyecto `dotnet new maui-blazor -n TiendaApp` y correrlo en el emulador Android
- [ ] `feat: proyecto TiendaApp (MAUI Blazor Hybrid)`
- [ ] `feat: configuración de URL base de la API y HttpClient`

### Martes 27 — Login y catálogo / roles y pago
**A**
- [ ] `feat: tabla Roles y relación con Usuarios` (+ migración)
- [ ] `feat: endpoints de gestión de usuarios y roles`
- [ ] `feat: pago simulado en ventas (medio de pago, estado, nro de operación)`

**B**
- [ ] `feat: pantalla de login y guardado del token (SecureStorage)`
- [ ] `feat: AuthenticationStateProvider con rol del usuario`
- [ ] `feat: catálogo de productos con imagen, precio y stock`
- [ ] `feat: búsqueda, filtro por categoría y paginado en el catálogo`

### Miércoles 28 — Carrito / auditoría y reportes
**A**
- [ ] `feat: tablas Auditoria y LogErrores`
- [ ] `feat: middleware global de manejo y registro de errores`
- [ ] `feat: registro de auditoría en login, ventas, ingresos y ABM`
- [ ] `feat: reporte PDF de stock` → después `de ingresos` y `de ventas` (por rango de fechas)
- [ ] Publicar en Somee y **probar que el PDF se genere en el hosting**

**B**
- [ ] `feat: servicio de carrito (agregar, quitar, cantidades, total)`
- [ ] `feat: pantalla de carrito con validación de stock`
- [ ] `feat: checkout con selección de cliente y pago simulado`
- [ ] `feat: pantalla de ingreso de mercadería (Admin)`

### Jueves 29 — Pantallas de admin y pulido
**A**
- [ ] `feat: pantalla de reportes (descargar, abrir y enviar por mail)`
- [ ] `feat: pantalla de usuarios y roles (Admin)`
- [ ] `feat: pantalla de auditoría y log de errores (Admin)`

**B**
- [ ] `feat: menú y rutas según rol (Admin / Empleado)`
- [ ] `feat: manejo de errores y token vencido en la app`
- [ ] `fix: ...` lo que salga probando en Android
- [ ] Generar APK de prueba

### Viernes 30 — Entrega
- [ ] **A:** `docs: DER actualizado y justificación de normalización 3FN`
- [ ] **A:** `docs: colección Postman actualizada`
- [ ] **B:** `docs: README con instrucciones, usuarios de prueba y capturas`
- [ ] Prueba completa en el celular / emulador: login → catálogo → carrito → pago → stock baja → ingreso → stock sube → reporte PDF por mail
- [ ] Publicación final en Somee y entrega

---

## 5. Decisiones técnicas

- **Reportes PDF:** se generan en la API con QuestPDF. La app descarga el PDF, lo abre y lo manda por mail con la app de correo del celular (`Email.ComposeAsync` con adjunto o `Share`). Así no hace falta configurar SMTP en el hosting.
- **Pago simulado:** pantalla con medio de pago (efectivo / tarjeta / transferencia). Si es tarjeta, se pide un número **ficticio** que solo se valida en formato. La API guarda medio, estado "Aprobado" y un código de operación generado.
- **Venta desde el carrito:** usa el cliente elegido en el checkout. Opción: crear un cliente "Consumidor Final" en el seed.
- **Android y HTTP:** si la URL de Somee es `http://`, hay que habilitar el tráfico sin cifrar en Android (`usesCleartextTraffic`). Mejor usar `https://` si Somee lo permite.
- **Token:** guardar en `SecureStorage`, mandarlo en el header `Authorization: Bearer`, y volver al login si la API responde 401.
