export type EstadoCotizacion = "Entrante" | "Pendiente" | "Realizado" | "Anulado";

export const ESTADOS_COTIZACION: EstadoCotizacion[] = ["Entrante", "Pendiente", "Realizado", "Anulado"];

export interface CrearCotizacionServicioDto {
  productoId: number;
  nombreContacto: string;
  telefono: string;
  email?: string;
  direccion: string;
  tamanoEspacio: string;
  fechaDeseada?: string;
  comentario?: string;
}

export interface CotizacionServicioDto {
  id: number;
  productoId: number;
  servicioNombre: string;
  nombreContacto: string;
  telefono: string;
  email?: string;
  direccion: string;
  tamanoEspacio: string;
  fechaDeseada?: string;
  comentario?: string;
  estado: EstadoCotizacion;
  fechaCreacion: string;
}
