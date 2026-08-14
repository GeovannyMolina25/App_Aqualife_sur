import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { PagedResult, RespuestaDto } from "../../models/comun/respuesta.model";
import {
  CotizacionServicioDto,
  CrearCotizacionServicioDto,
  EstadoCotizacion,
} from "../../models/cotizaciones/cotizacion.model";

@Injectable({ providedIn: "root" })
export class CotizacionesService {
  private url = `${environment.apiUrl}/cotizaciones`;
  constructor(private http: HttpClient) {}

  solicitar(dto: CrearCotizacionServicioDto) {
    return this.http.post<RespuestaDto<CotizacionServicioDto>>(this.url, dto);
  }

  obtenerTodas(pagina = 1, tamano = 20, estado?: EstadoCotizacion, busqueda?: string) {
    let params = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (estado) params = params.set("estado", estado);
    if (busqueda?.trim()) params = params.set("busqueda", busqueda.trim());
    return this.http.get<RespuestaDto<PagedResult<CotizacionServicioDto>>>(this.url, { params });
  }

  cambiarEstado(id: number, estado: EstadoCotizacion) {
    return this.http.put<RespuestaDto<CotizacionServicioDto>>(`${this.url}/${id}/estado`, { estado });
  }
}
