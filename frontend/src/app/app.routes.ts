import { Routes } from "@angular/router";
import { authGuard } from "./core/guards/auth.guard";
import { adminGuard } from "./core/guards/admin.guard";
import { colaboradorGuard } from "./core/guards/colaborador.guard";

export const routes: Routes = [
  { path: "", redirectTo: "/auth/login", pathMatch: "full" },
  {
    path: "auth",
    children: [
      {
        path: "login",
        loadComponent: () =>
          import("./features/auth/login/login.component").then(
            (m) => m.LoginComponent,
          ),
      },
      {
        path: "registro",
        loadComponent: () =>
          import("./features/auth/registro/registro.component").then(
            (m) => m.RegistroComponent,
          ),
      },
    ],
  },
  {
    path: "dashboard",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./features/layout/layout.component").then(
        (m) => m.LayoutComponent,
      ),
    children: [
      {
        path: "",
        loadComponent: () =>
          import("./features/dashboard/dashboard.component").then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: "productos",
        loadComponent: () =>
          import("./features/productos/productos.component").then(
            (m) => m.ProductosComponent,
          ),
      },
      {
        path: "ventas",
        canActivate: [colaboradorGuard],
        loadComponent: () =>
          import("./features/ventas/ventas.component").then(
            (m) => m.VentasComponent,
          ),
      },
      {
        path: "mi-historial",
        loadComponent: () =>
          import("./features/historial/historial.component").then(
            (m) => m.HistorialComponent,
          ),
      },
      {
        path: "usuarios",
        canActivate: [adminGuard],
        loadComponent: () =>
          import("./features/usuarios/usuarios.component").then(
            (m) => m.UsuariosComponent,
          ),
      },
      {
        path: "reportes",
        canActivate: [adminGuard],
        loadComponent: () =>
          import("./features/reportes/reportes.component").then(
            (m) => m.ReportesComponent,
          ),
      },
    ],
  },
  { path: "**", redirectTo: "/auth/login" },
];
