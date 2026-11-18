import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError, finalize, map, shareReplay, tap } from "rxjs/operators";
import { environment } from "../../../../environments/environment";
import {
  AuthResponse,
  DosFactoresSetup,
  LoginResultado,
  UsuarioAuth,
} from "../../models/auth/auth-response.model";
import { LoginDto } from "../../models/auth/login.model";
import { RegistroDto } from "../../models/auth/registro.model";
import { RespuestaDto } from "../../models/comun/respuesta.model";
import { OlvidePasswordDto, ActualizarPerfilDto, Usuario } from "../../models/usuarios/usuario.model";
import { PromocionBienvenidaService } from "../promocion-bienvenida/promocion-bienvenida.service";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly TOKEN_KEY = "rotter_token";
  private readonly REFRESH_TOKEN_KEY = "rotter_refresh_token";
  private readonly USER_KEY = "rotter_user";
  private readonly ACTIVIDAD_KEY = "rotter_ultima_actividad";
  private readonly SESION_EXPIRADA_KEY = "rotter_sesion_expirada";
  private readonly INACTIVIDAD_LIMITE_MS = 30 * 60 * 1000; // 30 minutos

  private refrescando$: Observable<string | null> | null = null;

  usuario = signal<UsuarioAuth | null>(this.leerStorage());

  constructor(
    private http: HttpClient,
    private router: Router,
    private promocionSrv: PromocionBienvenidaService,
  ) {}

  login(dto: LoginDto) {
    return this.http
      .post<RespuestaDto<LoginResultado>>(`${environment.apiUrl}/auth/login`, dto)
      .pipe(
        tap((r) => {
          if (r.exito && !r.datos.requiereDosFactores && r.datos.auth) this.guardar(r.datos.auth);
        }),
      );
  }

  verificarDosFactores(tokenDesafio: string, codigo: string) {
    return this.http
      .post<RespuestaDto<AuthResponse>>(`${environment.apiUrl}/auth/2fa/verificar`, { tokenDesafio, codigo })
      .pipe(
        tap((r) => {
          if (r.exito) this.guardar(r.datos);
        }),
      );
  }

  iniciarDosFactores() {
    return this.http.post<RespuestaDto<DosFactoresSetup>>(`${environment.apiUrl}/auth/2fa/iniciar`, {});
  }

  confirmarDosFactores(codigo: string) {
    return this.http.post<RespuestaDto<boolean>>(`${environment.apiUrl}/auth/2fa/confirmar`, { codigo });
  }

  desactivarDosFactores(codigo: string) {
    return this.http.post<RespuestaDto<boolean>>(`${environment.apiUrl}/auth/2fa/desactivar`, { codigo });
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

  recuperarPassword(dto: OlvidePasswordDto) {
  return this.http.post<RespuestaDto<string>>(
    `${environment.apiUrl}/auth/recuperar-password`,
    dto
  );
}

cambiarPassword(nuevaPassword: string) {
  return this.http.put<RespuestaDto<boolean>>(
    `${environment.apiUrl}/auth/cambiar-password`,
    { nuevaPassword }
  );
}

  logout() {
    if (this.getToken()) this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({ error: () => {} });

    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.ACTIVIDAD_KEY);
    this.usuario.set(null);
    this.router.navigate(["/auth/login"]);
  }

  /** Canjea el refresh token guardado por un nuevo access token. Comparte la llamada en curso si ya hay una. */
  refrescarToken(): Observable<string | null> {
    if (this.refrescando$) return this.refrescando$;

    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return of(null);

    this.refrescando$ = this.http
      .post<RespuestaDto<AuthResponse>>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(
        map((r) => {
          if (!r.exito) throw new Error("Refresh fallido");
          localStorage.setItem(this.TOKEN_KEY, r.datos.token);
          localStorage.setItem(this.REFRESH_TOKEN_KEY, r.datos.refreshToken);
          return r.datos.token;
        }),
        catchError(() => {
          this.logoutForzado("token");
          return of(null);
        }),
        finalize(() => (this.refrescando$ = null)),
        shareReplay(1),
      );

    return this.refrescando$;
  }

  /** Sesión terminada por el propio backend (401) o por inactividad del usuario, no por un logout manual. */
  private logoutForzado(motivo: "inactividad" | "token") {
    localStorage.setItem(this.SESION_EXPIRADA_KEY, motivo);
    this.logout();
  }

  /** Lee (y limpia) el motivo de un logout forzado, para mostrarlo una sola vez en la pantalla de login. */
  consumirMotivoSesionExpirada(): "inactividad" | "token" | null {
    const motivo = localStorage.getItem(this.SESION_EXPIRADA_KEY) as "inactividad" | "token" | null;
    if (motivo) localStorage.removeItem(this.SESION_EXPIRADA_KEY);
    return motivo;
  }

  registrarActividad() {
    if (this.estaAutenticado()) localStorage.setItem(this.ACTIVIDAD_KEY, Date.now().toString());
  }

  /** Se llama una sola vez al iniciar la app. Cierra la sesión automáticamente tras 30 min sin actividad. */
  iniciarMonitoreoInactividad() {
    this.registrarActividad();
    const eventos: (keyof WindowEventMap)[] = ["click", "keydown", "mousemove", "scroll", "touchstart"];
    let ultimoRegistro = 0;
    const alDetectarActividad = () => {
      const ahora = Date.now();
      if (ahora - ultimoRegistro < 5000) return; // throttle: máx. 1 escritura cada 5s
      ultimoRegistro = ahora;
      this.registrarActividad();
    };
    eventos.forEach((e) => window.addEventListener(e, alDetectarActividad, { passive: true }));

    setInterval(() => {
      if (!this.estaAutenticado()) return;
      const ultima = Number(localStorage.getItem(this.ACTIVIDAD_KEY) ?? 0);
      if (Date.now() - ultima > this.INACTIVIDAD_LIMITE_MS) this.logoutForzado("inactividad");
    }, 30_000);
  }

  /** Llamado por el interceptor HTTP cuando el backend responde 401 (token inválido o expirado). */
  manejarTokenInvalido() {
    if (this.estaAutenticado() || this.getToken()) this.logoutForzado("token");
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
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
  esSuperAdmin(): boolean {
    return this.getRol() === "SuperAdministrador";
  }

  private guardar(datos: AuthResponse) {
    localStorage.setItem(this.TOKEN_KEY, datos.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, datos.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(datos.usuario));
    this.usuario.set(datos.usuario);
    // Se revisa una sola vez por login/registro (nunca por navegación) si corresponde
    // mostrar la ruleta de bienvenida.
    this.promocionSrv.verificarAlIniciarSesion();
  }

  private leerStorage(): UsuarioAuth | null {
    try {
      const r = localStorage.getItem(this.USER_KEY);
      return r ? JSON.parse(r) : null;
    } catch {
      return null;
    }
  }

  obtenerPerfil() {
    return this.http.get<RespuestaDto<Usuario>>(`${environment.apiUrl}/usuarios/perfil`);
  }

  actualizarPerfil(dto: ActualizarPerfilDto) {
    return this.http.put<RespuestaDto<Usuario>>(`${environment.apiUrl}/usuarios/perfil`, dto);
  }
}
