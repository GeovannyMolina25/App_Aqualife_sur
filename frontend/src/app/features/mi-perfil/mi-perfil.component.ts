import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { AuthService } from "../../core/services/auth/auth.service";
import { Usuario } from "../../core/models/usuarios/usuario.model";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";

@Component({
  selector: "app-mi-perfil",
  standalone: true,
  imports: [CommonModule, FormsModule, AlertComponent, SpinnerComponent],
  templateUrl: "./mi-perfil.component.html",
  styleUrls: ["./mi-perfil.component.css"],
})
export class MiPerfilComponent implements OnInit {
  cargando = signal(true);
  guardando = signal(false);
  perfil = signal<Usuario | null>(null);
  error = signal("");
  exito = signal("");

  form = { nombre: "", apellido: "", direccion: "", telefono: "" };

  cambiandoPassword = signal(false);
  nuevaPassword = "";
  confirmarPassword = "";
  errorPassword = signal("");
  exitoPassword = signal("");

  constructor(private auth: AuthService) {}

  ngOnInit() {
    this.auth.obtenerPerfil().subscribe({
      next: (r) => {
        this.cargando.set(false);
        if (r.exito) {
          this.perfil.set(r.datos);
          this.form = {
            nombre: r.datos.nombre,
            apellido: r.datos.apellido,
            direccion: r.datos.direccion,
            telefono: r.datos.telefono ?? "",
          };
        } else {
          this.error.set(r.mensaje);
        }
      },
      error: () => {
        this.cargando.set(false);
        this.error.set("No se pudo cargar tu perfil.");
      },
    });
  }

  guardar() {
    if (!this.form.nombre || !this.form.apellido || !this.form.direccion) {
      this.error.set("Completa nombre, apellido y dirección.");
      return;
    }
    this.guardando.set(true);
    this.error.set("");
    this.exito.set("");
    this.auth.actualizarPerfil(this.form).subscribe({
      next: (r) => {
        this.guardando.set(false);
        if (r.exito) {
          this.perfil.set(r.datos);
          this.exito.set("Perfil actualizado correctamente.");
        } else {
          this.error.set(r.mensaje);
        }
      },
      error: (e) => {
        this.guardando.set(false);
        this.error.set(e?.error?.mensaje ?? "No se pudo actualizar el perfil.");
      },
    });
  }

  cambiarPassword() {
    if (!this.nuevaPassword || !this.confirmarPassword) {
      this.errorPassword.set("Completa ambos campos.");
      return;
    }
    if (this.nuevaPassword !== this.confirmarPassword) {
      this.errorPassword.set("Las contraseñas no coinciden.");
      return;
    }
    this.cambiandoPassword.set(true);
    this.errorPassword.set("");
    this.exitoPassword.set("");
    this.auth.cambiarPassword(this.nuevaPassword).subscribe({
      next: () => {
        this.cambiandoPassword.set(false);
        this.exitoPassword.set("Contraseña actualizada correctamente.");
        this.nuevaPassword = "";
        this.confirmarPassword = "";
      },
      error: (e) => {
        this.cambiandoPassword.set(false);
        this.errorPassword.set(e?.error?.mensaje ?? "No se pudo cambiar la contraseña.");
      },
    });
  }
}
