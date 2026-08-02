export interface Venta {
  id: number;
  numeroVenta: string;
  fechaVenta: string;
  subtotal: number;
  impuestos: number;
  total: number;
  estado: string;
  origen: string;
  metodoPago?: string;
  estadoPago?: string;
  comprobanteUrl?: string;
  direccionEnvio?: string;
  telefonoContacto?: string;
  nombreReceptor?: string;
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

export type MetodoPago = "TransferenciaProdubanco" | "TransferenciaPichincha" | "ContraEntrega";

export interface CheckoutDto {
  items: ItemVentaDto[];
  metodoPago: MetodoPago;
  direccionEnvio: string;
  telefonoContacto: string;
  nombreReceptor: string;
  observacion?: string;
}

export interface ActualizarEstadoPagoDto {
  nuevoEstadoPago: string;
}

export const ESTADOS_PAGO = ["PendienteVerificacion", "Verificado", "Rechazado", "ContraEntrega"] as const;

export const DATOS_BANCARIOS: Record<"TransferenciaProdubanco" | "TransferenciaPichincha", {
  banco: string;
  tipo?: string;
  numeroCuenta: string;
  nombre: string;
  identificacion: string;
  correo: string;
  celular: string;
}> = {
  TransferenciaProdubanco: {
    banco: "Produbanco",
    tipo: "Cuenta digital",
    numeroCuenta: "20005908610",
    nombre: "Nelson Geovanny Molina",
    identificacion: "1720762812",
    correo: "geovannysangucho25@gmail.com",
    celular: "0998628996",
  },
  TransferenciaPichincha: {
    banco: "Banco Pichincha",
    numeroCuenta: "2208444263",
    nombre: "Nelson Geovanny Molina",
    identificacion: "1720762812",
    correo: "geovannysangucho25@gmail.com",
    celular: "0998628996",
  },
};

export const WHATSAPP_PEDIDOS = "593997724788";
