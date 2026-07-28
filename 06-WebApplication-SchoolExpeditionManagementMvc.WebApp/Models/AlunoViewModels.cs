using System.ComponentModel.DataAnnotations;

public record ListarAlunoViewModel(
    string Id,
    string Nome,
    string NomeResponsavel,
    string Telefone
);

public record CadastrarAlunoViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve possuir entre 3 e 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Nome do responsável\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome do responsável\" deve possuir entre 3 e 100 caracteres.")]
    string NomeResponsavel,

    [Required(ErrorMessage = "O campo \"Telefone\" deve ser preenchido.")]
    [RegularExpression(@"^\d{10,11}$",
        ErrorMessage = "O campo \"Telefone\" deve possuir entre 10 e 11 dígitos.")]
    string Telefone
);

public record EditarAlunoViewModel(
    [Required]
    string Id,

    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve possuir entre 3 e 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Nome do responsável\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome do responsável\" deve possuir entre 3 e 100 caracteres.")]
    string NomeResponsavel,

    [Required(ErrorMessage = "O campo \"Telefone\" deve ser preenchido.")]
    [RegularExpression(@"^\d{10,11}$",
        ErrorMessage = "O campo \"Telefone\" deve possuir entre 10 e 11 dígitos.")]
    string Telefone
);

public record ExcluirAlunoViewModel(
    string Id,
    string Nome,
    string NomeResponsavel,
    string Telefone
);