import { Component, computed, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from "@angular/router";
import { AuthService } from "../../core/services/auth/auth.service";

@Component({
  selector: "app-layout",
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: "./layout.component.html",
  styleUrls: ["./layout.component.css"],
})
export class LayoutComponent {
  open = signal(false);
  iniciales = computed(() => {
    const u = this.auth.usuario();
    return u ? (u.nombre[0] + (u.apellido?.[0] ?? "")).toUpperCase() : "?";
  });
  rolClass = computed(
    () => `rb-${this.auth.getRol().toLowerCase().replace("ó", "o")}`,
  );
  constructor(
    public auth: AuthService,
    private router: Router,
  ) {}
  logout() {
    this.auth.logout();
  }
}
