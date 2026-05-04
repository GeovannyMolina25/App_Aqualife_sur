import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { AuthService } from "../../core/services/auth/auth.service";
import { ProductosService } from "../../core/services/productos/productos.service";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import {
  Producto,
  CrearProductoDto,
  CATEGORIAS,
} from "../../core/models/productos/producto.model";

@Component({
  selector: "app-productos",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AlertComponent,
    SpinnerComponent,
    PaginationComponent,
  ],
  templateUrl: "./productos.component.html",
  styleUrls: ["./productos.component.css"],
})
export class ProductosComponent implements OnInit {
  productos = signal<Producto[]>([]);
  cargando = signal(false);
  total = signal(0);
  pagina = signal(1);
  totalPaginas = signal(1);
  modal = signal(false);
  guardando = signal(false);
  errorModal = signal("");
  categorias = CATEGORIAS;
  nuevo: CrearProductoDto = {
    nombre: "",
    categoriaId: 0,
    precio: 0,
    stock: 0,
    esPromocion: false,
  };

  constructor(
    public auth: AuthService,
    private svc: ProductosService,
  ) {}
  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.svc.obtenerTodos(this.pagina(), 12).subscribe((r) => {
      this.cargando.set(false);
      if (r.exito) {
        this.productos.set(r.datos.items);
        this.total.set(r.datos.total);
        this.totalPaginas.set(r.datos.totalPaginas);
      }
    });
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargar();
  }
  abrirModal() {
    this.modal.set(true);
  }
  cerrarModal() {
    this.modal.set(false);
    this.errorModal.set("");
    this.nuevo = {
      nombre: "",
      categoriaId: 0,
      precio: 0,
      stock: 0,
      esPromocion: false,
    };
  }

  crear() {
    if (
      !this.nuevo.nombre ||
      !this.nuevo.categoriaId ||
      this.nuevo.precio <= 0
    ) {
      this.errorModal.set("Nombre, categoría y precio son obligatorios.");
      return;
    }
    this.guardando.set(true);
    this.svc.crear(this.nuevo).subscribe({
      next: (r) => {
        this.guardando.set(false);
        r.exito
          ? (this.cerrarModal(), this.cargar())
          : this.errorModal.set(r.mensaje);
      },
      error: () => {
        this.guardando.set(false);
        this.errorModal.set("Error al crear el producto.");
      },
    });
  }
}
