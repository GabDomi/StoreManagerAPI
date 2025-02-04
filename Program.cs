/*no terminal: dotnet add package
Microsoft.AspNetCore.Authentication.JwtBearer*/

using loja.controllers;
using Microsoft.EntityFrameworkCore;
using loja.data;
using loja.models;
using loja.services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
/*código padrão para lib de autenticação*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
/*final do código padrão para libs*/

var builder = WebApplication.CreateBuilder(args);

//Impedir o looping
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    
});

//Add services to the container
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<FornecedorService>();
builder.Services.AddScoped<VendaService>();
builder.Services.AddScoped<DepositoService>();


/*Código padrão: configuração do metodo de autenticação*/
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options=>
    {
        options.TokenValidationParameters= new TokenValidationParameters
        {
            ValidateIssuer=false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes("abcabcabcabcabcabcabcabcabcabcabc")
            )
        };
    });
    

// Adiciona o serviço de autorização com uma política padrão que requer autenticação
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

/*Fim da configuração da autenticação*/

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configurar a conexão com o BD
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LojaDbContext>(options=>options.UseMySql(connectionString,new MySqlServerVersion(new Version(8,0,26))));

var app = builder.Build();

//Configurar as requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();

}

app.MapControllers();

app.UseHttpsRedirection();

app.MapPost("/login",async (HttpContext context) =>{
    //Receber o request.body
    using var reader = new StreamReader(context.Request.Body);   
    var body = await reader.ReadToEndAsync();

    //Deserializar o objeto Json
    var json = JsonDocument.Parse(body);
    var username = json.RootElement.GetProperty("username").GetString();
    var email = json.RootElement.GetProperty("email").GetString();
    var senha = json.RootElement.GetProperty("senha").GetString();

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Credenciais inválidas. Por favor, preencha todos os campos.");
        return;
    }
    
    string token;
    if(senha == "1029")
    { token = GenerateToken(email);
    }else{
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Senha incorreta.");
        return;
    }
   
    await context.Response.WriteAsync(token);  
});

//Rota segura: toda rota tem corpo de código parecido
app.MapGet("/rotaSegura", async (HttpContext context)=>
{
    //Verificar se o token está presente
    if (!context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token não fornecido");
        return;
    }

    //Obter o token

    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    //Validar o token
    //Esta lógica será convertida em um método dentro de uma classe a ser reaproveitaada
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("abcabcabcabcabcabcabcabcabcabcabc");
    //Chave secreta (a mesma utilizada para gerar o token)
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    SecurityToken validateToken;
    try
    {
        //Decodifica, verifica e valida o token
        tokenHandler.ValidateToken(token, validationParameters, out validateToken);
    }
    catch (Exception)
    {
        //Caso o token seja inválido
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token inválido");
        return;
    }
    //Se o token é válido: dar andamento na lógica do endpoint
    await context.Response.WriteAsync("Autorizado");
});

//Metodo de criação do token
//Sera transportado para uma classe específica
string GenerateToken(string data){
    var tokenHandler = new JwtSecurityTokenHandler();
    //Esta chave secreta será gravada em uma variável de ambiente por questão de segurança
    var secretKey = Encoding.ASCII.GetBytes("abcabcabcabcabcabcabcabcabcabcabc");
    //Configurador do token
    var tokenDescriptor = new SecurityTokenDescriptor{
        Expires = DateTime.UtcNow.AddHours(1), //O token expeira em uma hora
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature
        )
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
    
}

//Endpoint produtos

app.MapPost("/produtos", async (Produto produto, ProductService productService) =>
{
    await productService.AddProductAsync(produto);
    return Results.Created($"/produtos/{produto.Id}", produto);
}).RequireAuthorization();


app.MapGet("/produtos", async (ProductService productService) =>
{
    var produtos = await productService.GetAllProductsAsync();
    return Results.Ok(produtos);
}).RequireAuthorization();

app.MapGet("/produtos/{id}", async (int id, ProductService productService) =>
{
    var produto = await productService.GetProductByIdAsync(id);
    if (produto == null)
    {
        return Results.NotFound($"Produto with ID {id} not found.");
    }

    return Results.Ok(produto);
}).RequireAuthorization();

app.MapDelete("/produtos/{id}", async (int id, ProductService productService) =>
{
    await productService.DeleteProductAsync(id);
    return Results.Ok();
}).RequireAuthorization();

app.MapPut("/produtos/{id}", async (int id, Produto produto, ProductService productService) =>
{
  if(id != produto.Id)
  {
    return Results.BadRequest("Product Id mismatch.");
  }

  await productService.UpdateProductAsync(produto);
  return Results.Ok();
}).RequireAuthorization();


app.MapPost("/createcliente", async (LojaDbContext dbContext, Cliente newCliente) =>
{
    dbContext.Clientes.Add(newCliente);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/createcliente/{newCliente.Id}", newCliente);
}).RequireAuthorization();

app.MapGet("/clientes", async (LojaDbContext dbContext) =>
{
    var clientes = await dbContext.Clientes.ToListAsync();
    return Results.Ok(clientes);
}).RequireAuthorization();

app.MapGet("/clientes/{id}", async (int id, LojaDbContext dbContext) =>
{
    var cliente = await dbContext.Clientes.FindAsync(id);
    if (cliente == null)
    {
        return Results.NotFound($"Cliente with Id {id} not found.");
    }

    return Results.Ok(cliente);
}).RequireAuthorization();

//Endpoint para atualizar um Cliente existente
app.MapPut("/clientes/{id}", async (int id, LojaDbContext dbContext, Cliente updatedCliente) =>
{
    //Verifica se o cliente existe na base, conforme o id informado
    //Se o cliente existir na base, será retornado para dentro do objeto existingCliente
    var existingCliente = await dbContext.Clientes.FindAsync(id);
    if (existingCliente == null)
    {
        return Results.NotFound($"Cliente with ID {id} not found.");
    }

    //Atualiza os dados do existingCliente
    existingCliente.Id = updatedCliente.Id;
    existingCliente.Nome = updatedCliente.Nome;
    existingCliente.Cpf = updatedCliente.Cpf;
    existingCliente.Email = updatedCliente.Email;

    //Salva no banco de dados
    await dbContext.SaveChangesAsync();

    //Retorna para o cliente que invocou o endpoint
    return Results.Ok(existingCliente);
}).RequireAuthorization();

//Endpoint fornecedores

app.MapPost("/fornecedores", async (Fornecedor fornecedor, FornecedorService fornecedorService) =>
{
    await fornecedorService.AddFornecedorAsync(fornecedor);
    return Results.Created($"/fornecedores/{fornecedor.Id}", fornecedor);
}).RequireAuthorization();

app.MapGet("/fornecedores", async (FornecedorService fornecedorService) =>
{
    var fornecedores = await fornecedorService.GetAllFornecedoresAsync();
    return Results.Ok(fornecedores);
}).RequireAuthorization();

app.MapGet("/fornecedores/{id}", async (int id, FornecedorService fornecedorService) =>
{
    var fornecedor = await fornecedorService.GetFornecedorByIdAsync(id);
    if (fornecedor == null)
    {
        return Results.NotFound($"Fornecedor with ID {id} not found.");
    }

    return Results.Ok(fornecedor);
}).RequireAuthorization();

app.MapPut("/fornecedores/{id}", async (int id, Fornecedor fornecedor,FornecedorService fornecedorService) =>
{
  if(id != fornecedor.Id)
  {
    return Results.BadRequest("Fornecedor Id mismatch.");
  }

  await fornecedorService.UpdateFornecedorAsync(fornecedor);
  return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/fornecedores/{id}", async (int id, FornecedorService fornecedorService) =>
{
    await fornecedorService.DeleteFornecedorAsync(id);
    return Results.Ok();
}).RequireAuthorization();


//Endpoint vendas

app.MapPost("/vendas", async (Venda venda, VendaService vendaService, DepositoService depositoService) =>
{
    var produtoDeposito = await depositoService.GetQuantidadeProdutoNoDepositoAsync(venda.ProdutoId);
    if (produtoDeposito == null)
    {
        return Results.NotFound("Produto não encontrado no depósito.");
    }

    if (venda.QuantidadeVendida > produtoDeposito.Quantidade)
    {
        return Results.BadRequest("Quantidade insuficiente no depósito.");
    }

    await vendaService.AddVendaAsync(venda);
    await depositoService.UpdateQuantidadeProdutoNoDepositoAsync(venda.ProdutoId, venda.QuantidadeVendida);
    return Results.Created($"/vendas/{venda.Id}", venda);
}).RequireAuthorization();

app.MapGet("/vendas/produto/detalhada/{produtoId}", async (int produtoId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorProdutoDetalhadaAsync(produtoId);
    return Results.Ok(vendas);
}).RequireAuthorization();

app.MapGet("/vendas/produto/sumarizada/{produtoId}", async (int produtoId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorProdutoSumarizadaAsync(produtoId);
    return Results.Ok(vendas);
}).RequireAuthorization();

app.MapGet("/vendas/cliente/detalhada/{clienteId}", async (int clienteId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorClienteDetalhadaAsync(clienteId);
    return Results.Ok(vendas);
}).RequireAuthorization();

app.MapGet("/vendas/cliente/sumarizada/{clienteId}", async (int clienteId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorClienteSumarizadaAsync(clienteId);
    return Results.Ok(vendas);
}).RequireAuthorization();

// Endpoints para Depósito

app.MapGet("/depositos/{depositoId}/produtos", async (int depositoId, VendaService vendaService) =>
{
    var produtos = await vendaService.GetProdutosNoDepositoAsync(depositoId);
    return Results.Ok(produtos);
}).RequireAuthorization();

app.MapGet("/depositos/produto/{produtoId}", async (int produtoId, VendaService vendaService) =>
{
    var produto = await vendaService.GetQuantidadeProdutoNoDepositoAsync(produtoId);
    return Results.Ok(produto);
}).RequireAuthorization();

// Endpoint para adicionar estoque
app.MapPost("/depositos/{depositoId}/produtos/{produtoId}/adicionar", async (int depositoId, int produtoId, [FromBody] AdicionarEstoque adicionarEstoque, DepositoService depositoService) =>
{
    await depositoService.AddEstoqueAsync(depositoId, produtoId, adicionarEstoque.Quantidade);
    return Results.Ok();
}).RequireAuthorization();

app.Run();

