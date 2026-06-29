namespace Clases_KioPlus.Logica.DTOs;

public record CategoriaDto(int IdCategoria, string Nombre, string Descripcion);

public record CategoriaCreateDto(string Nombre, string Descripcion);
