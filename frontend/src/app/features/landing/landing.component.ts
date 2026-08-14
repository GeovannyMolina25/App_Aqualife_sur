import { AfterViewInit, Component, ElementRef, HostListener, signal, ViewChild } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { AuthService } from "../../core/services/auth/auth.service";

const PROMO_CERRADA_KEY = "rotter_promo_bienvenida_cerrada";

@Component({
  selector: "app-landing",
  standalone: true,
  imports: [CommonModule, RouterLink, PublicHeaderComponent],
  templateUrl: "./landing.component.html",
  styleUrls: ["./landing.component.css"],
})
export class LandingComponent implements AfterViewInit {
  anioActual = new Date().getFullYear();

  @ViewChild("footerRef") footerRef?: ElementRef<HTMLElement>;

  promoOculta = signal(sessionStorage.getItem(PROMO_CERRADA_KEY) === "1");
  promoDetenida = signal(false);
  footerAltura = signal(140);

  constructor(public authSrv: AuthService) {}

  ngAfterViewInit() {
    this.medirFooter();
    this.actualizarPosicionPromo();
  }

  @HostListener("window:scroll")
  @HostListener("window:resize")
  actualizarPosicionPromo() {
    const footer = this.footerRef?.nativeElement;
    if (!footer) return;
    const margenInferior = 20;
    const alturaAprox = 130;
    const footerTop = footer.getBoundingClientRect().top;
    this.promoDetenida.set(footerTop <= window.innerHeight - margenInferior - alturaAprox);
  }

  private medirFooter() {
    const footer = this.footerRef?.nativeElement;
    if (footer) this.footerAltura.set(footer.offsetHeight);
  }

  cerrarPromo() {
    this.promoOculta.set(true);
    sessionStorage.setItem(PROMO_CERRADA_KEY, "1");
  }

  irASeccion(id: string) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });
  }
}
