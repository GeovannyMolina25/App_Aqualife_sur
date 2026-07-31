import { Component, OnInit, signal, computed } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { ProductosService } from "../../core/services/productos/productos.service";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { Venta, CrearVentaDto, ESTADOS_VENTA } from '../../core/models/ventas/venta.model';
import { Producto } from "../../core/models/productos/producto.model";
import { Usuario } from "../../core/models/usuarios/usuario.model";
import { UsuariosService } from "../../core/services/usuarios/usuarios.service";
import { AuthService } from "../../core/services/auth/auth.service";

@Component({
  selector: "app-ventas",
  standalone: true,
  imports: [CommonModule, FormsModule, AlertComponent, SpinnerComponent, PaginationComponent],
  templateUrl: "./ventas.component.html",
  styleUrls: ["./ventas.component.css"],
})
export class VentasComponent implements OnInit {
  ventas = signal<Venta[]>([]);
  ClietnesDisp = signal<Usuario[]>([])
  productosDisp = signal<Producto[]>([]);
  cargando = signal(false);
  modal = signal(false);
  guardando = signal(false);
  errorModal = signal("");
  exitoModal = signal("");
  totalEstimado = signal(0);
  dto: CrearVentaDto = { clienteId: 9, items: [] };
  modalFactura = signal(false);
  ventaSeleccionada = signal<Venta | null>(null);
  clienteBusqueda = signal("");
  mostrarListaClientes = signal(false);
  busquedaVentas = signal("");
  pagina = signal(1);
  total = signal(0);
  totalPaginas = signal(1);
  private debounceVentasId?: ReturnType<typeof setTimeout>;
  private ctds = new Map<number, { cantidad: number; precio: number }>();

  clientesFiltrados = computed(() => {
    const texto = this.clienteBusqueda().trim().toLowerCase();
    if (!texto) return this.ClietnesDisp();
    return this.ClietnesDisp().filter((u) =>
      `${u.nombre} ${u.apellido} ${u.email} ${u.direccion}`
        .toLowerCase()
        .includes(texto),
    );
  });

  estadosVenta = ESTADOS_VENTA;

  constructor(
    private vs: VentasService,
    private ps: ProductosService,
    private us: UsuariosService,
    public auth: AuthService,
  ) {}
  ngOnInit() {
    this.cargar();
    //productos
    this.ps.obtenerTodos(1, 100).subscribe((r) => {
      if (r.exito) this.productosDisp.set(r.datos.items);
    });
    // clientes
    this.us.obtenerTodos(1, 100).subscribe((r) => {
      if (r.exito) {
        const clientes = r.datos.items
          .filter((u) => u.rol === "Cliente")
          .sort((a, b) => {
            if (a.id === 9) return -1;
            if (b.id === 9) return 1;
            return 0;
          });
        this.ClietnesDisp.set(clientes);
        const actual = clientes.find((u) => u.id === this.dto.clienteId);
        if (actual) this.clienteBusqueda.set(`${actual.nombre} ${actual.apellido}`);
      }
    });
  }
  obtenerClienteSeleccionado() {
  return this.ClietnesDisp().find(
    (u) => u.id === this.dto.clienteId
  );
}
  seleccionarCliente(u: Usuario) {
    this.dto.clienteId = u.id;
    this.clienteBusqueda.set(`${u.nombre} ${u.apellido}`);
    this.mostrarListaClientes.set(false);
  }
  ocultarListaClientes() {
    setTimeout(() => this.mostrarListaClientes.set(false), 150);
  }
  cambiarEstado(v: Venta, nuevoEstado: string) {
    if (nuevoEstado === v.estado) return;
    this.vs.cambiarEstado(v.id, { nuevoEstado }).subscribe((r) => {
      if (r.exito) v.estado = nuevoEstado;
    });
  }

  buscarVentas(texto: string) {
    this.busquedaVentas.set(texto);
    this.pagina.set(1);
    clearTimeout(this.debounceVentasId);
    this.debounceVentasId = setTimeout(() => this.cargar(), 350);
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.vs
      .obtenerTodas(this.pagina(), 20, undefined, undefined, this.busquedaVentas())
      .subscribe((r) => {
        this.cargando.set(false);
        if (r.exito) {
          this.ventas.set(r.datos.items);
          this.total.set(r.datos.total);
          this.totalPaginas.set(r.datos.totalPaginas);
        }
      });
  }
  getCantidad(id: number) {
    return this.ctds.get(id)?.cantidad ?? 0;
  }
  setCantidad(id: number, qty: number, p: Producto) {
    qty > 0
      ? this.ctds.set(id, {
          cantidad: qty,
          precio: p.precioPromocion ?? p.precio,
        })
      : this.ctds.delete(id);
    this.dto.items = Array.from(this.ctds.entries()).map(([productoId, v]) => ({
      productoId,
      cantidad: v.cantidad,
    }));
    this.totalEstimado.set(
      Array.from(this.ctds.values()).reduce(
        (s, v) => s + v.precio * v.cantidad,
        0,
      ),
    );
  }
  cerrarModal() {
    this.modal.set(false);
    this.errorModal.set("");
    this.exitoModal.set("");
    this.ctds.clear();
    this.totalEstimado.set(0);
    this.dto = { clienteId: 0, items: [] };
    this.clienteBusqueda.set("");
    this.mostrarListaClientes.set(false);
  }
  abrirFactura(Venta: Venta) {
    this.ventaSeleccionada.set(Venta);
    this.modalFactura.set(true);
  }
  cerrarFactura(){
    this.modalFactura.set(false);
    this.ventaSeleccionada.set(null);
  }
  descargarPdf() {
    const venta = this.ventaSeleccionada();
    if (!venta) return;
    this.vs.descargarFacturaPdf(venta.id)
      .subscribe({
        next: (pdf) => {
          const blob = new Blob(
            [pdf],
            { type: 'application/pdf' }
          );
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `Factura_${venta.numeroVenta}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: (err) => {
          console.error(err);
          alert('Error al descargar PDF');
        }
      });
  }
  registrar() {
    if (!this.dto.clienteId || this.dto.items.length === 0) {
      this.errorModal.set(
        "Ingresa el ID del cliente y selecciona al menos un producto.",
      );
      return;
    }
    this.guardando.set(true);
    this.vs.registrar(this.dto).subscribe({
      next: (r) => {
        this.guardando.set(false);
        r.exito
          ? (this.exitoModal.set("✓ Venta registrada."),
            setTimeout(() => {
              this.cerrarModal();
              this.cargar();
            }, 1500))
          : this.errorModal.set(r.mensaje);
      },
      error: () => {
        this.guardando.set(false);
        this.errorModal.set("Error al registrar la venta.");
      },
    });
  }
}
