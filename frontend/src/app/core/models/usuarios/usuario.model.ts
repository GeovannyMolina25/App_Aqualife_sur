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
  twoFactorHabilitado: boolean;
  premioBienvenida?: string;
  recargasParaSeptimo: number;
  premioBienvenidaEntregado: boolean;
}
export interface CambiarRolDto {
  nuevoRolId: number;
}
export interface CrearUsuarioRapidoDto {
  nombre: string;
  apellido: string;
  email?: string;
  telefono?: string;
  direccion?: string;
}
export interface UsuarioRapidoCreado {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
}
export interface OlvidePasswordDto {
  dato: string;
}
export interface ActualizarPerfilDto {
  nombre: string;
  apellido: string;
  direccion: string;
  telefono?: string;
}
export const ROL_IDS = { ADMIN: 1, COLABORADOR: 2, CLIENTE: 3, SUPER_ADMIN: 4 } as const;

