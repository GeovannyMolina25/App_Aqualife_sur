import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-spinner",
  standalone: true,
  imports: [CommonModule],
  template: `<div class="sw" [class.full]="full">
    <div class="sp" [style.width]="size" [style.height]="size"></div>
  </div>`,
  styles: [
    `
      .sw {
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 16px;
      }
      .sw.full {
        padding: 64px;
      }
      .sp {
        border: 3px solid #efe4cc;
        border-top-color: #1a6b8a;
        border-radius: 50%;
        animation: spin 0.8s linear infinite;
      }
      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class SpinnerComponent {
  @Input() size = "32px";
  @Input() full = false;
}
