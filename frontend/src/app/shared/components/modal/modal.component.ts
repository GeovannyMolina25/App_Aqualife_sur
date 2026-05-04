import { Component, Input, Output, EventEmitter } from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-modal",
  standalone: true,
  imports: [CommonModule],
  template: `@if (visible) {
    <div class="ov" (click)="close()">
      <div class="mo" (click)="$event.stopPropagation()">
        <div class="mh">
          <span class="mt">{{ titulo }}</span
          ><button class="mc" (click)="close()">✕</button>
        </div>
        <div class="mb"><ng-content /></div>
        <div class="mf"><ng-content select="[slot=footer]" /></div>
      </div>
    </div>
  }`,
  styles: [
    `
      .ov {
        position: fixed;
        inset: 0;
        background: rgba(28, 43, 53, 0.5);
        backdrop-filter: blur(3px);
        z-index: 200;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 20px;
      }
      .mo {
        background: #fff;
        border-radius: 20px;
        width: 100%;
        max-width: 540px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
      }
      .mh {
        padding: 22px 26px 18px;
        border-bottom: 1px solid #efe4cc;
        display: flex;
        align-items: center;
        justify-content: space-between;
      }
      .mt {
        font-size: 1.1rem;
        font-weight: 600;
        color: #1c2b35;
      }
      .mc {
        background: none;
        border: none;
        cursor: pointer;
        color: #7a9aaa;
        font-size: 1.3rem;
      }
      .mb {
        padding: 22px 26px;
      }
      .mf {
        padding: 14px 26px 22px;
        display: flex;
        gap: 10px;
        justify-content: flex-end;
      }
    `,
  ],
})
export class ModalComponent {
  @Input() visible = false;
  @Input() titulo = "";
  @Output() visibleChange = new EventEmitter<boolean>();
  close() {
    this.visible = false;
    this.visibleChange.emit(false);
  }
}
