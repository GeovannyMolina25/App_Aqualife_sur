import { Injectable, computed, effect, signal } from "@angular/core";
import { Producto } from "../../models/productos/producto.model";
import { AuthService } from "../auth/auth.service";

export interface ItemCarrito {
  producto: Producto;
  cantidad: number;
}

const CLAVE_INVITADO = "rotter_carrito_invitado";
const claveDeUsuario = (usuarioId: number) => `rotter_carrito_usuario_${usuarioId}`;

@Injectable({ providedIn: "root" })
export class CarritoService {
  private claveActual: string;
  private items = signal<ItemCarrito[]>([]);

  itemsCarrito = this.items.asReadonly();
  cantidadTotal = computed(() => this.items().reduce((acc, i) => acc + i.cantidad, 0));
  subtotal = computed(() =>
    this.items().reduce((acc, i) => acc + this.precioUnitario(i.producto) * i.cantidad, 0),
  );

  constructor(private auth: AuthService) {
    const usuario = this.auth.usuario();
    this.claveActual = usuario ? claveDeUsuario(usuario.id) : CLAVE_INVITADO;
    this.items.set(this.leerStorage(this.claveActual));

    // El carrito pertenece a la cuenta, no al navegador: al iniciar sesión, lo que
    // se agregó como invitado se fusiona con el carrito guardado de ese usuario;
    // al cerrar sesión, se vuelve al carrito "de invitado" para no mezclar cuentas
    // que compartan el mismo equipo.
    effect(() => {
      const usuarioActual = this.auth.usuario();
      const claveNueva = usuarioActual ? claveDeUsuario(usuarioActual.id) : CLAVE_INVITADO;
      if (claveNueva === this.claveActual) return;

      if (usuarioActual) {
        const carritoInvitado = this.leerStorage(CLAVE_INVITADO);
        const carritoGuardado = this.leerStorage(claveNueva);
        this.claveActual = claveNueva;
        this.guardar(this.fusionar(carritoGuardado, carritoInvitado));
        if (carritoInvitado.length > 0) localStorage.removeItem(CLAVE_INVITADO);
      } else {
        this.claveActual = CLAVE_INVITADO;
        this.items.set(this.leerStorage(CLAVE_INVITADO));
      }
    });
  }

  precioUnitario(producto: Producto): number {
    return producto.esPromocion && producto.precioPromocion != null
      ? producto.precioPromocion
      : producto.precio;
  }

  agregar(producto: Producto, cantidad: number) {
    const actuales = this.items();
    const idx = actuales.findIndex((i) => i.producto.id === producto.id);
    const nuevos =
      idx >= 0
        ? actuales.map((i, n) =>
            n === idx ? { ...i, cantidad: Math.min(i.cantidad + cantidad, producto.stock) } : i,
          )
        : [...actuales, { producto, cantidad: Math.min(cantidad, producto.stock) }];
    this.guardar(nuevos);
  }

  actualizarCantidad(productoId: number, cantidad: number) {
    const nuevos = this.items().map((i) =>
      i.producto.id === productoId
        ? { ...i, cantidad: Math.max(1, Math.min(cantidad, i.producto.stock)) }
        : i,
    );
    this.guardar(nuevos);
  }

  quitar(productoId: number) {
    this.guardar(this.items().filter((i) => i.producto.id !== productoId));
  }

  vaciar() {
    this.guardar([]);
  }

  private fusionar(base: ItemCarrito[], extra: ItemCarrito[]): ItemCarrito[] {
    const resultado = [...base];
    for (const item of extra) {
      const idx = resultado.findIndex((i) => i.producto.id === item.producto.id);
      if (idx >= 0) {
        resultado[idx] = {
          ...resultado[idx],
          cantidad: Math.min(resultado[idx].cantidad + item.cantidad, item.producto.stock),
        };
      } else {
        resultado.push(item);
      }
    }
    return resultado;
  }

  private guardar(items: ItemCarrito[]) {
    this.items.set(items);
    localStorage.setItem(this.claveActual, JSON.stringify(items));
  }

  private leerStorage(clave: string): ItemCarrito[] {
    try {
      const raw = localStorage.getItem(clave);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }
}
