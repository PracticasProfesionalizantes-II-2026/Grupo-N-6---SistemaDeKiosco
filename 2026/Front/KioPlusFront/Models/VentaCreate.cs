namespace KioPlusFront.Models;

public class VentaCreate
{
    public DateTime FechaHora { get; set; }
    public int IdCuentaCorrienteCliente { get; set; }
    public FormaPago FormaDePago { get; set; }
    public DateTime FechaPago { get; set; }
}
