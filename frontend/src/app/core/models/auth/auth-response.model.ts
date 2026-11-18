export interface AuthResponse {
  token: string;
  refreshToken: string;
  usuario: UsuarioAuth;
  expiracion: string;
  debeCambiarPassword: boolean;
}
export interface LoginResultado {
  requiereDosFactores: boolean;
  tokenDesafio: string | null;
  auth: AuthResponse | null;
}
export interface DosFactoresSetup {
  secreto: string;
  uriConfiguracion: string;
}
export interface UsuarioAuth {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
}
