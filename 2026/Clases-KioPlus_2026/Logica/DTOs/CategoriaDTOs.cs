using System.ComponentModel.DataAnnotations;

namespace Clases_KioPlus.Logica.DTOs;

public record CategoriaDto(int IdCategoria, string Nombre, string Descripcion);

public record CategoriaCreateDto(
    [property: Required] string Nombre,
    [property: Required] string Descripcion);
