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

  obtenerTodos(pagina = 1, tamano = 20, busqueda?: string, categoriaId?: number, tipo?: string) {
    let params = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (busqueda?.trim()) params = params.set("busqueda", busqueda.trim());
    if (categoriaId) params = params.set("categoriaId", categoriaId);
    if (tipo) params = params.set("tipo", tipo);
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

  crearConImagen(nombre: string, descripcion: string, categoriaId: number, imagen: File | null) {
    const form = new FormData();
    form.append("nombre", nombre);
    if (descripcion) form.append("descripcion", descripcion);
    form.append("categoriaId", String(categoriaId));
    if (imagen) form.append("imagen", imagen);
    return this.http.post<RespuestaDto<Producto>>(`${this.url}/con-imagen`, form);
  }
}
