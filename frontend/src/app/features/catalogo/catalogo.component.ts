import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { environment } from "../../../environments/environment";
import { Producto, Categoria } from "../../core/models/productos/producto.model";
import { ProductosService } from "../../core/services/productos/productos.service";
import { CategoriasService } from "../../core/services/categorias/categorias.service";
import { CarritoService } from "../../core/services/carrito/carrito.service";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";

@Component({
  selector: "app-catalogo",
  standalone: true,
  imports: [CommonModule, FormsModule, PublicHeaderComponent, SpinnerComponent, PaginationComponent],
  templateUrl: "./catalogo.component.html",
  styleUrls: ["./catalogo.component.css"],
})
export class CatalogoComponent implements OnInit {
  productos = signal<Producto[]>([]);
  cargando = signal(true);
  pagina = signal(1);
  totalPaginas = signal(1);
  busqueda = "";
  cantidades: Record<number, number> = {};
  agregadoId = signal<number | null>(null);

  categorias = signal<Categoria[]>([]);
  categoriaSeleccionada = signal<number | null>(null);

  constructor(
    private productosSrv: ProductosService,
    private categoriasSrv: CategoriasService,
    public carrito: CarritoService,
  ) {}

  ngOnInit() {
    this.categoriasSrv.obtenerTodas().subscribe((r) => {
      if (r.exito) this.categorias.set(r.datos.filter((c) => c.tipo === "Producto"));
    });
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.productosSrv
      .obtenerTodos(this.pagina(), 12, this.busqueda, this.categoriaSeleccionada() ?? undefined, "Producto")
      .subscribe({
        next: (r) => {
          this.cargando.set(false);
          if (r.exito) {
            this.productos.set(r.datos.items);
            this.totalPaginas.set(r.datos.totalPaginas);
            r.datos.items.forEach((p) => {
              if (!(p.id in this.cantidades)) this.cantidades[p.id] = 1;
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

  precio(p: Producto): number {
    return p.esPromocion && p.precioPromocion != null ? p.precioPromocion : p.precio;
  }

  subtotal(p: Producto): number {
    return this.precio(p) * (this.cantidades[p.id] ?? 1);
  }

  incrementar(p: Producto) {
    this.cantidades[p.id] = Math.min((this.cantidades[p.id] ?? 1) + 1, p.stock);
  }

  decrementar(p: Producto) {
    this.cantidades[p.id] = Math.max((this.cantidades[p.id] ?? 1) - 1, 1);
  }

  onCantidadInput(p: Producto, valor: string) {
    const n = parseInt(valor, 10);
    this.cantidades[p.id] = isNaN(n) ? 1 : Math.max(1, Math.min(n, p.stock));
  }

  agregarAlCarrito(p: Producto, event: MouseEvent) {
    if (p.stock <= 0) return;
    this.carrito.agregar(p, this.cantidades[p.id] ?? 1);
    this.agregadoId.set(p.id);
    this.volarGotaAlCarrito(event.currentTarget as HTMLElement);
    setTimeout(() => {
      if (this.agregadoId() === p.id) this.agregadoId.set(null);
    }, 1600);
  }

  /** Anima una gotita desde el botón presionado hasta el ícono del carrito en el header. */
  private volarGotaAlCarrito(origenEl: HTMLElement) {
    const carritoIcono = document.querySelector<HTMLElement>(".carrito-link");
    if (!carritoIcono || !origenEl) return;

    const origen = origenEl.getBoundingClientRect();
    const destino = carritoIcono.getBoundingClientRect();
    const origenX = origen.left + origen.width / 2;
    const origenY = origen.top + origen.height / 2;
    const destinoX = destino.left + destino.width / 2;
    const destinoY = destino.top + destino.height / 2;

    const gota = document.createElement("div");
    gota.textContent = "💧";
    gota.style.cssText = `
      position: fixed;
      left: 0; top: 0;
      font-size: 1.5rem;
      z-index: 9999;
      pointer-events: none;
      will-change: transform, opacity;
    `;
    document.body.appendChild(gota);

    const puntoMedioY = Math.min(origenY, destinoY) - 90;

    const animacion = gota.animate(
      [
        { transform: `translate(${origenX}px, ${origenY}px) scale(1)`, opacity: 1, offset: 0 },
        {
          transform: `translate(${(origenX + destinoX) / 2}px, ${puntoMedioY}px) scale(1.3)`,
          opacity: 1,
          offset: 0.55,
        },
        { transform: `translate(${destinoX}px, ${destinoY}px) scale(0.3)`, opacity: 0.2, offset: 1 },
      ],
      { duration: 750, easing: "cubic-bezier(0.3, 0, 0.6, 1)" },
    );

    animacion.onfinish = () => {
      gota.remove();
      carritoIcono.animate(
        [
          { transform: "scale(1)" },
          { transform: "scale(1.35)" },
          { transform: "scale(1)" },
        ],
        { duration: 350, easing: "ease-out" },
      );
    };
  }
}
