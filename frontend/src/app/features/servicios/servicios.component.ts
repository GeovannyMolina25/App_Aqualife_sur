import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IconComponent } from "../../shared/components/icon/icon.component";
import { FormsModule } from "@angular/forms";
import { environment } from "../../../environments/environment";
import { Producto, Categoria } from "../../core/models/productos/producto.model";
import { CrearCotizacionServicioDto } from "../../core/models/cotizaciones/cotizacion.model";
import { ProductosService, DatosItemConImagen } from "../../core/services/productos/productos.service";
import { CategoriasService } from "../../core/services/categorias/categorias.service";
import { CotizacionesService } from "../../core/services/cotizaciones/cotizaciones.service";
import { CarritoService } from "../../core/services/carrito/carrito.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";

interface FormServicio {
  nombre: string;
  descripcion: string;
  caracteristicas: string;
  categoriaId: number | null;
  precio: number | null;
  esPromocion: boolean;
  precioPromocion: number | null;
}

@Component({
  selector: "app-servicios",
  standalone: true,
  imports: [IconComponent, CommonModule, FormsModule, PublicHeaderComponent, SpinnerComponent, PaginationComponent, AlertComponent],
  templateUrl: "./servicios.component.html",
  styleUrls: ["./servicios.component.css"],
})
export class ServiciosComponent implements OnInit {
  servicios = signal<Producto[]>([]);
  cargando = signal(true);
  pagina = signal(1);
  totalPaginas = signal(1);
  busqueda = "";

  categorias = signal<Categoria[]>([]);
  categoriaSeleccionada = signal<number | null>(null);

  /** Id del servicio recién agregado al carrito (para el estado "Agregado"). */
  agregadoId = signal<number | null>(null);

  // Modal "Cotizar"
  servicioCotizando = signal<Producto | null>(null);
  formCotizar: CrearCotizacionServicioDto = this.formCotizarVacio();
  enviandoCotizacion = signal(false);
  errorCotizacion = signal("");
  cotizacionEnviada = signal(false);

  // Modal "Agregar / Editar servicio" (administrador)
  mostrarModalServicio = signal(false);
  servicioEditando = signal<Producto | null>(null);
  formServicio: FormServicio = this.formServicioVacio();
  imagenServicio: File | null = null;
  vistaPreviaImagen = signal<string | null>(null);
  guardandoServicio = signal(false);
  errorServicio = signal("");
  cambiandoEstadoId = signal<number | null>(null);

  constructor(
    private productosSrv: ProductosService,
    private categoriasSrv: CategoriasService,
    private cotizacionesSrv: CotizacionesService,
    private carrito: CarritoService,
    public auth: AuthService,
  ) {}

  ngOnInit() {
    this.categoriasSrv.obtenerTodas().subscribe((r) => {
      if (!r.exito) return;
      this.categorias.set(r.datos.filter((c) => c.tipo === "Servicio"));
      this.cargar();
    });
  }

  /** Solo el administrador gestiona el catálogo de servicios. */
  esAdmin(): boolean {
    return this.auth.esAdmin() || this.auth.esSuperAdmin();
  }

  cargar() {
    this.cargando.set(true);
    this.productosSrv
      .obtenerTodos(this.pagina(), 12, this.busqueda, this.categoriaSeleccionada() ?? undefined, "Servicio", this.esAdmin())
      .subscribe({
        next: (r) => {
          this.cargando.set(false);
          if (r.exito) {
            this.servicios.set(r.datos.items);
            this.totalPaginas.set(r.datos.totalPaginas);
          }
        },
        error: () => this.cargando.set(false),
      });
  }

  buscar() {
    this.pagina.set(1);
    this.cargar();
  }

  seleccionarCategoria(id: number | null) {
    if (this.categoriaSeleccionada() === id) return;
    this.categoriaSeleccionada.set(id);
    this.pagina.set(1);
    this.cargar();
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargar();
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  imagenSrc(url: string): string {
    if (/^https?:\/\//i.test(url)) return url;
    return `${environment.apiUrl.replace(/\/api\/?$/, "")}${url}`;
  }

  // ===== Precio y compra =====

  precioFinal(s: Producto): number {
    return s.esPromocion && s.precioPromocion != null ? s.precioPromocion : s.precio;
  }

  /** Un servicio sin precio fijo ("a convenir") solo se puede cotizar. */
  tienePrecio(s: Producto): boolean {
    return this.precioFinal(s) > 0;
  }

  /** "Qué incluye": una característica por línea (también acepta ";"). */
  incluye(s: Producto): string[] {
    return (s.caracteristicas ?? "")
      .split(/\r?\n|;/)
      .map((x) => x.trim())
      .filter(Boolean)
      .slice(0, 4);
  }

  agregarAlCarrito(s: Producto) {
    if (!s.activo || !this.tienePrecio(s)) return;
    this.carrito.agregar(s, 1);
    this.agregadoId.set(s.id);
    setTimeout(() => {
      if (this.agregadoId() === s.id) this.agregadoId.set(null);
    }, 1600);
  }

  // ===== Cotizar =====

  private formCotizarVacio(): CrearCotizacionServicioDto {
    return { productoId: 0, nombreContacto: "", telefono: "", email: "", direccion: "", tamanoEspacio: "", fechaDeseada: "", comentario: "" };
  }

  abrirCotizar(p: Producto) {
    this.servicioCotizando.set(p);
    this.errorCotizacion.set("");
    this.cotizacionEnviada.set(false);
    this.formCotizar = this.formCotizarVacio();
    this.formCotizar.productoId = p.id;

    const usuario = this.auth.usuario();
    if (usuario && this.auth.estaAutenticado()) {
      this.formCotizar.nombreContacto = `${usuario.nombre} ${usuario.apellido}`.trim();
      this.formCotizar.email = usuario.email;
      this.auth.obtenerPerfil().subscribe({
        next: (r) => {
          if (r.exito) {
            this.formCotizar.telefono = r.datos.telefono ?? "";
            this.formCotizar.direccion = r.datos.direccion ?? "";
          }
        },
      });
    }
  }

  cerrarCotizar() {
    this.servicioCotizando.set(null);
  }

  enviarCotizacion() {
    const f = this.formCotizar;
    if (!f.nombreContacto.trim() || !f.telefono.trim() || !f.direccion.trim() || !f.tamanoEspacio.trim()) {
      this.errorCotizacion.set("Nombre, teléfono, dirección y tamaño del espacio son obligatorios.");
      return;
    }
    this.enviandoCotizacion.set(true);
    this.errorCotizacion.set("");
    const payload: CrearCotizacionServicioDto = {
      ...f,
      fechaDeseada: f.fechaDeseada || undefined,
      email: f.email || undefined,
      comentario: f.comentario || undefined,
    };
    this.cotizacionesSrv.solicitar(payload).subscribe({
      next: (r) => {
        this.enviandoCotizacion.set(false);
        if (r.exito) this.cotizacionEnviada.set(true);
        else this.errorCotizacion.set(r.mensaje);
      },
      error: (e) => {
        this.enviandoCotizacion.set(false);
        this.errorCotizacion.set(e?.error?.mensaje ?? "No se pudo enviar la solicitud. Intenta de nuevo.");
      },
    });
  }

  // ===== Agregar / editar servicio (administrador) =====

  private formServicioVacio(): FormServicio {
    return {
      nombre: "",
      descripcion: "",
      caracteristicas: "",
      categoriaId: this.categoriaSeleccionada() ?? this.categorias()[0]?.id ?? null,
      precio: null,
      esPromocion: false,
      precioPromocion: null,
    };
  }

  abrirCrearServicio() {
    this.servicioEditando.set(null);
    this.formServicio = this.formServicioVacio();
    this.prepararModal(null);
  }

  abrirEditarServicio(s: Producto) {
    this.servicioEditando.set(s);
    this.formServicio = {
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
    this.imagenServicio = null;
    this.vistaPreviaImagen.set(vistaPrevia);
    this.errorServicio.set("");
    this.mostrarModalServicio.set(true);
  }

  cerrarModalServicio() {
    this.mostrarModalServicio.set(false);
  }

  onImagenServicioSeleccionada(event: Event) {
    const input = event.target as HTMLInputElement;
    this.imagenServicio = input.files?.[0] ?? null;
    if (this.imagenServicio) this.vistaPreviaImagen.set(URL.createObjectURL(this.imagenServicio));
  }

  /** Porcentaje de descuento de la promoción, para mostrarlo en el formulario. */
  descuentoPromo(): number | null {
    const { precio, precioPromocion, esPromocion } = this.formServicio;
    if (!esPromocion || !precio || !precioPromocion || precioPromocion >= precio) return null;
    return Math.round((1 - precioPromocion / precio) * 100);
  }

  guardarServicio() {
    const f = this.formServicio;
    const precio = Number(f.precio ?? 0);
    const precioPromocion = f.precioPromocion != null ? Number(f.precioPromocion) : null;

    if (!f.nombre.trim()) return this.errorServicio.set("El nombre del servicio es obligatorio.");
    if (!f.categoriaId) return this.errorServicio.set("Selecciona una categoría.");
    if (precio < 0) return this.errorServicio.set("El precio no puede ser negativo.");
    if (f.esPromocion) {
      if (precio <= 0) return this.errorServicio.set("Para poner una promoción, primero define el precio normal.");
      if (!precioPromocion || precioPromocion <= 0) return this.errorServicio.set("Ingresa el precio promocional.");
      if (precioPromocion >= precio) return this.errorServicio.set("El precio promocional debe ser menor que el precio normal.");
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

    const editando = this.servicioEditando();
    const peticion = editando
      ? this.productosSrv.actualizarConImagen(editando.id, datos, this.imagenServicio)
      : this.productosSrv.crearConImagen(datos, this.imagenServicio);

    this.guardandoServicio.set(true);
    this.errorServicio.set("");
    peticion.subscribe({
      next: (r) => {
        this.guardandoServicio.set(false);
        if (r.exito) {
          this.mostrarModalServicio.set(false);
          this.cargar();
        } else {
          this.errorServicio.set(r.mensaje);
        }
      },
      error: (e) => {
        this.guardandoServicio.set(false);
        this.errorServicio.set(e?.error?.mensaje ?? "No se pudo guardar el servicio.");
      },
    });
  }

  cambiarEstado(s: Producto) {
    const activar = !s.activo;
    if (!activar && !confirm(`¿Ocultar "${s.nombre}"? Los clientes dejarán de verlo, pero se conserva su historial.`)) return;
    this.cambiandoEstadoId.set(s.id);
    this.productosSrv.cambiarEstado(s.id, activar).subscribe({
      next: (r) => {
        this.cambiandoEstadoId.set(null);
        if (r.exito) this.cargar();
        else alert(r.mensaje);
      },
      error: () => this.cambiandoEstadoId.set(null),
    });
  }
}
