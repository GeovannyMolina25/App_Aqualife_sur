import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { UsuariosService } from "../../core/services/usuarios/usuarios.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { Usuario } from "../../core/models/usuarios/usuario.model";

const TAMANO_PAGINA = 30;

@Component({
  selector: "app-usuarios",
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent, PaginationComponent],
  templateUrl: "./usuarios.component.html",
  styleUrls: ["./usuarios.component.css"],
})
export class UsuariosComponent implements OnInit {
  usuarios = signal<Usuario[]>([]);
  cargando = signal(false);
  busqueda = signal("");
  pagina = signal(1);
  total = signal(0);
  totalPaginas = signal(1);
  private debounceId?: ReturnType<typeof setTimeout>;

  constructor(private svc: UsuariosService) {}

  ngOnInit() {
    this.cargar();
  }

  buscar(texto: string) {
    this.busqueda.set(texto);
    this.pagina.set(1);
    clearTimeout(this.debounceId);
    this.debounceId = setTimeout(() => this.cargar(), 350);
  }

  cambiarPagina(p: number) {
    this.pagina.set(p);
    this.cargar();
  }

  cargar() {
    this.cargando.set(true);
    this.svc
      .obtenerTodos(this.pagina(), TAMANO_PAGINA, this.busqueda())
      .subscribe((r) => {
        this.cargando.set(false);
        if (r.exito) {
          this.usuarios.set(r.datos.items);
          this.total.set(r.datos.total);
          this.totalPaginas.set(r.datos.totalPaginas);
        }
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
