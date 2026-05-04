export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  fechaNacimiento: string;
  sexo: string;
  direccion: string;
  telefono?: string;
  rol: string;
  activo: boolean;
  fechaCreacion: string;
}
export interface CambiarRolDto {
  nuevoRolId: number;
}
export const ROL_IDS = { ADMIN: 1, COLABORADOR: 2, CLIENTE: 3 } as const;
