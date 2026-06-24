# Guia prático — projeto paralelo ASP.NET Core MVC

Este material foi escrito a partir da análise do projeto **ListaDeComprasWeb.WebApp**. O objetivo não é decorar arquivos nem apenas trocar nomes: é reconstruir o mesmo raciocínio em outro contexto de negócio, entendendo como os conceitos trabalham juntos dentro de uma aplicação MVC real.

O projeto paralelo proposto será uma **Gestão de Expedições Escolares**. A ideia é controlar materiais necessários para saídas pedagógicas: trilhas, visitas a museus, feiras científicas, acampamentos e atividades externas. O sistema terá categorias de materiais, materiais, expedições e itens necessários para cada expedição.

---

## 0. Diagnóstico do projeto analisado

### Qual versão é a mais completa?

A versão mais completa encontrada é a **`v4`**, no commit `57761d4`. Neste repositório, `main`, `origin/main`, `origin/HEAD` e `origin/v4` apontam para o mesmo commit. Portanto, diferente de outros projetos com branches divergentes, aqui a versão mais atual e completa também é o branch principal.

| Marco | Ganho principal |
|---|---|
| `v0` | projeto MVC inicial |
| `v1` | módulo de categorias e início de produtos |
| `v2` | módulo de produtos |
| `v3` | módulo de listas de compras |
| `v4` | módulo de itens da lista, exclusão de lista, “adicionar outro” e uso de `Filtrar()` |

O projeto foi compilado com `dotnet build ListaDeComprasWeb.slnx --no-restore`: sucesso, sem avisos e sem erros.

### O que a v4 entrega

- Organização modular em `Modulos/ModuloCategoria`, `ModuloProduto`, `ModuloListaCompras` e `ModuloItemLista`.
- Camadas por módulo: `Dominio`, `Infra`, `Aplicacao` e `Apresentacao`.
- CRUD completo de categorias, produtos e listas.
- Cadastro e exclusão de itens dentro de uma lista.
- Status de lista: `Aberta` e `Concluida`.
- Cálculo de total de itens e total estimado.
- Opção “Adicionar outro” ao cadastrar item.
- Relacionamentos: `Categoria → Produto`, `ListaCompras → ItemLista`, `Produto → ItemLista`.
- IDs com `Guid`, gerados com `Guid.CreateVersion7()`.
- Persistência em JSON usando `ContextoJson`.
- Enums salvos como texto com `JsonStringEnumConverter`.
- Repositório genérico com `IRepositorio<T>` e `RepositorioBaseEmArquivo<T>`.
- Filtro com `Predicate<T>` por meio do método `Filtrar`.
- Serviços de aplicação em todos os módulos.
- Resultados de operações com `FluentResults`.
- Mensagens pós-redirecionamento com `TempData`.
- Métodos de extensão para DI, `ModelState` e `TempData`.
- Mapeamento com AutoMapper.
- Views Razor com Bootstrap e Tag Helpers.

---

## 1. Visão geral da arquitetura

### Ideia central

O projeto usa MVC com uma organização modular. Em vez de colocar todos os controllers em uma pasta global e todos os modelos em outra, ele agrupa os arquivos pelo assunto do negócio.

```text
Modulos
  ├── ModuloCategoria
  │   ├── Dominio
  │   ├── Infra
  │   ├── Aplicacao
  │   └── Apresentacao
  ├── ModuloProduto
  ├── ModuloListaCompras
  └── ModuloItemLista
```

Essa organização ajuda a pensar em fatias completas da aplicação. Se você vai mexer em produto, procura dentro de `ModuloProduto`. Se vai mexer em item da lista, procura dentro de `ModuloItemLista`.

### Fluxo de uma requisição

Exemplo: cadastrar um item em uma lista.

```text
Navegador
  │ POST /ItemLista/Cadastrar
  ▼
Rota MVC
  │ encontra ItemListaController.Cadastrar(...)
  ▼
Controller
  │ recebe CadastrarItemListaViewModel
  │ valida ModelState
  │ mapeia ViewModel → DTO com AutoMapper
  ▼
Serviço de Aplicação
  │ busca ListaCompras pelo Guid
  │ busca Produto pelo Guid
  │ valida produto duplicado na lista
  │ cria ItemLista
  ▼
Repositório
  │ adiciona o item na lista em memória
  ▼
ContextoJson
  │ salva o dados.json
  ▼
Controller
  │ se AdicionarOutro = true, volta ao cadastro
  │ senão, redireciona para itens da lista
  ▼
View Razor
  │ mostra total de itens, total estimado e cards
```

### Papel de cada camada

| Camada | Por que existe | Exemplos |
|---|---|---|
| Apresentação | lida com HTTP, forms, views e navegação | Controllers, ViewModels, Views, Profiles |
| Aplicação | executa casos de uso e regras de negócio | `ServicoCategoria`, `ServicoProduto`, `ServicoListaCompras`, `ServicoItemLista` |
| Domínio | representa conceitos do negócio | `Categoria`, `Produto`, `ListaCompras`, `ItemLista` |
| Infraestrutura | salva e recupera dados | `ContextoJson`, repositórios em arquivo |
| Compartilhado | concentra código reutilizável | `EntidadeBase`, `IRepositorio`, extensões, DI |

O ponto mais importante: o Controller não salva diretamente nem concentra toda a regra. Ele recebe a requisição, valida a entrada inicial, chama o serviço e decide qual resposta HTTP devolver.

---

## 2. Mapa de aprendizado

### 1. Organização modular no MVC

**O que é:** organizar o projeto por módulos de negócio.

**Para que serve:** evitar que o projeto vire um amontoado de controllers, modelos e views sem contexto.

**Onde aparece:** `Modulos/ModuloCategoria`, `ModuloProduto`, `ModuloListaCompras`, `ModuloItemLista`.

**Como identificar:** cada módulo possui arquivos de domínio, infraestrutura, aplicação e apresentação.

**O que dominar:** criar um módulo novo mantendo o mesmo padrão de pastas e responsabilidades.

### 2. Razor View Location personalizada

**O que é:** configuração que ensina o MVC onde procurar views.

**Para que serve:** permitir que as views fiquem dentro dos módulos, e não apenas em `/Views`.

**Onde aparece:** `AddPresentationConfig`.

```text
/Modulos/Modulo{Controller}/Apresentacao/Views/{Action}.cshtml
/Compartilhado/Apresentacao/Views/{Action}.cshtml
```

**O que dominar:** entender que `ProdutoController.Listar()` procura a view em `Modulos/ModuloProduto/Apresentacao/Views/Listar.cshtml`.

### 3. Controllers e actions

**O que é:** Controller é a entrada HTTP; action é o método executado.

**Para que serve:** transformar URL, verbo HTTP e dados do formulário em uma operação da aplicação.

**Onde aparece:** `CategoriaController`, `ProdutoController`, `ListaComprasController`, `ItemListaController`.

**O que dominar:** GET mostra tela; POST altera estado.

```text
GET Cadastrar  → monta o formulário
POST Cadastrar → valida, chama serviço e redireciona
```

### 4. ViewModels

**O que é:** modelos específicos para a tela.

**Para que serve:** não expor entidades diretamente ao formulário.

**Onde aparece:** `CategoriaViewModels`, `ProdutoViewModels`, `ListaComprasViewModels`, `ItemListaViewModels`.

**Exemplo importante:** `CadastrarItemListaViewModel` tem `ListaComprasId`, `ProdutoId`, `Quantidade`, `AdicionarOutro` e `Produtos`. Ele não recebe uma entidade `ListaCompras` nem uma entidade `Produto`.

**O que dominar:** criar ViewModels diferentes para listar, cadastrar, editar e excluir.

### 5. Model Binding, DataAnnotations e ModelState

**O que é:** Model Binding monta o ViewModel com dados do form; DataAnnotations validam campos; ModelState guarda erros.

**Para que serve:** validar entrada antes de chamar regras de negócio.

**Onde aparece:** `[Required]`, `[StringLength]`, `[Range]` e `if (!ModelState.IsValid)`.

**Detalhe novo:** IDs são `Guid`. Quando um select inicia com `Guid.Empty`, o serviço ainda precisa validar se aquele ID corresponde a uma entidade real.

**O que dominar:** validação de campo não substitui validação de relacionamento.

### 6. Bootstrap

**O que é:** biblioteca visual usada para criar navbar, cards, botões, tabelas e alertas.

**Para que serve:** entregar uma interface organizada sem escrever muito CSS.

**Onde aparece:** `_Layout.cshtml`, cards de listagem, alerts de erro, botões arredondados e formulários.

**O que dominar:** usar classes como `container`, `navbar`, `card`, `alert`, `btn`, `form-control`, `form-select`, `badge`.

### 7. Tag Helpers

**O que é:** atributos Razor que conectam HTML ao MVC.

**Para que serve:** gerar rotas, nomes de campos e mensagens de validação sem strings frágeis.

**Onde aparece:** `asp-action`, `asp-controller`, `asp-route-id`, `asp-route-listaId`, `asp-for`, `asp-validation-for`.

**O que dominar:** entender que `asp-route-listaId="@Model.Lista.Id"` monta a rota com o parâmetro certo para o Controller receber.

### 8. Injeção de Dependência

**O que é:** mecanismo que cria e entrega dependências para controllers e serviços.

**Para que serve:** o Controller não precisa fazer `new ServicoProduto(...)`; o container faz isso.

**Onde aparece:** `Program.cs`, `AddInfraRepositories`, `AddApplicationServices`, `AddPresentationConfig`.

```text
Program.cs
  → registra repositórios
  → registra serviços
  → registra MVC + AutoMapper
```

**O que dominar:** entender `AddScoped`: uma instância por requisição.

### 9. Camada de Aplicação: Serviços

**O que é:** camada que executa casos de uso.

**Para que serve:** deixar regras importantes fora dos controllers.

**Onde aparece:** todos os módulos possuem serviço: categoria, produto, lista e item.

**Exemplos de regras nos serviços:**

- categoria com nome duplicado;
- categoria não pode ser excluída se possui produtos;
- produto precisa de categoria válida;
- produto não pode repetir nome na mesma categoria;
- produto com itens vinculados não pode ser excluído;
- item não pode repetir produto na mesma lista;
- total da lista é calculado pelos itens.

**O que dominar:** serviço recebe DTO, valida regra, cria entidade, chama repositório e retorna `Result`.

### 10. FluentResults

**O que é:** biblioteca para representar sucesso ou falha de forma explícita.

**Para que serve:** serviços retornam `Result.Ok()` ou `Result.Fail(...)` sem usar exceção para erro esperado de negócio.

**Onde aparece:** todos os serviços e extensões de `ModelState`/`TempData`.

```text
Result.Ok()
Result.Fail("Produto não encontrado.")
Result.Ok(detalhesDto)
```

**O que dominar:** usar `Result` quando a operação não devolve dados e `Result<T>` quando devolve.

### 11. Métodos de extensão

**O que é:** métodos estáticos que parecem métodos naturais de outro tipo.

**Para que serve:** organizar código repetitivo e deixar chamadas mais expressivas.

**Onde aparece:**

- `services.AddInfraRepositories()`
- `services.AddApplicationServices()`
- `services.AddPresentationConfig()`
- `ModelState.AddModelError(resultado)`
- `TempData.AddErrorMessage(resultado)`

**O que dominar:** reconhecer o `this` no primeiro parâmetro do método.

### 12. AutoMapper

**O que é:** biblioteca para mapear objetos de um tipo para outro.

**Para que serve:** reduzir código repetitivo entre ViewModels e DTOs.

**Onde aparece:** `CategoriaProfile`, `ProdutoProfile`, `ListaComprasProfile`, `ItemListaProfile`.

**O que dominar:** AutoMapper mapeia dados, mas não decide regra de negócio.

```text
ViewModel → DTO → Serviço
DTO → ViewModel → View
```

### 13. Repositórios, Predicate e Filtrar

**O que é:** repositório encapsula acesso aos dados. `Predicate<T>` representa uma função que recebe `T` e devolve `bool`.

**Para que serve:** reutilizar o mesmo algoritmo de filtragem para várias entidades.

**Onde aparece:** `IRepositorio<T>.Filtrar(Predicate<T> filtro)` e `RepositorioBaseEmArquivo<T>.Filtrar`.

**Exemplo:** selecionar itens de uma lista.

```csharp
repositorioItemLista.Filtrar(i => i.ListaCompras.Id == listaId);
```

**O que dominar:** a lambda define o critério; o repositório sabe percorrer a lista.

### 14. LINQ

**O que é:** conjunto de métodos para consultar e transformar coleções.

**Para que serve:** filtrar, projetar, somar e verificar existência.

**Onde aparece:** `Select`, `Any`, `Sum`, `ToList`.

**Exemplos importantes:**

```csharp
produtos.Any(p => p.Categoria.Id == categoriaId)
itens.Sum(i => i.CalcularSubtotal())
categorias.Select(c => new OpcaoCategoriaDto(...))
```

**O que dominar:** `Any` responde “existe?”, `Select` transforma e `Sum` calcula total.

### 15. Guid e Guid v7

**O que é:** identificador único global.

**Para que serve:** gerar IDs sem depender de contador incremental.

**Onde aparece:** `EntidadeBase<T>`:

```csharp
public Guid Id { get; set; } = Guid.CreateVersion7();
```

**O que dominar:** o Controller pode receber `Guid id` pela rota, e o model binder converte o texto da URL para `Guid`.

### 16. Enums e JsonStringEnumConverter

**O que é:** enum representa um conjunto fechado de opções.

**Para que serve:** evitar strings soltas para status e unidade.

**Onde aparece:** `UnidadeMedida`, `StatusListaCompras` e `ContextoJson`.

**Detalhe importante:** `JsonStringEnumConverter` salva enums como texto no JSON, melhorando leitura e manutenção.

---

## 3. Projeto paralelo: Gestão de Expedições Escolares

### Por que este tema?

Imagine uma escola que organiza expedições pedagógicas: visita ao museu, trilha ecológica, feira de ciências, observação astronômica, viagem histórica. Cada expedição precisa de materiais: coletes, pranchetas, garrafas de água, lanternas, kits de primeiros socorros, crachás e equipamentos simples.

O sistema vai ajudar a planejar quais materiais são necessários, em qual quantidade e qual o custo estimado.

O paralelo com o projeto original fica assim:

```text
Lista de Compras              Gestão de Expedições
Categoria                     CategoriaMaterial
Produto                       Material
ListaCompras                  Expedicao
ItemLista                     ItemExpedicao
UnidadeMedida                 UnidadeMedida
StatusListaCompras            StatusExpedicao
```

### Entidades

| Entidade | Campos principais | Papel |
|---|---|---|
| `CategoriaMaterial` | Id, Nome, Cor | agrupa materiais por tipo |
| `Material` | Id, Nome, Categoria, UnidadeMedida, CustoEstimado | item que pode ser usado em expedições |
| `Expedicao` | Id, Nome, DataCriacao, Status | planejamento de uma saída escolar |
| `ItemExpedicao` | Id, Expedicao, Material, Quantidade | material necessário em uma expedição |

### Relacionamentos

```text
CategoriaMaterial 1 ──── * Material 1 ──── * ItemExpedicao * ──── 1 Expedicao
```

Ou, pensando pelo uso:

```text
Expedicao
  ├── ItemExpedicao: 10 Garrafas de Água
  ├── ItemExpedicao: 2 Kits de Primeiros Socorros
  └── ItemExpedicao: 30 Crachás
```

### Regras de negócio

#### CategoriaMaterial

- Nome obrigatório.
- Nome com no máximo 50 caracteres.
- Cor obrigatória.
- Não pode haver categorias com nome duplicado.
- Não pode excluir categoria com materiais vinculados.

#### Material

- Nome obrigatório.
- Nome entre 2 e 100 caracteres.
- Categoria obrigatória.
- Unidade de medida obrigatória.
- Custo estimado maior ou igual a zero.
- Não pode haver material com mesmo nome na mesma categoria.
- Não pode excluir material usado em itens de expedição.

#### Expedicao

- Nome obrigatório.
- Nome entre 3 e 100 caracteres.
- Data de criação automática.
- Status: `Aberta` ou `Concluida`.
- Total de itens calculado automaticamente.
- Custo total estimado calculado automaticamente.

#### ItemExpedicao

- Expedição obrigatória.
- Material obrigatório.
- Quantidade maior que zero.
- Não pode adicionar o mesmo material duas vezes na mesma expedição.
- Subtotal = quantidade × custo estimado do material.

### Telas

| Tela | O que pratica |
|---|---|
| Home/Index | layout, navegação e Bootstrap |
| CategoriaMaterial/Listar | cards, cor, TempData |
| CategoriaMaterial/Cadastrar | form simples e validação |
| Material/Cadastrar | select de categoria e enum de unidade |
| Material/Listar | relacionamento e preço estimado |
| Expedicao/Listar | status, total de itens e custo total |
| Expedicao/Concluir | POST sem formulário visual grande |
| ItemExpedicao/Listar | gerenciamento de itens de uma expedição |
| ItemExpedicao/Cadastrar | select de material, quantidade e “adicionar outro” |
| ItemExpedicao/Excluir | confirmação e retorno para a expedição |

---

## 4. Roadmap de implementação

### Etapa 1 — Criar o projeto e a estrutura modular

**Objetivo:** iniciar o projeto MVC e criar a organização base.

**Conceitos praticados:** template MVC, `Program.cs`, rota padrão, Razor, layout, Bootstrap e ViewLocation personalizada.

**Resultado esperado:** aplicação abre na Home e encontra views dentro dos módulos.

```text
Modulos
  ├── ModuloCategoriaMaterial
  ├── ModuloMaterial
  ├── ModuloExpedicao
  └── ModuloItemExpedicao
```

### Etapa 2 — Criar domínio e enums

**Objetivo:** criar `EntidadeBase<T>`, entidades e enums.

**Conceitos praticados:** domínio, `Guid`, enums, propriedades, construtores, validação de entidade.

**Resultado esperado:** classes representam o negócio sem depender de MVC.

### Etapa 3 — Criar ContextoJson e repositórios

**Objetivo:** persistir dados em arquivo.

**Conceitos praticados:** infraestrutura, `IRepositorio<T>`, `RepositorioBaseEmArquivo<T>`, JSON, `ReferenceHandler.Preserve`, `JsonStringEnumConverter`.

**Resultado esperado:** entidades são salvas e carregadas de `dados.json`.

### Etapa 4 — Configurar DI com métodos de extensão

**Objetivo:** registrar repositórios, serviços, MVC e AutoMapper.

**Conceitos praticados:** injeção de dependência, `AddScoped`, métodos de extensão e composição.

**Resultado esperado:** `Program.cs` fica limpo:

```text
builder.Services.AddInfraRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddPresentationConfig();
```

### Etapa 5 — CRUD de CategoriaMaterial

**Objetivo:** implementar o primeiro módulo completo.

**Conceitos praticados:** Controller, ViewModel, DTO, Serviço, FluentResults, AutoMapper, Bootstrap, TempData.

**Resultado esperado:** categorias podem ser cadastradas, editadas, excluídas e listadas.

### Etapa 6 — CRUD de Material

**Objetivo:** cadastrar materiais vinculados a categorias.

**Conceitos praticados:** relacionamento, dropdown, enum, `ValidateNever`, repopulação de select e validação de duplicidade.

**Resultado esperado:** material só é salvo com categoria válida.

### Etapa 7 — CRUD de Expedicao

**Objetivo:** criar planejamentos de expedição.

**Conceitos praticados:** status, data automática, edição, listagem com totais e ação POST para concluir.

**Resultado esperado:** expedições abertas podem ser concluídas.

### Etapa 8 — Itens da Expedição

**Objetivo:** adicionar materiais a uma expedição.

**Conceitos praticados:** relacionamento entre duas entidades, subtotal, total estimado, “adicionar outro”, retorno com `listaId` equivalente.

**Resultado esperado:** cada expedição mostra seus materiais, quantidades, subtotais e total.

### Etapa 9 — Regras de integridade

**Objetivo:** impedir exclusões problemáticas e duplicidades.

**Conceitos praticados:** `Any`, `Filtrar`, `Result.Fail`, `TempData`, `ModelState`.

**Resultado esperado:** o sistema não permite categoria com materiais, material em itens, nem item duplicado na mesma expedição.

### Etapa 10 — Revisão e refatoração

**Objetivo:** revisar responsabilidades.

**Conceitos praticados:** boas práticas, serviços limpos, controllers finos, AutoMapper Profiles e métodos privados.

**Resultado esperado:** você consegue explicar onde cada regra mora e por quê.

---

## 5. Fluxos de conhecimento

### Fluxo A — Cadastrar uma CategoriaMaterial

```text
Formulário
  → Model Binding
  → CadastrarCategoriaMaterialViewModel
  → DataAnnotations
  → ModelState
  → AutoMapper
  → CadastrarCategoriaMaterialDto
  → ServicoCategoriaMaterial
  → valida nome duplicado
  → cria CategoriaMaterial
  → repositório salva
  → RedirectToAction(Listar)
```

Se o nome estiver duplicado, o serviço retorna `Result.Fail` com metadata do campo. O Controller usa `ModelState.AddModelError(resultado)` e devolve a view.

### Fluxo B — Cadastrar um Material

```text
GET Material/Cadastrar
  → serviço busca categorias
  → controller monta ViewModel com Categorias
  → view renderiza <select>

POST Material/Cadastrar
  → binder preenche Nome, CategoriaId, UnidadeMedida, CustoEstimado
  → ModelState valida campos simples
  → serviço busca categoria pelo Guid
  → serviço valida duplicidade dentro da categoria
  → cria Material
  → salva
```

O navegador envia `CategoriaId`, não a categoria inteira. Por isso a camada de aplicação precisa buscar a categoria real antes de criar o material.

Se o POST voltar com erro, recarregue as categorias:

```text
return View(vm with { Categorias = SelecionarCategorias() });
```

### Fluxo C — Criar uma Expedição

```text
POST Expedicao/Cadastrar
  → CadastrarExpedicaoViewModel
  → CadastrarExpedicaoDto
  → ServicoExpedicao
  → nova Expedicao(nome, DateTime.Now)
  → Validar()
  → Repositório
  → JSON
```

A data de criação não vem do formulário. Ela nasce no servidor. Isso evita que o usuário altere um dado que pertence ao sistema.

### Fluxo D — Adicionar item na expedição

```text
Usuário abre os itens da expedição
  → /ItemExpedicao/Listar?expedicaoId=...
  → serviço busca detalhes da expedição
  → serviço filtra itens daquela expedição
  → view mostra total

Usuário adiciona item
  → seleciona Material
  → informa Quantidade
  → marca ou não "Adicionar outro"
  → serviço valida material e expedição
  → serviço impede material duplicado
  → cria ItemExpedicao
  → salva
```

Aqui está a parte mais rica do projeto. Ela junta:

- `Guid` vindo da rota;
- formulário com hidden input;
- dropdown de materiais;
- validação de quantidade;
- validação de relacionamento;
- validação de duplicidade;
- cálculo de subtotal;
- redirecionamento condicional.

### Fluxo E — “Adicionar outro”

No projeto original, `CadastrarItemListaViewModel` possui `AdicionarOutro`.

No paralelo, faça igual:

```text
se AdicionarOutro = true
  → RedirectToAction(Cadastrar, new { expedicaoId })
senão
  → RedirectToAction(Listar, new { expedicaoId })
```

Esse detalhe parece pequeno, mas ensina algo importante: o resultado de uma action pode depender de uma intenção do usuário, não apenas de sucesso ou falha.

### Fluxo F — Concluir expedição

Na listagem, uma expedição aberta mostra botão `Concluir`.

```text
POST Expedicao/Concluir/{id}
  → busca detalhes da expedição
  → cria EditarExpedicaoDto com Status = Concluida
  → serviço edita
  → redirect para listagem
```

É uma action pequena, mas útil para entender operações específicas que não são CRUD puro.

### Fluxo G — Calcular totais

O total da expedição não deve ser digitado. Ele deve ser calculado.

```text
Itens da expedição
  → cada ItemExpedicao calcula Subtotal
  → serviço soma os subtotais com Sum
  → DTO leva TotalItens e TotalEstimado
  → ViewModel exibe em card
```

O cálculo pertence ao domínio/serviço, não ao HTML.

---

## 6. Estrutura sugerida de arquivos

```text
GestaoExpedicoesEscolares.WebApp
  ├── Compartilhado
  │   ├── Aplicacao
  │   │   └── InjecaoDependencia.cs
  │   ├── Apresentacao
  │   │   ├── Extensions
  │   │   │   ├── ModelStateExtensions.cs
  │   │   │   └── TempDataExtensions.cs
  │   │   ├── Views
  │   │   │   ├── _Layout.cshtml
  │   │   │   └── Index.cshtml
  │   │   └── InjecaoDependencia.cs
  │   ├── Dominio
  │   │   ├── EntidadeBase.cs
  │   │   └── IRepositorio.cs
  │   └── Infra
  │       ├── Arquivos
  │       │   ├── ContextoJson.cs
  │       │   └── RepositorioBaseEmArquivo.cs
  │       └── InjecaoDependencia.cs
  ├── Modulos
  │   ├── ModuloCategoriaMaterial
  │   ├── ModuloMaterial
  │   ├── ModuloExpedicao
  │   └── ModuloItemExpedicao
  └── Program.cs
```

Mantenha consistência. O maior ganho dessa arquitetura não é “ter muitas pastas”; é saber imediatamente onde cada responsabilidade deve morar.

---

## 7. Boas práticas para aplicar

- Use entidade para representar negócio, não formulário.
- Use ViewModel para entrada e saída de tela.
- Use DTO entre Controller e Serviço.
- Use AutoMapper para mapeamentos mecânicos.
- Não coloque regra de negócio no Profile do AutoMapper.
- Valide campos simples com DataAnnotations.
- Valide regras dependentes do estado do sistema no Serviço.
- Use `ModelState` quando o erro pertence ao formulário atual.
- Use `TempData` quando o erro aparece depois de redirect.
- Use `Result`/`Result<T>` para sucesso e falha de serviço.
- Recarregue dropdowns ao retornar uma view inválida.
- Não confie em IDs enviados pelo navegador.
- Use `Any` para verificar existência.
- Use `Sum` para totais calculados.
- Use `Filtrar` quando quiser praticar `Predicate<T>`.
- Use `RedirectToAction` após POST válido.

---

## 8. Desafios extras

1. **Filtro por status da expedição:** abertas e concluídas.
2. **Filtro por categoria de material:** segurança, alimentação, primeiros socorros etc.
3. **Busca por nome de material:** usando `Contains`.
4. **Ordenação por custo estimado:** materiais mais caros primeiro.
5. **Paginação:** `Skip`, `Take` e preservação dos filtros.
6. **Duplicar expedição:** criar nova expedição copiando itens de uma anterior.
7. **Marcar material como indisponível:** impedir uso em novos itens.
8. **Exportar resumo:** gerar uma tela simples de impressão da expedição.
9. **Dashboard:** total de expedições abertas, concluídas e custo total planejado.
10. **Validação personalizada:** criar atributo para impedir custo negativo ou quantidade inválida.
11. **Trocar persistência:** criar repositório em memória para testes.
12. **Refinar enums:** exibir nomes amigáveis para unidades de medida.

---

## 9. Checklist final

Marque apenas quando conseguir explicar sozinho.

- [ ] Sei dizer por que a versão usada como referência é `v4/main`.
- [ ] Sei explicar a organização modular do projeto.
- [ ] Sei configurar ViewLocation para views dentro dos módulos.
- [ ] Sei explicar o caminho URL → Controller → Serviço → Repositório → JSON → View.
- [ ] Sei diferenciar GET e POST.
- [ ] Sei criar ViewModels específicos para listar, cadastrar, editar e excluir.
- [ ] Sei explicar Model Binding.
- [ ] Sei explicar DataAnnotations.
- [ ] Sei explicar `ModelState.IsValid`.
- [ ] Sei adicionar erro manual ao ModelState.
- [ ] Sei usar TempData depois de redirect.
- [ ] Sei explicar a função da camada de aplicação.
- [ ] Sei criar serviço retornando `Result` e `Result<T>`.
- [ ] Sei transformar erro de FluentResults em erro de formulário.
- [ ] Sei transformar erro de FluentResults em mensagem de listagem.
- [ ] Sei registrar dependências com `AddScoped`.
- [ ] Sei criar métodos de extensão para DI.
- [ ] Sei configurar AutoMapper com Profiles.
- [ ] Sei mapear ViewModel → DTO e DTO → ViewModel.
- [ ] Sei criar relacionamento `CategoriaMaterial → Material`.
- [ ] Sei criar relacionamento `Expedicao → ItemExpedicao → Material`.
- [ ] Sei usar `Guid` como ID na rota e no formulário.
- [ ] Sei explicar `Guid.CreateVersion7()`.
- [ ] Sei explicar enums e `JsonStringEnumConverter`.
- [ ] Sei usar `Predicate<T>` com `Filtrar`.
- [ ] Sei usar LINQ com `Any`, `Select`, `Sum` e `ToList`.
- [ ] Sei calcular subtotal e total sem colocar cálculo na View.
- [ ] Sei implementar “Adicionar outro” com redirecionamento condicional.
- [ ] Sei impedir duplicidade de material na mesma expedição.
- [ ] Sei impedir exclusões que quebram relacionamentos.
- [ ] Sei criar uma aplicação MVC pequena, modular e completa.

Quando essa checklist fizer sentido, você não terá apenas criado outro CRUD. Você terá entendido como a Lista de Compras organiza módulos, valida regras, calcula totais, usa serviços, persiste dados e conecta tudo em um fluxo MVC previsível.
