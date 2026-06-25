---
name: frontend-react
description: 'Crea o actualiza el frontend React + Vite + TypeScript de la aplicación. Úsalo cuando quieras crear el proyecto frontend, añadir una página o componente nuevo, crear el servicio de API para un recurso, o configurar el proxy Vite hacia el backend ASP.NET Core.'
argument-hint: 'Recurso o componente a generar (opcional, por defecto: scaffold completo del frontend)'
---

# Skill: Frontend React + Vite + TypeScript

## Cuándo usar este skill

- El usuario pide "crear el frontend", "generar la interfaz", "crear la página de tareas"
- Se quiere añadir un componente o página nueva para un recurso de la API
- Se quiere crear el servicio de llamadas a la API para un recurso
- Se necesita configurar o corregir el proxy Vite → ASP.NET Core

## Prerequisitos

Antes de generar código frontend, verificar que existe:

1. `docs/analisis-diseño.md` — para conocer los endpoints de la API
2. `Dtos/` — los DTOs del backend son la fuente de verdad de los tipos TypeScript

## Arquitectura del frontend

```
frontend/                        ← carpeta raíz del proyecto Vite
├── index.html
├── vite.config.ts               ← proxy hacia el backend ASP.NET Core
├── tsconfig.json
├── package.json
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── types/
    │   └── index.ts             ← interfaces TypeScript (espejo de los DTOs .NET)
    ├── services/
    │   ├── tareas.service.ts    ← llamadas fetch a /api/tareas
    │   ├── categorias.service.ts
    │   ├── plantillas.service.ts
    │   └── usuarios.service.ts
    ├── components/
    │   ├── TareasList.tsx
    │   ├── TareaForm.tsx
    │   └── ...
    └── pages/
        ├── TareasPage.tsx
        ├── CategoriasPage.tsx
        └── ...
```

**Reglas de diseño:**
- Un fichero de servicio por recurso de la API.
- Los servicios son funciones puras (no clases), exportadas con nombre.
- Los componentes reciben datos por props — sin fetch directo en componentes.
- Las páginas orquestan: llaman al servicio y pasan datos a componentes.
- Texto de la UI en **español** (etiquetas, placeholders, mensajes de error).
- Nombres de ficheros y funciones en **inglés** (convenio estándar de React).

---

## Procedimiento

### Paso 1 — Leer el contexto

Leer siempre antes de generar:

- [`docs/analisis-diseño.md`](../../docs/analisis-diseño.md) — sección 4 (modelo) y sección 5 (endpoints)
- Los ficheros `Dtos/*.cs` del recurso a implementar — para derivar los tipos TypeScript
- [`frontend/vite.config.ts`](../../frontend/vite.config.ts) si ya existe — para no sobreescribir configuración manual

### Paso 2 — Scaffold inicial (solo si `frontend/` no existe)

Si la carpeta `frontend/` no existe, ejecutar en terminal:

```bash
cd c:\w\repos\DemoCopilot
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

Después de crear el proyecto, reemplazar `frontend/vite.config.ts` con la configuración de proxy del skill (ver ejemplo en `sample_codes/vite.config.ts`).

### Paso 3 — Tipos TypeScript

Los tipos van en `frontend/src/types/index.ts`. Derivan directamente de los DTOs de `Dtos/*.cs`:

| DTO .NET | Interface TypeScript |
|---|---|
| `TareaDto` | `Tarea` |
| `CrearTareaDto` | `CrearTarea` |
| `ActualizarTareaDto` | `ActualizarTarea` |
| `CategoriaDto` | `Categoria` |
| `CrearCategoriaDto` | `CrearCategoria` |
| `ActualizarCategoriaDto` | `ActualizarCategoria` |
| `PlantillaDto` | `Plantilla` |
| `UsuarioAsignadoDto` | `UsuarioAsignado` |

**Mapeo de tipos C# → TypeScript:**

| C# | TypeScript |
|---|---|
| `int` | `number` |
| `string` | `string` |
| `bool` | `boolean` |
| `DateTime` | `string` (ISO 8601) |
| `int?` | `number \| null` |
| `string?` | `string \| null` |
| `bool?` | `boolean \| null` |
| `DateTime?` | `string \| null` |
| enum `TipoRecurrencia` | `'Diaria' \| 'Semanal' \| 'Mensual'` |

Ver ejemplo completo en [`sample_codes/tipos.ts`](sample_codes/tipos.ts).

### Paso 4 — Servicio de API

Crear un fichero de servicio por recurso en `frontend/src/services/`. El servicio usa `fetch` nativo y lanza un error si la respuesta HTTP no es `ok`.

**Patrón base:**

```typescript
const BASE = '/api/tareas';

export async function listarTareas(): Promise<Tarea[]> {
  const res = await fetch(BASE);
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}

export async function crearTarea(datos: CrearTarea): Promise<Tarea> {
  const res = await fetch(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(datos),
  });
  if (!res.ok) throw new Error(`Error ${res.status}`);
  return res.json();
}
```

Ver ejemplo completo en [`sample_codes/tareas.service.ts`](sample_codes/tareas.service.ts).

### Paso 5 — Componentes y páginas

**Página** (`pages/TareasPage.tsx`): gestiona el estado con `useState` + `useEffect`, llama al servicio, pasa datos a los componentes.

**Componente lista** (`components/TareasList.tsx`): recibe `tareas: Tarea[]` por props, renderiza la lista.

**Componente formulario** (`components/TareaForm.tsx`): recibe `onGuardar: (datos: CrearTarea) => void` por props, gestiona el estado del formulario localmente.

**Patrón de página:**

```tsx
export default function TareasPage() {
  const [tareas, setTareas] = useState<Tarea[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    listarTareas()
      .then(setTareas)
      .catch(e => setError(e.message))
      .finally(() => setCargando(false));
  }, []);

  if (cargando) return <p>Cargando...</p>;
  if (error) return <p>Error: {error}</p>;

  return (
    <div>
      <h1>Tareas</h1>
      <TareasList tareas={tareas} />
    </div>
  );
}
```

### Paso 6 — Configuración del proxy Vite

El proxy redirige `/api/*` al backend ASP.NET Core para evitar problemas de CORS en desarrollo. Ver [`sample_codes/vite.config.ts`](sample_codes/vite.config.ts).

**Regla clave:** si el backend corre en HTTPS (puerto 7279), el proxy debe usar `https://localhost:7279` con `secure: false`. Si se apunta a HTTP, el backend redirige con 307 y se pierden headers.

---

## Convenciones de código

- `useState`, `useEffect` para estado y efectos — sin librerías de estado externas salvo que se pida.
- Los tipos exportados en `types/index.ts` se importan con `import type { Tarea } from '../types'`.
- Los servicios exportan funciones, no clases: `export async function listarTareas()`.
- Mensajes de error genéricos en español: "No se pudo cargar la lista de tareas."
- Botones y etiquetas en español: "Crear tarea", "Eliminar", "Guardar cambios".

---

## Navegación entre recursos

Si el usuario pide múltiples recursos, añadir rutas en `App.tsx` usando React Router:

```bash
npm install react-router-dom
```

```tsx
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';

<BrowserRouter>
  <nav>
    <Link to="/">Tareas</Link>
    <Link to="/categorias">Categorías</Link>
  </nav>
  <Routes>
    <Route path="/" element={<TareasPage />} />
    <Route path="/categorias" element={<CategoriasPage />} />
  </Routes>
</BrowserRouter>
```
