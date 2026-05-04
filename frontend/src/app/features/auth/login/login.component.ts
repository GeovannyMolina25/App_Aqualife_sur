import { Component, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/services/auth/auth.service";
import { AlertComponent } from "../../../shared/components/alert/alert.component";

@Component({
  selector: "app-login",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AlertComponent],
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"],
})
export class LoginComponent {
  email = "";
  password = "";
  cargando = signal(false);
  error = signal("");
  verPass = signal(false);
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}
  demo(e: string, p: string) {
    this.email = e;
    this.password = p;
  }
  onLogin() {
    if (!this.email || !this.password) {
      this.error.set("Completa todos los campos.");
      return;
    }
    this.cargando.set(true);
    this.error.set("");
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (r) => {
        this.cargando.set(false);
        r.exito
          ? this.router.navigate(["/dashboard"])
          : this.error.set(r.mensaje);
      },
      error: (e) => {
        this.cargando.set(false);
        this.error.set(e?.error?.mensaje ?? "Error de conexión.");
      },
    });
  }
}
