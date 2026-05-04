import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";

@Injectable({ providedIn: "root" })
export class ReportesService {
  private url = `${environment.apiUrl}/reportes`;
  constructor(private http: HttpClient) {}

  pdfMensual(anio: number, mes: number) {
    return this.http.get(`${this.url}/pdf-mensual?anio=${anio}&mes=${mes}`, {
      responseType: "blob",
    });
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

  descargar(blob: Blob, nombre: string) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = nombre;
    a.click();
    URL.revokeObjectURL(url);
  }
}
