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

        [HttpGet]
        public ActionResult Cadastrar()
        {
            CadastrarFacilitadorViewModel cadastrarVm = new CadastrarFacilitadorViewModel(
                string.Empty,
                string.Empty,
                string.Empty
            );

            return View(cadastrarVm);
        }

        [HttpPost]
        public ActionResult Cadastrar(CadastrarFacilitadorViewModel cadastrarVm)
        {
            if (!ModelState.IsValid)
                return View(cadastrarVm);

            Facilitador novoFacilitador = new Facilitador(
                cadastrarVm.Nome,
                cadastrarVm.Email,
                cadastrarVm.Especialidade
            );

            repositorioFacilitador.Cadastrar(novoFacilitador);

            return RedirectToAction(nameof(Listar));
        }

        [HttpGet]
        public ActionResult Editar(string id)
        {
            Facilitador? facilitador = repositorioFacilitador.SelecionarPorId(id);

            if (facilitador == null)
                return RedirectToAction(nameof(Listar));

            EditarFacilitadorViewModel editarVm = new EditarFacilitadorViewModel(
                id,
                facilitador.Nome,
                facilitador.Email,
                facilitador.Especialidade
            );

            return View(editarVm);
        }

        [HttpPost]
        public ActionResult Editar(EditarFacilitadorViewModel editarVm)
        {
            if (!ModelState.IsValid)
                return View(editarVm);

            Facilitador facilitadorAtualizado = new Facilitador(
                editarVm.Nome,
                editarVm.Email,
                editarVm.Especialidade
            );

            repositorioFacilitador.Editar(editarVm.Id, facilitadorAtualizado);

            return RedirectToAction(nameof(Listar));
        }
    }
}
