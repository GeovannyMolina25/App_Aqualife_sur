import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { tap } from "rxjs/operators";
import { environment } from "../../../../environments/environment";
import {
  AuthResponse,
  UsuarioAuth,
} from "../../models/auth/auth-response.model";
import { LoginDto } from "../../models/auth/login.model";
import { RegistroDto } from "../../models/auth/registro.model";
import { RespuestaDto } from "../../models/comun/respuesta.model";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly TOKEN_KEY = "rotter_token";
  private readonly USER_KEY = "rotter_user";

  usuario = signal<UsuarioAuth | null>(this.leerStorage());

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  login(dto: LoginDto) {
    return this.http
      .post<RespuestaDto<AuthResponse>>(`${environment.apiUrl}/auth/login`, dto)
      .pipe(
        tap((r) => {
          if (r.exito) this.guardar(r.datos);
        }),
      );
  }

  registro(dto: RegistroDto) {
    return this.http
      .post<
        RespuestaDto<AuthResponse>
      >(`${environment.apiUrl}/auth/registro`, dto)
      .pipe(
        tap((r) => {
          if (r.exito) this.guardar(r.datos);
        }),
      );
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.usuario.set(null);
    this.router.navigate(["/auth/login"]);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  estaAutenticado(): boolean {
    const t = this.getToken();
    if (!t) return false;
    try {
      const p = JSON.parse(atob(t.split(".")[1]));
      return p.exp > Date.now() / 1000;
    } catch {
      return false;
    }
  }

  getRol(): string {
    return this.usuario()?.rol ?? "";
  }
  esAdmin(): boolean {
    return this.getRol() === "Administrador";
  }
  esColaborador(): boolean {
    return this.getRol() === "Colaborador";
  }
  esCliente(): boolean {
    return this.getRol() === "Cliente";
  }

  private guardar(datos: AuthResponse) {
    localStorage.setItem(this.TOKEN_KEY, datos.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(datos.usuario));
    this.usuario.set(datos.usuario);
  }

  private leerStorage(): UsuarioAuth | null {
    try {
      const r = localStorage.getItem(this.USER_KEY);
      return r ? JSON.parse(r) : null;
    } catch {
      return null;
    }
  }
}
