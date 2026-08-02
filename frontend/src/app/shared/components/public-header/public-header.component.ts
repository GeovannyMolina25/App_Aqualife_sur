import { Component, HostListener, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/services/auth/auth.service";
import { CarritoService } from "../../../core/services/carrito/carrito.service";

@Component({
  selector: "app-public-header",
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: "./public-header.component.html",
  styleUrls: ["./public-header.component.css"],
})
export class PublicHeaderComponent {
  menuAbierto = signal(false);
  menuUsuarioAbierto = signal(false);
  scrolled = signal(false);

  constructor(
    public auth: AuthService,
    public carrito: CarritoService,
    private router: Router,
  ) {}

  @HostListener("window:scroll")
  onScroll() {
    this.scrolled.set(window.scrollY > 12);
  }

  toggleMenu() {
    this.menuAbierto.set(!this.menuAbierto());
  }
  cerrarMenu() {
    this.menuAbierto.set(false);
  }
  toggleMenuUsuario() {
    this.menuUsuarioAbierto.set(!this.menuUsuarioAbierto());
  }
  cerrarMenuUsuario() {
    this.menuUsuarioAbierto.set(false);
  }

  irASeccion(id: string) {
    this.cerrarMenu();
    if (this.router.url === "/") {
      this.scrollASeccion(id);
    } else {
      this.router.navigate(["/"]).then(() => setTimeout(() => this.scrollASeccion(id), 200));
    }
  }

  private scrollASeccion(id: string) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });
  }

  iniciales(): string {
    const u = this.auth.usuario();
    if (!u) return "";
    return `${u.nombre?.[0] ?? ""}${u.apellido?.[0] ?? ""}`.toUpperCase();
  }

  cerrarSesion() {
    this.cerrarMenuUsuario();
    this.auth.logout();
  }
}
