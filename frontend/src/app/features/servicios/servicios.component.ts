import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IconComponent } from "../../shared/components/icon/icon.component";
import { FormsModule } from "@angular/forms";
import { environment } from "../../../environments/environment";
import { Producto, Categoria } from "../../core/models/productos/producto.model";
import { CrearCotizacionServicioDto } from "../../core/models/cotizaciones/cotizacion.model";
import { ProductosService } from "../../core/services/productos/productos.service";
import { CategoriasService } from "../../core/services/categorias/categorias.service";
import { CotizacionesService } from "../../core/services/cotizaciones/cotizaciones.service";
import { CarritoService, MAX_CANTIDAD_SERVICIO } from "../../core/services/carrito/carrito.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";

/** Catálogo público de servicios: ver y comprar (la gestión está en el panel interno). */
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

  cantidades: Record<number, number> = {};
  agregadoId = signal<number | null>(null);

  // Modal "Cotizar" (solo para servicios sin precio fijo)
  servicioCotizando = signal<Producto | null>(null);
  formCotizar: CrearCotizacionServicioDto = this.formCotizarVacio();
  enviandoCotizacion = signal(false);
  errorCotizacion = signal("");
  cotizacionEnviada = signal(false);

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
            r.datos.items.forEach((s) => {
              if (!(s.id in this.cantidades)) this.cantidades[s.id] = 1;
            });
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

  // ===== Precio y compra (igual que el catálogo de productos) =====

  precio(s: Producto): number {
    return s.esPromocion && s.precioPromocion != null ? s.precioPromocion : s.precio;
  }

  /** Un servicio sin precio fijo ("a convenir") no se compra directo: se cotiza. */
  tienePrecio(s: Producto): boolean {
    return this.precio(s) > 0;
  }

  incluye(s: Producto): string[] {
    return (s.caracteristicas ?? "")
      .split(/\r?\n|;/)
      .map((x) => x.trim())
      .filter(Boolean)
      .slice(0, 4);
  }

  subtotal(s: Producto): number {
    return this.precio(s) * (this.cantidades[s.id] ?? 1);
  }

  incrementar(s: Producto) {
    this.cantidades[s.id] = Math.min((this.cantidades[s.id] ?? 1) + 1, MAX_CANTIDAD_SERVICIO);
  }

  decrementar(s: Producto) {
    this.cantidades[s.id] = Math.max((this.cantidades[s.id] ?? 1) - 1, 1);
  }

  onCantidadInput(s: Producto, valor: string) {
    const n = parseInt(valor, 10);
    this.cantidades[s.id] = isNaN(n) ? 1 : Math.max(1, Math.min(n, MAX_CANTIDAD_SERVICIO));
  }

  agregarAlCarrito(s: Producto) {
    if (!this.tienePrecio(s)) return;
    this.carrito.agregar(s, this.cantidades[s.id] ?? 1);
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
}
