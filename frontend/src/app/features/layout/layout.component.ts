import { Component, computed, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from "@angular/router";
import { AuthService } from "../../core/services/auth/auth.service";
import { CarritoService } from "../../core/services/carrito/carrito.service";

@Component({
  selector: "app-layout",
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: "./layout.component.html",
  styleUrls: ["./layout.component.css"],
})
export class LayoutComponent {
  open = signal(false);
  menuUsuarioAbierto = signal(false);
  iniciales = computed(() => {
    const u = this.auth.usuario();
    return u ? (u.nombre[0] + (u.apellido?.[0] ?? "")).toUpperCase() : "?";
  });
  rolClass = computed(
    () => `rb-${this.auth.getRol().toLowerCase().replace("ó", "o")}`,
  );
  constructor(
    public auth: AuthService,
    public carrito: CarritoService,
    private router: Router,
  ) {}
  toggleMenuUsuario() {
    this.menuUsuarioAbierto.set(!this.menuUsuarioAbierto());
  }
  cerrarMenuUsuario() {
    this.menuUsuarioAbierto.set(false);
  }
  logout() {
    this.auth.logout();
  }
}
