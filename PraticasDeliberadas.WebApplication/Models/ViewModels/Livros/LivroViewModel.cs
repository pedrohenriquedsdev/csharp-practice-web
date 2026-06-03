using System;

namespace PraticasDeliberadas.WebApplication.Models;

public record ListarLivroViewModel(
    int Id,
    string Titulo,
    string Autor,
    bool EstaDisponivel
);

public record DetalhesLivroViewModel(
    int Id,
    string Titulo,
    string Autor,
    string Genero,
    bool EstaDisponivel
);

public record CadastrarLivroViewModel(
    string Titulo,
    string Autor,
    string Genero
);

public record EditarLivroViewModel(
    int Id,
    string Titulo,
    string Autor,
    string Genero,
    bool EstaDisponivel
);

public record ExcluirLivroViewModel(
    int Id,
    string Titulo,
    string Autor
);

