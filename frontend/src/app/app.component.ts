import { Component, OnInit } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { AuthService } from "./core/services/auth/auth.service";
import { RuletaBienvenidaComponent } from "./shared/components/ruleta-bienvenida/ruleta-bienvenida.component";
import { TarjetaBotellonComponent } from "./shared/components/tarjeta-botellon/tarjeta-botellon.component";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [RouterOutlet, RuletaBienvenidaComponent, TarjetaBotellonComponent],
  template: `
    <router-outlet />
    <app-ruleta-bienvenida />
    <app-tarjeta-botellon />
  `,
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService) {}

  ngOnInit() {
    this.auth.iniciarMonitoreoInactividad();
  }
}
