using System.ComponentModel.DataAnnotations;

namespace _01_WebApplication_CourseInstructorMvc.Models;

public record ListarInstrutoresViewModel(
    string Id,
    string Nome,
    string Email,
    string Telefone
);

public record CadastrarInstrutorViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Nome\" deve conter entre 2 e 50 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Email\" deve ser preenchido.")]
    [EmailAddress(ErrorMessage = "O campo \"Email\" deve conter um email válido.")]
    string Email,

    [Required(ErrorMessage = "O campo \"Telefone\" deve ser preenchido.")]
    string Telefone
);

public record EditarInstrutorViewModel(
    string Id,

    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Nome\" deve conter entre 2 e 50 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Email\" deve ser preenchido.")]
    [EmailAddress(ErrorMessage = "O campo \"Email\" deve conter um email válido.")]
    string Email,

    [Required(ErrorMessage = "O campo \"Telefone\" deve ser preenchido.")]
    string Telefone
);

public record ExcluirInstrutorViewModel(
    string Id,
    string Nome,
    string Email,
    string Telefone
);
