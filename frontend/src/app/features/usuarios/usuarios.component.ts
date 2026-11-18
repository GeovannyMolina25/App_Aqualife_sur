import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { UsuariosService } from "../../core/services/usuarios/usuarios.service";
import { SpinnerComponent } from "../../shared/components/spinner/spinner.component";
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";
import { IconComponent } from "../../shared/components/icon/icon.component";
import {
  Usuario,
  CrearUsuarioRapidoDto,
  UsuarioRapidoCreado,
} from "../../core/models/usuarios/usuario.model";
import { PREMIO_SEPTIMO_BOTELLON, nombrePremio } from "../../core/models/usuarios/promocion.model";
import { AuthService } from "../../core/services/auth/auth.service";

const TAMANO_PAGINA = 30;

@Component({
  selector: "app-usuarios",
  standalone: true,
  imports: [CommonModule, FormsModule, SpinnerComponent, PaginationComponent, AlertComponent, IconComponent],
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

  modalRapido = signal(false);
  guardandoRapido = signal(false);
  errorRapido = signal("");
  creado = signal<UsuarioRapidoCreado | null>(null);
  rapido: CrearUsuarioRapidoDto = this.rapidoVacio();

  constructor(private svc: UsuariosService, public auth: AuthService) {}

  private rapidoVacio(): CrearUsuarioRapidoDto {
    return { nombre: "", apellido: "", email: "", telefono: "", direccion: "" };
  }

  abrirRapido() {
    this.rapido = this.rapidoVacio();
    this.errorRapido.set("");
    this.creado.set(null);
    this.modalRapido.set(true);
  }

  cerrarRapido() {
    this.modalRapido.set(false);
  }

  crearRapido() {
    if (!this.rapido.nombre.trim() || !this.rapido.apellido.trim()) {
      this.errorRapido.set("El nombre y el apellido son obligatorios.");
      return;
    }
    this.errorRapido.set("");
    this.guardandoRapido.set(true);
    this.svc.crearRapido(this.rapido).subscribe({
      next: (r) => {
        this.guardandoRapido.set(false);
        if (r.exito) {
          this.creado.set(r.datos);
          this.rapido = this.rapidoVacio();
          this.cargar();
        } else {
          this.errorRapido.set(r.errores?.length ? r.errores.join("<br>") : r.mensaje);
        }
      },
      error: (e) => {
        this.guardandoRapido.set(false);
        const r = e?.error;
        this.errorRapido.set(r?.errores?.length ? r.errores.join("<br>") : r?.mensaje ?? "Error al crear el usuario.");
      },
    });
  }

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

  nombrePremio(u: Usuario) {
    return nombrePremio(u.premioBienvenida);
  }

  listoParaEntregar(u: Usuario): boolean {
    if (!u.premioBienvenida || u.premioBienvenidaEntregado) return false;
    return u.premioBienvenida !== PREMIO_SEPTIMO_BOTELLON || u.recargasParaSeptimo >= 7;
  }

  entregarPremio(u: Usuario) {
    if (!this.listoParaEntregar(u)) return;
    this.svc.entregarPremioBienvenida(u.id).subscribe((r) => {
      if (r.exito) this.cargar();
      else alert(r.mensaje);
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
