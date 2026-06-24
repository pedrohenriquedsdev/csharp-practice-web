# Guia prático — projeto paralelo ASP.NET Core MVC

Este material foi escrito a partir da análise do projeto **ClubeDaLeituraWeb.WebApp**. A ideia não é copiar o sistema original com outros nomes: é reconstruir o mesmo raciocínio em outro contexto, entendendo por que cada camada existe e como uma requisição atravessa a aplicação.

O projeto paralelo proposto aqui será uma **Gestão de Figurinos Teatrais Comunitários**. Ele mantém o mesmo nível de complexidade do Clube da Leitura: cadastros simples, cadastros dependentes, retirada/devolução, status, regras de negócio, serviços, repositórios, TempData, FluentResults, métodos de extensão e AutoMapper.

---

## 0. Diagnóstico do projeto analisado

### Qual versão é a mais completa?

A versão mais completa encontrada é a branch **`origin/v8`**, no commit `3b0b54a`. A escolha não foi feita apenas pelo número da versão: foi feita comparando o histórico e o conteúdo das branches.

| Marco | Ganho principal |
|---|---|
| `v0`/`v1` | base MVC, Bootstrap, rota, Home, CRUD inicial de caixas |
| `v2` | organização de views/imports e Home em área compartilhada |
| `v3` | módulo de revistas com cadastro, edição e exclusão |
| `v4` | módulo de amigos |
| `v5` | módulo de empréstimos, status e devolução |
| `v6` | camada de aplicação no módulo de caixas e validações de negócio |
| `v7` | métodos de extensão, TempDataExtensions, ModelStateExtensions e AutoMapper |
| `v8` | refatoração de revistas para camada de aplicação e melhorias visuais/listagem |

O `main` local não é a versão mais completa funcionalmente: ele está em uma etapa anterior do histórico. A branch `origin/v8` contém todos os módulos do sistema e as refatorações mais recentes: organização modular, DI, camada de aplicação parcial, FluentResults, extensões e AutoMapper.

### O que a v8 entrega

- MVC organizado por módulos: `ModuloCaixa`, `ModuloRevista`, `ModuloAmigo`, `ModuloEmprestimo`.
- Camadas dentro dos módulos: `Dominio`, `Infra`, `Aplicacao` e `Apresentacao`.
- CRUD completo de caixas, revistas e amigos.
- Cadastro e devolução de empréstimos.
- Relacionamentos: `Caixa → Revista`, `Amigo → Emprestimo`, `Revista → Emprestimo`.
- Status de revista: disponível/emprestada.
- Status de empréstimo: aberto/concluído/atrasado.
- Persistência em JSON com `ContextoJson`.
- Repositório genérico com `IRepositorio<T>` e `RepositorioBaseEmArquivo<T>`.
- Injeção de dependência em `Program.cs` usando métodos de extensão.
- Camada de aplicação nos módulos `Caixa` e `Revista`.
- Retorno de operações com `FluentResults`.
- Mensagens entre redirecionamentos com `TempData`.
- Conversão de erros do serviço para `ModelState`.
- Mapeamento com AutoMapper nos módulos refatorados.
- Views Razor com Bootstrap e Tag Helpers.

### Uma observação importante

A arquitetura está em evolução. `Caixa` e `Revista` já usam Controller → Serviço → Repositório. `Amigo` e `Emprestimo` ainda usam Controller → Repositório diretamente. Isso é ótimo para estudo, porque mostra duas fases do mesmo projeto:

```text
Fase inicial:
Controller → Repositório → JSON

Fase refatorada:
Controller → Serviço de Aplicação → Repositório → JSON
```

No projeto paralelo, a recomendação é aplicar a arquitetura refatorada em todos os módulos, para consolidar o desenho mais atual.

---

## 1. Visão geral da arquitetura

### Por que organizar em módulos?

Em projetos pequenos, é comum começar com pastas globais: `Controllers`, `Models`, `Views`, `Repositories`. Isso funciona no início, mas conforme os módulos crescem, arquivos de assuntos diferentes ficam misturados.

O Clube da Leitura usa uma organização modular:

```text
ModuloCaixa
  ├── Dominio
  ├── Infra
  ├── Aplicacao
  └── Apresentacao

ModuloRevista
  ├── Dominio
  ├── Infra
  ├── Aplicacao
  └── Apresentacao
```

O ganho mental é simples: se você quer mexer em revista, quase tudo está dentro de `ModuloRevista`. O módulo vira uma pequena fatia vertical da aplicação.

### Caminho de uma requisição

Exemplo: cadastrar uma revista.

```text
Navegador
  │ POST /Revista/Cadastrar
  ▼
Rota MVC
  │ encontra RevistaController.Cadastrar(...)
  ▼
Controller
  │ recebe CadastrarRevistaViewModel via Model Binding
  │ valida ModelState
  │ chama ServicoRevista
  ▼
Serviço de Aplicação
  │ valida regras de negócio
  │ busca Caixa pelo id
  │ cria entidade Revista
  │ retorna Result.Ok ou Result.Fail
  ▼
Repositório
  │ adiciona/edita/exclui registros
  ▼
ContextoJson
  │ salva dados.json
  ▼
Controller
  │ em sucesso: RedirectToAction
  │ em erro: ModelState.AddModelError(...)
  ▼
Razor View
  │ exibe formulário, erros e campos com Tag Helpers
```

### Papel de cada camada

| Camada | Por que existe | Exemplos no projeto |
|---|---|---|
| Apresentação | Lida com HTTP, telas e entrada do usuário | Controllers, ViewModels, Views, AutoMapper Profiles |
| Aplicação | Coordena regras de uso do sistema | `ServicoCaixa`, `ServicoRevista`, DTOs |
| Domínio | Representa conceitos do negócio | `Caixa`, `Revista`, `Amigo`, `Emprestimo` |
| Infraestrutura | Salva e recupera dados | `ContextoJson`, repositórios em arquivo |
| Compartilhado | Código usado por vários módulos | `EntidadeBase`, `IRepositorio`, extensões, DI |

O Controller não deve concentrar todas as regras. Ele deve orquestrar a requisição. A regra mais importante deve ir para o serviço ou para o domínio, dependendo do tipo da regra.

---

## 2. Mapa de aprendizado

### 1. Organização Modular no MVC

**O que é:** separar o projeto por assunto de negócio, e não apenas por tipo técnico de arquivo.

**Para que serve:** manter perto arquivos que mudam juntos. Se uma alteração afeta revista, você procura no módulo de revista.

**Onde aparece:** `ModuloCaixa`, `ModuloRevista`, `ModuloAmigo`, `ModuloEmprestimo`.

**O que dominar:** criar um novo módulo com `Dominio`, `Infra`, `Aplicacao` e `Apresentacao`, sabendo o que colocar em cada pasta.

### 2. Controllers e Actions

**O que é:** Controller é a porta de entrada HTTP; Action é o método chamado pela rota.

**Para que serve:** transformar uma interação do navegador em uma operação do sistema.

**Onde aparece:** `CaixaController`, `RevistaController`, `AmigoController`, `EmprestimoController`.

**O que dominar:** diferenciar GET e POST. GET mostra tela; POST processa alteração.

```text
GET Cadastrar  → monta formulário
POST Cadastrar → valida, salva e redireciona
```

### 3. Razor Views e Tag Helpers

**O que é:** Razor é HTML com C#. Tag Helpers conectam HTML às rotas e aos modelos do MVC.

**Para que serve:** criar telas sem montar HTML manualmente no Controller.

**Onde aparece:** arquivos `.cshtml`, usando `asp-action`, `asp-controller`, `asp-route-id`, `asp-for` e `asp-validation-for`.

**O que dominar:** entender que `asp-for="Titulo"` gera `name`, `id`, valor atual e integração com validação.

### 4. Bootstrap

**O que é:** biblioteca CSS/JS para layout responsivo e componentes visuais.

**Para que serve:** dar aparência boa rapidamente, sem criar todo o CSS do zero.

**Onde aparece:** `_Layout.cshtml`, cards, tabelas, navbar, alerts, botões e formulários.

**O que dominar:** usar classes como `container`, `row`, `card`, `btn`, `alert`, `table`, `form-control`, `form-select`.

### 5. ViewModels

**O que é:** modelos específicos para as telas.

**Para que serve:** evitar expor a entidade inteira ao formulário e deixar claro o que cada tela precisa.

**Onde aparece:** `CaixaViewModels.cs`, `RevistaViewModels.cs`, `AmigoViewModels.cs`, `EmprestimoViewModels.cs`.

**O que dominar:** criar ViewModels diferentes para listar, cadastrar, editar e excluir.

### 6. DataAnnotations, Model Binding e ModelState

**O que é:** Model Binding monta o ViewModel a partir do formulário. DataAnnotations declaram validações. ModelState guarda erros e valores enviados.

**Para que serve:** validar a entrada do usuário antes de criar ou alterar entidades.

**Onde aparece:** `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]` e `if (!ModelState.IsValid)`.

**O que dominar:** quando retornar a mesma view com o ViewModel inválido e quando redirecionar.

### 7. Validações de regra de negócio

**O que é:** validações que dependem do estado do sistema, não apenas do formato do campo.

**Para que serve:** impedir situações inválidas, como etiqueta duplicada ou caixa excluída com revista vinculada.

**Onde aparece:** `ServicoCaixa` e `ServicoRevista`.

Exemplos:

- não permitir caixas com etiqueta duplicada;
- não excluir caixa com revistas;
- não permitir revista com mesmo título e edição;
- não cadastrar revista em caixa inexistente.

**O que dominar:** diferença entre validação de campo e validação de negócio.

```text
Campo obrigatório → DataAnnotation
Etiqueta duplicada → Serviço
Caixa inexistente → Serviço/Controller
```

### 8. Camada de Aplicação: Serviços

**O que é:** camada que coordena casos de uso.

**Para que serve:** tirar regras importantes do Controller e deixar o Controller mais fino.

**Onde aparece:** `ServicoCaixa` e `ServicoRevista`.

**O que dominar:** serviço recebe DTO, valida regras, chama repositórios e retorna sucesso/falha.

```text
Controller → DTO → Serviço → Entidade → Repositório
```

### 9. FluentResults

**O que é:** biblioteca para representar sucesso ou falha sem depender de exceções para fluxo comum.

**Para que serve:** um serviço pode retornar `Result.Ok()` ou `Result.Fail(...)`.

**Onde aparece:** `ServicoCaixa`, `ServicoRevista`, `ModelStateExtensions`, `TempDataExtensions`.

**O que dominar:** usar `Result` quando não há valor de retorno e `Result<T>` quando há valor.

```text
Result.Ok()
Result.Fail("Caixa não encontrada.")
Result.Ok(detalhesCaixaDto)
```

### 10. TempData

**O que é:** armazenamento temporário que sobrevive a um redirecionamento.

**Para que serve:** mostrar mensagem depois de um `RedirectToAction`.

**Onde aparece:** quando um registro não é encontrado ou quando uma exclusão falha.

**O que dominar:** usar TempData para mensagens pós-redirect, não para dados permanentes.

```text
POST Excluir → falha → TempData["MensagemErro"] → Redirect → Listar mostra alert
```

### 11. Métodos de Extensão

**O que é:** métodos estáticos que parecem métodos de um tipo existente.

**Para que serve:** melhorar legibilidade e centralizar código repetido.

**Onde aparece:**

- `AddInfraRepositories(this IServiceCollection services)`
- `AddApplicationServices(this IServiceCollection services)`
- `AddPresentation(this IServiceCollection services)`
- `AddModelError(this ModelStateDictionary modelState, ResultBase result)`
- `AddErrorMessage(this ITempDataDictionary tempData, ResultBase result)`

**O que dominar:** identificar o parâmetro `this` no primeiro argumento.

### 12. AutoMapper

**O que é:** biblioteca para mapear objetos de um tipo para outro.

**Para que serve:** reduzir repetição ao converter ViewModel ↔ DTO.

**Onde aparece:** `CaixaProfile`, `RevistaProfile`, `IMapper`.

**O que dominar:** configurar `CreateMap<Origem, Destino>()` e entender que AutoMapper não substitui regra de negócio.

### 13. Repositórios e Persistência

**O que é:** abstração para salvar, editar, excluir e consultar registros.

**Para que serve:** o restante da aplicação não precisa saber como os dados são salvos.

**Onde aparece:** `IRepositorio<T>`, `RepositorioBaseEmArquivo<T>`, `ContextoJson`.

**O que dominar:** CRUD genérico, `Filtrar(Predicate<T>)`, persistência JSON e limitações desse tipo de armazenamento.

### 14. LINQ, lambdas e delegates

**O que é:** formas de consultar coleções e passar comportamento como parâmetro.

**Para que serve:** filtrar, transformar e ordenar listas.

**Onde aparece:** `Select`, `Any`, `ToList`, filtros com lambda e `Predicate<T>`.

**O que dominar:** ler expressões como:

```csharp
revistas.Any(r => r.Titulo == titulo)
revistas.Select(r => new ListarRevistasDto(...))
repositorioRevista.Filtrar(r => r.Status == StatusRevista.Disponivel)
```

---

## 3. Projeto paralelo: Gestão de Figurinos Teatrais Comunitários

### Por que este tema?

Um grupo de teatro comunitário possui figurinos guardados em araras, baús e armários. A equipe precisa controlar onde cada peça está, quem retirou, quando deve devolver e se a peça está disponível, emprestada ou em manutenção.

É um contexto diferente do Clube da Leitura, mas com o mesmo desenho técnico:

```text
Clube da Leitura              Projeto paralelo
Caixa                         GuardaRoupa
Revista                       Figurino
Amigo                         Integrante
Empréstimo                    Retirada
```

Esse tema permite praticar os mesmos conceitos sem apenas trocar "revista" por outro item óbvio. Ele obriga você a pensar novamente sobre relacionamento, status, regras, tela, serviço e persistência.

### Entidades

| Entidade | Campos principais | Papel no sistema |
|---|---|---|
| `GuardaRoupa` | Id, Identificacao, Localizacao, CorEtiqueta, DiasMaximosRetirada | local onde figurinos ficam armazenados |
| `Figurino` | Id, Nome, Categoria, Tamanho, AnoConfeccao, GuardaRoupa, Status | item controlado pelo sistema |
| `Integrante` | Id, Nome, NomeResponsavel, Telefone | pessoa que pode retirar figurinos |
| `Retirada` | Id, Integrante, Figurino, DataRetirada, DataPrevistaDevolucao, DataDevolvido | operação de retirada/devolução |

### Relacionamentos

```text
GuardaRoupa 1 ───── * Figurino 1 ───── * Retirada * ───── 1 Integrante
                         │
                         └─ StatusFigurino: Disponivel, Retirado, Manutencao

Retirada
  └─ StatusRetirada: Aberta, Concluida, Atrasada
```

### Funcionalidades

1. Tela inicial com cards de navegação.
2. CRUD de guarda-roupas.
3. CRUD de figurinos.
4. CRUD de integrantes.
5. Registrar retirada de figurino.
6. Registrar devolução.
7. Listar retiradas abertas, concluídas e atrasadas.
8. Bloquear retirada de figurino indisponível.
9. Bloquear exclusão de guarda-roupa que possui figurinos.
10. Exibir mensagens de erro com `TempData`.
11. Usar serviços com `FluentResults`.
12. Mapear ViewModels e DTOs com AutoMapper.

### Regras de negócio

#### GuardaRoupa

- Identificação obrigatória.
- Identificação não pode ser duplicada.
- Dias máximos de retirada deve ser maior que zero.
- Não pode excluir guarda-roupa com figurinos vinculados.

#### Figurino

- Nome obrigatório.
- Categoria obrigatória.
- Tamanho obrigatório.
- Ano de confecção deve ser válido.
- Deve pertencer a um guarda-roupa existente.
- Não pode haver figurino com mesmo nome, categoria e tamanho no mesmo guarda-roupa.

#### Integrante

- Nome obrigatório.
- Telefone obrigatório.
- Telefone deve conter 10 ou 11 dígitos.
- Não pode haver integrante com mesmo nome e telefone.

#### Retirada

- Deve selecionar integrante.
- Deve selecionar figurino disponível.
- Data de retirada é automática.
- Data prevista de devolução é calculada pelo guarda-roupa.
- Ao retirar, o figurino muda para `Retirado`.
- Ao devolver, o figurino volta para `Disponivel`.
- Uma retirada sem devolução fica `Atrasada` se passar da data prevista.

### Telas

| Tela | O que pratica |
|---|---|
| Home/Index | layout, Bootstrap, navegação |
| GuardaRoupa/Listar | cards, TempData, ações por item |
| GuardaRoupa/Cadastrar | form simples, DataAnnotations |
| GuardaRoupa/Editar | GET por id, POST com id oculto |
| GuardaRoupa/Excluir | confirmação e regra de exclusão |
| Figurino/Cadastrar | dropdown de guarda-roupa |
| Figurino/Listar | status, relacionamento e badges |
| Integrante/Cadastrar | validação com regex |
| Retirada/Cadastrar | dois dropdowns dependentes |
| Retirada/Listar | tabela, filtro e destaque de atrasados |
| Retirada/Devolver | confirmação de operação |

---

## 4. Roadmap de implementação

### Etapa 1 — Projeto, layout e organização modular

**Objetivo:** criar o projeto MVC e preparar a estrutura de pastas.

**Conceitos praticados:** MVC, Bootstrap, Razor, rota padrão, layout compartilhado e organização modular.

**Resultado esperado:** aplicação abre na Home, possui navbar e pastas por módulo.

```text
ModuloGuardaRoupa
ModuloFigurino
ModuloIntegrante
ModuloRetirada
Compartilhado
```

### Etapa 2 — Domínio

**Objetivo:** criar entidades, enums e classe base.

**Conceitos praticados:** entidades de domínio, encapsulamento básico, propriedades calculadas, herança e regras internas.

**Resultado esperado:** classes `GuardaRoupa`, `Figurino`, `Integrante` e `Retirada` existem e representam o negócio.

### Etapa 3 — Infraestrutura e repositórios

**Objetivo:** criar `ContextoJson`, `IRepositorio<T>` e repositórios em arquivo.

**Conceitos praticados:** interfaces, genéricos, persistência JSON, herança e `Predicate<T>`.

**Resultado esperado:** registros são mantidos em listas e salvos em `dados.json`.

### Etapa 4 — Injeção de dependência

**Objetivo:** registrar repositórios, serviços, MVC e AutoMapper usando métodos de extensão.

**Conceitos praticados:** DI, `IServiceCollection`, `AddScoped`, métodos de extensão e composição da aplicação.

**Resultado esperado:** `Program.cs` fica limpo:

```text
builder.Services.AddInfraRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddPresentation();
```

### Etapa 5 — CRUD de GuardaRoupa com Serviço

**Objetivo:** implementar CRUD completo usando Controller → Serviço → Repositório.

**Conceitos praticados:** ViewModels, DTOs, AutoMapper, FluentResults, ModelStateExtensions e TempDataExtensions.

**Resultado esperado:** guarda-roupas podem ser cadastrados, editados, listados e excluídos com validação de identificação duplicada.

### Etapa 6 — CRUD de Figurino com dropdown

**Objetivo:** cadastrar figurinos vinculados a um guarda-roupa.

**Conceitos praticados:** cadastro dependente, select, `ValidateNever`, repopulação de dropdown, regra de ID válido.

**Resultado esperado:** figurino só é cadastrado quando existe um guarda-roupa válido selecionado.

### Etapa 7 — CRUD de Integrante

**Objetivo:** implementar cadastro de pessoas que podem retirar figurinos.

**Conceitos praticados:** DataAnnotations, regex, duplicidade e mapeamento.

**Resultado esperado:** integrantes são cadastrados e validados corretamente.

### Etapa 8 — Retirada e devolução

**Objetivo:** registrar retirada de figurinos e devolução.

**Conceitos praticados:** relacionamento com duas entidades, status, data calculada, regra de disponibilidade e PRG.

**Resultado esperado:** ao retirar, figurino fica indisponível; ao devolver, volta para disponível.

### Etapa 9 — Filtros e listagem inteligente

**Objetivo:** filtrar retiradas por status.

**Conceitos praticados:** query string, Tag Helpers de rota, `Predicate<T>`, lambda, LINQ e destaque visual.

**Resultado esperado:** listagem mostra todas, abertas, concluídas ou atrasadas.

### Etapa 10 — Refatoração final

**Objetivo:** revisar responsabilidades e reduzir repetição.

**Conceitos praticados:** boas práticas, métodos privados, métodos de extensão, profiles do AutoMapper e serviços.

**Resultado esperado:** Controllers finos, serviços com regras claras e views sem lógica de negócio.

---

## 5. Fluxos de conhecimento

### Fluxo A — Cadastrar um GuardaRoupa

```text
Formulário
  → Model Binding
  → CadastrarGuardaRoupaViewModel
  → DataAnnotations
  → ModelState
  → AutoMapper
  → CadastrarGuardaRoupaDto
  → ServicoGuardaRoupa
  → FluentResults
  → Repositório
  → ContextoJson
  → RedirectToAction
```

O Controller não deve verificar manualmente todas as regras. Ele valida o formato do formulário com `ModelState`, transforma a entrada em DTO e pede ao serviço para executar o caso de uso.

Se o serviço detectar identificação duplicada, retorna `Result.Fail`. O Controller converte essa falha para `ModelState` e devolve a mesma view.

### Fluxo B — Cadastrar um Figurino

```text
GET Cadastrar
  → Serviço busca guarda-roupas
  → Controller monta CadastrarFigurinoViewModel
  → View exibe <select>

POST Cadastrar
  → Binder preenche FigurinoViewModel
  → DataAnnotations validam campos simples
  → Serviço valida GuardaRoupaId
  → Serviço valida duplicidade
  → Serviço cria Figurino
  → Repositório salva
```

Aqui aparece uma ideia central: o navegador envia o `GuardaRoupaId`, não o objeto `GuardaRoupa`. O serviço precisa buscar o objeto real antes de criar o figurino.

Se o formulário voltar com erro, recarregue a lista do dropdown antes de retornar a view.

### Fluxo C — Registrar uma Retirada

```text
Usuário escolhe integrante e figurino
  → Controller recebe IntegranteId e FigurinoId
  → Serviço busca integrante
  → Serviço busca figurino
  → Serviço verifica se figurino está disponível
  → calcula DataPrevistaDevolucao
  → cria Retirada
  → muda StatusFigurino para Retirado
  → salva
  → redireciona para Listar
```

Essa funcionalidade conecta quase tudo:

- relacionamento entre entidades;
- dropdowns;
- validação de negócio;
- status;
- data calculada;
- persistência;
- redirecionamento;
- listagem com ViewModel.

### Fluxo D — Devolver um Figurino

GET `Devolver(id)` mostra uma tela de confirmação. POST `Devolver` executa a operação.

```text
Retirada aberta
  → RegistrarDevolucao()
  → DataDevolvido = hoje
  → Figurino.Status = Disponivel
  → Repositório.Editar(...)
  → Redirect
```

O status da retirada pode ser calculado:

```text
se DataDevolvido existe → Concluida
senão se hoje > DataPrevista → Atrasada
senão → Aberta
```

### Fluxo E — Excluir com TempData

Quando você tenta excluir um guarda-roupa com figurinos, não faz sentido retornar a tela de exclusão com erro de campo. O problema acontece depois da tentativa de operação.

```text
POST Excluir
  → Serviço detecta figurinos vinculados
  → Result.Fail(...)
  → TempData.AddErrorMessage(resultado)
  → RedirectToAction(Listar)
  → Listar exibe alert Bootstrap
```

TempData é ideal aqui porque a mensagem precisa sobreviver ao redirect.

### Fluxo F — Filtro com lambda e LINQ

Na listagem de retiradas:

```text
/Retirada/Listar
/Retirada/Listar?status=abertas
/Retirada/Listar?status=concluidas
/Retirada/Listar?status=atrasadas
```

O Controller recebe `status` e escolhe o critério.

```csharp
retiradas = status switch
{
    "abertas" => retiradas.Where(r => r.Status == StatusRetirada.Aberta).ToList(),
    "concluidas" => retiradas.Where(r => r.Status == StatusRetirada.Concluida).ToList(),
    "atrasadas" => retiradas.Where(r => r.Status == StatusRetirada.Atrasada).ToList(),
    _ => retiradas
};
```

O importante não é decorar o `switch`. É entender que a tela passa uma intenção, o Controller traduz essa intenção, e a consulta filtra os dados.

---

## 6. Estrutura sugerida de arquivos

```text
GestaoFigurinos.WebApp
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
  ├── ModuloGuardaRoupa
  │   ├── Aplicacao
  │   ├── Apresentacao
  │   ├── Dominio
  │   └── Infra
  ├── ModuloFigurino
  ├── ModuloIntegrante
  ├── ModuloRetirada
  └── Program.cs
```

Mantenha o mesmo padrão em todos os módulos. O objetivo é criar memória arquitetural: quando você abrir um módulo, já sabe onde procurar Controller, Serviço, Entidade, Repositório e Views.

---

## 7. Boas práticas para aplicar

- Use ViewModels diferentes para cada tela.
- Não receba entidade de domínio diretamente no POST.
- Não coloque regra de negócio na View.
- Não salve dados no Controller quando houver serviço para esse caso de uso.
- Sempre valide o ID de entidades relacionadas no servidor.
- Repopule dropdowns quando retornar a View com erro.
- Use `RedirectToAction` depois de POST válido.
- Use `TempData` para mensagens após redirect.
- Use `ModelState` para erros que pertencem ao formulário atual.
- Use `FluentResults` para representar sucesso/falha de serviços.
- Use AutoMapper apenas para mapeamentos mecânicos, não para regra de negócio.
- Mantenha o `Program.cs` como ponto de composição da aplicação.
- Use métodos de extensão para agrupar registros de DI.

---

## 8. Desafios extras

1. **Filtro por categoria de figurino:** permita listar figurinos por categoria.
2. **Filtro por status:** disponível, retirado ou manutenção.
3. **Paginação:** use `Skip`, `Take` e preserve o filtro atual.
4. **Busca por nome:** filtre figurinos ou integrantes com `Contains`.
5. **Validação personalizada:** crie um atributo para validar telefone ou ano de confecção.
6. **Serviços para todos os módulos:** se você começar com serviço só em GuardaRoupa/Figurino, depois refatore Integrante/Retirada.
7. **AutoMapper Profile por módulo:** crie `GuardaRoupaProfile`, `FigurinoProfile`, `IntegranteProfile`, `RetiradaProfile`.
8. **Manutenção de figurino:** crie ação para marcar figurino como em manutenção.
9. **Bloqueio de exclusão de integrante:** não permitir excluir integrante com retiradas vinculadas.
10. **Dashboard:** mostrar total de figurinos disponíveis, retirados, atrasados e em manutenção.

---

## 9. Checklist final

Marque apenas quando você conseguir explicar sem olhar o código.

- [ ] Sei explicar por que a versão usada como referência é a `origin/v8`.
- [ ] Sei explicar a diferença entre organização por tipo de arquivo e organização modular.
- [ ] Sei criar um módulo com Apresentação, Aplicação, Domínio e Infra.
- [ ] Sei explicar a rota até chegar em uma action.
- [ ] Sei diferenciar action GET e action POST.
- [ ] Sei explicar por que uso ViewModel em vez de entidade no formulário.
- [ ] Sei explicar Model Binding.
- [ ] Sei explicar DataAnnotations e ModelState.
- [ ] Sei adicionar erro manual com `ModelState.AddModelError`.
- [ ] Sei explicar quando usar TempData.
- [ ] Sei explicar por que serviços deixam Controllers mais limpos.
- [ ] Sei criar um serviço que retorna `Result` ou `Result<T>`.
- [ ] Sei transformar erro de FluentResults em erro de ModelState.
- [ ] Sei transformar erro de FluentResults em mensagem de TempData.
- [ ] Sei registrar repositórios e serviços com DI.
- [ ] Sei explicar `AddScoped`.
- [ ] Sei criar métodos de extensão para DI.
- [ ] Sei configurar AutoMapper com Profile.
- [ ] Sei mapear ViewModel para DTO e DTO para ViewModel.
- [ ] Sei criar um CRUD completo com Bootstrap e Tag Helpers.
- [ ] Sei criar dropdown de entidade relacionada.
- [ ] Sei repopular dropdown quando o POST volta com erro.
- [ ] Sei modelar relacionamento `GuardaRoupa → Figurino → Retirada`.
- [ ] Sei alterar status de figurino ao registrar retirada/devolução.
- [ ] Sei calcular status de retirada.
- [ ] Sei bloquear exclusões que quebrariam relacionamentos.
- [ ] Sei usar LINQ com `Where`, `Select`, `Any` e `ToList`.
- [ ] Sei ler uma expressão lambda.
- [ ] Sei explicar o papel do repositório e do `ContextoJson`.
- [ ] Sei seguir o fluxo completo: formulário → Controller → Serviço → Repositório → JSON → Redirect → View.

Quando este checklist fizer sentido, você não terá apenas feito outro CRUD. Você terá entendido como os conceitos novos do Clube da Leitura trabalham juntos dentro de uma aplicação MVC real: módulos organizam o código, controllers recebem a interação, serviços protegem regras, repositórios persistem dados, views mostram o resultado e o fluxo inteiro fica previsível.
