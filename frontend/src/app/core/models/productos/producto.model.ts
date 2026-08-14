export type TipoCategoria = "Producto" | "Servicio";

export interface Producto {
  id: number;
  nombre: string;
  descripcion?: string;
  caracteristicas?: string;
  precio: number;
  stock: number;
  categoriaId: number;
  categoria: string;
  categoriaTipo: TipoCategoria;
  imagenUrl?: string;
  esPromocion: boolean;
  precioPromocion?: number;
  fechaInicioPromocion?: string;
  fechaFinPromocion?: string;
  activo: boolean;
}
export interface CrearProductoDto {
  nombre: string;
  descripcion?: string;
  caracteristicas?: string;
  precio: number;
  stock: number;
  categoriaId: number;
  imagenUrl?: string;
  esPromocion: boolean;
  precioPromocion?: number;
}
export interface Categoria {
  id: number;
  nombre: string;
  descripcion?: string;
  tipo: TipoCategoria;
}
