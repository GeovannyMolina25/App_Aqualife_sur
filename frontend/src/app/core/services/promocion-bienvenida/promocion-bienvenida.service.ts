import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { RespuestaDto } from "../../models/comun/respuesta.model";
import {
  EstadoPromocionDto,
  PREMIO_SEPTIMO_BOTELLON,
  ResultadoGiroDto,
} from "../../models/usuarios/promocion.model";

const DURACION_CONFETI_MS = 3000;

@Injectable({ providedIn: "root" })
export class PromocionBienvenidaService {
  private url = `${environment.apiUrl}/usuarios/promocion-bienvenida`;

  mostrarRuleta = signal(false);
  mostrarTarjeta = signal(false);
  tarjetaRecargas = signal(0);
  premioSimpleGanado = signal<string | null>(null);
  mostrarConfeti = signal(false);

  /** Premio ganado y aún no entregado por el personal — controla el aviso
   *  "Reclama tu premio" junto al nombre del usuario en los headers. */
  premioListoParaReclamar = signal(false);

  private confetiTimeoutId?: ReturnType<typeof setTimeout>;

  constructor(private http: HttpClient) {}

  /** Se llama una sola vez, justo después de un login o registro exitoso (nunca por navegación). */
  verificarAlIniciarSesion() {
    this.http.get<RespuestaDto<EstadoPromocionDto>>(this.url).subscribe({
      next: (r) => {
        if (!r.exito) return;
        if (r.datos.debeGirar) this.mostrarRuleta.set(true);
        this.evaluarPremioListo(r.datos);
      },
    });
  }

  /** Sincroniza en silencio el aviso "Reclama tu premio" (nunca dispara la ruleta) —
   *  segura de llamar en cada arranque de la app o navegación, aunque el usuario
   *  ya llevara un rato con la sesión abierta. */
  sincronizarPremioListo() {
    this.http.get<RespuestaDto<EstadoPromocionDto>>(this.url).subscribe({
      next: (r) => {
        if (r.exito) this.evaluarPremioListo(r.datos);
      },
    });
  }

  private evaluarPremioListo(estado: EstadoPromocionDto) {
    if (!estado.premioBienvenida || estado.premioBienvenidaEntregado) {
      this.premioListoParaReclamar.set(false);
      return;
    }
    const listo =
      estado.premioBienvenida === PREMIO_SEPTIMO_BOTELLON ? estado.recargasParaSeptimo >= 7 : true;
    this.premioListoParaReclamar.set(listo);
  }

  girar() {
    return this.http.post<RespuestaDto<ResultadoGiroDto>>(`${this.url}/girar`, {});
  }

  cerrarRuleta() {
    this.mostrarRuleta.set(false);
  }

  /** Se llama con el resultado que acaba de devolver girar(), tras la animación de la rueda. */
  procesarResultadoGiro(resultado: ResultadoGiroDto) {
    this.mostrarRuleta.set(false);
    this.dispararConfeti();
    if (resultado.premioReal === PREMIO_SEPTIMO_BOTELLON) {
      this.tarjetaRecargas.set(0);
      this.mostrarTarjeta.set(true);
    } else {
      this.premioSimpleGanado.set(resultado.premioReal);
      this.premioListoParaReclamar.set(true);
    }
  }

  private dispararConfeti() {
    this.mostrarConfeti.set(true);
    clearTimeout(this.confetiTimeoutId);
    this.confetiTimeoutId = setTimeout(() => this.mostrarConfeti.set(false), DURACION_CONFETI_MS);
  }

  cerrarPremioSimple() {
    this.premioSimpleGanado.set(null);
  }

  /**
   * Se llama tras un checkout exitoso del propio cliente para reflejar la nueva X en la tarjeta.
   * Solo se muestra mientras se está llenando (recarga 1 a 7). Ya completadas las 7 recargas,
   * el cliente sigue comprando normalmente sin que la tarjeta vuelva a aparecer — lo que falta
   * es que el personal le entregue el botellón, no seguir juntando X.
   */
  mostrarProgresoTrasCompra() {
    this.http.get<RespuestaDto<EstadoPromocionDto>>(this.url).subscribe({
      next: (r) => {
        if (!r.exito) return;
        const estado = r.datos;
        this.evaluarPremioListo(estado);
        const sigueLlenandoTarjeta = estado.recargasParaSeptimo > 0 && estado.recargasParaSeptimo <= 7;
        if (
          estado.premioBienvenida === PREMIO_SEPTIMO_BOTELLON &&
          !estado.premioBienvenidaEntregado &&
          sigueLlenandoTarjeta
        ) {
          this.tarjetaRecargas.set(estado.recargasParaSeptimo);
          this.mostrarTarjeta.set(true);
        }
      },
    });
  }

  cerrarTarjeta() {
    this.mostrarTarjeta.set(false);
  }
}
