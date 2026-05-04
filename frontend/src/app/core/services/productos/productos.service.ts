import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import {
  Producto,
  CrearProductoDto,
} from "../../models/productos/producto.model";
import { PagedResult, RespuestaDto } from "../../models/comun/respuesta.model";

@Injectable({ providedIn: "root" })
export class ProductosService {
  private url = `${environment.apiUrl}/productos`;
  constructor(private http: HttpClient) {}

  obtenerTodos(pagina = 1, tamano = 20) {
    const params = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    return this.http.get<RespuestaDto<PagedResult<Producto>>>(this.url, {
      params,
    });
  }

  obtenerPromociones() {
    return this.http.get<RespuestaDto<Producto[]>>(`${this.url}/promociones`);
  }

  crear(dto: CrearProductoDto) {
    return this.http.post<RespuestaDto<Producto>>(this.url, dto);
  }
}
