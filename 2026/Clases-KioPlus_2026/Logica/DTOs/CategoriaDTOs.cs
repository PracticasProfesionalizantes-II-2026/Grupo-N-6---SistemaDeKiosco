using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record CategoriaDto(int IdCategoria, string Nombre, string? Descripcion);

public record CategoriaCreateDto(
    [property: Required(ErrorMessage = "Ingresá el nombre")] string Nombre,
    string? Descripcion);
