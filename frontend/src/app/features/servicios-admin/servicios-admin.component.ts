import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { environment } from "../../../environments/environment";
import { IconComponent } from "../../shared/components/icon/icon.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { ProductosService, DatosItemConImagen } from "../../core/services/productos/productos.service";
import { CategoriasService } from "../../core/services/categorias/categorias.service";
import { Producto, Categoria } from "../../core/models/productos/producto.model";

interface FormServicio {
  nombre: string;
  descripcion: string;
  caracteristicas: string;
  categoriaId: number | null;
  precio: number | null;
  esPromocion: boolean;
  precioPromocion: number | null;
}

/** Gestión interna de servicios (solo administradores): agregar, editar, ocultar/publicar y eliminar. */
@Component({
  selector: "app-servicios-admin",
  standalone: true,
  imports: [IconComponent, CommonModule, FormsModule, AlertComponent, SpinnerComponent, PaginationComponent],
  templateUrl: "./servicios-admin.component.html",
  styleUrls: ["./servicios-admin.component.css"],
})
export class ServiciosAdminComponent implements OnInit {
  servicios = signal<Producto[]>([]);
  categorias = signal<Categoria[]>([]);
  cargando = signal(false);
  total = signal(0);
  pagina = signal(1);
  totalPaginas = signal(1);
  busqueda = signal("");
  aviso = signal("");
  private debounceId?: ReturnType<typeof setTimeout>;

  // Modal agregar / editar
  modal = signal(false);
  editando = signal<Producto | null>(null);
  form: FormServicio = this.formVacio();
  imagen: File | null = null;
  vistaPrevia = signal<string | null>(null);
  guardando = signal(false);
  errorModal = signal("");
  ocupadoId = signal<number | null>(null);

  constructor(
    private svc: ProductosService,
    private categoriasSvc: CategoriasService,
  ) {}

  ngOnInit() {
    this.categoriasSvc.obtenerTodas().subscribe((r) => {
      if (r.exito) this.categorias.set(r.datos.filter((c) => c.tipo === "Servicio"));
    });
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.svc.obtenerTodos(this.pagina(), 12, this.busqueda(), undefined, "Servicio", true).subscribe({
      next: (r) => {
        this.cargando.set(false);
        if (r.exito) {
          this.servicios.set(r.datos.items);
          this.total.set(r.datos.total);
          this.totalPaginas.set(r.datos.totalPaginas);
        }
      },
      error: () => this.cargando.set(false),
    });
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

  imagenSrc(url: string): string {
    if (/^https?:\/\//i.test(url)) return url;
    return `${environment.apiUrl.replace(/\/api\/?$/, "")}${url}`;
  }

  precioFinal(s: Producto): number {
    return s.esPromocion && s.precioPromocion != null ? s.precioPromocion : s.precio;
  }

  incluye(s: Producto): string[] {
    return (s.caracteristicas ?? "").split(/\r?\n|;/).map((x) => x.trim()).filter(Boolean);
  }

  private mostrarAviso(texto: string) {
    this.aviso.set(texto);
    setTimeout(() => this.aviso.set(""), 5000);
  }

  // ===== Agregar / editar =====

  private formVacio(): FormServicio {
    return {
      nombre: "",
      descripcion: "",
      caracteristicas: "",
      categoriaId: this.categorias?.()[0]?.id ?? null,
      precio: null,
      esPromocion: false,
      precioPromocion: null,
    };
  }

  abrirNuevo() {
    this.editando.set(null);
    this.form = this.formVacio();
    this.prepararModal(null);
  }

  abrirEditar(s: Producto) {
    this.editando.set(s);
    this.form = {
      nombre: s.nombre,
      descripcion: s.descripcion ?? "",
      caracteristicas: s.caracteristicas ?? "",
      categoriaId: s.categoriaId,
      precio: s.precio,
      esPromocion: s.esPromocion,
      precioPromocion: s.precioPromocion ?? null,
    };
    this.prepararModal(s.imagenUrl ? this.imagenSrc(s.imagenUrl) : null);
  }

  private prepararModal(vistaPrevia: string | null) {
    this.imagen = null;
    this.vistaPrevia.set(vistaPrevia);
    this.errorModal.set("");
    this.modal.set(true);
  }

  cerrarModal() {
    this.modal.set(false);
  }

  onImagen(event: Event) {
    const input = event.target as HTMLInputElement;
    this.imagen = input.files?.[0] ?? null;
    if (this.imagen) this.vistaPrevia.set(URL.createObjectURL(this.imagen));
  }

  descuento(): number | null {
    const { precio, precioPromocion, esPromocion } = this.form;
    if (!esPromocion || !precio || !precioPromocion || precioPromocion >= precio) return null;
    return Math.round((1 - precioPromocion / precio) * 100);
  }

  guardar() {
    const f = this.form;
    const precio = Number(f.precio ?? 0);
    const precioPromocion = f.precioPromocion != null ? Number(f.precioPromocion) : null;

    if (!f.nombre.trim()) return this.errorModal.set("El nombre del servicio es obligatorio.");
    if (!f.categoriaId) return this.errorModal.set("Selecciona una categoría.");
    if (precio < 0) return this.errorModal.set("El precio no puede ser negativo.");
    if (f.esPromocion) {
      if (precio <= 0) return this.errorModal.set("Para poner una promoción, primero define el precio normal.");
      if (!precioPromocion || precioPromocion <= 0) return this.errorModal.set("Ingresa el precio promocional.");
      if (precioPromocion >= precio) return this.errorModal.set("El precio promocional debe ser menor que el precio normal.");
    }

    const datos: DatosItemConImagen = {
      nombre: f.nombre.trim(),
      descripcion: f.descripcion.trim(),
      caracteristicas: f.caracteristicas.trim(),
      categoriaId: f.categoriaId,
      precio,
      esPromocion: f.esPromocion,
      precioPromocion: f.esPromocion ? precioPromocion : null,
    };

    const s = this.editando();
    const peticion = s ? this.svc.actualizarConImagen(s.id, datos, this.imagen) : this.svc.crearConImagen(datos, this.imagen);

    this.guardando.set(true);
    this.errorModal.set("");
    peticion.subscribe({
      next: (r) => {
        this.guardando.set(false);
        if (r.exito) {
          this.modal.set(false);
          this.mostrarAviso(s ? "Cambios guardados." : "Servicio agregado.");
          this.cargar();
        } else {
          this.errorModal.set(r.mensaje);
        }
      },
      error: (e) => {
        this.guardando.set(false);
        this.errorModal.set(e?.error?.mensaje ?? "No se pudo guardar el servicio.");
      },
    });
  }

  // ===== Ocultar / publicar / eliminar =====

  cambiarEstado(s: Producto) {
    this.ocupadoId.set(s.id);
    this.svc.cambiarEstado(s.id, !s.activo).subscribe({
      next: (r) => {
        this.ocupadoId.set(null);
        if (r.exito) {
          this.mostrarAviso(r.mensaje);
          this.cargar();
        }
      },
      error: () => this.ocupadoId.set(null),
    });
  }

  eliminar(s: Producto) {
    if (!confirm(`¿Eliminar "${s.nombre}"? Si ya tiene ventas o cotizaciones, solo se ocultará para conservar el historial.`)) return;
    this.ocupadoId.set(s.id);
    this.svc.eliminar(s.id).subscribe({
      next: (r) => {
        this.ocupadoId.set(null);
        if (r.exito) {
          this.mostrarAviso(r.mensaje);
          this.cargar();
        }
      },
      error: (e) => {
        this.ocupadoId.set(null);
        this.mostrarAviso(e?.error?.mensaje ?? "No se pudo eliminar.");
      },
    });
  }
}
