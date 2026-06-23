# Guia prático — projeto paralelo ASP.NET Core MVC

Este material foi escrito a partir da análise do repositório **Gestão de Equipamentos Web**. A ideia não é decorar uma sequência de arquivos: é conseguir prever o caminho que os dados percorrem e saber por que cada peça existe.

## 0. Diagnóstico do projeto analisado

### Qual versão é a mais completa?

A base recomendada é a branch **`v5`**, no commit `f584077` (20/05/2026). Ela é a versão funcionalmente mais completa e a que está selecionada neste diretório. A conclusão não foi tomada pelo número da versão: foi feita pela comparação do histórico e do conteúdo das branches.

| Marco | Ganho principal |
|---|---|
| `v1` | Home e CRUD de fabricantes |
| `v2` | Layout e estilo |
| `v3` | ViewModels e CRUD de equipamentos |
| `v4` | CRUD de chamados, DataAnnotations, ModelState e Tag Helpers |
| `v5` | status de chamado, filtro, delegate `Predicate<T>` e lambdas |
| `refatoração-organizacao` | branch posterior (22/05), com reorganização experimental, mas sem as adições da v5 |

A branch `refatoração-organizacao` é cronologicamente posterior, mas saiu do commit `47655c1`, antes das cinco alterações exclusivas da v5. Ela reorganiza pastas e contém uma tentativa de configurar DI/log, porém o `Program.cs` não chama a configuração, os controllers ainda constroem repositórios com `new`, e a alteração não inclui o filtro nem a edição de status. Por isso ela não substitui a v5 como referência de aplicação completa. É uma boa fonte para um **desafio de refatoração**, não a base do estudo.

O projeto v5 foi compilado com `dotnet build`: sucesso, sem avisos nem erros. Há uma alteração local não relacionada em `ModuloChamado/IRepositorioChamado.cs`; ela foi preservada e não foi usada como evidência da análise.

### O que a v5 entrega, de ponta a ponta

- Cadastro, listagem, edição e exclusão de `Fabricante`, `Equipamento` e `Chamado`.
- Relações `Fabricante → Equipamento → Chamado` e selects dependentes dessas relações.
- Validação de entrada com ViewModels, DataAnnotations, `ModelState` e uma validação extra de ID selecionado.
- Views Razor, layout compartilhado, navegação e Tag Helpers.
- Persistência em um único arquivo JSON no diretório LocalAppData do usuário, com preservação de referências circulares.
- Abstração de repositório genérico e uma especialização para chamados.
- Atualização de status de chamado e filtro “todos / em aberto / concluídos”.
- `Predicate<T>` e expressões lambda no filtro.

### Lacunas reais (e por que são ótimos exercícios)

Não trate “conceito citado” como “conceito já aplicado”. Na v5:

- **DI de aplicação não está implementada.** `AddControllersWithViews()` registra a infraestrutura MVC, mas os controllers têm construtores sem parâmetros e usam `new ContextoJson()`/`new Repositorio...()`. A abstração `IRepositorio<T>` existe, mas não é injetada pelo container.
- **LINQ não é usado.** Listas são percorridas com `foreach`; o filtro é manual.
- **Método anônimo (`delegate (...) { ... }`) não aparece.** Há delegate do tipo `Predicate<T>` e há lambdas, que são uma forma mais moderna e concisa de fornecer o comportamento.
- A validação de domínio `Validar()` existe em `EntidadeBase<T>` e nas entidades, mas os controllers não a chamam; a validação efetiva da tela vem das DataAnnotations dos ViewModels.
- Não há banco de dados, migrations, autenticação, filtros MVC, testes, paginação nem tratamento de integridade referencial ao excluir pai. Por exemplo, excluir um fabricante que ainda possui equipamentos pode deixar objetos relacionados persistidos de forma inconsistente.

Essas lacunas não diminuem o valor do projeto: elas definem exatamente o próximo degrau do projeto paralelo.

---

## 1. A arquitetura: uma história de uma requisição

MVC separa responsabilidades para que uma mudança na tela não obrigue a mexer na persistência, e uma troca de arquivo JSON por banco não obrigue a reescrever cada view.

```text
Navegador
  │ GET /Chamado/Listar?status=em-aberto
  ▼
Middleware: UseStaticFiles → UseRouting
  ▼
Rota convencional: {controller=Home}/{action=Index}/{id?}
  ▼
ChamadoController.Listar(status)
  │    cria ContextoJson e repositórios (na v5)
  ▼
IRepositorioChamado / RepositorioChamadoEmArquivo
  ▼
RepositorioBaseEmArquivo<Chamado> → List<Chamado> no ContextoJson
  ▼
Controller mapeia Entidade → ListarChamadoViewModel
  ▼
Views/Chamado/Listar.cshtml + Views/Shared/_Layout.cshtml
  ▼
HTML de volta ao navegador
```

No envio válido de um formulário, a rota é parecida, mas tem um ponto crucial: o navegador envia valores simples, não uma entidade pronta.

```text
POST /Equipamento/Cadastrar
formulário HTML
  → model binding cria CadastrarEquipamentoViewModel
  → DataAnnotations alimentam ModelState
  → controller procura o Fabricante pelo FabricanteId
  → valida ModelState e a relação
  → cria Equipamento (entidade de domínio)
  → repositório adiciona na lista e ContextoJson.Salvar()
  → RedirectToAction(Listar) [novo GET; padrão PRG]
```

### Papel de cada área da v5

| Área | Responsabilidade | Exemplos |
|---|---|---|
| `Controllers` | Orquestra HTTP, decide view/redirect, mapeia dados | `EquipamentoController` |
| `Models` | Contratos próprios de cada tela e validação de entrada | `CadastrarEquipamentoViewModel` |
| `Modulo...` | Regras e dados do negócio por assunto | `Fabricante`, `Chamado` |
| `Compartilhado` | Código reutilizável | `EntidadeBase<T>`, `IRepositorio<T>` |
| `Compartilhado/Arquivos` | Infraestrutura de persistência | `ContextoJson`, repositório base |
| `Views` | HTML Razor que apresenta o ViewModel | `Views/Chamado/Listar.cshtml` |
| `wwwroot` | Arquivos estáticos servidos diretamente | `css/styles.css` |
| `Program.cs` | Composição do aplicativo e pipeline HTTP | MVC, middlewares e rota |

O projeto atual separa razoavelmente interface, domínio e persistência, mas ainda mistura a composição de objetos nos controllers. No paralelo, evolua para uma organização por módulo e DI — sem transformar o estudo em uma catedral de pastas antes da hora.

---

## 2. Mapa de aprendizado — o que dominar e como reconhecer

Leia esta ordem como uma trilha. Cada item ganha sentido porque resolve uma necessidade criada pelo anterior.

### 1. Roteamento, controllers e actions

**Por quê existe:** o servidor precisa transformar uma URL e um verbo HTTP em uma operação C# previsível. Controller é o ponto de entrada da aplicação; action é o caso de interação HTTP.

**No projeto:** `app.MapDefaultControllerRoute()` habilita a rota convencional. `ChamadoController.Listar(string? status)` atende `GET /Chamado/Listar?status=...`; `Cadastrar` aparece duas vezes, diferenciada por `[HttpGet]` e `[HttpPost]`.

**Identifique:** classes que herdam de `Controller`, métodos que retornam `ActionResult`, atributos `[HttpGet]`, `[HttpPost]` e `[ActionName]`.

**Domine:** inferir que `/Equipamento/Editar/abc` combina controller, action e `id`; saber quando retornar `View(model)`, `RedirectToAction(...)` e quando redirecionar se o ID não existe.

### 2. Razor Views e navegação

**Por quê existe:** a action não deveria montar uma string gigante de HTML. A view recebe dados e declara a apresentação.

**No projeto:** cada action procura por convenção `Views/{Controller}/{Action}.cshtml`. `_Layout.cshtml` concentra navegação, `<head>` e `@RenderBody()`; cada view define `ViewBag.Titulo` e `Layout = "_Layout"`.

**Identifique:** `@model`, `@foreach`, `@if`, `@Model`, `@ViewBag`, arquivos `.cshtml` e `_Layout.cshtml`.

**Domine:** a view deve exibir e coletar dados, não salvar JSON nem decidir regra de negócio; links do layout devem navegar via rota, não via URL montada na mão.

### 3. Entidade de domínio

**Por quê existe:** representa algo que faz sentido no negócio independentemente da tela: `Fabricante`, `Equipamento`, `Chamado`.

**No projeto:** entidades herdam `EntidadeBase<T>`, recebem ID aleatório hexadecimal, têm construtores e implementam `AtualizarDados`. `Chamado` calcula `TempoDecorrido` e possui `EstaConcluido`.

**Identifique:** classes em `ModuloFabricante`, `ModuloEquipamento` e `ModuloChamado`, propriedades de negócio e métodos que não conhecem HTML.

**Domine:** uma entidade não é automaticamente um bom modelo de formulário. Ela pode conter navegações, propriedades calculadas e regras que não devem ser enviadas pelo cliente.

### 4. ViewModels e mapeamento

**Por quê existe:** cada tela precisa somente dos dados que usa e precisa de regras de entrada próprias. Isso evita expor a entidade inteira ao formulário e deixa explícita a intenção da tela.

**No projeto:** há ViewModels distintos para listar, cadastrar, editar e excluir cada módulo. A listagem de equipamento leva `Fabricante` como texto; cadastro leva `FabricanteId` como texto. O controller faz os dois mapeamentos manualmente com construtores `new ...ViewModel(...)` e `new Equipamento(...)`.

**Domine:** explicar os dois sentidos: Entidade → VM para mostrar; VM → Entidade depois de validar. No paralelo, mantenha o mapeamento explícito primeiro; só avalie AutoMapper quando você enxergar repetição real.

### 5. Model Binding, DataAnnotations e ModelState

**Por quê existe:** HTTP envia texto. O model binder converte campos por nome para o ViewModel; as DataAnnotations declaram validações; `ModelState` guarda valores tentados e erros dessa requisição.

**No projeto:** `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]` e `[DataType(DataType.Date)]` estão em `Models/*ViewModels.cs`. Os POSTs testam `if (!ModelState.IsValid) return View(vm);`. Equipamento e chamado acrescentam erro com `ModelState.AddModelError` caso um ID não vazio não exista.

**Domine:** sempre repopular dropdowns antes de devolver a view inválida, pois `ViewBag` não sobrevive ao POST; nunca confiar só no `required`/`min` HTML, pois o cliente pode ser burlado.

### 6. Tag Helpers

**Por quê existe:** conectam elementos HTML às convenções MVC e reduzem divergências entre propriedade, nome do campo, valor atual, rota e mensagens.

**No projeto:** `_ViewImports.cshtml` registra `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`. As views usam `asp-for`, `asp-validation-for`, `asp-action`, `asp-controller`, `asp-route-id`, `asp-route-status`, `asp-items` e `asp-format`.

**Domine:** `asp-for="Nome"` gera `name`, `id` e valor coerentes para o binder; `asp-validation-for` mostra o erro da propriedade; `asp-route-id` cria parâmetro de rota/query sem concatenação frágil de URL.

### 7. CRUD e padrão PRG

**Por quê existe:** operações mutáveis precisam ser previsíveis e não devem repetir ao atualizar a página. PRG significa Post → Redirect → Get.

**No projeto:** em cada módulo: `Listar` (read), GET/POST `Cadastrar` (create), GET/POST `Editar` (update), GET/POST `Excluir` (delete). Depois de salvar/excluir, a action faz `RedirectToAction(nameof(Listar))`.

**Domine:** GET mostra formulário/confirmação e não altera dados; POST valida e altera dados; o redirect impede reenvio acidental de formulário pelo refresh.

### 8. Repositórios, interfaces e persistência

**Por quê existe:** controller não deveria saber como JSON é salvo. A interface descreve a capacidade; a implementação trata o armazenamento.

**No projeto:** `IRepositorio<T>` declara CRUD e `Filtrar`; `RepositorioBaseEmArquivo<T>` compartilha o algoritmo; os três repositórios concretos apenas escolhem a lista certa. `ContextoJson` carrega/salva `dados.json` em `%LocalAppData%/GestaoDeEquipamentosWeb`.

**Domine:** interface é contrato, não “uma classe vazia”; classe genérica `T` reutiliza algoritmo com diferentes entidades; serialização com `ReferenceHandler.Preserve` é necessária porque fabricante/equipamento/chamado formam referências de objetos que podem se repetir ou ciclar.

### 9. Relações, cadastros dependentes e SelectListItem

**Por quê existe:** um equipamento pertence a um fabricante; um chamado pertence a um equipamento. A tela deve permitir escolher um objeto existente sem aceitar um objeto inteiro vindo do browser.

**No projeto:** `Equipamento.Fabricante` e `Chamado.Equipamento` são relações de navegação. A action carrega pai(s), converte-os em `List<SelectListItem>` no `ViewBag`, e a view cria `<select asp-for="FabricanteId" asp-items="...">`. No POST, o controller busca o pai por ID antes de criar o filho.

**Domine:** primeiro cadastre fabricante; depois equipamento; depois chamado. O ID é a fronteira entre HTML e objeto. Valide a existência do pai no servidor.

### 10. Delegates, lambdas e filtro

**Por quê existe:** comportamento variável pode ser recebido como dado. O repositório sabe percorrer uma lista; quem chama define o critério.

**No projeto:** `IRepositorio<T>.Filtrar(Predicate<T> filtro)` recebe um delegate que devolve `bool`. `ChamadoController` passa `chamado => !chamado.EstaConcluido` ou `chamado => chamado.EstaConcluido`. O método percorre `registros` e mantém os itens aprovados.

**Domine:** ler `Predicate<Chamado>` como “função que recebe Chamado e devolve bool”. Lambda não é LINQ; ela pode ser passada a qualquer API compatível. Não há método anônimo explícito na v5; a forma equivalente para praticar é `delegate(Chamado c) { return !c.EstaConcluido; }`.

### 11. DI — próximo passo obrigatório

**Por quê existe:** o controller deve pedir o que necessita, não escolher concretamente como criar. Isso reduz acoplamento e permite trocar JSON, banco e fakes de teste.

**Na v5:** ainda não aplicada nos controllers. O ponto a refatorar é substituir `new ContextoJson()` e `new Repositorio...()` por dependências no construtor, registradas em `Program.cs`, por exemplo `AddScoped<IRepositorio<Oficina>, RepositorioOficinaEmArquivo>()`.

**Domine:** ciclo de vida `Scoped` (uma instância por requisição), construtor recebendo interface e composição centralizada. A DI não elimina interfaces; ela é justamente quem conecta uma interface à implementação.

### 12. LINQ — próximo passo, não uma lacuna a esconder

**Por quê existe:** descreve consultas e transformações de coleções de forma declarativa.

**Na v5:** não aparece. O `foreach` manual de `Filtrar` pode virar `registros.Where(filtro).ToList()` porque `Predicate<T>` pode ser adaptado, ou melhor, a assinatura pode passar a `Func<T, bool>` e usar LINQ.

**Domine:** `Where`, `Select`, `Any`, `OrderBy`, `Count`; saber quando um `foreach` claro é preferível a uma consulta comprimida e ilegível.

---

## 3. Proposta do projeto paralelo: Gestão de Oficinas Criativas Comunitárias

### Por que este tema

Um centro comunitário oferece oficinas de fotografia, cerâmica, teatro e jardinagem. O sistema organiza quem facilita cada oficina, quem participa e o estado de cada inscrição. É um contexto diferente do projeto original, mas preserva relações reais, formulários dependentes e um estado que muda — o terreno ideal para reaprender o MVC sem apenas trocar nomes.

### Entidades e relações

```text
Facilitador 1 ───── * Oficina 1 ───── * Inscricao * ───── 1 Participante
                       │                  │
                       │                  └─ status: ativa/cancelada, data de inscrição
                       └─ título, carga horária, vagas, valor de contribuição
```

| Entidade | Campos essenciais | Papel no treino |
|---|---|---|
| `Facilitador` | Id, Nome, Email, Especialidade | cadastro pai; CRUD simples |
| `Oficina` | Id, Titulo, CargaHoraria, Vagas, ValorContribuicao, Facilitador | filho de facilitador; dropdown |
| `Participante` | Id, Nome, Email, Telefone | outro CRUD simples e pai de inscrição |
| `Inscricao` | Id, Oficina, Participante, DataInscricao, EstaCancelada | duas relações, cálculo de dias, status e filtro |

Para espelhar exatamente a progressão da v5, trate uma oficina como uma turma. Depois, como desafio, crie `EdicaoOficina` para separar o tema da oficina de suas turmas em datas diferentes.

### Funcionalidades e telas

1. Início com links para todos os módulos.
2. Facilitadores: listar, cadastrar, editar, confirmar exclusão.
3. Participantes: o mesmo CRUD, implementado depois sem copiar sem entender.
4. Oficinas: CRUD; ao cadastrar/editar, selecionar facilitador.
5. Inscrições: CRUD; selecionar oficina e participante, registrar uma observação opcional e cancelar/reativar a inscrição.
6. Listagem de inscrições com filtros Todas, Ativas e Canceladas.
7. Regra: não criar inscrição ativa repetida para o mesmo participante na mesma oficina; não ultrapassar o número de vagas. Use `Any` e `Count` como evolução LINQ.
8. Regra: não excluir facilitador se houver oficinas; não excluir oficina se houver inscrições. Comece exibindo erro amigável no `ModelState`/mensagem e só depois decida se quer cascata.

### ViewModels mínimos

Crie, para cada módulo, `ListarXViewModel`, `CadastrarXViewModel`, `EditarXViewModel`, `ExcluirXViewModel`. Para `Inscricao`, cadastro/edição devem levar `OficinaId` e `ParticipanteId`, nunca objetos `Oficina` e `Participante` inteiros. Para tela mais rica, um `InscricaoFormularioViewModel` pode agrupar o formulário e as listas dos selects, eliminando `ViewBag` — uma evolução recomendada após você entender a versão original.

---

## 4. Roadmap de implementação

Faça uma etapa por vez, com uma pequena demonstração manual no fim. Só avance quando conseguir explicar o fluxo em voz alta.

### Etapa 0 — Preparação e rota

**Objetivo:** criar solução MVC, `Program.cs`, layout, página inicial e links de navegação. **Pratica:** pipeline, `AddControllersWithViews`, `UseStaticFiles`, `UseRouting`, rota padrão, Razor e layout. **Resultado esperado:** `/` abre Home e os links geram URLs pelo Tag Helper.

### Etapa 1 — Domínio e contratos

**Objetivo:** criar `EntidadeBase<T>`, `Facilitador`, `Oficina`, `Participante`, `Inscricao` e seus métodos `AtualizarDados`. **Pratica:** POO, construtores, propriedades, composição e relações. **Resultado esperado:** o código representa o negócio sem depender de controller/view.

### Etapa 2 — Persistência JSON e repositórios

**Objetivo:** criar `ContextoJson`, `IRepositorio<T>`, `RepositorioBaseEmArquivo<T>` e repositórios concretos. **Pratica:** interfaces, genéricos, herança, serialização e persistência. **Resultado esperado:** cadastrar, editar e excluir em uma lista salva no JSON.

### Etapa 3 — CRUD vertical de Facilitador

**Objetivo:** entregar uma fatia completa simples antes de multiplicar módulos. **Pratica:** controller, actions GET/POST, ViewModels, views Razor, mapeamento, DataAnnotations e PRG. **Resultado esperado:** Facilitador funciona de ponta a ponta, incluindo erros de formulário.

### Etapa 4 — CRUD de Participante sem muleta

**Objetivo:** repetir o padrão conscientemente. **Pratica:** mesmas peças com menos consulta ao código anterior. **Resultado esperado:** você consegue dizer por que existem duas actions `Cadastrar`.

### Etapa 5 — Oficina e primeiro cadastro dependente

**Objetivo:** cadastrar oficina selecionando facilitador. **Pratica:** `SelectListItem`, `asp-items`, `FacilitadorId`, `ModelState.AddModelError`, repopulação do select em erro. **Resultado esperado:** ao submeter sem facilitador ou com ID inválido, a tela retorna com opções e mensagem; com ID válido, a oficina referencia o facilitador.

### Etapa 6 — Inscrição com duas dependências

**Objetivo:** selecionar oficina e participante e controlar cancelamento. **Pratica:** duas consultas por ID, checkbox, propriedade calculada e mapping. **Resultado esperado:** listagem mostra oficina, participante, data, dias desde a inscrição e status.

### Etapa 7 — Filtro, delegate, método anônimo, lambda e LINQ

**Objetivo:** filtrar inscrições ativas/canceladas em uma única listagem. **Pratica:** `Predicate<T>` ou `Func<T,bool>`, método anônimo, lambda, query string, `asp-route-status`, `Where`, `OrderByDescending`. **Resultado esperado:** URLs diferentes mantêm filtro e estado visual do link ativo.

### Etapa 8 — DI e organização em camadas/módulos

**Objetivo:** retirar `new Repositorio...` dos controllers. **Pratica:** container, `AddScoped`, injeção por construtor, interfaces e composição. **Resultado esperado:** controller recebe `IRepositorio<Oficina>` e `IRepositorio<Facilitador>`; trocar JSON por fake não muda a action.

### Etapa 9 — Integridade, validações de domínio e refatoração

**Objetivo:** tornar o comportamento seguro e eliminar repetição justificadamente. **Pratica:** validação de negócio, `Any`, ViewModel de formulário, métodos de mapeamento e tratamento de exclusão dependente. **Resultado esperado:** nenhuma relação fica órfã e validações têm uma casa clara.

---

## 5. Fluxos de conhecimento: siga os fios, não apenas os arquivos

### A. Cadastrar uma Oficina

1. Usuário abre `GET /Oficina/Cadastrar`.
2. `OficinaController.Cadastrar()` consulta facilitadores e monta itens `{ Text = Nome, Value = Id }`.
3. View recebe `CadastrarOficinaViewModel` e lista de opções; `asp-for="FacilitadorId"` dá ao `<select>` o nome que o binder reconhecerá.
4. Usuário envia o formulário. O browser envia pares como `Titulo=...&FacilitadorId=a1b2c3`.
5. Model binder instancia `CadastrarOficinaViewModel` e converte valores; DataAnnotations registram erros em `ModelState`.
6. Controller procura `FacilitadorId` no repositório. Mesmo que a validação `[Required]` passe, ID inventado é rejeitado por `ModelState.AddModelError`.
7. Se inválido, a action recarrega facilitadores e retorna a mesma view com o VM. Os Tag Helpers preservam valores e mostram `asp-validation-for`.
8. Se válido, controller converte VM em `Oficina`, usando o objeto `Facilitador` encontrado, e chama `Cadastrar`.
9. Repositório adiciona à lista e contexto serializa JSON.
10. `RedirectToAction(nameof(Listar))` dispara GET de listagem; a view recebe VMs de listagem, não entidades cruas.

```text
Form → Binder → CadastrarOficinaVM → DataAnnotations/ModelState
     → buscar Facilitador por ID → Oficina → Repositório → JSON
     → Redirect → GET Listar → ListarOficinaVM → Razor
```

### B. Editar e manter o valor selecionado

O GET busca a oficina existente e cria `EditarOficinaViewModel`, incluindo `FacilitadorId` atual. A action também carrega os facilitadores. Na view, `asp-for="FacilitadorId"` compara o valor do VM com `SelectListItem.Value` e marca a opção correspondente. O POST repete validação e busca do pai: dados de formulário nunca são confiáveis só porque vieram da própria tela.

### C. Excluir com confirmação

GET `Excluir(id)` não exclui: procura o registro, cria VM de leitura e mostra a decisão. POST `[ActionName("Excluir")] ExcluirConfirmado(vm)` usa o mesmo nome público de action para casar com o formulário e então exclui. No paralelo, antes de excluir Facilitador/Oficina, consulte dependentes e bloqueie com uma explicação; isso é integridade de negócio, não detalhe visual.

### D. Filtrar inscrições

`GET /Inscricao/Listar?status=ativas` chega como string no parâmetro da action. Normalize e escolha o critério. A opção inicial, pedagógica, pode ser:

```csharp
Predicate<Inscricao> ativa = delegate (Inscricao i)
{
    return !i.EstaCancelada;
};

var resultado = repositorio.Filtrar(ativa);
```

Depois simplifique para a lambda equivalente e, em seguida, LINQ:

```csharp
var resultado = inscricoes
    .Where(i => !i.EstaCancelada)
    .OrderByDescending(i => i.DataInscricao)
    .ToList();
```

O ganho não é escrever menos caracteres; é perceber que o algoritmo de filtragem e a regra de seleção são responsabilidades separadas.

### E. DI corrigindo o acoplamento

Na v5, o controller constrói a implementação concreta. No paralelo, o fluxo deve ser:

```text
Program.cs registra IRepositorio<Oficina> → RepositorioOficinaEmArquivo
             │
Container MVC cria OficinaController(IRepositorio<Oficina>, IRepositorio<Facilitador>)
             │
Action usa contrato, sem saber se os dados vêm de JSON, SQL ou memória
```

Registre também um único `ContextoJson` com ciclo de vida coerente. Decida e documente se o contexto é carregado na criação, por requisição ou por operação. O objetivo é que todos os repositórios de uma mesma requisição compartilhem o mesmo contexto — relação objeto e persistência precisam enxergar os mesmos dados.

---

## 6. Boas práticas: o que preservar e o que melhorar

**Boas práticas já vistas:** ViewModels distintos por caso de uso; ações GET/POST explícitas; PRG; validação de relação no servidor; ID oculto na edição/exclusão; `nameof(Listar)` em redirects; layout e imports compartilhados; interface e repositório genérico; propriedades calculadas no domínio; Tag Helpers em vez de URLs montadas na mão.

**Melhorias para aplicar deliberadamente:**

- Injetar dependências e remover composição dos controllers.
- Não retornar a lista interna mutável diretamente em `SelecionarTodos`; preferir cópia/leitura quando o projeto crescer.
- Centralizar o mapeamento em métodos privados/extension methods quando a repetição estiver comprovada.
- Mover validações de invariantes realmente de domínio para entidade/serviço e chamá-las de fato. DataAnnotations continuam sendo a validação de entrada da UI.
- Criar um ViewModel de formulário que transporte selects tipados em vez de `ViewBag`.
- Implementar regra de exclusão para dependentes e mensagens claras ao usuário.
- Adicionar `[ValidateAntiForgeryToken]` nos POSTs e `@Html.AntiForgeryToken()` (ou o mecanismo correspondente do form Tag Helper) como prática de proteção CSRF.
- Preferir um banco/ORM em etapa posterior, mantendo a interface para não quebrar os controllers.

---

## 7. Desafios extras

1. **Filtro MVC reutilizável:** crie um action filter que preencha título ou registre tempo de execução. Explique pipeline de filtros versus middleware.
2. **Paginação:** aceite `pagina` e `tamanho`, use `Count`, `Skip`, `Take`, links com `asp-route-pagina` e preserve filtro atual.
3. **Validação personalizada:** implemente atributo que valide carga horária ou impeça uma data de inscrição futura; compare com validação no domínio.
4. **Busca e ordenação LINQ:** título da oficina/nome do participante, `Where`, `OrderBy`, `ThenBy`; mantenha query string nos links.
5. **Integridade de vagas:** bloqueie inscrição acima do limite e inscrição ativa duplicada; teste manualmente duas tentativas.
6. **Troca de infraestrutura:** escreva `RepositorioOficinaEmMemoria` e registre-o via DI sem tocar no controller.
7. **Logger injetado:** registre `ILogger<InscricaoController>` e escreva logs de criação/cancelamento, sem registrar dados sensíveis.
8. **Tratamento de erros:** crie página de erro e valide IDs inexistentes de forma consistente.
9. **Testes:** teste repositório e regras de domínio; depois teste controller com repositório fake.
10. **Edições de oficina:** introduza `EdicaoOficina` e evolua de “oficina como turma” para “tema com várias turmas”; observe como uma nova relação muda telas e regras.

---

## 8. Checklist final — só marque quando conseguir explicar

- [ ] Sei explicar URL → rota → controller → action → view.
- [ ] Sei diferenciar GET que mostra formulário de POST que altera estado.
- [ ] Sei explicar por que uso PRG após POST válido.
- [ ] Consigo criar uma entidade sem colocar HTML, `ViewBag` ou `ModelState` nela.
- [ ] Sei justificar cada ViewModel e mapear entidade ↔ ViewModel sem “mágica”.
- [ ] Sei dizer o que model binding faz e o que ele não valida sozinho.
- [ ] Sei dizer como DataAnnotations chegam ao `ModelState` e por que retorno a mesma view quando inválido.
- [ ] Sei criar Tag Helpers para campo, erro, link, rota e select.
- [ ] Sei repopular dropdown ao retornar formulário inválido.
- [ ] Sei modelar e validar `Facilitador → Oficina → Inscricao → Participante`.
- [ ] Sei explicar interface, classe genérica e herança no repositório.
- [ ] Sei localizar o arquivo JSON, explicar serialização e o limite dessa persistência.
- [ ] Sei explicar `Predicate<T>`, escrever uma lambda e reescrevê-la como método anônimo.
- [ ] Sei usar `Where`, `Select`, `Any`, ordenação e paginação básica com LINQ.
- [ ] Sei explicar por que DI melhora o controller e registrar uma interface com ciclo de vida `Scoped`.
- [ ] Sei impedir exclusão que viola relações e explicar a política escolhida.
- [ ] Consigo adicionar um CRUD novo repetindo o fluxo inteiro, não apenas copiando arquivos.
- [ ] Consigo apontar onde uma regra pertence: view, ViewModel, controller, domínio, repositório ou infraestrutura.

Quando todos os itens fizerem sentido, você não terá apenas “feito um sistema de oficinas”. Terá construído o mapa mental de uma aplicação MVC pequena, porém inteira — exatamente o tipo de base que torna os próximos projetos menos misteriosos.
