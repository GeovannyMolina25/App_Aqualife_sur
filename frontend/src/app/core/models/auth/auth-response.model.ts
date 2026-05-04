export interface AuthResponse {
  token: string;
  refreshToken: string;
  usuario: UsuarioAuth;
  expiracion: string;
}
export interface UsuarioAuth {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
}
