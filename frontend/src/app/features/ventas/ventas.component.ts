import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { ProductosService } from "../../core/services/productos/productos.service";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { Venta, CrearVentaDto } from "../../core/models/ventas/venta.model";
import { Producto } from "../../core/models/productos/producto.model";
import { Usuario } from "../../core/models/usuarios/usuario.model";
import { UsuariosService } from "../../core/services/usuarios/usuarios.service";

@Component({
  selector: "app-ventas",
  standalone: true,
  imports: [CommonModule, FormsModule, AlertComponent, SpinnerComponent],
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
  private ctds = new Map<number, { cantidad: number; precio: number }>();

  constructor(
    private vs: VentasService,
    private ps: ProductosService,
    private us: UsuariosService,
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
      }
    });
  }
  obtenerClienteSeleccionado() {
  return this.ClietnesDisp().find(
    (u) => u.id === this.dto.clienteId
  );
}
  cargar() {
    this.cargando.set(true);
    this.vs.obtenerTodas().subscribe((r) => {
      this.cargando.set(false);
      if (r.exito) this.ventas.set(r.datos.items);
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
