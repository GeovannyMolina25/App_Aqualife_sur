import { inject } from "@angular/core";
import { Router, type CanActivateFn } from "@angular/router";
import { AuthService } from "../services/auth/auth.service";

export const authGuard: CanActivateFn = (route, state) => {

  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.estaAutenticado()
    ? true
    : router.createUrlTree(
        ["/auth/login"],
        { queryParams: { returnUrl: state.url } },
      );
};