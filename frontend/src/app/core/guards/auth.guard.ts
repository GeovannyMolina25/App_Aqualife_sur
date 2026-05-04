import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth/auth.service";

export const authGuard = () => {
  const auth = inject(AuthService),
    router = inject(Router);
  return auth.estaAutenticado() ? true : router.navigate(["/auth/login"]);
};
