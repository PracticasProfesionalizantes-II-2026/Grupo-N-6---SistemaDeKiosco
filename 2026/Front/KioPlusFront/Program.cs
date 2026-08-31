using KioPlusFront.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
    http.Timeout = TimeSpan.FromSeconds(30);
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
