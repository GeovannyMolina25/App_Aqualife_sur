import { Component, OnInit, signal, computed } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { CajaService } from "../../core/services/caja/caja.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import {
  CrearIngresoCajaDto,
  CrearSalidaCajaDto,
  MovimientoCaja,
  SaldoCaja,
} from "../../core/models/caja/caja.model";
import { Venta } from "../../core/models/ventas/venta.model";

const BANCOS = [
  "Banco Pichincha",
  "Banco Produbanco",
  "Banco Guayaquil",
  "Banco del Pacífico",
  "Otro",
];

@Component({
  selector: "app-caja",
  standalone: true,
  imports: [CommonModule, FormsModule, AlertComponent, SpinnerComponent, PaginationComponent],
  templateUrl: "./caja.component.html",
  styleUrls: ["./caja.component.css"],
})
export class CajaComponent implements OnInit {
  bancos = BANCOS;

  saldo = signal<SaldoCaja | null>(null);
  cargandoSaldo = signal(false);

  historial = signal<MovimientoCaja[]>([]);
  cargandoHistorial = signal(false);
  pagina = signal(1);
  totalPaginas = signal(1);
  busquedaHistorial = signal("");
  private debounceHistorialId?: ReturnType<typeof setTimeout>;

  ventasPendientes = signal<Venta[]>([]);
  ventasSeleccionadas = signal<Set<number>>(new Set());

  modalIngreso = signal(false);
  modalSalida = signal(false);
  guardando = signal(false);
  errorModal = signal("");
  exitoModal = signal("");

  ingresoDto: CrearIngresoCajaDto = { banco: "", numeroTransaccion: "", totalIngresado: 0 };
  salidaDto: CrearSalidaCajaDto = { valor: 0, motivo: "" };
  bancoIngresoSel = signal("");
  bancoIngresoOtro = signal("");
  bancoSalidaSel = signal("");
  bancoSalidaOtro = signal("");

  totalSeleccionado = computed(() => {
    const ids = this.ventasSeleccionadas();
    return this.ventasPendientes()
      .filter((v) => ids.has(v.id))
      .reduce((s, v) => s + v.total, 0);
  });
  totalBloqueado = computed(() => this.ventasSeleccionadas().size > 0);

  busquedaVentasPendientes = signal("");
  ventasPendientesFiltradas = computed(() => {
    const texto = this.busquedaVentasPendientes().trim().toLowerCase();
    if (!texto) return this.ventasPendientes();
    return this.ventasPendientes().filter((v) =>
      `${v.numeroVenta} ${v.nombreCliente}`.toLowerCase().includes(texto),
    );
  });

  constructor(
    private svc: CajaService,
    public auth: AuthService,
  ) {}

  ngOnInit() {
    this.cargarSaldo();
    if (this.auth.esSuperAdmin()) this.cargarHistorial();
  }

  cargarSaldo() {
    this.cargandoSaldo.set(true);
    this.svc.obtenerSaldo().subscribe((r) => {
      this.cargandoSaldo.set(false);
      if (r.exito) this.saldo.set(r.datos);
    });
  }

  cargarHistorial() {
    this.cargandoHistorial.set(true);
    this.svc.obtenerHistorial(this.pagina(), 20, this.busquedaHistorial()).subscribe((r) => {
      this.cargandoHistorial.set(false);
      if (r.exito) {
        this.historial.set(r.datos.items);
        this.totalPaginas.set(r.datos.totalPaginas);
      }
    });
  }

  buscarHistorial(texto: string) {
    this.busquedaHistorial.set(texto);
    this.pagina.set(1);
    clearTimeout(this.debounceHistorialId);
    this.debounceHistorialId = setTimeout(() => this.cargarHistorial(), 350);
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargarHistorial();
  }

  abrirModalIngreso() {
    this.ingresoDto = { banco: "", numeroTransaccion: "", totalIngresado: 0 };
    this.bancoIngresoSel.set("");
    this.bancoIngresoOtro.set("");
    this.ventasSeleccionadas.set(new Set());
    this.busquedaVentasPendientes.set("");
    this.errorModal.set("");
    this.exitoModal.set("");
    this.modalIngreso.set(true);
    this.svc.obtenerVentasPendientes().subscribe((r) => {
      if (r.exito) this.ventasPendientes.set(r.datos);
    });
  }

  cerrarModalIngreso() {
    this.modalIngreso.set(false);
  }

  toggleVenta(id: number) {
    const set = new Set(this.ventasSeleccionadas());
    set.has(id) ? set.delete(id) : set.add(id);
    this.ventasSeleccionadas.set(set);
    if (set.size > 0) this.ingresoDto.totalIngresado = this.totalSeleccionado();
  }

  registrarIngreso() {
    const banco = this.bancoIngresoSel() === "Otro" ? this.bancoIngresoOtro().trim() : this.bancoIngresoSel();
    if (!banco || !this.ingresoDto.numeroTransaccion || this.ingresoDto.totalIngresado <= 0) {
      this.errorModal.set("Banco, número de transacción y total son obligatorios.");
      return;
    }
    this.guardando.set(true);
    const dto: CrearIngresoCajaDto = {
      ...this.ingresoDto,
      banco,
      ventaIds: Array.from(this.ventasSeleccionadas()),
    };
    this.svc.registrarIngreso(dto).subscribe({
      next: (r) => {
        this.guardando.set(false);
        if (r.exito) {
          this.exitoModal.set("✓ Ingreso registrado.");
          setTimeout(() => {
            this.cerrarModalIngreso();
            this.cargarSaldo();
            this.cargarHistorial();
          }, 1200);
        } else this.errorModal.set(r.mensaje);
      },
      error: () => {
        this.guardando.set(false);
        this.errorModal.set("Error al registrar el ingreso.");
      },
    });
  }

  abrirModalSalida() {
    this.salidaDto = { valor: 0, motivo: "" };
    this.bancoSalidaSel.set("");
    this.bancoSalidaOtro.set("");
    this.errorModal.set("");
    this.exitoModal.set("");
    this.modalSalida.set(true);
  }

  cerrarModalSalida() {
    this.modalSalida.set(false);
  }

  registrarSalida() {
    if (!this.salidaDto.motivo || this.salidaDto.valor <= 0) {
      this.errorModal.set("Motivo y valor son obligatorios.");
      return;
    }
    const banco = this.bancoSalidaSel() === "Otro" ? this.bancoSalidaOtro().trim() : this.bancoSalidaSel();
    this.guardando.set(true);
    const dto: CrearSalidaCajaDto = { ...this.salidaDto, banco: banco || undefined };
    this.svc.registrarSalida(dto).subscribe({
      next: (r) => {
        this.guardando.set(false);
        if (r.exito) {
          this.exitoModal.set("✓ Salida registrada.");
          setTimeout(() => {
            this.cerrarModalSalida();
            this.cargarSaldo();
            this.cargarHistorial();
          }, 1200);
        } else this.errorModal.set(r.mensaje);
      },
      error: () => {
        this.guardando.set(false);
        this.errorModal.set("Error al registrar la salida.");
      },
    });
  }
}
