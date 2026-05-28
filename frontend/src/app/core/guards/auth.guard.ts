import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth/auth.service";

export const authGuard = () => {

  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.estaAutenticado()
    ? true
    : router.createUrlTree(
        ["/auth/login"]
      );
};