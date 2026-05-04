import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
} from "@angular/core";
import { CommonModule } from "@angular/common";

@Component({
  selector: "app-pagination",
  standalone: true,
  imports: [CommonModule],
  template: `@if (totalPaginas > 1) {
    <div class="pag">
      <button class="pb" [disabled]="p === 1" (click)="emit(p - 1)">‹</button>
      @for (n of pages; track n) {
        <button class="pb" [class.act]="n === p" (click)="emit(n)">
          {{ n }}
        </button>
      }
      <button class="pb" [disabled]="p === totalPaginas" (click)="emit(p + 1)">
        ›
      </button>
    </div>
  }`,
  styles: [
    `
      .pag {
        display: flex;
        gap: 6px;
        justify-content: center;
        margin-top: 24px;
      }
      .pb {
        padding: 7px 13px;
        border: 1.5px solid #e2d0af;
        border-radius: 8px;
        background: #fff;
        cursor: pointer;
        font-size: 0.85rem;
        color: #4a6572;
        transition: all 0.2s;
        font-family: "DM Sans", sans-serif;
      }
      .pb:hover,
      .pb.act {
        background: #1a6b8a;
        color: #fff;
        border-color: #1a6b8a;
      }
      .pb:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
    `,
  ],
})
export class PaginationComponent implements OnChanges {
  @Input() p = 1;
  @Input() totalPaginas = 1;
  @Output() paginaCambiada = new EventEmitter<number>();
  pages: number[] = [];
  ngOnChanges() {
    this.pages = Array.from({ length: this.totalPaginas }, (_, i) => i + 1);
  }
  emit(n: number) {
    if (n >= 1 && n <= this.totalPaginas) this.paginaCambiada.emit(n);
  }
}
