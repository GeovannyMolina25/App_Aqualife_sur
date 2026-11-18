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

  /** `incluirInactivos` solo tiene efecto para administradores (el backend lo ignora para el resto). */
  obtenerTodos(pagina = 1, tamano = 20, busqueda?: string, categoriaId?: number, tipo?: string, incluirInactivos = false) {
    let params = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (busqueda?.trim()) params = params.set("busqueda", busqueda.trim());
    if (categoriaId) params = params.set("categoriaId", categoriaId);
    if (tipo) params = params.set("tipo", tipo);
    if (incluirInactivos) params = params.set("incluirInactivos", true);
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

  crearConImagen(datos: DatosItemConImagen, imagen: File | null) {
    return this.http.post<RespuestaDto<Producto>>(`${this.url}/con-imagen`, this.formulario(datos, imagen));
  }

  /** Edita un producto/servicio; si `imagen` es null se conserva la foto actual. */
  actualizarConImagen(id: number, datos: DatosItemConImagen, imagen: File | null) {
    return this.http.put<RespuestaDto<Producto>>(`${this.url}/${id}/con-imagen`, this.formulario(datos, imagen));
  }

  cambiarEstado(id: number, activo: boolean) {
    return this.http.put<RespuestaDto<boolean>>(`${this.url}/${id}/estado`, { activo });
  }

  private formulario(d: DatosItemConImagen, imagen: File | null) {
    const form = new FormData();
    form.append("nombre", d.nombre);
    if (d.descripcion) form.append("descripcion", d.descripcion);
    if (d.caracteristicas) form.append("caracteristicas", d.caracteristicas);
    form.append("categoriaId", String(d.categoriaId));
    form.append("precio", String(d.precio ?? 0));
    form.append("esPromocion", String(d.esPromocion));
    if (d.esPromocion && d.precioPromocion != null) form.append("precioPromocion", String(d.precioPromocion));
    if (imagen) form.append("imagen", imagen);
    return form;
  }
}

export interface DatosItemConImagen {
  nombre: string;
  descripcion?: string;
  caracteristicas?: string;
  categoriaId: number;
  precio: number;
  esPromocion: boolean;
  precioPromocion?: number | null;
}
