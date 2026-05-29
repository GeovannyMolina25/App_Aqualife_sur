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
  errorLogo = signal(false);
  modalCambiarPassword = signal(false);
  nuevaPassword = "";
  confirmarPassword = "";
  mensajeCambioPassword = signal("");
  tipoCambioPassword = signal<"success" | "error">("error");
  email = "";
  password = "";
  cargando = signal(false);
  error = signal("");
  exito = signal("");
  verPass = signal(false);
  olvideModal = signal(false);
  datoRecuperacion = "";
  mensajeRecuperacion = signal("");
  tipoMensajeRecuperacion = signal<"success" | "error">("error");
  cargandoRecuperacion = signal(false);
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
          if (r.exito) {
            if (r.datos.debeCambiarPassword) {
              this.modalCambiarPassword.set(true);
            } else {
              this.router.navigate(["/dashboard"]);
            }
          } else {
            this.error.set(r.mensaje);
          }
        },
        error: (e) => {
          this.cargando.set(false);
          this.error.set(e?.error?.mensaje ?? "Error de conexión.");
        },
      });
    }
    recuperarPassword() {
      if (!this.datoRecuperacion) {
        this.mensajeRecuperacion.set("Ingresa tu correo o teléfono.");
        return;
      }
      this.cargandoRecuperacion.set(true);
      this.mensajeRecuperacion.set("");
      this.auth
        .recuperarPassword({
          dato: this.datoRecuperacion,
        })
        .subscribe({
          next: (r) => {
            this.cargandoRecuperacion.set(false);
            this.tipoMensajeRecuperacion.set("success");
            this.mensajeRecuperacion.set("Correo enviado correctamente. <b>Por favor revisa tu bandeja de entrada o en spam.</b>");
            setTimeout(() => {
              this.olvideModal.set(false);
              this.datoRecuperacion = "";
              this.mensajeRecuperacion.set("");
              window.location.reload();
            }, 3000);
          },
          error: (e) => {
            this.cargandoRecuperacion.set(false);
            this.tipoMensajeRecuperacion.set("error");
            this.mensajeRecuperacion.set(
              e?.error?.mensaje ?? "Error al recuperar contraseña.",
            );
          },
        });
    }
    cambiarPassword() {
    if (
      !this.nuevaPassword ||
      !this.confirmarPassword
    ) {
      this.tipoCambioPassword.set("error");
      this.mensajeCambioPassword.set(
        "Completa todos los campos."
      );
      return;
    }
    if (
      this.nuevaPassword !==
      this.confirmarPassword
    ) {
      this.tipoCambioPassword.set("error");
      this.mensajeCambioPassword.set(
        "Las contraseñas no coinciden."
      );
      return;
    }
    this.auth.cambiarPassword(
      this.nuevaPassword
    ).subscribe({
      next: () => {
        this.tipoCambioPassword.set("success");
        this.mensajeCambioPassword.set(
          "La contraseña se ha cambiado exitosamente. <b>NO TE LA OLVIDES.</b>"
        );
        setTimeout(() => {
          this.modalCambiarPassword.set(false);
          this.auth.logout();
        }, 3000);
      },
      error: () => {
        this.tipoCambioPassword.set("error");
        this.mensajeCambioPassword.set(
          "Error al cambiar contraseña."
        );
      },
    });
  }
}
