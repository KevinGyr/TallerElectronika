using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TallerElectronika.Data;
using TallerElectronika.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Colors;
using System.IO;

namespace TallerElectronika.Controllers
{
    [Auth]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: ReporteVentas
        public IActionResult ReporteVentas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.VentaDetalles)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value.AddDays(1));

                var ventas = query.OrderByDescending(v => v.FechaVenta).ToList();

                ViewBag.FechaInicio = fechaInicio;
                ViewBag.FechaFin = fechaFin;
                ViewBag.TotalVentas = ventas.Sum(v => v.Total);
                ViewBag.CantidadVentas = ventas.Count;

                return View(ventas);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ReporteVentas: " + ex.Message);
                return View(new List<Venta>());
            }
        }

        // GET: ReporteInventario
        public IActionResult ReporteInventario()
        {
            try
            {
                var productos = _context.Productos
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToList();

                var productosConBajoStock = productos.Where(p => p.Stock < p.StockMinimo).ToList();

                ViewBag.TotalProductos = productos.Count;
                ViewBag.ProductosBajoStock = productosConBajoStock.Count;
                ViewBag.ValorInventario = productos.Sum(p => p.Stock * p.PrecioUnitario).ToString("C");

                return View(productos);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ReporteInventario: " + ex.Message);
                return View(new List<Producto>());
            }
        }

        // GET: ReporteClientes
        public IActionResult ReporteClientes()
        {
            try
            {
                var clientes = _context.Clientes
                    .Include(c => c.Ventas)
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Nombre)
                    .ToList();

                ViewBag.TotalClientes = clientes.Count;
                ViewBag.ClientesConVentas = clientes.Count(c => c.Ventas != null && c.Ventas.Count > 0);

                return View(clientes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en ReporteClientes: " + ex.Message);
                return View(new List<Cliente>());
            }
        }

        // POST: GenerarPDFVentas
        [HttpPost]
        public IActionResult GenerarPDFVentas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                var query = _context.Ventas
                    .Include(v => v.Cliente)
                    .AsQueryable();

                if (fechaInicio.HasValue)
                    query = query.Where(v => v.FechaVenta >= fechaInicio.Value);

                if (fechaFin.HasValue)
                    query = query.Where(v => v.FechaVenta <= fechaFin.Value.AddDays(1));

                var ventas = query.OrderByDescending(v => v.FechaVenta).ToList();

                var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Título
                var titulo = new Paragraph("REPORTE DE VENTAS")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(titulo);

                // Fecha
                var fecha = new Paragraph("Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(fecha);

                // Rango de fechas
                if (fechaInicio.HasValue || fechaFin.HasValue)
                {
                    var rango = "Período: ";
                    if (fechaInicio.HasValue)
                        rango += fechaInicio.Value.ToString("dd/MM/yyyy");
                    if (fechaFin.HasValue)
                        rango += " - " + fechaFin.Value.ToString("dd/MM/yyyy");

                    var rangoText = new Paragraph(rango)
                        .SetFontSize(10)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    document.Add(rangoText);
                }

                document.Add(new Paragraph("\n"));

                // Tabla
                var tabla = new Table(5);
                tabla.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                // Encabezados
                tabla.AddCell(CrearCeldaEncabezado("ID Venta"));
                tabla.AddCell(CrearCeldaEncabezado("Cliente"));
                tabla.AddCell(CrearCeldaEncabezado("Fecha"));
                tabla.AddCell(CrearCeldaEncabezado("Subtotal"));
                tabla.AddCell(CrearCeldaEncabezado("Total"));

                decimal totalGeneral = 0;

                // Datos
                foreach (var venta in ventas)
                {
                    tabla.AddCell(new Cell().Add(new Paragraph(venta.VentaId.ToString())));
                    tabla.AddCell(new Cell().Add(new Paragraph((venta.Cliente?.Nombre ?? "") + " " + (venta.Cliente?.Apellido ?? ""))));
                    tabla.AddCell(new Cell().Add(new Paragraph(venta.FechaVenta.ToString("dd/MM/yyyy"))));
                    tabla.AddCell(new Cell().Add(new Paragraph(venta.SubTotal.ToString("C")))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabla.AddCell(new Cell().Add(new Paragraph(venta.Total.ToString("C")))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    totalGeneral += venta.Total;
                }

                document.Add(tabla);
                document.Add(new Paragraph("\n"));

                // Resumen
                var resumen = new Paragraph("TOTAL GENERAL: " + totalGeneral.ToString("C"))
                    .SetBold()
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                document.Add(resumen);

                document.Close();

                byte[] pdfBytes = stream.ToArray();
                stream.Dispose();

                Console.WriteLine("PDF de Ventas generado exitosamente. Tamaño: " + pdfBytes.Length + " bytes");

                return File(pdfBytes, "application/pdf", "Reporte_Ventas_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al generar PDF Ventas: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return RedirectToAction("ReporteVentas");
            }
        }

        // POST: GenerarPDFInventario
        [HttpPost]
        public IActionResult GenerarPDFInventario()
        {
            try
            {
                var productos = _context.Productos
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToList();

                var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Título
                var titulo = new Paragraph("REPORTE DE INVENTARIO")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(titulo);

                // Fecha
                var fecha = new Paragraph("Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(fecha);
                document.Add(new Paragraph("\n"));

                // Tabla
                var tabla = new Table(6);
                tabla.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                // Encabezados
                tabla.AddCell(CrearCeldaEncabezado("Código"));
                tabla.AddCell(CrearCeldaEncabezado("Nombre"));
                tabla.AddCell(CrearCeldaEncabezado("Categoría"));
                tabla.AddCell(CrearCeldaEncabezado("Precio Unit."));
                tabla.AddCell(CrearCeldaEncabezado("Stock"));
                tabla.AddCell(CrearCeldaEncabezado("Valor Total"));

                decimal valorTotal = 0;

                // Datos
                foreach (var producto in productos)
                {
                    tabla.AddCell(new Cell().Add(new Paragraph(producto.CodigoProducto)));
                    tabla.AddCell(new Cell().Add(new Paragraph(producto.Nombre)));
                    tabla.AddCell(new Cell().Add(new Paragraph(producto.Categoria ?? "")));
                    tabla.AddCell(new Cell().Add(new Paragraph(producto.PrecioUnitario.ToString("C")))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabla.AddCell(new Cell().Add(new Paragraph(producto.Stock.ToString()))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    var valor = producto.Stock * producto.PrecioUnitario;
                    tabla.AddCell(new Cell().Add(new Paragraph(valor.ToString("C")))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    valorTotal += valor;
                }

                document.Add(tabla);
                document.Add(new Paragraph("\n"));

                // Resumen
                var resumen = new Paragraph("VALOR TOTAL DEL INVENTARIO: " + valorTotal.ToString("C"))
                    .SetBold()
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                document.Add(resumen);

                document.Close();

                byte[] pdfBytes = stream.ToArray();
                stream.Dispose();

                Console.WriteLine("PDF de Inventario generado exitosamente. Tamaño: " + pdfBytes.Length + " bytes");

                return File(pdfBytes, "application/pdf", "Reporte_Inventario_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al generar PDF Inventario: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return RedirectToAction("ReporteInventario");
            }
        }

        // POST: GenerarPDFClientes
        [HttpPost]
        public IActionResult GenerarPDFClientes()
        {
            try
            {
                var clientes = _context.Clientes
                    .Include(c => c.Ventas)
                    .Where(c => c.Activo)
                    .OrderBy(c => c.Nombre)
                    .ToList();

                var stream = new MemoryStream();
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Título
                var titulo = new Paragraph("REPORTE DE CLIENTES")
                    .SetFontSize(16)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(titulo);

                // Fecha
                var fecha = new Paragraph("Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(fecha);
                document.Add(new Paragraph("\n"));

                // Tabla
                var tabla = new Table(6);
                tabla.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                // Encabezados
                tabla.AddCell(CrearCeldaEncabezado("Nombre"));
                tabla.AddCell(CrearCeldaEncabezado("Email"));
                tabla.AddCell(CrearCeldaEncabezado("Teléfono"));
                tabla.AddCell(CrearCeldaEncabezado("Ciudad"));
                tabla.AddCell(CrearCeldaEncabezado("Total Compras"));
                tabla.AddCell(CrearCeldaEncabezado("Monto Total"));

                decimal montoTotalClientes = 0;
                int totalCompras = 0;

                // Datos
                foreach (var cliente in clientes)
                {
                    var cantidadCompras = cliente.Ventas != null ? cliente.Ventas.Count : 0;
                    var montoCliente = cliente.Ventas != null ? cliente.Ventas.Sum(v => v.Total) : 0;

                    tabla.AddCell(new Cell().Add(new Paragraph(cliente.Nombre + " " + cliente.Apellido)));
                    tabla.AddCell(new Cell().Add(new Paragraph(cliente.Email ?? "")));
                    tabla.AddCell(new Cell().Add(new Paragraph(cliente.Telefono ?? "")));
                    tabla.AddCell(new Cell().Add(new Paragraph(cliente.Ciudad ?? "")));
                    tabla.AddCell(new Cell().Add(new Paragraph(cantidadCompras.ToString()))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabla.AddCell(new Cell().Add(new Paragraph(montoCliente.ToString("C")))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    montoTotalClientes += montoCliente;
                    totalCompras += cantidadCompras;
                }

                document.Add(tabla);
                document.Add(new Paragraph("\n"));

                // Resumen
                var resumenLinea1 = new Paragraph("Total de Clientes: " + clientes.Count)
                    .SetFontSize(11)
                    .SetBold();
                document.Add(resumenLinea1);

                var resumenLinea2 = new Paragraph("Total de Compras: " + totalCompras)
                    .SetFontSize(11)
                    .SetBold();
                document.Add(resumenLinea2);

                var resumenLinea3 = new Paragraph("MONTO TOTAL: " + montoTotalClientes.ToString("C"))
                    .SetFontSize(12)
                    .SetBold()
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                document.Add(resumenLinea3);

                document.Close();

                byte[] pdfBytes = stream.ToArray();
                stream.Dispose();

                Console.WriteLine("PDF de Clientes generado exitosamente. Tamaño: " + pdfBytes.Length + " bytes");

                return File(pdfBytes, "application/pdf", "Reporte_Clientes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al generar PDF Clientes: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return RedirectToAction("ReporteClientes");
            }
        }

        // Método auxiliar para crear celdas de encabezado
        private Cell CrearCeldaEncabezado(string texto)
        {
            var celda = new Cell()
                .Add(new Paragraph(texto).SetBold())
                .SetBackgroundColor(ColorConstants.DARK_GRAY)
                .SetFontColor(ColorConstants.WHITE);
            return celda;
        }
    }
}