export interface Producto {
  id: number;
  nombre: string;
  descripcion?: string;
  caracteristicas?: string;
  precio: number;
  stock: number;
  categoria: string;
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
}
export const CATEGORIAS: Categoria[] = [
  { id: 1, nombre: "Agua Natural" },
  { id: 2, nombre: "Agua con Gas" },
  { id: 3, nombre: "Agua Purificada" },
  { id: 4, nombre: "Bidón" },
  { id: 5, nombre: "Promociones" },
];
