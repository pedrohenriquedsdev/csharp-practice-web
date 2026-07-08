using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.Compartilhado.Arquivos;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.Models;
using _02_WebApplication_CommunityWorkshopMvc.WebApp.ModuloFacilitador;
using Microsoft.AspNetCore.Mvc;

namespace _02_WebApplication_CommunityWorkshopMvc.WebApp.Controllers
{
    public class FacilitadorController : Controller
    {
        private readonly IRepositorio<Facilitador> repositorioFacilitador;

        public FacilitadorController()
        {
            ContextoJson contexto = new ContextoJson();
            contexto.Carregar();

            repositorioFacilitador =
                new RepositorioFacilitadorEmArquivo(contexto);
        }

        [HttpGet]
        public ActionResult Listar()
        {
            List<Facilitador> facilitadores = repositorioFacilitador.SelecionarTodos();

            List<ListarFacilitadorViewModel> listarVms = new List<ListarFacilitadorViewModel>();

            foreach (Facilitador f in facilitadores)
            {
                // mapear objeto por objeto para viewModels
                ListarFacilitadorViewModel viewModel = new ListarFacilitadorViewModel(
                    f.Id,
                    f.Nome,
                    f.Email,
                    f.Especialidade
                );

                listarVms.Add(viewModel);
            }

            return View(listarVms);
        }
    }
}
