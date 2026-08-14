import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { environment } from "../../../environments/environment";
import { Producto, Categoria } from "../../core/models/productos/producto.model";
import { CrearCotizacionServicioDto } from "../../core/models/cotizaciones/cotizacion.model";
import { ProductosService } from "../../core/services/productos/productos.service";
import { CategoriasService } from "../../core/services/categorias/categorias.service";
import { CotizacionesService } from "../../core/services/cotizaciones/cotizaciones.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";

@Component({
  selector: "app-servicios",
  standalone: true,
  imports: [CommonModule, FormsModule, PublicHeaderComponent, SpinnerComponent, PaginationComponent, AlertComponent],
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

  // Modal "Cotizar"
  servicioCotizando = signal<Producto | null>(null);
  formCotizar: CrearCotizacionServicioDto = this.formCotizarVacio();
  enviandoCotizacion = signal(false);
  errorCotizacion = signal("");
  cotizacionEnviada = signal(false);

  // Modal "Agregar servicio" (staff)
  mostrarModalCrearServicio = signal(false);
  nuevoServicio = { nombre: "", descripcion: "" };
  imagenServicio: File | null = null;
  guardandoServicio = signal(false);
  errorCrearServicio = signal("");

  constructor(
    private productosSrv: ProductosService,
    private categoriasSrv: CategoriasService,
    private cotizacionesSrv: CotizacionesService,
    public auth: AuthService,
  ) {}

  ngOnInit() {
    this.categoriasSrv.obtenerTodas().subscribe((r) => {
      if (!r.exito) return;
      this.categorias.set(r.datos.filter((c) => c.tipo === "Servicio"));
      this.cargar();
    });
  }

  cargar() {
    this.cargando.set(true);
    this.productosSrv
      .obtenerTodos(this.pagina(), 12, this.busqueda, this.categoriaSeleccionada() ?? undefined, "Servicio")
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

  categoriaActual(): Categoria | null {
    const id = this.categoriaSeleccionada();
    return id ? (this.categorias().find((c) => c.id === id) ?? null) : (this.categorias()[0] ?? null);
  }

  puedeCrearServicios(): boolean {
    return this.auth.esAdmin() || this.auth.esColaborador() || this.auth.esSuperAdmin();
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

  // ===== Agregar servicio (staff) =====

  abrirCrearServicio() {
    this.nuevoServicio = { nombre: "", descripcion: "" };
    this.imagenServicio = null;
    this.errorCrearServicio.set("");
    this.mostrarModalCrearServicio.set(true);
  }

  cerrarCrearServicio() {
    this.mostrarModalCrearServicio.set(false);
  }

  onImagenServicioSeleccionada(event: Event) {
    const input = event.target as HTMLInputElement;
    this.imagenServicio = input.files?.[0] ?? null;
  }

  guardarServicio() {
    const categoria = this.categoriaActual();
    if (!this.nuevoServicio.nombre.trim() || !categoria) {
      this.errorCrearServicio.set("El nombre del servicio es obligatorio.");
      return;
    }
    this.guardandoServicio.set(true);
    this.errorCrearServicio.set("");
    this.productosSrv
      .crearConImagen(this.nuevoServicio.nombre.trim(), this.nuevoServicio.descripcion.trim(), categoria.id, this.imagenServicio)
      .subscribe({
        next: (r) => {
          this.guardandoServicio.set(false);
          if (r.exito) {
            this.mostrarModalCrearServicio.set(false);
            this.cargar();
          } else {
            this.errorCrearServicio.set(r.mensaje);
          }
        },
        error: (e) => {
          this.guardandoServicio.set(false);
          this.errorCrearServicio.set(e?.error?.mensaje ?? "No se pudo crear el servicio.");
        },
      });
  }
}
