export interface Venta {
  id: number;
  numeroVenta: string;
  fechaVenta: string;
  total: number;
  estado: string;
  observacion?: string;
  nombreCliente: string;
  emailCliente: string;
  nombreColaborador: string;
  detalles: DetalleVenta[];
}
export interface DetalleVenta {
  productoId: number;
  nombreProducto: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}
export interface CrearVentaDto {
  clienteId: number;
  observacion?: string;
  items: ItemVentaDto[];
}
export interface ItemVentaDto {
  productoId: number;
  cantidad: number;
}
export interface CambiarEstadoVentaDto {
  nuevoEstado: string;
}
export const ESTADOS_VENTA = ["Pendiente", "Completada", "Ingresada", "Anulada"] as const;
