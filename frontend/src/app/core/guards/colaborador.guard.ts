import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth/auth.service";

export const colaboradorGuard = () => {
  const auth = inject(AuthService),
    router = inject(Router);
  return auth.estaAutenticado() &&
    (auth.esAdmin() || auth.esColaborador() || auth.esSuperAdmin())
    ? true
    : router.navigate(["/dashboard"]);
};
