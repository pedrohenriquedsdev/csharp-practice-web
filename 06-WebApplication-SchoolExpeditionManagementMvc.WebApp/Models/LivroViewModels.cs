using System.ComponentModel.DataAnnotations;

public record ListarLivroViewModel(
    string Id,
    string Titulo,
    int NumeroEdicao,
    DateTime DataPublicacao,
    string Estante,
    StatusLivro Status
);

public record CadastrarLivroViewModel(
    [Required(ErrorMessage = "O campo \"Título\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O campo \"Título\" deve possuir entre 2 e 100 caracteres.")]
    string Titulo,

    [Range(1, int.MaxValue, ErrorMessage = "O número da edição deve ser maior que zero.")]
    int NumeroEdicao,

    [Required(ErrorMessage = "O campo \"Data de publicação\" deve ser preenchido.")]
    DateTime DataPublicacao,

    [Required(ErrorMessage = "O campo \"Estante\" deve ser preenchido.")]
    string EstanteId
);

public record EditarLivroViewModel(
    [Required]
    string Id,

    [Required(ErrorMessage = "O campo \"Título\" deve ser preenchido.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O campo \"Título\" deve possuir entre 2 e 100 caracteres.")]
    string Titulo,

    [Range(1, int.MaxValue, ErrorMessage = "O número da edição deve ser maior que zero.")]
    int NumeroEdicao,

    [Required(ErrorMessage = "O campo \"Data de publicação\" deve ser preenchido.")]
    DateTime DataPublicacao,

    [Required(ErrorMessage = "O campo \"Estante\" deve ser preenchido.")]
    string EstanteId
);

public record ExcluirLivroViewModel(
    string Id,
    string Titulo,
    int NumeroEdicao,
    DateTime DataPublicacao,
    string Estante,
    StatusLivro Status
);