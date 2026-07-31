export interface RespuestaDto<T> {
  exito: boolean;
  mensaje: string;
  datos: T;
  errores: string[];
  statusCode: number;
}
export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamanioPagina: number;
  totalPaginas: number;
}
