import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IconComponent } from "../icon/icon.component";
import { PromocionBienvenidaService } from "../../../core/services/promocion-bienvenida/promocion-bienvenida.service";

@Component({
  selector: "app-tarjeta-botellon",
  standalone: true,
  imports: [IconComponent, CommonModule],
  templateUrl: "./tarjeta-botellon.component.html",
  styleUrls: ["./tarjeta-botellon.component.css"],
})
export class TarjetaBotellonComponent {
  circulos = [0, 1, 2, 3, 4, 5];

  constructor(public promoSrv: PromocionBienvenidaService) {}

  marcado(indice: number): boolean {
    return indice < this.promoSrv.tarjetaRecargas();
  }

  esUltimaMarcada(indice: number): boolean {
    return indice === this.promoSrv.tarjetaRecargas() - 1;
  }
}
