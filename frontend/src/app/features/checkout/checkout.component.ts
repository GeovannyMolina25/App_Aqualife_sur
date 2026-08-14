import { Component, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { CarritoService } from "../../core/services/carrito/carrito.service";
import { VentasService } from "../../core/services/ventas/ventas.service";
import { AuthService } from "../../core/services/auth/auth.service";
import { PromocionBienvenidaService } from "../../core/services/promocion-bienvenida/promocion-bienvenida.service";
import { Venta, MetodoPago, DATOS_BANCARIOS, WHATSAPP_PEDIDOS } from "../../core/models/ventas/venta.model";
import { PublicHeaderComponent } from "../../shared/components/public-header/public-header.component";
import { AlertComponent } from "../../shared/components/alert/alert.component";

const IVA = 0.15;

@Component({
  selector: "app-checkout",
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PublicHeaderComponent, AlertComponent],
  templateUrl: "./checkout.component.html",
  styleUrls: ["./checkout.component.css"],
})
export class CheckoutComponent implements OnInit {
  datosBancarios = DATOS_BANCARIOS;
  metodoSeleccionado = signal<MetodoPago>("TransferenciaProdubanco");
  procesando = signal(false);
  error = signal("");
  pedido = signal<Venta | null>(null);

  archivoComprobante: File | null = null;
  subiendoComprobante = signal(false);
  comprobanteSubido = signal(false);
  errorComprobante = signal("");

  envio = { direccion: "", telefono: "", receptor: "" };

  constructor(
    public carrito: CarritoService,
    private ventasSrv: VentasService,
    private auth: AuthService,
    private promoSrv: PromocionBienvenidaService,
    private router: Router,
  ) {}

  ngOnInit() {
    if (this.carrito.itemsCarrito().length === 0) {
      this.router.navigate(["/catalogo"]);
      return;
    }

    const usuario = this.auth.usuario();
    if (usuario) this.envio.receptor = `${usuario.nombre} ${usuario.apellido}`.trim();

    this.auth.obtenerPerfil().subscribe({
      next: (r) => {
        if (r.exito) {
          this.envio.direccion = r.datos.direccion ?? "";
          this.envio.telefono = r.datos.telefono ?? "";
          this.envio.receptor = `${r.datos.nombre} ${r.datos.apellido}`.trim();
        }
      },
    });
  }

  get subtotal(): number {
    return this.carrito.subtotal();
  }
  get impuestos(): number {
    return Math.round(this.subtotal * IVA * 100) / 100;
  }
  get total(): number {
    return this.subtotal + this.impuestos;
  }

  seleccionarMetodo(metodo: MetodoPago) {
    this.metodoSeleccionado.set(metodo);
  }

  esTransferencia(metodo: MetodoPago = this.metodoSeleccionado()): boolean {
    return metodo === "TransferenciaProdubanco" || metodo === "TransferenciaPichincha";
  }

  pedidoEsTransferencia(): boolean {
    const metodo = this.pedido()?.metodoPago;
    return metodo === "TransferenciaProdubanco" || metodo === "TransferenciaPichincha";
  }

  datosBancoActual() {
    const metodo = this.metodoSeleccionado();
    return metodo === "TransferenciaProdubanco" || metodo === "TransferenciaPichincha"
      ? this.datosBancarios[metodo]
      : null;
  }

  confirmarPedido() {
    if (!this.envio.direccion.trim() || !this.envio.telefono.trim() || !this.envio.receptor.trim()) {
      this.error.set("Completa la dirección de envío, el teléfono y el nombre de quien recibe el pedido.");
      return;
    }

    this.error.set("");
    this.procesando.set(true);

    const dto = {
      items: this.carrito.itemsCarrito().map((i) => ({ productoId: i.producto.id, cantidad: i.cantidad })),
      metodoPago: this.metodoSeleccionado(),
      direccionEnvio: this.envio.direccion.trim(),
      telefonoContacto: this.envio.telefono.trim(),
      nombreReceptor: this.envio.receptor.trim(),
    };

    this.ventasSrv.checkout(dto).subscribe({
      next: (r) => {
        this.procesando.set(false);
        if (r.exito) {
          this.pedido.set(r.datos);
          this.carrito.vaciar();
          // Se muestra un poco después de la confirmación para no encimar dos modales a la vez.
          setTimeout(() => this.promoSrv.mostrarProgresoTrasCompra(), 700);
        } else {
          this.error.set(r.mensaje);
        }
      },
      error: (e) => {
        this.procesando.set(false);
        this.error.set(e?.error?.mensaje ?? "No se pudo procesar el pedido. Intenta de nuevo.");
      },
    });
  }

  onArchivoSeleccionado(event: Event) {
    const input = event.target as HTMLInputElement;
    this.archivoComprobante = input.files?.[0] ?? null;
    this.errorComprobante.set("");
  }

  subirComprobante() {
    const pedido = this.pedido();
    if (!pedido || !this.archivoComprobante) return;

    this.subiendoComprobante.set(true);
    this.errorComprobante.set("");

    this.ventasSrv.subirComprobante(pedido.id, this.archivoComprobante).subscribe({
      next: (r) => {
        this.subiendoComprobante.set(false);
        if (r.exito) {
          this.comprobanteSubido.set(true);
        } else {
          this.errorComprobante.set(r.mensaje);
        }
      },
      error: (e) => {
        this.subiendoComprobante.set(false);
        this.errorComprobante.set(e?.error?.mensaje ?? "No se pudo subir el comprobante.");
      },
    });
  }

  enlaceWhatsapp(): string {
    const pedido = this.pedido();
    if (!pedido) return "#";
    const mensaje = `Hola, quiero confirmar mi pedido ${pedido.numeroVenta} por un total de $${pedido.total.toFixed(2)} (pago contra entrega).`;
    return `https://wa.me/${WHATSAPP_PEDIDOS}?text=${encodeURIComponent(mensaje)}`;
  }
}
