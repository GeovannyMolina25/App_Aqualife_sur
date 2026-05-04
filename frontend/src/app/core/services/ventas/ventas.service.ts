import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { Venta, CrearVentaDto } from "../../models/ventas/venta.model";
import { Metricas } from "../../models/ventas/metricas.model";
import { PagedResult, RespuestaDto } from "../../models/comun/respuesta.model";

@Injectable({ providedIn: "root" })
export class VentasService {
  private url = `${environment.apiUrl}/ventas`;
  constructor(private http: HttpClient) {}

  obtenerTodas(pagina = 1, tamano = 20, desde?: string, hasta?: string) {
    let p = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (desde) p = p.set("desde", desde);
    if (hasta) p = p.set("hasta", hasta);
    return this.http.get<RespuestaDto<PagedResult<Venta>>>(this.url, {
      params: p,
    });
  }

  obtenerMetricas() {
    return this.http.get<RespuestaDto<Metricas>>(`${this.url}/metricas`);
  }

  obtenerHistorial(pagina = 1, tamano = 20) {
    const p = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    return this.http.get<RespuestaDto<PagedResult<Venta>>>(
      `${this.url}/mi-historial`,
      { params: p },
    );
  }

  registrar(dto: CrearVentaDto) {
    return this.http.post<RespuestaDto<Venta>>(this.url, dto);
  }
}
