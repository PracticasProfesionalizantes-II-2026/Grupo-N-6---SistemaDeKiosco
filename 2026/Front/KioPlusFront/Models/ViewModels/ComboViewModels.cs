namespace KioPlusFront.Models.ViewModels;

// Una opción del buscador con lista desplegable.
// Etiqueta es lo que queda escrito al elegirla; Detalle es el renglón completo
// que se muestra en la lista y sobre el que se busca.
public class ComboOpcion
{
    public string Valor { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public bool Atenuada { get; set; }
}

// Configuración del buscador. Reemplaza al par "input de búsqueda + combo":
// se escribe adentro y la lista se filtra, y con el campo vacío se ven todas
// las opciones igual que en un combo común.
public class ComboViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public string Placeholder { get; set; } = "Escribí para buscar";
    public string? ValorSeleccionado { get; set; }
    public string TextoVacio { get; set; } = "No hay coincidencias";
    public IReadOnlyList<ComboOpcion> Opciones { get; set; } = Array.Empty<ComboOpcion>();
}

// Armado de los combos que se repiten en varias pantallas
// Los filtros por rango de fechas arrancan en histórico: sin fechas límite.
// Al elegir "Entre fechas" se habilitan los dos calendarios, y si se los vacía
// el filtro vuelve solo al histórico en lugar de quedar sin rango válido.
public interface IFiltroPorPeriodo
{
    string Periodo { get; set; }
    DateTime? FechaDesde { get; set; }
    DateTime? FechaHasta { get; set; }
}

public static class Periodos
{
    public const string Historico = "historico";
    public const string Rango = "rango";

    // Las fechas solo cuentan cuando se eligió un rango
    public static DateTime? DesdeEfectiva(IFiltroPorPeriodo filtro) =>
        filtro.Periodo == Rango ? filtro.FechaDesde : null;

    public static DateTime? HastaEfectiva(IFiltroPorPeriodo filtro) =>
        filtro.Periodo == Rango ? filtro.FechaHasta : null;
}

public static class Combos
{
    private const string Moneda = "N2";

    public static ComboViewModel Productos(
        IReadOnlyList<Api.ProductoDto> productos,
        int? seleccionado = null,
        string etiqueta = "Producto",
        bool mostrarPrecio = true)
    {
        return new ComboViewModel
        {
            Id = "buscadorProducto",
            Nombre = "idProducto",
            Etiqueta = etiqueta,
            Placeholder = "Escribí el nombre o la marca",
            ValorSeleccionado = seleccionado?.ToString(),
            TextoVacio = "Ningún producto coincide",
            Opciones = productos.Select(p => new ComboOpcion
            {
                Valor = p.IdProducto.ToString(),
                Etiqueta = p.Nombre + " - " + p.Marca,
                Detalle = mostrarPrecio
                    ? p.Nombre + " - " + p.Marca + " — $" + p.PrecioVenta.ToString(Moneda) +
                      " (stock " + p.StockDisponible + ")"
                    : p.Nombre + " - " + p.Marca + " (stock " + p.StockDisponible + ")",
                Atenuada = p.StockDisponible == 0
            }).ToList()
        };
    }

    public static ComboViewModel Clientes(
        IReadOnlyList<Api.CuentaCorrienteClienteDto> clientes,
        int seleccionado,
        int idConsumidorFinal)
    {
        return new ComboViewModel
        {
            Id = "buscadorCliente",
            Nombre = "idCliente",
            Etiqueta = "Cliente",
            Placeholder = "Escribí el nombre, el apellido o el DNI",
            ValorSeleccionado = seleccionado.ToString(),
            TextoVacio = "Ningún cliente coincide",
            Opciones = clientes.Select(c => new ComboOpcion
            {
                Valor = c.IdCuentaCorrienteCliente.ToString(),
                Etiqueta = c.IdCuentaCorrienteCliente == idConsumidorFinal
                    ? "Consumidor final"
                    : c.Dni + " - " + c.Nombre + " " + c.Apellido,
                Detalle = c.IdCuentaCorrienteCliente == idConsumidorFinal
                    ? "Consumidor final"
                    : c.Dni + " - " + c.Nombre + " " + c.Apellido
            }).ToList()
        };
    }
}
