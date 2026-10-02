using KioPlusFront.Models;
using KioPlusFront.Services;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//
// Los mensajes de abajo son los que arma el propio framework cuando el dato que
// llegó no se puede convertir al tipo del campo: por ejemplo letras en un campo
// numérico. Vienen en inglés, así que se reemplazan acá, en un solo lugar.
// El mismo proveedor alimenta la validación del navegador (data-val-number),
// por lo que el mensaje queda en español tanto antes como después de enviar.
builder.Services.AddControllersWithViews(opciones =>
{
    var mensajes = opciones.ModelBindingMessageProvider;

    mensajes.SetValueMustBeANumberAccessor(campo => $"{campo} tiene que ser un número");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(() => "Tiene que ser un número");

    mensajes.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"\"{valor}\" no es un valor válido para {campo}");
    mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"\"{valor}\" no es un valor válido");

    mensajes.SetUnknownValueIsInvalidAccessor(campo => $"El valor de {campo} no es válido");
    mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor no es válido");

    mensajes.SetValueIsInvalidAccessor(valor => $"{valor} no es un valor válido");

    mensajes.SetValueMustNotBeNullAccessor(_ => "Completá este campo");
    mensajes.SetMissingBindRequiredValueAccessor(campo => $"Completá {campo}");
    mensajes.SetMissingKeyOrValueAccessor(() => "Completá este campo");
    mensajes.SetMissingRequestBodyRequiredValueAccessor(() => "No llegaron los datos del formulario");
});

// El [Required] que MVC agrega solo a los campos numéricos y de fecha no tiene
// dónde declarar su mensaje, así que lo completa esta clase. Ver su comentario.
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, MensajesDeValidacion>();

// Sesión en memoria: guarda el usuario logueado y el carrito de la venta/compra en curso.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(8);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name = "KioPlus.Session";
});
builder.Services.AddHttpContextAccessor();

// Cliente HTTP apuntando a la API de Clases-KioPlus
var urlApi = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<ApiClient>(http =>
{
    http.BaseAddress = new Uri(urlApi);
    // Si la base de datos no responde, la API tarda unos pocos segundos en
    // devolver el 503. Esperar más solo deja la pantalla cargando de gusto.
    http.Timeout = TimeSpan.FromSeconds(15);
});

// Un servicio por recurso de la API, en espejo con la capa Logica del back
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ILoteService, LoteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<ICuentaCorrienteService, CuentaCorrienteService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<ICajaService, CajaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// La raíz entra al menú; el filtro [Autenticado] manda al login si no hay sesión.
// El action por defecto tiene que ser Index para que "/Ventas" o "/Usuarios" resuelvan.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
