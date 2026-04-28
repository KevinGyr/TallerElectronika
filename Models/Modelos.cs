using System;
using System.Collections.Generic;

namespace TallerElectronika.Models
{
    // Cliente
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public ICollection<Venta>? Ventas { get; set; }
    }

    // Producto
    public class Producto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string CodigoProducto { get; set; } = null!;
        public decimal PrecioUnitario { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public ICollection<MovimientoInventario>? MovimientosInventario { get; set; }
        public ICollection<VentaDetalle>? VentaDetalles { get; set; }
    }

    // Venta
    public class Venta
    {
        public int VentaId { get; set; }
        public int ClienteId { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Observaciones { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public Cliente? Cliente { get; set; }
        public ICollection<VentaDetalle>? VentaDetalles { get; set; }
    }

    // VentaDetalle
    public class VentaDetalle
    {
        public int VentaDetalleId { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public Venta? Venta { get; set; }
        public Producto? Producto { get; set; }
    }

    // MovimientoInventario
    public class MovimientoInventario
    {
        public int MovimientoId { get; set; }
        public int ProductoId { get; set; }
        public string TipoMovimiento { get; set; } = null!;
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string? UsuarioRegistro { get; set; }
        public Producto? Producto { get; set; }
    }

    // Usuario
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}