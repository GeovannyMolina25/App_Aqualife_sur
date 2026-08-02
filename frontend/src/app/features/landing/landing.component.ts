import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";

@Component({
  selector: "app-landing",
  standalone: true,
  imports: [CommonModule, RouterLink, PublicHeaderComponent],
  templateUrl: "./landing.component.html",
  styleUrls: ["./landing.component.css"],
})
export class LandingComponent {
  anioActual = new Date().getFullYear();

  irASeccion(id: string) {
    document.getElementById(id)?.scrollIntoView({ behavior: "smooth" });
  }
}
