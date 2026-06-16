namespace _01_WebApplication_CourseInstructorMvc.Models;

public record ListarCursosViewModel(
    string Id,
    string Nome,
    decimal Valor,
    DateTime DataInicio,
    string Instrutor
);

public record CadastrarCursoViewModel(
    string Nome,
    decimal Valor,
    DateTime DataInicio,
    string InstrutorId
);

public record EditarCursoViewModel(
    string Id,
    string Nome,
    decimal Valor,
    DateTime DataInicio,
    string InstrutorId
);

public record ExcluirCursoViewModel(
    string Id,
    string Nome,
    decimal Valor,
    DateTime DataInicio,
    string Instrutor
);