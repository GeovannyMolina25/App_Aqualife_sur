import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { Categoria } from "../../models/productos/producto.model";
import { RespuestaDto } from "../../models/comun/respuesta.model";

@Injectable({ providedIn: "root" })
export class CategoriasService {
  private url = `${environment.apiUrl}/categorias`;
  constructor(private http: HttpClient) {}

  obtenerTodas() {
    return this.http.get<RespuestaDto<Categoria[]>>(this.url);
  }
}
