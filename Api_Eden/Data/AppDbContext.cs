using System;
using System.Collections.Generic;
using Api_Eden.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Api_Eden.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adopcione> Adopciones { get; set; }

    public virtual DbSet<Alimento> Alimentos { get; set; }

    public virtual DbSet<Animale> Animales { get; set; }

    public virtual DbSet<Categoriasgasto> Categoriasgastos { get; set; }

    public virtual DbSet<Cierresmensuale> Cierresmensuales { get; set; }

    public virtual DbSet<Donacione> Donaciones { get; set; }

    public virtual DbSet<Donante> Donantes { get; set; }

    public virtual DbSet<Especy> Especies { get; set; }

    public virtual DbSet<Estadosgenerale> Estadosgenerales { get; set; }

    public virtual DbSet<Fallecimiento> Fallecimientos { get; set; }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<Historialmedico> Historialmedicos { get; set; }

    public virtual DbSet<Historialmovimiento> Historialmovimientos { get; set; }

    public virtual DbSet<Medicamento> Medicamentos { get; set; }

    public virtual DbSet<Movimientosinventario> Movimientosinventarios { get; set; }

    public virtual DbSet<Objetivo> Objetivos { get; set; }

    public virtual DbSet<Presupuestomensual> Presupuestomensuals { get; set; }

    public virtual DbSet<Tiposdonacion> Tiposdonacions { get; set; }

    public virtual DbSet<Tiposvacuna> Tiposvacunas { get; set; }

    public virtual DbSet<Transferencia> Transferencias { get; set; }

    public virtual DbSet<Tratamiento> Tratamientos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VResumenfinancieromesactual> VResumenfinancieromesactuals { get; set; }

    public virtual DbSet<Vacuna> Vacunas { get; set; }

    public virtual DbSet<Zona> Zonas { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Adopcione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("adopciones")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioResponsableId, "UsuarioResponsableId");

            entity.HasIndex(e => e.NombreAdoptante, "idx_adopciones_adoptante");

            entity.HasIndex(e => e.AnimalId, "idx_adopciones_animal");

            entity.HasIndex(e => e.EstadoAdopcion, "idx_adopciones_estado");

            entity.HasIndex(e => e.FechaAdopcion, "idx_adopciones_fecha");

            entity.Property(e => e.DireccionAdoptante)
                .HasColumnType("text")
                .HasColumnName("direccion_adoptante");
            entity.Property(e => e.DocumentoIdentidad)
                .HasMaxLength(50)
                .HasColumnName("documento_identidad");
            entity.Property(e => e.EmailAdoptante)
                .HasMaxLength(150)
                .HasColumnName("email_adoptante");
            entity.Property(e => e.EstadoAdopcion)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','Aprobada','Rechazada','Devuelto')")
                .HasColumnName("estado_adopcion");
            entity.Property(e => e.FechaAdopcion).HasColumnName("fecha_adopcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaSeguimiento).HasColumnName("fecha_seguimiento");
            entity.Property(e => e.NombreAdoptante)
                .HasMaxLength(200)
                .HasColumnName("nombre_adoptante");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.TelefonoAdoptante)
                .HasMaxLength(20)
                .HasColumnName("telefono_adoptante");
            entity.Property(e => e.UsuarioResponsableId).HasColumnName("usuario_responsable_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Adopciones)
                .HasForeignKey(d => d.AnimalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("adopciones_ibfk_1");

            entity.HasOne(d => d.UsuarioResponsable).WithMany(p => p.Adopciones)
                .HasForeignKey(d => d.UsuarioResponsableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("adopciones_ibfk_2");
        });

        modelBuilder.Entity<Alimento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("alimentos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CantidadDisponible, "idx_alimentos_stock");

            entity.HasIndex(e => e.TipoAnimal, "idx_alimentos_tipo");

            entity.HasIndex(e => e.FechaVencimiento, "idx_alimentos_vencimiento");

            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.CantidadDisponible)
                .HasPrecision(10, 2)
                .HasColumnName("cantidad_disponible");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.Marca)
                .HasMaxLength(100)
                .HasColumnName("marca");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.StockMinimo)
                .HasPrecision(10, 2)
                .HasColumnName("stock_minimo");
            entity.Property(e => e.TipoAnimal)
                .HasColumnType("enum('Perro','Gato','Ave','Otro')")
                .HasColumnName("tipo_animal");
            entity.Property(e => e.UnidadMedida)
                .HasColumnType("enum('Kg','Lb','Unidad','Bolsa')")
                .HasColumnName("unidad_medida");
        });

        modelBuilder.Entity<Animale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("animales")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioRegistroId, "UsuarioRegistroId");

            entity.HasIndex(e => e.EspecieId, "id_animales_especie");

            entity.HasIndex(e => e.EstadoGeneral, "idx_animales_estado");

            entity.HasIndex(e => e.Nombre, "idx_animales_nombre");

            entity.HasIndex(e => e.ZonaActualId, "idx_animales_zona");

            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.EspecieId).HasColumnName("especie_id");
            entity.Property(e => e.EstadoGeneral)
                .HasDefaultValueSql("'Activo'")
                .HasColumnType("enum('Activo','Adoptado','Fallecido','Transferido')")
                .HasColumnName("estado_general");
            entity.Property(e => e.EstadoSalud)
                .HasDefaultValueSql("'Saludable'")
                .HasColumnType("enum('Saludable','EnTratamiento','Critico','Recuperado')")
                .HasColumnName("estado_salud");
            entity.Property(e => e.FechaAdopcion).HasColumnName("fecha_adopcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFallecimiento).HasColumnName("fecha_fallecimiento");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaNacimientoEstimada).HasColumnName("fecha_nacimiento_estimada");
            entity.Property(e => e.FechaTransferencia).HasColumnName("fecha_transferencia");
            entity.Property(e => e.FechaUltimaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultima_modificacion");
            entity.Property(e => e.FotografiaUrl).HasColumnName("fotografia_url");
            entity.Property(e => e.LugarTransferencia)
                .HasMaxLength(200)
                .HasColumnName("lugar_transferencia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Raza)
                .HasMaxLength(100)
                .HasColumnName("raza");
            entity.Property(e => e.Sexo)
                .HasColumnType("enum('Macho','Hembra','Desconocido')")
                .HasColumnName("sexo");
            entity.Property(e => e.UnidadEdad)
                .HasDefaultValueSql("'años'")
                .HasColumnType("enum('años','meses','semanas')")
                .HasColumnName("unidad_edad");
            entity.Property(e => e.UsuarioRegistroId).HasColumnName("usuario_registro_id");
            entity.Property(e => e.ZonaActualId).HasColumnName("zona_actual_id");

            entity.HasOne(d => d.Especie).WithMany(p => p.Animales)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_animales_especie");

            entity.HasOne(d => d.UsuarioRegistro).WithMany(p => p.Animales)
                .HasForeignKey(d => d.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("animales_ibfk_2");

            entity.HasOne(d => d.ZonaActual).WithMany(p => p.Animales)
                .HasForeignKey(d => d.ZonaActualId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("animales_ibfk_1");
        });

        modelBuilder.Entity<Categoriasgasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("categoriasgasto");

            entity.HasIndex(e => e.Nombre, "Nombre").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValueSql("'1'");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Cierresmensuale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cierresmensuales");

            entity.HasIndex(e => e.UsuarioCierreId, "UsuarioCierreId");

            entity.HasIndex(e => new { e.Año, e.Mes }, "idx_cierres_periodo").IsUnique();

            entity.Property(e => e.Balance).HasPrecision(10, 2);
            entity.Property(e => e.BalanceAnterior).HasPrecision(10, 2);
            entity.Property(e => e.BalanceFinal).HasPrecision(10, 2);
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.TotalEgresos).HasPrecision(10, 2);
            entity.Property(e => e.TotalIngresos).HasPrecision(10, 2);

            entity.HasOne(d => d.UsuarioCierre).WithMany(p => p.Cierresmensuales)
                .HasForeignKey(d => d.UsuarioCierreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cierresmensuales_ibfk_1");
        });

        modelBuilder.Entity<Donacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("donaciones");

            entity.HasIndex(e => e.AlimentoId, "AlimentoId");

            entity.HasIndex(e => e.MedicamentoId, "MedicamentoId");

            entity.HasIndex(e => e.UsuarioRegistroId, "UsuarioRegistroId");

            entity.HasIndex(e => e.ObjetivoId, "fk_donacion_objetivo");

            entity.HasIndex(e => e.DonanteId, "idx_donaciones_donante");

            entity.HasIndex(e => e.FechaDonacion, "idx_donaciones_fecha");

            entity.HasIndex(e => e.TipoDonacionId, "idx_donaciones_tipo");

            entity.Property(e => e.CertificadoGenerado).HasDefaultValueSql("'0'");
            entity.Property(e => e.DescripcionDonacion).HasColumnType("text");
            entity.Property(e => e.DocumentoAdjunto).HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FormaPago).HasColumnType("enum('Efectivo','Transferencia','Tarjeta','Cheque','Especie')");
            entity.Property(e => e.MontoDinero).HasPrecision(10, 2);
            entity.Property(e => e.NumeroTransaccion).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.RequiereCertificado).HasDefaultValueSql("'0'");
            entity.Property(e => e.ValorEstimado).HasPrecision(10, 2);

            entity.HasOne(d => d.Alimento).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.AlimentoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("donaciones_ibfk_3");

            entity.HasOne(d => d.Donante).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.DonanteId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("donaciones_ibfk_1");

            entity.HasOne(d => d.Medicamento).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.MedicamentoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("donaciones_ibfk_4");

            entity.HasOne(d => d.Objetivo).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.ObjetivoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_donacion_objetivo");

            entity.HasOne(d => d.TipoDonacion).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.TipoDonacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("donaciones_ibfk_2");

            entity.HasOne(d => d.UsuarioRegistro).WithMany(p => p.Donaciones)
                .HasForeignKey(d => d.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("donaciones_ibfk_5");
        });

        modelBuilder.Entity<Donante>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("donantes");

            entity.HasIndex(e => e.Nombre, "idx_donantes_nombre");

            entity.HasIndex(e => e.EsRecurrente, "idx_donantes_recurrente");

            entity.HasIndex(e => e.TipoDonante, "idx_donantes_tipo");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.Direccion).HasColumnType("text");
            entity.Property(e => e.DocumentoIdentidad).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EsRecurrente).HasDefaultValueSql("'0'");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("curdate()");
            entity.Property(e => e.FrecuenciaDonacion).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.Rnc)
                .HasMaxLength(50)
                .HasColumnName("RNC");
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.TipoDonante).HasColumnType("enum('Persona','Empresa')");
        });

        modelBuilder.Entity<Especy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("especies")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Nombre, "idx_especies_nombre").IsUnique();

            entity.Property(e => e.Activa)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activa");
            entity.Property(e => e.CuidadosEspeciales)
                .HasColumnType("text")
                .HasColumnName("cuidados_especiales");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Estadosgenerale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("estadosgenerales")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioResponsableId, "UsuarioResponsableId");

            entity.HasIndex(e => e.AnimalId, "idx_estados_animal");

            entity.HasIndex(e => e.EstadoNuevo, "idx_estados_estado");

            entity.HasIndex(e => e.FechaCambio, "idx_estados_fecha");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.CausaFallecimiento)
                .HasColumnType("text")
                .HasColumnName("causa_fallecimiento");
            entity.Property(e => e.EstadoAnterior)
                .HasColumnType("enum('Activo','Adoptado','Fallecido','Transferido')")
                .HasColumnName("estado_anterior");
            entity.Property(e => e.EstadoNuevo)
                .HasColumnType("enum('Activo','Adoptado','Fallecido','Transferido')")
                .HasColumnName("estado_nuevo");
            entity.Property(e => e.FechaCambio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_cambio");
            entity.Property(e => e.LugarTransferencia)
                .HasMaxLength(200)
                .HasColumnName("lugar_transferencia");
            entity.Property(e => e.Motivo)
                .HasColumnType("text")
                .HasColumnName("motivo");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.UsuarioResponsableId).HasColumnName("usuario_responsable_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Estadosgenerales)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("estadosgenerales_ibfk_1");

            entity.HasOne(d => d.UsuarioResponsable).WithMany(p => p.Estadosgenerales)
                .HasForeignKey(d => d.UsuarioResponsableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("estadosgenerales_ibfk_2");
        });

        modelBuilder.Entity<Fallecimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("fallecimientos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioRegistroId, "UsuarioRegistroId");

            entity.HasIndex(e => e.VeterinarioId, "VeterinarioId");

            entity.HasIndex(e => e.AnimalId, "idx_fallecimientos_animal");

            entity.HasIndex(e => e.Fecha, "idx_fallecimientos_fecha");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.Causa)
                .HasColumnType("text")
                .HasColumnName("causa");
            entity.Property(e => e.DocumentoAdjunto)
                .HasMaxLength(255)
                .HasColumnName("documento_adjunto");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Hora)
                .HasColumnType("time")
                .HasColumnName("hora");
            entity.Property(e => e.Lugar)
                .HasMaxLength(200)
                .HasColumnName("lugar");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.UsuarioRegistroId).HasColumnName("usuario_registro_id");
            entity.Property(e => e.VeterinarioId).HasColumnName("veterinario_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Fallecimientos)
                .HasForeignKey(d => d.AnimalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fallecimientos_ibfk_1");

            entity.HasOne(d => d.UsuarioRegistro).WithMany(p => p.FallecimientoUsuarioRegistros)
                .HasForeignKey(d => d.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fallecimientos_ibfk_3");

            entity.HasOne(d => d.Veterinario).WithMany(p => p.FallecimientoVeterinarios)
                .HasForeignKey(d => d.VeterinarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fallecimientos_ibfk_2");
        });

        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("gastos");

            entity.HasIndex(e => e.AlimentoId, "AlimentoId");

            entity.HasIndex(e => e.MedicamentoId, "MedicamentoId");

            entity.HasIndex(e => e.CategoriaGastoId, "idx_gastos_categoria");

            entity.HasIndex(e => e.FechaGasto, "idx_gastos_fecha");

            entity.HasIndex(e => e.UsuarioRegistroId, "idx_gastos_usuario");

            entity.Property(e => e.Concepto).HasMaxLength(200);
            entity.Property(e => e.DocumentoAdjunto).HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FormaPago).HasColumnType("enum('Efectivo','Transferencia','Tarjeta','Cheque')");
            entity.Property(e => e.Monto).HasPrecision(10, 2);
            entity.Property(e => e.NombreProveedor).HasMaxLength(200);
            entity.Property(e => e.NumeroFactura).HasMaxLength(100);
            entity.Property(e => e.NumeroTransaccion).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.TelefonoProveedor).HasMaxLength(20);

            entity.HasOne(d => d.Alimento).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.AlimentoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gastos_ibfk_2");

            entity.HasOne(d => d.CategoriaGasto).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.CategoriaGastoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gastos_ibfk_1");

            entity.HasOne(d => d.Medicamento).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.MedicamentoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gastos_ibfk_3");

            entity.HasOne(d => d.UsuarioRegistro).WithMany(p => p.Gastos)
                .HasForeignKey(d => d.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("gastos_ibfk_4");
        });

        modelBuilder.Entity<Historialmedico>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("historialmedico")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AnimalId, "idx_medico_animal");

            entity.HasIndex(e => e.Fecha, "idx_medico_fecha");

            entity.HasIndex(e => e.VeterinarioId, "idx_medico_veterinario");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.Diagnostico)
                .HasColumnType("text")
                .HasColumnName("diagnostico");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Peso)
                .HasPrecision(5, 2)
                .HasColumnName("peso");
            entity.Property(e => e.Sintomas)
                .HasColumnType("text")
                .HasColumnName("sintomas");
            entity.Property(e => e.Temperatura)
                .HasPrecision(4, 2)
                .HasColumnName("temperatura");
            entity.Property(e => e.VeterinarioId).HasColumnName("veterinario_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Historialmedicos)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("historialmedico_ibfk_1");

            entity.HasOne(d => d.Veterinario).WithMany(p => p.Historialmedicos)
                .HasForeignKey(d => d.VeterinarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historialmedico_ibfk_2");
        });

        modelBuilder.Entity<Historialmovimiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("historialmovimientos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ZonaDestinoId, "ZonaDestinoId");

            entity.HasIndex(e => e.ZonaOrigenId, "ZonaOrigenId");

            entity.HasIndex(e => e.AnimalId, "idx_movimientos_animal");

            entity.HasIndex(e => e.Fecha, "idx_movimientos_fecha");

            entity.HasIndex(e => e.UsuarioResponsableId, "idx_movimientos_usuario");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Motivo)
                .HasColumnType("text")
                .HasColumnName("motivo");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.UsuarioResponsableId).HasColumnName("usuario_responsable_id");
            entity.Property(e => e.ZonaDestinoId).HasColumnName("zona_destino_id");
            entity.Property(e => e.ZonaOrigenId).HasColumnName("zona_origen_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Historialmovimientos)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("historialmovimientos_ibfk_1");

            entity.HasOne(d => d.UsuarioResponsable).WithMany(p => p.Historialmovimientos)
                .HasForeignKey(d => d.UsuarioResponsableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("historialmovimientos_ibfk_4");

            entity.HasOne(d => d.ZonaDestino).WithMany(p => p.HistorialmovimientoZonaDestinos)
                .HasForeignKey(d => d.ZonaDestinoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("historialmovimientos_ibfk_3");

            entity.HasOne(d => d.ZonaOrigen).WithMany(p => p.HistorialmovimientoZonaOrigens)
                .HasForeignKey(d => d.ZonaOrigenId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("historialmovimientos_ibfk_2");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("medicamentos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Nombre, "idx_medicamentos_nombre").IsUnique();

            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Concentracion)
                .HasMaxLength(50)
                .HasColumnName("concentracion");
            entity.Property(e => e.Contraindicaciones)
                .HasColumnType("text")
                .HasColumnName("contraindicaciones");
            entity.Property(e => e.EfectosSecundarios)
                .HasColumnType("text")
                .HasColumnName("efectos_secundarios");
            entity.Property(e => e.Fabricante)
                .HasMaxLength(100)
                .HasColumnName("fabricante");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Indicaciones)
                .HasColumnType("text")
                .HasColumnName("indicaciones");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .HasColumnName("nombre");
            entity.Property(e => e.Presentacion)
                .HasMaxLength(100)
                .HasColumnName("presentacion");
            entity.Property(e => e.PrincipioActivo)
                .HasMaxLength(200)
                .HasColumnName("principio_activo");
            entity.Property(e => e.RequiereReceta)
                .HasDefaultValueSql("'0'")
                .HasColumnName("requiere_receta");
        });

        modelBuilder.Entity<Movimientosinventario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("movimientosinventario")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AlimentoId, "idx_inventario_alimento");

            entity.HasIndex(e => e.FechaMovimiento, "idx_inventario_fecha");

            entity.HasIndex(e => e.TipoMovimiento, "idx_inventario_tipo");

            entity.HasIndex(e => e.UsuarioResponsableId, "idx_inventario_usuario");

            entity.Property(e => e.AlimentoId).HasColumnName("alimento_id");
            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 2)
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoUnitario)
                .HasPrecision(10, 2)
                .HasColumnName("costo_unitario");
            entity.Property(e => e.FechaMovimiento)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_movimiento");
            entity.Property(e => e.Motivo)
                .HasMaxLength(200)
                .HasColumnName("motivo");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoMovimiento)
                .HasColumnType("enum('Entrada','Salida')")
                .HasColumnName("tipo_movimiento");
            entity.Property(e => e.UsuarioResponsableId).HasColumnName("usuario_responsable_id");

            entity.HasOne(d => d.Alimento).WithMany(p => p.Movimientosinventarios)
                .HasForeignKey(d => d.AlimentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("movimientosinventario_ibfk_1");

            entity.HasOne(d => d.UsuarioResponsable).WithMany(p => p.Movimientosinventarios)
                .HasForeignKey(d => d.UsuarioResponsableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("movimientosinventario_ibfk_2");
        });

        modelBuilder.Entity<Objetivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("objetivos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioCreoId, "UsuarioCreoId");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Activo'")
                .HasColumnType("enum('Activo','Completado','Pausado')");
            entity.Property(e => e.FechaActualizacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.MontoObjetivo).HasPrecision(12, 2);
            entity.Property(e => e.MontoRecaudado).HasPrecision(12, 2);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Observaciones).HasColumnType("text");

            entity.HasOne(d => d.UsuarioCreo).WithMany(p => p.Objetivos)
                .HasForeignKey(d => d.UsuarioCreoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("objetivos_ibfk_1");
        });

        modelBuilder.Entity<Presupuestomensual>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("presupuestomensual");

            entity.HasIndex(e => e.CategoriaGastoId, "idx_presupuesto_categoria");

            entity.HasIndex(e => new { e.Año, e.Mes }, "idx_presupuesto_periodo");

            entity.HasIndex(e => new { e.Año, e.Mes, e.CategoriaGastoId }, "unique_presupuesto").IsUnique();

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.MontoEjecutado)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.MontoPresupuestado).HasPrecision(10, 2);
            entity.Property(e => e.Observaciones).HasColumnType("text");

            entity.HasOne(d => d.CategoriaGasto).WithMany(p => p.Presupuestomensuals)
                .HasForeignKey(d => d.CategoriaGastoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("presupuestomensual_ibfk_1");
        });

        modelBuilder.Entity<Tiposdonacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tiposdonacion");

            entity.HasIndex(e => e.Nombre, "Nombre").IsUnique();

            entity.Property(e => e.Activa).HasDefaultValueSql("'1'");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Tiposvacuna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tiposvacunas")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.EspecieId, "idx_tipos_vacunas_especie");

            entity.HasIndex(e => new { e.Nombre, e.EspecieId }, "unique_vacuna_especie").IsUnique();

            entity.Property(e => e.Activa)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activa");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.DuracionMeses).HasColumnName("duracion_meses");
            entity.Property(e => e.EdadMinima).HasColumnName("edad_minima");
            entity.Property(e => e.EspecieId).HasColumnName("especie_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Obligatoria)
                .HasDefaultValueSql("'0'")
                .HasColumnName("obligatoria");

            entity.HasOne(d => d.Especie).WithMany(p => p.Tiposvacunas)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tiposvacunas_ibfk_1");
        });

        modelBuilder.Entity<Transferencia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("transferencias")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UsuarioResponsableId, "UsuarioResponsableId");

            entity.HasIndex(e => e.AnimalId, "idx_transferencias_animal");

            entity.HasIndex(e => e.FechaTransferencia, "idx_transferencias_fecha");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.ContactoDestino)
                .HasMaxLength(200)
                .HasColumnName("contacto_destino");
            entity.Property(e => e.DireccionDestino)
                .HasColumnType("text")
                .HasColumnName("direccion_destino");
            entity.Property(e => e.DocumentoTransferencia)
                .HasMaxLength(255)
                .HasColumnName("documento_transferencia");
            entity.Property(e => e.EmailDestino)
                .HasMaxLength(150)
                .HasColumnName("email_destino");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaTransferencia).HasColumnName("fecha_transferencia");
            entity.Property(e => e.LugarDestino)
                .HasMaxLength(200)
                .HasColumnName("lugar_destino");
            entity.Property(e => e.MotivoTransferencia)
                .HasColumnType("text")
                .HasColumnName("motivo_transferencia");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.TelefonoDestino)
                .HasMaxLength(20)
                .HasColumnName("telefono_destino");
            entity.Property(e => e.UsuarioResponsableId).HasColumnName("usuario_responsable_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Transferencia)
                .HasForeignKey(d => d.AnimalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transferencias_ibfk_1");

            entity.HasOne(d => d.UsuarioResponsable).WithMany(p => p.Transferencia)
                .HasForeignKey(d => d.UsuarioResponsableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transferencias_ibfk_2");
        });

        modelBuilder.Entity<Tratamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tratamientos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.MedicamentoId, "MedicamentoId");

            entity.HasIndex(e => e.VeterinarioId, "VeterinarioId");

            entity.HasIndex(e => e.Estado, "idx_tratamientos_estado");

            entity.HasIndex(e => new { e.FechaInicio, e.FechaFin }, "idx_tratamientos_fechas");

            entity.HasIndex(e => e.HistorialMedicoId, "idx_tratamientos_historial");

            entity.Property(e => e.Dosis)
                .HasMaxLength(100)
                .HasColumnName("dosis");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Activo'")
                .HasColumnType("enum('Activo','Completado','Suspendido')")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Frecuencia)
                .HasMaxLength(100)
                .HasColumnName("frecuencia");
            entity.Property(e => e.HistorialMedicoId).HasColumnName("historial_medico_id");
            entity.Property(e => e.MedicamentoId).HasColumnName("medicamento_id");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.VeterinarioId).HasColumnName("veterinario_id");
            entity.Property(e => e.ViaAdministracion)
                .HasMaxLength(50)
                .HasColumnName("via_administracion");

            entity.HasOne(d => d.HistorialMedico).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.HistorialMedicoId)
                .HasConstraintName("tratamientos_ibfk_1");

            entity.HasOne(d => d.Medicamento).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.MedicamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tratamientos_ibfk_4");

            entity.HasOne(d => d.Veterinario).WithMany(p => p.Tratamientos)
                .HasForeignKey(d => d.VeterinarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tratamientos_ibfk_3");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("usuarios")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Rol, "idx_usuarios_rol");

            entity.Property(e => e.Activo)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificado).HasColumnName("email_verificado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaUltimaModificacion)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_ultima_modificacion");
            entity.Property(e => e.FotoPerfilUrl).HasColumnName("foto_perfil_url");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasColumnType("enum('Administrador','Veterinario','Trabajador')")
                .HasColumnName("rol");
            entity.Property(e => e.TokenActivacion)
                .HasMaxLength(255)
                .HasColumnName("token_activacion");
            entity.Property(e => e.TokenExpiracion)
                .HasColumnType("datetime")
                .HasColumnName("token_expiracion");
        });

        modelBuilder.Entity<VResumenfinancieromesactual>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_resumenfinancieromesactual");

            entity.Property(e => e.Balance).HasPrecision(33, 2);
            entity.Property(e => e.Periodo)
                .HasMaxLength(10)
                .HasDefaultValueSql("''");
            entity.Property(e => e.TotalEgresos).HasPrecision(32, 2);
            entity.Property(e => e.TotalIngresos).HasPrecision(32, 2);
        });

        modelBuilder.Entity<Vacuna>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("vacunas")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.TipoVacunaId, "TipoVacunaId");

            entity.HasIndex(e => e.VeterinarioId, "VeterinarioId");

            entity.HasIndex(e => e.AnimalId, "idx_vacunas_animal");

            entity.HasIndex(e => e.FechaAplicacion, "idx_vacunas_fecha");

            entity.HasIndex(e => e.ProximaDosis, "idx_vacunas_proxima");

            entity.Property(e => e.AnimalId).HasColumnName("animal_id");
            entity.Property(e => e.FechaAplicacion).HasColumnName("fecha_aplicacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Lote)
                .HasMaxLength(50)
                .HasColumnName("lote");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.ProximaDosis).HasColumnName("proxima_dosis");
            entity.Property(e => e.TipoVacunaId).HasColumnName("tipo_vacuna_id");
            entity.Property(e => e.VeterinarioId).HasColumnName("veterinario_id");

            entity.HasOne(d => d.Animal).WithMany(p => p.Vacunas)
                .HasForeignKey(d => d.AnimalId)
                .HasConstraintName("vacunas_ibfk_1");

            entity.HasOne(d => d.TipoVacuna).WithMany(p => p.Vacunas)
                .HasForeignKey(d => d.TipoVacunaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vacunas_ibfk_3");

            entity.HasOne(d => d.Veterinario).WithMany(p => p.Vacunas)
                .HasForeignKey(d => d.VeterinarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vacunas_ibfk_2");
        });

        modelBuilder.Entity<Zona>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("zonas")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Activa, "idx_zonas_activa");

            entity.Property(e => e.Activa)
                .HasDefaultValueSql("'1'")
                .HasColumnName("activa");
            entity.Property(e => e.CantidadActual)
                .HasDefaultValueSql("'0'")
                .HasColumnName("cantidad_actual");
            entity.Property(e => e.CapacidadMaxima).HasColumnName("capacidad_maxima");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
