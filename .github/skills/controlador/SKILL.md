---
name: controlador
description: 'Crea o actualiza los controladores ASP.NET Core de la aplicación a partir de los endpoints definidos en el análisis. Úsalo cuando quieras generar los controladores del proyecto, añadir un nuevo endpoint, o sincronizar los controladores con el documento de análisis y diseño.'
argument-hint: 'Recurso a generar (opcional, por defecto: todos los recursos de la sección 5 del análisis)'
---

# Skill: Crear Controladores ASP.NET Core

## Cuándo usar este skill

- El usuario pide "crear el controlador", "generar los controladores", "crear la capa de controllers"
- Se quiere sincronizar los controladores con los endpoints definidos en el análisis
- Se ha añadido un nuevo recurso al análisis y hay que crear su controlador
- Se quiere añadir un endpoint nuevo a un controlador existente

## Prerequisitos

Antes de usar este skill, deben existir:
1. `docs/analisis-diseño.md` con la sección 5 (endpoints) completa. Si no existe, ejecutar primero el skill `diseño-analisis`.
2. Los modelos de dominio en `Models/`. Si no existen, ejecutar primero el skill `modelo`.
3. Los DTOs en `Dtos/`. Si no existen, ejecutar primero el skill `dto`.
4. Las interfaces e implementaciones de servicios en `Services/`. Si no existen, advertir al usuario — los controladores compilarán pero no funcionarán hasta que los servicios estén creados.

## Procedimiento

### Paso 1 — Leer el contexto

Leer los siguientes ficheros antes de generar nada:

- [`docs/analisis-diseño.md`](../../docs/analisis-diseño.md) — fuente de verdad de los endpoints (sección 5) y la arquitectura (sección 3)
- [`.github/copilot-instructions.md`](../copilot-instructions.md) — convenciones de código del proyecto

Si `docs/analisis-diseño.md` no existe, detener y pedir al usuario que primero ejecute el skill `diseño-analisis`.

### Paso 2 — Verificar que los servicios usan DTOs

Antes de generar o modificar controladores, comprobar que los servicios existentes en `Services/` usan DTOs en su firma pública:

- Las interfaces `I<Recurso>Service` deben tener métodos que reciben `Crear<Recurso>Dto` / `Actualizar<Recurso>Dto` y devuelven `<Recurso>Dto`.
- Si los servicios usan entidades directamente (p. ej. reciben `TodoItem` en lugar de `CrearTareaDto`), **no generar los controladores** — primero corregir los servicios ejecutando de nuevo el skill `servicio`.

### Paso 3 — Localizar el proyecto y verificar qué controladores existen

Buscar el fichero `.csproj` del proyecto principal (excluir proyectos de tests). La carpeta `Controllers/` siempre es relativa a ese `.csproj`.

Ubicaciones habituales, en orden de preferencia:
1. `src/<NombreProyecto>/Controllers/` — si hay carpeta `src/`
2. `<NombreProyecto>/Controllers/` — si el proyecto tiene su propia subcarpeta
3. `Controllers/` — si el `.csproj` está en la raíz del repositorio

Una vez localizada la carpeta, comprobar qué ficheros contiene.
Si ya existen controladores, leer su contenido antes de modificar para evitar sobreescribir cambios manuales.

### Paso 4 — Identificar los recursos de la sección 5

En `docs/analisis-diseño.md`, sección 5, los endpoints están agrupados por recurso (p. ej. "Tareas — `/api/tareas`", "Plantillas — `/api/plantillas`"). Cada grupo da lugar a un controlador independiente.

Para cada grupo:
- Anotar el prefijo de ruta (p. ej. `/api/tareas`)
- Listar todos los endpoints: verbo HTTP, ruta, descripción, respuesta OK y errores posibles
- Identificar si hay endpoints de **acción** (rutas con un segmento adicional después del `{id}`, como `/{id}/completar` o `/{id}/instanciar`)

### Paso 5 — Generar un controlador por recurso

Crear el fichero `<NombreRecursoEnPascalCase>Controller.cs` dentro de `Controllers/`.

#### Estructura obligatoria de cada controlador

Los controladores **siempre reciben y devuelven DTOs**, nunca entidades de dominio.

```csharp
namespace <Namespace>.Controllers;

[ApiController]
[Route("api/[controller]")]
public class <Recurso>Controller : ControllerBase
{
    private readonly I<Recurso>Service _servicio;

    public <Recurso>Controller(I<Recurso>Service servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<<Recurso>Dto>>> ObtenerTodos()
    {
        var resultado = await _servicio.ObtenerTodosAsync();
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<<Recurso>Dto>> ObtenerPorId(int id)
    {
        var resultado = await _servicio.ObtenerPorIdAsync(id);
        if (resultado is null)
            return NotFound();
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<<Recurso>Dto>> Crear(Crear<Recurso>Dto dto)
    {
        var creado = await _servicio.CrearAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<<Recurso>Dto>> Actualizar(int id, Actualizar<Recurso>Dto dto)
    {
        var actualizado = await _servicio.ActualizarAsync(id, dto);
        if (actualizado is null)
            return NotFound();
        return Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(int id)
    {
        var eliminado = await _servicio.EliminarAsync(id);
        if (!eliminado)
            return NotFound();
        return NoContent();
    }
}
```

#### Reglas de generación

- **Namespace**: derivar del namespace raíz del `.csproj` + `.Controllers`.
- **Herencia**: siempre `ControllerBase`, nunca `Controller` (no se usan vistas).
- **Atributos de clase**: `[ApiController]` y `[Route("api/[controller]")]` siempre presentes.
- **Inyección de dependencias**: constructor con el servicio correspondiente (`I<Recurso>Service`), almacenado en campo `readonly` privado. Nunca `new` directo.
- **Sin lógica de negocio**: el cuerpo de cada método solo llama al servicio y retorna el `ActionResult` adecuado. Ninguna regla de negocio, validación de dominio ni cálculo dentro del controlador.
- **`async/await`** en todos los métodos que llamen al servicio.
- **Tipo de retorno**: `Task<ActionResult>` o `Task<ActionResult<T>>` según corresponda.
- **Nombres de métodos**: en castellano, descriptivos del endpoint (p. ej. `ObtenerTodos`, `ObtenerPorId`, `Crear`, `Actualizar`, `Eliminar`, `Completar`, `Instanciar`).

#### Mapeo de verbos HTTP y códigos de respuesta

Respetar exactamente los códigos de respuesta definidos en la tabla de la sección 5:

| Tipo de endpoint | Atributo HTTP | Respuesta éxito | Respuesta error |
|---|---|---|---|
| Listar todos | `[HttpGet]` | `Ok(lista)` → 200 | — |
| Obtener por ID | `[HttpGet("{id}")]` | `Ok(entidad)` → 200 | `NotFound()` → 404 |
| Crear | `[HttpPost]` | `CreatedAtAction(...)` → 201 | `BadRequest()` → 400 |
| Actualizar | `[HttpPut("{id}")]` | `Ok(entidad)` → 200 | `NotFound()` → 404, `BadRequest()` → 400 |
| Eliminar | `[HttpDelete("{id}")]` | `NoContent()` → 204 | `NotFound()` → 404 |
| Acción especial | `[HttpPost("{id}/<accion>")]` | según análisis | según análisis |

#### Patrón para `CreatedAtAction` (POST de creación)

```csharp
return CreatedAtAction(nameof(ObtenerPorId), new { id = entidadCreada.Id }, entidadCreada);
```

#### Patrón para verificar existencia antes de responder 404

El controlador no busca en base de datos directamente. El servicio devuelve `null` cuando la entidad no existe:

```csharp
var resultado = await _servicio.ObtenerPorIdAsync(id);
if (resultado is null)
    return NotFound();
return Ok(resultado);
```

#### Endpoints de acción especiales

Los endpoints con rutas del tipo `/{id}/completar` o `/{id}/instanciar` se implementan como métodos `[HttpPost("{id}/<accion>")]`:

```csharp
[HttpPost("{id}/completar")]
public async Task<ActionResult> Completar(int id)
{
    var resultado = await _servicio.CompletarAsync(id);
    if (resultado is null)
        return NotFound();
    return Ok(resultado);
}
```

#### Validación de entrada en POST y PUT

`[ApiController]` activa automáticamente la validación del `ModelState`. Solo añadir `if (!ModelState.IsValid) return BadRequest(ModelState);` si la validación automática no es suficiente (caso poco habitual con `[ApiController]`).

### Paso 6 — Comprobar y actualizar `Program.cs`

Abrir `Program.cs` y verificar:

1. Que `builder.Services.AddControllers()` ya está presente (normalmente lo incluye la plantilla de ASP.NET Core).
2. Que `app.MapControllers()` ya está presente.
3. Para cada servicio referenciado por los controladores generados, comprobar si ya está registrado. Si **no** está registrado **y el servicio ya existe** en `Services/`, añadir:
   ```csharp
   builder.Services.AddScoped<I<Recurso>Service, <Recurso>Service>();
   ```
   Si el servicio **no existe** aún en `Services/`, no añadir el registro — indicarlo al usuario como pendiente.

Colocar los registros de servicios agrupados antes de `var app = builder.Build();`.

### Paso 7 — Actualizar el fichero `.http`

Localizar el fichero `.http` en la raíz del proyecto (normalmente `<NombreProyecto>.http`). Si no existe, crearlo.

Reescribir su contenido completo con **todas** las peticiones de **todos** los controladores del proyecto (no solo los recién creados). Usar el siguiente formato:

```http
@<NombreProyecto>_HostAddress = http://localhost:5104

### ===========================
### <RECURSO EN MAYÚSCULAS>  /api/<recurso>
### ===========================

### Listar todos
GET {{<NombreProyecto>_HostAddress}}/api/<recurso>
Accept: application/json

###

### Obtener por ID
GET {{<NombreProyecto>_HostAddress}}/api/<recurso>/1
Accept: application/json

###

### Crear
POST {{<NombreProyecto>_HostAddress}}/api/<recurso>
Content-Type: application/json

{
  <campos del Crear<Recurso>Dto con valores de ejemplo>
}

###

### Actualizar
PUT {{<NombreProyecto>_HostAddress}}/api/<recurso>/1
Content-Type: application/json

{
  <campos del Actualizar<Recurso>Dto con valores de ejemplo>
}

###

### Eliminar
DELETE {{<NombreProyecto>_HostAddress}}/api/<recurso>/1

###

### <Acción especial, si existe>
POST {{<NombreProyecto>_HostAddress}}/api/<recurso>/1/<accion>

###
```

**Reglas:**

- El puerto en `@<NombreProyecto>_HostAddress` debe coincidir con el valor ya existente en el `.http` (o con `Properties/launchSettings.json` si se crea desde cero).
- Incluir un bloque por cada endpoint de cada controlador, en el mismo orden en que aparecen en el código.
- Los cuerpos JSON de POST/PUT deben usar valores de ejemplo realistas (no `"string"` ni `null` para campos obligatorios).
- Los campos opcionales (`null`) se pueden incluir o excluir — preferir incluirlos comentados para que sirvan de referencia.
- Separar grupos de endpoints con un comentario de sección `### === ... ===`.

### Paso 8 — Confirmar

Informar al usuario con una lista de los ficheros creados o modificados con sus rutas relativas.

Si algún servicio referenciado no existe aún, listarlos explícitamente indicando que los controladores compilarán con errores hasta que se creen. Sugerir ejecutar el skill correspondiente (p. ej. `servicio`) como siguiente paso.

Si los controladores ya existían y se han modificado, señalar qué cambios se han hecho para que el usuario pueda revisarlos.

Si algún servicio referenciado no existe aún, listarlos explícitamente indicando que los controladores compilarán con errores hasta que se creen. Sugerir ejecutar el skill correspondiente (p. ej. `servicio`) como siguiente paso.

Si los controladores ya existían y se han modificado, señalar qué cambios se han hecho para que el usuario pueda revisarlos.
