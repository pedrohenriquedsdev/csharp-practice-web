using System.ComponentModel.DataAnnotations;

public record ListarEstanteViewModel(
    string Id,
    string Codigo,
    string Cor,
    int DiasEmprestimo
);

public record CadastrarEstanteViewModel(
    [Required(ErrorMessage = "O campo \"Código\" deve ser preenchido.")]
    [StringLength(50,
    ErrorMessage = "O campo \"Código\" deve possuir no máximo 50 caracteres.")]
    string Codigo,

    [Required(ErrorMessage = "O campo \"Cor\" deve ser preenchido.")]
    string Cor,

    [Range(1, int.MaxValue,
        ErrorMessage = "Os dias de empréstimo devem ser maiores que zero.")]
    int DiasEmprestimo = 7
);

public record EditarEstanteViewModel(
    [Required]
    string Id,

    [Required(ErrorMessage = "O campo \"Código\" deve ser preenchido.")]
    [StringLength(50,
    ErrorMessage = "O campo \"Código\" deve possuir no máximo 50 caracteres.")]
    string Codigo,

    [Required(ErrorMessage = "O campo \"Cor\" deve ser preenchido.")]
    string Cor,

    [Range(1, int.MaxValue,
        ErrorMessage = "Os dias de empréstimo devem ser maiores que zero.")]
    int DiasEmprestimo = 7
);

public record ExcluirEstanteViewModel(
    string Id,
    string Codigo,
    string Cor,
    int DiasEmprestimo
);
