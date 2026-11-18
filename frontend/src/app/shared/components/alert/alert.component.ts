import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { IconComponent } from "../icon/icon.component";
export type AlertTipo = 
| "error" 
| "success" 
| "info";

@Component({
  selector: "app-alert",
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `@if (mensaje) {
  <div class="al" [class]="'al-' + tipo">
    <app-icon
      class="al-ic"
      [name]="tipo === 'success' ? 'circle-check' : tipo === 'info' ? 'info' : 'circle-alert'"
      size="18px"
      tono="none"
    />

    <div [innerHTML]="mensaje"></div>

  </div>
}`,
  styles: [
    `
      .al {
        padding: 11px 16px;
        border-radius: 8px;
        font-size: 0.875rem;
        margin-bottom: 14px;
        display: flex;
        align-items: flex-start;
        gap: 10px;
      }
      .al-ic {
        margin-top: 1px;
      }
      .al-error {
        background: rgba(224, 92, 92, 0.1);
        color: #c04040;
        border-left: 3px solid #e05c5c;
      }
      .al-success {
        background: rgba(60, 179, 113, 0.1);
        color: #2a8c5a;
        border-left: 3px solid #3cb371;
      }
      .al-info {
        background: rgba(26, 107, 138, 0.08);
        color: #1a6b8a;
        border-left: 3px solid #3d8fa1;
      }
    `,
  ],
})
export class AlertComponent {
  @Input() tipo: AlertTipo = "error";
  @Input() mensaje = "";
}
