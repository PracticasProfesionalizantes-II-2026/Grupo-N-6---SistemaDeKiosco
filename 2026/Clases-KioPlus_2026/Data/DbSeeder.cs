using Clases_KioPlus.Logica;
using Clases_KioPlus.Models;
using Microsoft.EntityFrameworkCore;

namespace Clases_KioPlus.Data;

// Datos mínimos sin los cuales el sistema no puede operar:
// el cliente Consumidor Final (id 1) y un usuario para poder iniciar sesión.
public static class DbSeeder
{
    public const string UsuarioInicial = "admin";
    public const string ContraseniaInicial = "admin123";

    public static async Task SembrarAsync(ApplicationDbContext db, ILogger logger)
    {
        await db.Database.MigrateAsync();
        await SembrarConsumidorFinal(db, logger);
        await SembrarUsuarioInicial(db, logger);
        await HashearContraseniasEnTextoPlano(db, logger);
    }

    // Se inserta con id explícito porque toda venta sin cliente apunta a este registro.
    private static async Task SembrarConsumidorFinal(ApplicationDbContext db, ILogger logger)
    {
        var id = CuentaCorrienteCliente.IdConsumidorFinal;
        if (await db.CuentasCorrientesClientes.AnyAsync(c => c.Id == id)) return;

        await db.Database.ExecuteSqlRawAsync(@"
            SET IDENTITY_INSERT CuentasCorrientesClientes ON;
            INSERT INTO CuentasCorrientesClientes
                (Id, Nombre, Apellido, Dni, Telefono, Direccion, CorreoElectronico, MontoAdeudado, Estado, FechaAlta, Activo)
            VALUES ({0}, 'Consumidor', 'Final', 0, '-', '-', '', 0, {1}, GETDATE(), 1);
            SET IDENTITY_INSERT CuentasCorrientesClientes OFF;",
            id, (int)CuentaCorrienteCliente.EstadoDeuda.AlDia);

        logger.LogInformation("Cuenta Consumidor Final creada con id {Id}", id);
    }

    // Sin al menos un usuario activo nadie podría entrar a la aplicación.
    private static async Task SembrarUsuarioInicial(ApplicationDbContext db, ILogger logger)
    {
        if (await db.Usuarios.AnyAsync(u => u.Activo)) return;

        db.Usuarios.Add(new Usuario
        {
            NombreApellido = "Administrador",
            Telefono = "-",
            NombreUsuario = UsuarioInicial,
            ContraseniaUsuario = HashContrasenia.Calcular(ContraseniaInicial),
            TipoUsuario = Usuario.TipoDeUsuario.SuperAdmin,
            Estado = true
        });
        await db.SaveChangesAsync();

        logger.LogWarning(
            "Usuario inicial creado: {Usuario} / {Contrasenia}. Cambialo antes de usar el sistema en serio.",
            UsuarioInicial, ContraseniaInicial);
    }

    // Las contraseñas cargadas antes de usar el hash quedaron en texto plano.
    // Se hashean una sola vez al arrancar, así esos usuarios siguen entrando
    // con la misma contraseña de siempre.
    private static async Task HashearContraseniasEnTextoPlano(ApplicationDbContext db, ILogger logger)
    {
        var pendientes = (await db.Usuarios.ToListAsync())
            .Where(u => !HashContrasenia.EsHash(u.ContraseniaUsuario))
            .ToList();
        if (pendientes.Count == 0) return;

        foreach (var usuario in pendientes)
            usuario.ContraseniaUsuario = HashContrasenia.Calcular(usuario.ContraseniaUsuario);
        await db.SaveChangesAsync();

        logger.LogInformation("Contraseñas hasheadas con SHA-256: {Cantidad}", pendientes.Count);
    }
}
