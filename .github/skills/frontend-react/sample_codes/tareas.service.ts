// frontend/src/services/tareas.service.ts
// Servicio de llamadas a la API REST del backend para el recurso Tareas.
// El proxy de Vite redirige /api/* → https://localhost:7279.

import type { Tarea, CrearTarea, ActualizarTarea } from '../types';

const BASE = '/api/tareas';

export async function listarTareas(): Promise<Tarea[]> {
  const res = await fetch(BASE);
  if (!res.ok) throw new Error(`Error ${res.status} al listar tareas`);
  return res.json();
}

export async function obtenerTarea(id: number): Promise<Tarea> {
  const res = await fetch(`${BASE}/${id}`);
  if (!res.ok) throw new Error(`Error ${res.status} al obtener la tarea`);
  return res.json();
}

export async function crearTarea(datos: CrearTarea): Promise<Tarea> {
  const res = await fetch(BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(datos),
  });
  if (!res.ok) throw new Error(`Error ${res.status} al crear la tarea`);
  return res.json();
}

export async function actualizarTarea(id: number, datos: ActualizarTarea): Promise<Tarea> {
  const res = await fetch(`${BASE}/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(datos),
  });
  if (!res.ok) throw new Error(`Error ${res.status} al actualizar la tarea`);
  return res.json();
}

export async function eliminarTarea(id: number): Promise<void> {
  const res = await fetch(`${BASE}/${id}`, { method: 'DELETE' });
  if (!res.ok) throw new Error(`Error ${res.status} al eliminar la tarea`);
}

export async function completarTarea(id: number): Promise<Tarea> {
  const res = await fetch(`${BASE}/${id}/completar`, { method: 'POST' });
  if (!res.ok) throw new Error(`Error ${res.status} al completar la tarea`);
  return res.json();
}
