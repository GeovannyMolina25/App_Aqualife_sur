import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { AuthService } from "../../core/services/auth/auth.service";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { ProductosService } from "../../core/services/productos/productos.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { Metricas } from "../../core/models/ventas/metricas.model";
import { Producto } from "../../core/models/productos/producto.model";

@Component({
  selector: "app-dashboard",
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.css"],
})
export class DashboardComponent implements OnInit {
  metricas = signal<Metricas | null>(null);
  promociones = signal<Producto[]>([]);
  cargandoPromos = signal(false);
  hoy = new Date().toLocaleDateString("es-ES", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });
  constructor(
    public auth: AuthService,
    private vs: VentasService,
    private ps: ProductosService,
  ) {}
  ngOnInit() {
    if (this.auth.esAdmin() || this.auth.esColaborador())
      this.vs.obtenerMetricas().subscribe((r) => {
        if (r.exito) this.metricas.set(r.datos);
      });
    if (this.auth.esCliente()) {
      this.cargandoPromos.set(true);
      this.ps.obtenerPromociones().subscribe((r) => {
        this.cargandoPromos.set(false);
        if (r.exito) this.promociones.set(r.datos);
      });
    }
  }
}
