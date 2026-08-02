import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { CarritoService, ItemCarrito } from "../../core/services/carrito/carrito.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";

@Component({
  selector: "app-carrito",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PublicHeaderComponent],
  templateUrl: "./carrito.component.html",
  styleUrls: ["./carrito.component.css"],
})
export class CarritoComponent {
  constructor(
    public carrito: CarritoService,
    private auth: AuthService,
    private router: Router,
  ) {}

  precioUnitario(item: ItemCarrito): number {
    return this.carrito.precioUnitario(item.producto);
  }

  subtotalItem(item: ItemCarrito): number {
    return this.precioUnitario(item) * item.cantidad;
  }

  incrementar(item: ItemCarrito) {
    this.carrito.actualizarCantidad(item.producto.id, item.cantidad + 1);
  }

  decrementar(item: ItemCarrito) {
    this.carrito.actualizarCantidad(item.producto.id, item.cantidad - 1);
  }

  onCantidadInput(item: ItemCarrito, valor: string) {
    const n = parseInt(valor, 10);
    this.carrito.actualizarCantidad(item.producto.id, isNaN(n) ? 1 : n);
  }

  quitar(productoId: number) {
    this.carrito.quitar(productoId);
  }

  procederAlPago() {
    if (this.auth.estaAutenticado()) {
      this.router.navigate(["/checkout"]);
    } else {
      this.router.navigate(["/auth/login"], { queryParams: { returnUrl: "/checkout" } });
    }
  }
}
