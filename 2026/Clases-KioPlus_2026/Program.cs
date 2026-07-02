using System.Text.Json.Serialization;
using Clases_KioPlus.Data;
using Clases_KioPlus.Endpoints;
using Clases_KioPlus.Logica;
using Clases_KioPlus.Middleware;
using Clases_KioPlus.Repositorios;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Serializar los enums como texto (ej. "Administrador") en lugar de números
builder.Services.ConfigureHttpJsonOptions(opt =>
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// 3. Inyección de dependencias (Repositorios + Lógica)
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<ICategoriaLogica, CategoriaLogica>();

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IUsuarioLogica, UsuarioLogica>();

builder.Services.AddScoped<IProveedorRepositorio, ProveedorRepositorio>();
builder.Services.AddScoped<IProveedorLogica, ProveedorLogica>();

builder.Services.AddScoped<ICuentaCorrienteClienteRepositorio, CuentaCorrienteClienteRepositorio>();
builder.Services.AddScoped<ICuentaCorrienteClienteLogica, CuentaCorrienteClienteLogica>();

builder.Services.AddScoped<IProductoRepositorio, ProductoRepositorio>();
builder.Services.AddScoped<IProductoLogica, ProductoLogica>();

builder.Services.AddScoped<ILoteRepositorio, LoteRepositorio>();
builder.Services.AddScoped<ILoteLogica, LoteLogica>();

builder.Services.AddScoped<IProductoProveedorRepositorio, ProductoProveedorRepositorio>();
builder.Services.AddScoped<IProductoProveedorLogica, ProductoProveedorLogica>();

builder.Services.AddScoped<IVentaRepositorio, VentaRepositorio>();
builder.Services.AddScoped<IVentaLogica, VentaLogica>();

builder.Services.AddScoped<IDetalleVentaRepositorio, DetalleVentaRepositorio>();
builder.Services.AddScoped<IDetalleVentaLogica, DetalleVentaLogica>();

builder.Services.AddScoped<INotificacionRepositorio, NotificacionRepositorio>();
builder.Services.AddScoped<INotificacionLogica, NotificacionLogica>();

builder.Services.AddScoped<ICompraRepositorio, CompraRepositorio>();
builder.Services.AddScoped<ICompraLogica, CompraLogica>();

builder.Services.AddScoped<IDetalleCompraRepositorio, DetalleCompraRepositorio>();
builder.Services.AddScoped<IDetalleCompraLogica, DetalleCompraLogica>();

builder.Services.AddScoped<ICajaRepositorio, CajaRepositorio>();
builder.Services.AddScoped<ICajaLogica, CajaLogica>();

// 4. Documentación OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Manejo global de excepciones: 503 ante fallas de conexión a la BD, 500 en cualquier otro caso
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

// 5. Registrar rutas
app.MapUsuarioEndpoints();
app.MapProductoEndpoints();
app.MapCategoriaEndpoints();
app.MapLoteEndpoints();
app.MapVentaEndpoints();
app.MapDetalleVentaEndpoints();
app.MapProveedorEndpoints();
app.MapCuentaCorrienteClienteEndpoints();
app.MapProductoProveedorEndpoints();
app.MapNotificacionEndpoints();
app.MapCompraEndpoints();
app.MapDetalleCompraEndpoints();
app.MapCajaEndpoints();

app.Run();
