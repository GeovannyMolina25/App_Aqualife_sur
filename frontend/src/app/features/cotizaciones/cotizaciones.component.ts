import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { CotizacionesService } from "../../core/services/cotizaciones/cotizaciones.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { CotizacionServicioDto, EstadoCotizacion, ESTADOS_COTIZACION } from "../../core/models/cotizaciones/cotizacion.model";

@Component({
  selector: "app-cotizaciones",
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent, PaginationComponent],
  templateUrl: "./cotizaciones.component.html",
  styleUrls: ["./cotizaciones.component.css"],
})
export class CotizacionesComponent implements OnInit {
  cotizaciones = signal<CotizacionServicioDto[]>([]);
  cargando = signal(false);
  pagina = signal(1);
  totalPaginas = signal(1);
  busqueda = signal("");
  estadoFiltro = signal<EstadoCotizacion | null>(null);

  estados = ESTADOS_COTIZACION;
  detalle = signal<CotizacionServicioDto | null>(null);

  private debounceId?: ReturnType<typeof setTimeout>;

  constructor(private cotizacionesSrv: CotizacionesService) {}

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.cotizacionesSrv
      .obtenerTodas(this.pagina(), 20, this.estadoFiltro() ?? undefined, this.busqueda())
      .subscribe({
        next: (r) => {
          this.cargando.set(false);
          if (r.exito) {
            this.cotizaciones.set(r.datos.items);
            this.totalPaginas.set(r.datos.totalPaginas);
          }
        },
        error: () => this.cargando.set(false),
      });
  }

  filtrarPorEstado(estado: EstadoCotizacion | null) {
    if (this.estadoFiltro() === estado) return;
    this.estadoFiltro.set(estado);
    this.pagina.set(1);
    this.cargar();
  }

  buscar(texto: string) {
    this.busqueda.set(texto);
    this.pagina.set(1);
    clearTimeout(this.debounceId);
    this.debounceId = setTimeout(() => this.cargar(), 350);
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargar();
  }

  cambiarEstado(c: CotizacionServicioDto, nuevoEstado: EstadoCotizacion) {
    if (nuevoEstado === c.estado) return;
    this.cotizacionesSrv.cambiarEstado(c.id, nuevoEstado).subscribe((r) => {
      if (r.exito) {
        c.estado = nuevoEstado;
        if (this.detalle()?.id === c.id) this.detalle.set({ ...c });
      }
    });
  }

  verDetalle(c: CotizacionServicioDto) {
    this.detalle.set(c);
  }

  cerrarDetalle() {
    this.detalle.set(null);
  }

  telHref(telefono: string): string {
    return `tel:${telefono.replace(/\s+/g, "")}`;
  }
}
