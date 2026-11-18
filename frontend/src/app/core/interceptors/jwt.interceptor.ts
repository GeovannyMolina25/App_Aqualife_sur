import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth/auth.service';

const RUTAS_AUTH_PUBLICAS = /\/auth\/(login|registro|refresh|recuperar-password)$/;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();
  const conToken = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(conToken).pipe(
    catchError((error) => {
      const puedeRefrescar = error?.status === 401 && !RUTAS_AUTH_PUBLICAS.test(req.url) && auth.getRefreshToken();

      if (!puedeRefrescar) {
        if (error?.status === 401) auth.manejarTokenInvalido();
        return throwError(() => error);
      }

      return auth.refrescarToken().pipe(
        switchMap((nuevoToken) => {
          if (!nuevoToken) return throwError(() => error);
          const reintento = req.clone({ setHeaders: { Authorization: `Bearer ${nuevoToken}` } });
          return next(reintento);
        }),
      );
    }),
  );
};
