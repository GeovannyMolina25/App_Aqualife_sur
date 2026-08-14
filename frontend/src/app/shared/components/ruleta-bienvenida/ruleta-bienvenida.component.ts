import { Component, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PromocionBienvenidaService } from "../../../core/services/promocion-bienvenida/promocion-bienvenida.service";
import { SEGMENTOS_RULETA, nombrePremio } from "../../../core/models/usuarios/promocion.model";

const VUELTAS_EXTRA = 6;
const DURACION_ANIMACION_MS = 4600;

@Component({
  selector: "app-ruleta-bienvenida",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./ruleta-bienvenida.component.html",
  styleUrls: ["./ruleta-bienvenida.component.css"],
})
export class RuletaBienvenidaComponent {
  segmentos = SEGMENTOS_RULETA;
  rotacionGrados = signal(0);
  girando = signal(false);
  error = signal("");

  constructor(public promoSrv: PromocionBienvenidaService) {}

  girar() {
    if (this.girando()) return;
    this.girando.set(true);
    this.error.set("");

    this.promoSrv.girar().subscribe({
      next: (r) => {
        if (!r.exito) {
          this.girando.set(false);
          this.error.set(r.mensaje);
          return;
        }
        const resultado = r.datos;
        const anguloCentroSector = resultado.segmentoIndice * 45 + 22.5;
        this.rotacionGrados.set(VUELTAS_EXTRA * 360 + (360 - anguloCentroSector));

        setTimeout(() => {
          this.girando.set(false);
          this.promoSrv.procesarResultadoGiro(resultado);
        }, DURACION_ANIMACION_MS);
      },
      error: (e) => {
        this.girando.set(false);
        this.error.set(e?.error?.mensaje ?? "No se pudo girar la ruleta. Intenta de nuevo.");
      },
    });
  }

  premioSimpleNombre() {
    return nombrePremio(this.promoSrv.premioSimpleGanado() ?? undefined);
  }
}
