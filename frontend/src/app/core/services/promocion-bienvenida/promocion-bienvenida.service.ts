import { Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { RespuestaDto } from "../../models/comun/respuesta.model";
import {
  EstadoPromocionDto,
  PREMIO_SEPTIMO_BOTELLON,
  ResultadoGiroDto,
} from "../../models/usuarios/promocion.model";

@Injectable({ providedIn: "root" })
export class PromocionBienvenidaService {
  private url = `${environment.apiUrl}/usuarios/promocion-bienvenida`;

  mostrarRuleta = signal(false);
  mostrarTarjeta = signal(false);
  tarjetaRecargas = signal(0);
  premioSimpleGanado = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  /** Se llama una sola vez, justo después de un login o registro exitoso (nunca por navegación). */
  verificarAlIniciarSesion() {
    this.http.get<RespuestaDto<EstadoPromocionDto>>(this.url).subscribe({
      next: (r) => {
        if (r.exito && r.datos.debeGirar) this.mostrarRuleta.set(true);
      },
    });
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
    if (resultado.premioReal === PREMIO_SEPTIMO_BOTELLON) {
      this.tarjetaRecargas.set(0);
      this.mostrarTarjeta.set(true);
    } else {
      this.premioSimpleGanado.set(resultado.premioReal);
    }
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
