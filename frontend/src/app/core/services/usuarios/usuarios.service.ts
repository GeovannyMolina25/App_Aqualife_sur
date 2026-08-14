import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { Usuario, CambiarRolDto } from "../../models/usuarios/usuario.model";
import { PagedResult, RespuestaDto } from "../../models/comun/respuesta.model";

@Injectable({ providedIn: "root" })
export class UsuariosService {
  private url = `${environment.apiUrl}/usuarios`;
  constructor(private http: HttpClient) {}

  obtenerTodos(pagina = 1, tamano = 20, busqueda?: string) {
    let p = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (busqueda?.trim()) p = p.set("busqueda", busqueda.trim());
    return this.http.get<RespuestaDto<PagedResult<Usuario>>>(this.url, {
      params: p,
    });
  }

  cambiarRol(id: number, dto: CambiarRolDto) {
    return this.http.put<RespuestaDto<boolean>>(`${this.url}/${id}/rol`, dto);
  }

  entregarPremioBienvenida(id: number) {
    return this.http.put<RespuestaDto<boolean>>(`${this.url}/${id}/premio-bienvenida/entregar`, {});
  }
}
