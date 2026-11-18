import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../../environments/environment";

@Injectable({ providedIn: "root" })
export class ReportesService {
  private url = `${environment.apiUrl}/reportes`;
  constructor(private http: HttpClient) {}

  pdfMensual(anio: number, mes: number) {
    return this.http.get(
      `${this.url}/pdf-mensual?anio=${anio}&mes=${mes}`,
      {
        responseType: "blob",
      }
    );
  }
  pdfColaborador(id: number, d: string, h: string) {
    return this.http.get(
      `${this.url}/pdf-colaborador?colaboradorId=${id}&desde=${d}&hasta=${h}`,
      { responseType: "blob" },
    );
  }
  excel(desde: string, hasta: string) {
    return this.http.get(`${this.url}/excel?desde=${desde}&hasta=${hasta}`, {
      responseType: "blob",
    });
  }

  /** Ranking completo de clientes, productos y servicios. Sin fechas = todo el historial. */
  ranking(formato: "pdf" | "excel", desde?: string, hasta?: string) {
    let params = new HttpParams().set("formato", formato);
    if (desde) params = params.set("desde", desde);
    if (hasta) params = params.set("hasta", hasta);
    return this.http.get(`${this.url}/ranking`, { params, responseType: "blob" });
  }

  descargar(blob: Blob, nombre: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = nombre;
    a.click();
    URL.revokeObjectURL(url);
  }
}
