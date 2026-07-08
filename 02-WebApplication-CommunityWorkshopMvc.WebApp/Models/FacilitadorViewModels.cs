using System;
using System.ComponentModel.DataAnnotations;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Models;

public record ListarFacilitadorViewModel(
    string Id,
    string Nome,
    string Email,
    string Especialidade
);

public record CadastrarFacilitadorViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Nome\" deve conter entre 2 e 50 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Email\" deve ser preenchido.")]
    [EmailAddress(ErrorMessage = "O campo \"Email\" deve conter um endereço de e-mail válido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Email\" deve conter entre 2 e 50 caracteres.")]
    string Email,

    [Required(ErrorMessage = "O campo \"Especialidade\" deve ser preenchido.")]
    [StringLength(500, ErrorMessage = "O campo \"Especialidade\" deve conter no máximo 500 caracteres.")]
    string Especialidade
);

public record EditarFacilitadorViewModel(
    string Id,

    [Required(ErrorMessage = "O campo \"Nome\" deve ser preenchido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Nome\" deve conter entre 2 e 50 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O campo \"Email\" deve ser preenchido.")]
    [EmailAddress(ErrorMessage = "O campo \"Email\" deve conter um endereço de e-mail válido.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O campo \"Email\" deve conter entre 2 e 50 caracteres.")]
    string Email,

    [Required(ErrorMessage = "O campo \"Especialidade\" deve ser preenchido.")]
    [StringLength(500, ErrorMessage = "O campo \"Especialidade\" deve conter no máximo 500 caracteres.")]
    string Especialidade
);

public record ExcluirFacilitadorViewModel(
    string Id,
    string Nome,
    string Email,
    string Especialidade
);