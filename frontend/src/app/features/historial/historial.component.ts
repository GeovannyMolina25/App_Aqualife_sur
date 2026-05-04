import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { Venta } from "../../core/models/ventas/venta.model";

@Component({
  selector: "app-historial",
  standalone: true,
  imports: [CommonModule, SpinnerComponent],
  templateUrl: "./historial.component.html",
  styleUrls: ["./historial.component.css"],
})
export class HistorialComponent implements OnInit {
  ventas = signal<Venta[]>([]);
  cargando = signal(false);
  abierta = signal<number | null>(null);

  constructor(private svc: VentasService) {}

  ngOnInit() {
    this.cargando.set(true);
    this.svc.obtenerHistorial().subscribe((r) => {
      this.cargando.set(false);
      if (r.exito) this.ventas.set(r.datos.items);
    });
  }

  toggle(id: number) {
    this.abierta.set(this.abierta() === id ? null : id);
  }
}
