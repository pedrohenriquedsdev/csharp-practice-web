using System;

namespace _01_WebApplication_CourseInstructorMvc.Models;

public record ListarInstrutoresViewModel(
    string Id,
    string Nome,
    string Email,
    string Telefone
);

public record CadastrarInstrutorViewModel(
    string Nome,
    string Email,
    string Telefone
);

public record EditarInstrutorViewModel(
    string Id,
    string Nome,
    string Email,
    string Telefone
);

public record ExcluirInstrutorViewModel(
    string Id,
    string Nome,
    string Email,
    string Telefone
);
