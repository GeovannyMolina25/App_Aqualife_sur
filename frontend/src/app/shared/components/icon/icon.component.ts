import { Component, Input } from "@angular/core";

export type IconoPeso = "duotone" | "fill" | "bold" | "regular";
export type IconoTono =
  | "sky" | "teal" | "blue" | "green" | "orange" | "violet" | "pink" | "amber" | "red";

/** Paleta: color principal del ícono y fondo pastel para el chip. */
const TONOS: Record<IconoTono, { c: string; bg: string }> = {
  sky:    { c: "#1b9fd6", bg: "#e1f3fb" },
  teal:   { c: "#109c8b", bg: "#dff5f1" },
  blue:   { c: "#3b74e0", bg: "#e6eefc" },
  green:  { c: "#24a55c", bg: "#e3f5ea" },
  orange: { c: "#ee8a2c", bg: "#fdeedf" },
  violet: { c: "#7c5ce8", bg: "#eee9fd" },
  pink:   { c: "#e0517a", bg: "#fce8ee" },
  amber:  { c: "#d99a06", bg: "#fcf3d8" },
  red:    { c: "#e0524a", bg: "#fce7e5" },
};

/** Nombre interno → [ícono de Phosphor (https://phosphoricons.com), tono]. Sin tono = hereda el color del texto. */
export const ICONOS: Record<string, [string, IconoTono?]> = {
  droplet: ["drop", "sky"],
  check: ["check"],
  "circle-check": ["check-circle", "green"],
  "circle-alert": ["warning-circle", "red"],
  info: ["info", "blue"],
  "arrow-left": ["arrow-left"],
  eye: ["eye"],
  "eye-off": ["eye-slash"],
  x: ["x"],
  cart: ["shopping-cart-simple", "orange"],
  search: ["magnifying-glass"],
  bank: ["bank", "teal"],
  package: ["package", "violet"],
  message: ["whatsapp-logo", "green"],
  wallet: ["wallet", "teal"],
  dollar: ["currency-circle-dollar", "green"],
  chart: ["chart-bar", "blue"],
  trophy: ["trophy", "amber"],
  bag: ["shopping-bag", "orange"],
  edit: ["note-pencil", "violet"],
  clipboard: ["clipboard-text", "blue"],
  users: ["users-three", "pink"],
  user: ["user-circle", "blue"],
  "user-plus": ["user-plus", "pink"],
  "file-text": ["file-pdf", "red"],
  "file-sheet": ["file-xls", "green"],
  gift: ["gift", "pink"],
  card: ["credit-card", "violet"],
  truck: ["truck", "orange"],
  heart: ["handshake", "pink"],
  leaf: ["leaf", "green"],
  cash: ["money", "green"],
  phone: ["phone", "teal"],
  smartphone: ["device-mobile", "violet"],
  mail: ["envelope-simple", "blue"],
  "map-pin": ["map-pin", "red"],
  clock: ["clock", "amber"],
  dashboard: ["squares-four", "sky"],
  menu: ["list"],
  logout: ["sign-out", "red"],
  loader: ["circle-notch"],
  download: ["download-simple"],
  paperclip: ["paperclip", "blue"],
  receipt: ["receipt", "amber"],
  sparkles: ["broom", "violet"],
  confetti: ["confetti", "pink"],
  star: ["star", "amber"],
  exclamation: ["exclamation-mark", "red"],
};

/** HTML del ícono, para usarlo fuera de plantillas (DOM directo). */
export function htmlIcono(nombre: string, peso: IconoPeso = "fill"): string {
  const [ph, tono] = ICONOS[nombre] ?? [nombre];
  const color = tono ? ` style="color:${TONOS[tono].c}"` : "";
  return `<i class="ph-${peso} ph-${ph}"${color} aria-hidden="true"></i>`;
}

@Component({
  selector: "app-icon",
  standalone: true,
  template: `<i [class]="clase" [style.font-size]="size"></i>`,
  host: {
    "[class.spin]": "spin",
    "[class.toned]": "!!paleta",
    "[class.chip]": "chip",
    "[style.--ic]": "paleta?.c",
    "[style.--ic-bg]": "paleta?.bg",
    "aria-hidden": "true",
  },
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        line-height: 1;
        vertical-align: -0.125em;
        flex-shrink: 0;
      }
      :host(.toned) {
        color: var(--ic);
      }
      /* Capa de relleno del duotono: más presente que el 20% por defecto de Phosphor. */
      :host(.toned) i.ph-duotone::before {
        opacity: 0.32;
      }
      :host(.chip) {
        width: 1.9em;
        height: 1.9em;
        border-radius: 0.6em;
        background: var(--ic-bg, rgba(26, 107, 138, 0.1));
        vertical-align: middle;
      }
      :host(.spin) i {
        animation: icon-spin 0.9s linear infinite;
      }
      @keyframes icon-spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class IconComponent {
  @Input({ required: true }) name = "";
  @Input() weight?: IconoPeso;
  @Input() spin = false;
  @Input() size = "1em";
  /** Muestra el ícono sobre un cuadro pastel de su color. */
  @Input({ transform: (v: unknown) => v !== false && v !== "false" }) chip = false;
  /** "auto" usa el tono del ícono; "none" hereda el color del texto (para fondos de color). */
  @Input() tono: IconoTono | "auto" | "none" = "auto";

  get paleta() {
    if (this.tono === "none") return null;
    const t = this.tono === "auto" ? ICONOS[this.name]?.[1] : this.tono;
    return t ? TONOS[t] : null;
  }

  get clase() {
    const [ph, tono] = ICONOS[this.name] ?? [this.name];
    const peso = this.weight ?? (this.chip ? "fill" : tono ? "duotone" : "bold");
    return `ph-${peso} ph-${ph}`;
  }
}
