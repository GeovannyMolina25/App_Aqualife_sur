export interface Metricas {
  totalVentasHoy: number;
  totalVentasMes: number;
  ventasHoy: number;
  productosActivos: number;
  topColaboradores: TopColaborador[];
}
export interface TopColaborador {
  nombre: string;
  totalVentas: number;
  montoTotal: number;
}
