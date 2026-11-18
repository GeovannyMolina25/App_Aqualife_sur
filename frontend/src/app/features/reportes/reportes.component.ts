import { Component, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IconComponent } from "../../shared/components/icon/icon.component";
import { FormsModule } from "@angular/forms";
import { ReportesService } from "../../core/services/reportes/reportes.service";
import { AlertComponent } from "../../shared/components/alert/alert.component";

@Component({
  selector: "app-reportes",
  standalone: true,
  imports: [IconComponent, CommonModule, FormsModule, AlertComponent],
  templateUrl: "./reportes.component.html",
  styleUrls: ["./reportes.component.css"],
})
export class ReportesComponent {
  anio = new Date().getFullYear();
  mes = new Date().getMonth() + 1;
  desde = "";
  hasta = "";

  cargandoPdf = signal(false);
  cargandoExcel = signal(false);
  exitoPdf = signal("");
  exitoExcel = signal("");

  meses = [
    { num: 1, nombre: "Enero" },
    { num: 2, nombre: "Febrero" },
    { num: 3, nombre: "Marzo" },
    { num: 4, nombre: "Abril" },
    { num: 5, nombre: "Mayo" },
    { num: 6, nombre: "Junio" },
    { num: 7, nombre: "Julio" },
    { num: 8, nombre: "Agosto" },
    { num: 9, nombre: "Septiembre" },
    { num: 10, nombre: "Octubre" },
    { num: 11, nombre: "Noviembre" },
    { num: 12, nombre: "Diciembre" },
  ];

  rankingDesde = "";
  rankingHasta = "";
  cargandoRanking = signal<"pdf" | "excel" | null>(null);
  exitoRanking = signal("");
  errorRanking = signal("");

  constructor(private svc: ReportesService) {}

  descargarRanking(formato: "pdf" | "excel") {
    if (this.rankingDesde && this.rankingHasta && this.rankingDesde > this.rankingHasta) {
      this.errorRanking.set("La fecha 'Desde' no puede ser mayor que 'Hasta'.");
      return;
    }
    this.cargandoRanking.set(formato);
    this.exitoRanking.set("");
    this.errorRanking.set("");
    this.svc.ranking(formato, this.rankingDesde, this.rankingHasta).subscribe({
      next: (blob) => {
        this.cargandoRanking.set(null);
        const fecha = new Date().toISOString().slice(0, 10);
        this.svc.descargar(blob, `ranking-ventas-${fecha}.${formato === "pdf" ? "pdf" : "xlsx"}`);
        this.exitoRanking.set(`${formato === "pdf" ? "PDF" : "Excel"} descargado exitosamente.`);
        setTimeout(() => this.exitoRanking.set(""), 4000);
      },
      error: () => {
        this.cargandoRanking.set(null);
        this.errorRanking.set("No se pudo generar el reporte.");
      },
    });
  }

  descargarPdf() {
    this.cargandoPdf.set(true);
    this.exitoPdf.set("");
    this.svc.pdfMensual(this.anio, this.mes).subscribe({
      next: (blob) => {
        this.cargandoPdf.set(false);
        this.exitoPdf.set("PDF descargado exitosamente.");
        this.svc.descargar(
          blob,
          `ventas-${this.anio}-${String(this.mes).padStart(2, "0")}.pdf`,
        );
        setTimeout(() => this.exitoPdf.set(""), 4000);
      },
      error: () => this.cargandoPdf.set(false),
    });
  }

  descargarExcel() {
    if (!this.desde || !this.hasta) return;
    this.cargandoExcel.set(true);
    this.exitoExcel.set("");
    this.svc.excel(this.desde, this.hasta).subscribe({
      next: (blob) => {
        this.cargandoExcel.set(false);
        this.exitoExcel.set("Excel descargado exitosamente.");
        this.svc.descargar(blob, `productos-vendidos-${this.desde}.xlsx`);
        setTimeout(() => this.exitoExcel.set(""), 4000);
      },
      error: () => this.cargandoExcel.set(false),
    });
  }
}
