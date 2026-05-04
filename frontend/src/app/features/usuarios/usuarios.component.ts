import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { UsuariosService } from "../../core/services/usuarios/usuarios.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { Usuario } from "../../core/models/usuarios/usuario.model";

@Component({
  selector: "app-usuarios",
  standalone: true,
  imports: [CommonModule, SpinnerComponent],
  templateUrl: "./usuarios.component.html",
  styleUrls: ["./usuarios.component.css"],
})
export class UsuariosComponent implements OnInit {
  usuarios = signal<Usuario[]>([]);
  cargando = signal(false);

  constructor(private svc: UsuariosService) {}

  ngOnInit() {
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.svc.obtenerTodos().subscribe((r) => {
      this.cargando.set(false);
      if (r.exito) this.usuarios.set(r.datos.items);
    });
  }

  cambiarRol(id: number, nuevoRolId: number) {
    this.svc.cambiarRol(id, { nuevoRolId }).subscribe((r) => {
      if (r.exito) this.cargar();
    });
  }

  iniciales(u: Usuario): string {
    return (u.nombre[0] + (u.apellido?.[0] ?? "")).toUpperCase();
  }

  rolClass(rol: string): string {
    const map: Record<string, string> = {
      Administrador: "admin",
      Colaborador: "colab",
      Cliente: "cliente",
    };
    return map[rol] ?? "cliente";
  }
}
