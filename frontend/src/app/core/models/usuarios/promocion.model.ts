export const PREMIO_SEPTIMO_BOTELLON = "SeptimoBotellonGratis";
export const PREMIO_BOTELLON_LLAVES = "BotellonLlavesGratis";
export const PREMIO_TOMA_TODO = "TomaTodo";

export type PremioBienvenida =
  | typeof PREMIO_SEPTIMO_BOTELLON
  | typeof PREMIO_BOTELLON_LLAVES
  | typeof PREMIO_TOMA_TODO;

export interface EstadoPromocionDto {
  debeGirar: boolean;
  premioBienvenida?: PremioBienvenida;
  recargasParaSeptimo: number;
  premioBienvenidaEntregado: boolean;
}

export interface ResultadoGiroDto {
  yaGiro: boolean;
  premioMostrado: string;
  premioReal: PremioBienvenida;
  segmentoIndice: number;
}

/** Mismo orden que el backend (SegmentosRuleta) — solo sirve para dibujar la rueda;
 *  el índice ganador siempre lo decide la API. */
export const SEGMENTOS_RULETA: { etiqueta: string; premioReal: PremioBienvenida }[] = [
  { etiqueta: "Sorpresa", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "7º Botellón Gratis", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "Sorpresa", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "Botellón + Llaves Gratis", premioReal: PREMIO_BOTELLON_LLAVES },
  { etiqueta: "Sorpresa", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "7º Botellón Gratis", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "Sorpresa", premioReal: PREMIO_SEPTIMO_BOTELLON },
  { etiqueta: "Toma Todo", premioReal: PREMIO_TOMA_TODO },
];

export function nombrePremio(premio: PremioBienvenida | string | undefined): string {
  switch (premio) {
    case PREMIO_SEPTIMO_BOTELLON:
      return "7º Botellón Gratis";
    case PREMIO_BOTELLON_LLAVES:
      return "Botellón + Llaves Gratis";
    case PREMIO_TOMA_TODO:
      return "Toma Todo";
    default:
      return "—";
  }
}
