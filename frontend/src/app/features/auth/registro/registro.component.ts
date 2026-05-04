import { Component, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AuthService } from "../../../core/services/auth/auth.service";
import { AlertComponent } from "../../../shared/components/alert/alert.component";
import { RegistroDto } from "../../../core/models/auth/registro.model";

@Component({
  selector: "app-registro",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, AlertComponent],
  templateUrl: "./registro.component.html",
  styleUrls: ["./registro.component.css"],
})
export class RegistroComponent {
  dto: RegistroDto = {
    nombre: "",
    apellido: "",
    email: "",
    password: "",
    fechaNacimiento: "",
    sexo: "",
    direccion: "",
    telefono: "",
  };
  cargando = signal(false);
  error = signal("");
  exito = signal("");
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}
  onRegistro() {
    const {
      nombre,
      apellido,
      email,
      password,
      fechaNacimiento,
      sexo,
      direccion,
    } = this.dto;
    if (
      !nombre ||
      !apellido ||
      !email ||
      !password ||
      !fechaNacimiento ||
      !sexo ||
      !direccion
    ) {
      this.error.set("Completa todos los campos obligatorios (*).");
      return;
    }
    this.cargando.set(true);
    this.error.set("");
    this.auth.registro(this.dto).subscribe({
      next: (r) => {
        this.cargando.set(false);
        r.exito
          ? (this.exito.set("¡Registro exitoso! Redirigiendo..."),
            setTimeout(() => this.router.navigate(["/dashboard"]), 1500))
          : this.error.set(r.mensaje);
      },
      error: (e) => {
        this.cargando.set(false);
        this.error.set(e?.error?.mensaje ?? "Error de conexión.");
      },
    });
  }
}
