using Api_Eden.Data;
using Api_Eden.DTOs.GastosDto;
using Api_Eden.Models;
using Api_Eden.Services.GastosService.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.GastosService
{
    public class GastoService : IGastoService
    {
        private readonly AppDbContext _db;

        public GastoService(AppDbContext db) => _db = db;

        public async Task<object> GetGastos()
        {
            return await _db.Gastos
                .Include(g => g.CategoriaGasto)
                .Include(g => g.UsuarioRegistro)
                .OrderByDescending(g => g.FechaGasto)
                .Select(g => new
                {
                    g.Id,
                    Categoria = g.CategoriaGasto.Nombre,
                    g.Concepto,
                    g.Monto,
                    g.FechaGasto,
                    g.FormaPago,
                    g.NumeroFactura,
                    g.NombreProveedor,
                    g.Observaciones,
                    RegistradoPor = $"{g.UsuarioRegistro.Nombre} {g.UsuarioRegistro.Apellido}",
                    g.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<(bool ok, string mensaje, object? data)> GetGasto(int id)
        {
            var g = await _db.Gastos
                .Include(g => g.CategoriaGasto)
                .Include(g => g.UsuarioRegistro)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (g is null)
                return (false, "Gasto no encontrado.", null);

            var data = new
            {
                g.Id,
                Categoria = g.CategoriaGasto.Nombre,
                g.Concepto,
                g.Monto,
                g.FechaGasto,
                g.FormaPago,
                g.NumeroFactura,
                g.NumeroTransaccion,
                g.NombreProveedor,
                g.TelefonoProveedor,
                g.AlimentoId,
                g.MedicamentoId,
                g.Observaciones,
                RegistradoPor = $"{g.UsuarioRegistro.Nombre} {g.UsuarioRegistro.Apellido}",
                g.FechaCreacion
            };

            return (true, "OK", data);
        }

        public async Task<object> GetCategorias()
        {
            return await _db.Categoriasgastos
                .Where(c => c.Activa == true)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Descripcion
                })
                .ToListAsync();
        }

        public async Task<(bool ok, string mensaje)> CrearGasto(CrearGastoDto dto)
        {
            // Validaciones de negocio
            var categoria = await _db.Categoriasgastos.FindAsync(dto.CategoriaGastoId);
            if (categoria is null)
                return (false, "Categoría de gasto no encontrada.");

            var formasValidas = new[] { "Efectivo", "Transferencia", "Tarjeta", "Cheque" };
            if (!formasValidas.Contains(dto.FormaPago))
                return (false, "Forma de pago inválida. Usa: Efectivo, Transferencia, Tarjeta o Cheque.");

            if (dto.Monto <= 0)
                return (false, "El monto debe ser mayor a 0.");

            if (dto.AlimentoId.HasValue)
            {
                var alimento = await _db.Alimentos.FindAsync(dto.AlimentoId);
                if (alimento is null)
                    return (false, "El alimento especificado no existe.");
            }

            // Inserción directa con Entity Framework Core.
            // Antes se usaba el procedimiento almacenado SP_RegistrarGasto, pero en
            // producción (MySQL sobre Linux, sensible a mayúsculas) fallaba porque el
            // SP referenciaba la tabla "Gastos" y en el servidor se llama "gastos".
            var gasto = new Gasto
            {
                CategoriaGastoId = dto.CategoriaGastoId,
                Concepto = dto.Concepto,
                Monto = dto.Monto,
                FechaGasto = dto.FechaGasto,
                FormaPago = dto.FormaPago,
                NumeroFactura = dto.NumeroFactura,
                NumeroTransaccion = dto.NumeroTransaccion,
                NombreProveedor = dto.NombreProveedor,
                TelefonoProveedor = dto.TelefonoProveedor,
                AlimentoId = dto.AlimentoId,
                MedicamentoId = dto.MedicamentoId,
                Observaciones = dto.Observaciones,
                UsuarioRegistroId = dto.UsuarioRegistroId,
                FechaCreacion = DateTime.Now
            };

            _db.Gastos.Add(gasto);
            await _db.SaveChangesAsync();

            return (true, "Gasto registrado correctamente.");
        }

        public async Task<(bool ok, string mensaje)> ActualizarGasto(int id, ActualizarGastoDto dto)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto is null)
                return (false, "Gasto no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Concepto))
                gasto.Concepto = dto.Concepto;

            if (dto.Monto.HasValue)
            {
                if (dto.Monto <= 0)
                    return (false, "El monto debe ser mayor a 0.");
                gasto.Monto = dto.Monto.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.FormaPago))
            {
                var formasValidas = new[] { "Efectivo", "Transferencia", "Tarjeta", "Cheque" };
                if (!formasValidas.Contains(dto.FormaPago))
                    return (false, "Forma de pago inválida. Usa: Efectivo, Transferencia, Tarjeta o Cheque.");
                gasto.FormaPago = dto.FormaPago;
            }

            if (!string.IsNullOrWhiteSpace(dto.NumeroFactura))
                gasto.NumeroFactura = dto.NumeroFactura;

            if (!string.IsNullOrWhiteSpace(dto.NumeroTransaccion))
                gasto.NumeroTransaccion = dto.NumeroTransaccion;

            if (!string.IsNullOrWhiteSpace(dto.NombreProveedor))
                gasto.NombreProveedor = dto.NombreProveedor;

            if (!string.IsNullOrWhiteSpace(dto.TelefonoProveedor))
                gasto.TelefonoProveedor = dto.TelefonoProveedor;

            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                gasto.Observaciones = dto.Observaciones;

            await _db.SaveChangesAsync();
            return (true, "Gasto actualizado correctamente.");
        }

        public async Task<(bool ok, string mensaje)> EliminarGasto(int id)
        {
            var gasto = await _db.Gastos.FindAsync(id);
            if (gasto is null)
                return (false, "Gasto no encontrado.");

            _db.Gastos.Remove(gasto);
            await _db.SaveChangesAsync();

            return (true, "Gasto eliminado correctamente.");
        }

        public async Task<object> GetResumenMensual()
        {
            var hoy = DateTime.Today;
            var year = hoy.Year;
            var mes = hoy.Month;

            var totalGastos = await _db.Gastos
                .Where(g => g.FechaGasto.Year == year && g.FechaGasto.Month == mes)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var gastosPorCategoria = await _db.Gastos
                .Where(g => g.FechaGasto.Year == year && g.FechaGasto.Month == mes)
                .Include(g => g.CategoriaGasto)
                .GroupBy(g => g.CategoriaGasto.Nombre)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(x => x.Monto),
                    Cantidad = g.Count()
                })
                .OrderByDescending(g => g.Total)
                .ToListAsync();

            return new
            {
                periodo = $"{hoy:MMMM yyyy}",
                totalGastos,
                gastosPorCategoria
            };
        }
        public async Task<object> GetSerieMensual(int? year)
        {
            var query = _db.Gastos.AsQueryable();

            if (year.HasValue)
                query = query.Where(g => g.FechaGasto.Year == year.Value);

            var serie = await query
                .GroupBy(g => new { g.FechaGasto.Year, g.FechaGasto.Month })
                .Select(grp => new
                {
                    anio = grp.Key.Year,
                    mes = grp.Key.Month,
                    total = grp.Sum(x => x.Monto)
                })
                .OrderBy(x => x.anio)
                .ThenBy(x => x.mes)
                .ToListAsync();

            return serie;
        }

        public async Task<object> GetGastosPorCategoria(int year, int mes)
        {
            if (mes < 1 || mes > 12)
                return new { error = "Mes inválido. Debe estar entre 1 y 12." };

            return await _db.Gastos
                .Where(g => g.FechaGasto.Year == year && g.FechaGasto.Month == mes)
                .Include(g => g.CategoriaGasto)
                .GroupBy(g => g.CategoriaGasto.Nombre)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(x => x.Monto),
                    Cantidad = g.Count()
                })
                .OrderByDescending(g => g.Total)
                .ToListAsync();
        }
    } 
}
