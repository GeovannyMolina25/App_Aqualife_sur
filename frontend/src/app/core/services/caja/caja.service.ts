import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import {
  CrearIngresoCajaDto,
  CrearSalidaCajaDto,
  IngresoCaja,
  MovimientoCaja,
  SaldoCaja,
  SalidaCaja,
} from "../../models/caja/caja.model";
import { PagedResult, RespuestaDto } from "../../models/comun/respuesta.model";
import { Venta } from "../../models/ventas/venta.model";

@Injectable({ providedIn: "root" })
export class CajaService {
  private url = `${environment.apiUrl}/caja`;
  constructor(private http: HttpClient) {}

  obtenerSaldo() {
    return this.http.get<RespuestaDto<SaldoCaja>>(`${this.url}/saldo`);
  }

  obtenerHistorial(pagina = 1, tamano = 20, busqueda?: string) {
    let p = new HttpParams().set("pagina", pagina).set("tamano", tamano);
    if (busqueda?.trim()) p = p.set("busqueda", busqueda.trim());
    return this.http.get<RespuestaDto<PagedResult<MovimientoCaja>>>(
      `${this.url}/historial`,
      { params: p },
    );
  }

  obtenerVentasPendientes() {
    return this.http.get<RespuestaDto<Venta[]>>(`${this.url}/ventas-pendientes`);
  }

  registrarIngreso(dto: CrearIngresoCajaDto) {
    return this.http.post<RespuestaDto<IngresoCaja>>(`${this.url}/ingresos`, dto);
  }

  registrarSalida(dto: CrearSalidaCajaDto) {
    return this.http.post<RespuestaDto<SalidaCaja>>(`${this.url}/salidas`, dto);
  }
}
