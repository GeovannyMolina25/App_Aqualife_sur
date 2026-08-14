import { Component, OnInit } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { AuthService } from "./core/services/auth/auth.service";
import { PromocionBienvenidaService } from "./core/services/promocion-bienvenida/promocion-bienvenida.service";
import { RuletaBienvenidaComponent } from "./shared/components/ruleta-bienvenida/ruleta-bienvenida.component";
import { TarjetaBotellonComponent } from "./shared/components/tarjeta-botellon/tarjeta-botellon.component";
import { ConfetiComponent } from "./shared/components/confeti/confeti.component";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [RouterOutlet, RuletaBienvenidaComponent, TarjetaBotellonComponent, ConfetiComponent],
  template: `
    <router-outlet />
    <app-ruleta-bienvenida />
    <app-tarjeta-botellon />
    <app-confeti />
  `,
})
export class AppComponent implements OnInit {
  constructor(
    private auth: AuthService,
    private promocionSrv: PromocionBienvenidaService,
  ) {}

  ngOnInit() {
    this.auth.iniciarMonitoreoInactividad();
    if (this.auth.estaAutenticado()) this.promocionSrv.sincronizarPremioListo();
  }
}
