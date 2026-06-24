// O método CreateBuilder() cria uma instância de WebApplicationBuilder. 
// Esse objeto contém tudo o que será necessário para montar a aplicação: 
// configuração, serviços, logs, leitura de arquivos de configuração, variáveis de ambiente e injeção de dependência.
var builder = WebApplication.CreateBuilder(args);

// O Services é um contêiner de recursos da aplicação, 
// e o AddControllersWithViews() adiciona ao contêiner toda a funcionalidade necessária para que o MVC funcione.
builder.Services.AddControllersWithViews();

// builder.Build() cria a aplicação utilizando todas as configurações e serviços registrados anteriormente, 
// gerando uma instância de WebApplication pronta para ser configurada e executada.
var app = builder.Build();

// Esse bloco aplica configurações de segurança e tratamento de erros para ambientes que não são de desenvolvimento, 
// evitando expor informações técnicas aos usuários e reforçando o uso de HTTPS.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// UseRouting() habilita o mecanismo de roteamento, permitindo que o ASP.NET interprete URLs 
// e determine qual Controller e Action deverão atender a requisição.
app.UseRouting();

// UseAuthorization() verifica se o usuário está autorizado a acessar o recurso solicitado, 
// aplicando as regras de permissão da aplicação.
app.UseAuthorization();

// MapStaticAssets() mapeia os arquivos estáticos da aplicação, permitindo que CSS, JavaScript, imagens e outros recursos sejam acessados diretamente pelo navegador.
app.MapStaticAssets();

// O MapControllerRoute() é o que efetivamente conecta o sistema de rotas aos Controllers da aplicação. 
// Antes, o UseRouting() apenas habilitou o mecanismo de roteamento; aqui você define uma rota concreta que o MVC poderá utilizar.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Inicia o loop da aplicação
app.Run();
