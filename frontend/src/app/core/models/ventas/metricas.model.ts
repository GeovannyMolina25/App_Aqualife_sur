export interface Metricas {
  totalVentasHoy: number;
  totalVentasMes: number;
  ventasHoy: number;
  productosActivos: number;
  topColaboradores: TopColaborador[];
  topProductos: TopProducto[];
  topServicios: TopProducto[];
  topClientes: TopCliente[];
}
export interface TopProducto {
  nombre: string;
  unidades: number;
  montoTotal: number;
}
export interface TopCliente {
  nombre: string;
  email: string;
  compras: number;
  unidades: number;
  totalGastado: number;
}
export interface TopColaborador {
  nombre: string;
  totalVentas: number;
  montoTotal: number;
}
