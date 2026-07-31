import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { AuthService } from "../../core/services/auth/auth.service";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { ProductosService } from "../../core/services/productos/productos.service";
import { CajaService } from "../../core/services/caja/caja.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { Metricas } from "../../core/models/ventas/metricas.model";
import { Producto } from "../../core/models/productos/producto.model";
import { SaldoCaja } from "../../core/models/caja/caja.model";

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
  saldoCaja = signal<SaldoCaja | null>(null);
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
    private cs: CajaService,
  ) {}
  ngOnInit() {
    if (this.auth.esAdmin() || this.auth.esColaborador() || this.auth.esSuperAdmin())
      this.vs.obtenerMetricas().subscribe((r) => {
        if (r.exito) this.metricas.set(r.datos);
      });
    if (this.auth.esAdmin() || this.auth.esColaborador() || this.auth.esSuperAdmin())
      this.cs.obtenerSaldo().subscribe((r) => {
        if (r.exito) this.saldoCaja.set(r.datos);
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
