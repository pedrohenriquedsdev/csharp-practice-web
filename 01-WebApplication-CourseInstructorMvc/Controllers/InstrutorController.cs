using _01_WebApplication_CourseInstructorMvc.Compartilhado;
using _01_WebApplication_CourseInstructorMvc.Compartilhado.Arquivos;
using _01_WebApplication_CourseInstructorMvc.Models;
using _01_WebApplication_CourseInstructorMvc.ModuloInstrutor;
using Microsoft.AspNetCore.Mvc;

namespace _01_WebApplication_CourseInstructorMvc.Controllers
{
    public class InstrutorController : Controller
    {
        // 
        private readonly IRepositorio<Instrutor> repositorioInstrutor;

        public InstrutorController()
        {
            // Aqui estamos criando manualmente a estrutura de persistência da aplicação.
            //
            // O ContextoJson representa o "banco de dados em arquivo" da aplicação.
            // Ele contém as listas que serão salvas e carregadas do arquivo dados.json,
            // como, por exemplo, a lista de Instrutores.
            //
            // Como este projeto ainda não está usando Injeção de Dependência,
            // o próprio Controller precisa criar o ContextoJson manualmente.
            ContextoJson contexto = new ContextoJson();

            // Após criar o contexto, carregamos os dados já salvos no arquivo JSON.
            // Se o arquivo dados.json existir, os registros serão lidos e colocados
            // novamente dentro das listas do contexto, como contexto.Instrutores.
            contexto.Carregar();

            // Aqui criamos o repositório de Instrutores e entregamos o contexto para ele.
            //
            // O RepositorioInstrutorEmArquivo recebe esse contexto no construtor
            // e repassa para a classe base RepositorioBaseEmArquivo.
            //
            // Dentro da base, o método CarregarRegistros() é chamado.
            // No caso do repositório de Instrutores, esse método retorna contexto.Instrutores.
            //
            // Com isso, o repositório passa a manipular a lista de instrutores
            // que está dentro do ContextoJson. Quando cadastrar, editar ou excluir,
            // ele altera essa lista e depois chama contexto.Salvar() para persistir no JSON.
            repositorioInstrutor = new RepositorioInstrutorEmArquivo(contexto);
        }

        [HttpGet]
        public ActionResult Listar()
        {
            List<Instrutor> instrutores = repositorioInstrutor.SelecionarTodos();

            List<ListarInstrutoresViewModel> listarVms = new List<ListarInstrutoresViewModel>();

            foreach (Instrutor i in instrutores)
            {
                ListarInstrutoresViewModel viewModel = new ListarInstrutoresViewModel(
                    i.Id,
                    i.Nome,
                    i.Email,
                    i.Telefone
                );

                listarVms.Add(viewModel);
            }

            return View(listarVms);
        }

        // CADASTRAR
        [HttpGet]
        public ActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Cadastrar(CadastrarInstrutorViewModel cadastrarVm)
        {
            Instrutor novoInstrutor = new Instrutor(
            cadastrarVm.Nome,
            cadastrarVm.Email,
            cadastrarVm.Telefone
        );

            repositorioInstrutor.Cadastrar(novoInstrutor);

            return RedirectToAction(nameof(Listar));
        }

        // EDITAR
        [HttpGet]
        public ActionResult Editar(string id)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            EditarInstrutorViewModel editarVm = new EditarInstrutorViewModel(
                id,
                instrutor.Nome,
                instrutor.Email,
                instrutor.Telefone
            );

            return View(editarVm);
        }

        [HttpPost]
        public ActionResult Editar(EditarInstrutorViewModel editarVm)
        {
            Instrutor instrutorAtualizado = new Instrutor(
                editarVm.Nome,
                editarVm.Email,
                editarVm.Telefone
            );

            repositorioInstrutor.Editar(editarVm.Id, instrutorAtualizado);

            return RedirectToAction(nameof(Listar));
        }

        // EXCLUSÃO
        [HttpGet]
        public ActionResult Excluir(string id)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            ExcluirInstrutorViewModel excluirVm = new ExcluirInstrutorViewModel(
                id,
                instrutor.Nome,
                instrutor.Email,
                instrutor.Telefone
            );

            return View(excluirVm);
        }

        [HttpPost]
        [ActionName("Excluir")]
        public ActionResult ExcluirConfirmado(ExcluirInstrutorViewModel excluirVm)
        {
            Instrutor? instrutor = repositorioInstrutor.SelecionarPorId(excluirVm.Id);

            if (instrutor == null)
                return RedirectToAction(nameof(Listar));

            repositorioInstrutor.Excluir(instrutor);

            return RedirectToAction(nameof(Listar));
        }
    }
}
