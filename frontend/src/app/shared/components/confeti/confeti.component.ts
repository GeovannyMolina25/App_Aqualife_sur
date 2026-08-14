import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PromocionBienvenidaService } from "../../../core/services/promocion-bienvenida/promocion-bienvenida.service";

interface PiezaConfeti {
  left: string;
  color: string;
  delay: string;
  duracion: string;
  rotacionInicial: string;
}

const COLORES = ["#1a6b8a", "#38a89a", "#4db8a8", "#b8956a", "#2a7a8c", "#f2c94c"];

@Component({
  selector: "app-confeti",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./confeti.component.html",
  styleUrls: ["./confeti.component.css"],
})
export class ConfetiComponent {
  piezas: PiezaConfeti[] = Array.from({ length: 60 }, () => ({
    left: `${Math.random() * 100}%`,
    color: COLORES[Math.floor(Math.random() * COLORES.length)],
    delay: `${Math.random() * 0.6}s`,
    duracion: `${2.2 + Math.random() * 1.4}s`,
    rotacionInicial: `${Math.random() * 360}deg`,
  }));

  constructor(public promoSrv: PromocionBienvenidaService) {}
}
