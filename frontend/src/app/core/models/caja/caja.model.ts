export interface SaldoCaja {
  totalIngresos: number;
  totalSalidas: number;
  saldo: number;
}

export interface MovimientoCaja {
  id: number;
  tipo: "Ingreso" | "Salida";
  numero: string;
  fecha: string;
  valor: number;
  nombreUsuario: string;
  detalle?: string;
}

export interface IngresoCaja {
  id: number;
  numeroIngreso: string;
  banco: string;
  numeroTransaccion: string;
  comprobanteUrl?: string;
  totalIngresado: number;
  observacion?: string;
  nombreUsuario: string;
  fechaIngreso: string;
  ventaIds: number[];
}

export interface CrearIngresoCajaDto {
  banco: string;
  numeroTransaccion: string;
  totalIngresado: number;
  observacion?: string;
  comprobanteUrl?: string;
  ventaIds?: number[];
}

export interface SalidaCaja {
  id: number;
  numeroSalida: string;
  fechaSalida: string;
  valor: number;
  banco?: string;
  numeroTransaccion?: string;
  comprobanteUrl?: string;
  motivo: string;
  descripcion?: string;
  nombreUsuario: string;
}

export interface CrearSalidaCajaDto {
  valor: number;
  motivo: string;
  descripcion?: string;
  banco?: string;
  numeroTransaccion?: string;
  comprobanteUrl?: string;
}
