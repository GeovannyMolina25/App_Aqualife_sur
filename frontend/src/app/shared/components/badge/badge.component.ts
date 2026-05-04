import { Component, Input } from "@angular/core";
export type BadgeTipo =
  | "admin"
  | "colaborador"
  | "cliente"
  | "success"
  | "danger"
  | "warning";

@Component({
  selector: "app-badge",
  standalone: true,
  imports: [],
  template: `<span class="b" [class]="'b-' + tipo">{{ texto }}</span>`,
  styles: [
    `
      .b {
        display: inline-block;
        padding: 3px 10px;
        border-radius: 100px;
        font-size: 0.73rem;
        font-weight: 500;
      }
      .b-admin {
        background: rgba(26, 107, 138, 0.12);
        color: #1a6b8a;
      }
      .b-colaborador {
        background: rgba(77, 184, 168, 0.15);
        color: #2d8070;
      }
      .b-cliente {
        background: rgba(196, 168, 130, 0.2);
        color: #b8956a;
      }
      .b-success {
        background: rgba(60, 179, 113, 0.12);
        color: #2a8c5a;
      }
      .b-danger {
        background: rgba(224, 92, 92, 0.12);
        color: #c44;
      }
      .b-warning {
        background: rgba(255, 180, 50, 0.12);
        color: #996600;
      }
    `,
  ],
})
export class BadgeComponent {
  @Input() tipo: BadgeTipo = "success";
  @Input() texto = "";
}
