// frontend/src/types/index.ts
// Tipos TypeScript que espejean los DTOs del backend ASP.NET Core.
// Fuente de verdad: carpeta Dtos/ del proyecto backend.

export type TipoRecurrencia = 'Diaria' | 'Semanal' | 'Mensual';

// ──────────────────────────────────────
// Tareas  (espejo de TareasDtos.cs)
// ──────────────────────────────────────

export interface Tarea {
  id: number;
  title: string;
  isCompleted: boolean;
  createdAt: string;           // DateTime → string ISO 8601
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
  proximaFecha: string | null;
  plantillaId: number | null;
  usuarioAsignadoId: number | null;
  categoriaId: number | null;
  categoriaNombre: string | null;
  usuarioAsignadoNombre: string | null;
}

export interface CrearTarea {
  title: string;
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
  plantillaId: number | null;
  usuarioAsignadoId: number | null;
  categoriaId: number | null;
}

export interface ActualizarTarea {
  title: string;
  isCompleted: boolean;
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
  usuarioAsignadoId: number | null;
  categoriaId: number | null;
}

// ──────────────────────────────────────
// Categorías  (espejo de CategoriasDtos.cs)
// ──────────────────────────────────────

export interface Categoria {
  id: number;
  nombre: string;
  color: string;               // formato #RRGGBB
}

export interface CrearCategoria {
  nombre: string;
  color: string;
}

export interface ActualizarCategoria {
  nombre: string;
  color: string;
}

// ──────────────────────────────────────
// Plantillas  (espejo de PlantillasDtos.cs)
// ──────────────────────────────────────

export interface Plantilla {
  id: number;
  titulo: string;
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
}

export interface CrearPlantilla {
  titulo: string;
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
}

export interface ActualizarPlantilla {
  titulo: string;
  esRepetitiva: boolean;
  recurrencia: TipoRecurrencia | null;
}

// ──────────────────────────────────────
// Usuarios asignados  (espejo de UsuariosAsignadosDtos.cs)
// ──────────────────────────────────────

export interface UsuarioAsignado {
  id: number;
  nombre: string;
  email: string;
}

export interface CrearUsuarioAsignado {
  nombre: string;
  email: string;
}

export interface ActualizarUsuarioAsignado {
  nombre: string;
  email: string;
}
