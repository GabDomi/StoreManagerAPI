using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using loja.data;
using loja.models;


namespace loja.services
{
    public class VendaService
    {
        private readonly LojaDbContext _dbContext;

        public VendaService(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddVendaAsync(Venda venda)
        {
            _dbContext.Vendas.Add(venda);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Venda>> GetVendasPorProdutoDetalhadaAsync(int produtoId)
        {
            return await _dbContext.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .Where(v => v.ProdutoId == produtoId)
                .ToListAsync();
        }

        public async Task<List<object>> GetVendasPorProdutoSumarizadaAsync(int produtoId)
        {
            var vendas = await _dbContext.Vendas
                .Where(v => v.ProdutoId == produtoId)
                .GroupBy(v => v.Produto)
                .Select(g => new
                {
                    ProdutoNome = g.Key.Nome,
                    QuantidadeVendida = g.Sum(v => v.QuantidadeVendida),
                    TotalPreco = g.Sum(v => v.QuantidadeVendida * v.PrecoUnitario)
                })
                .ToListAsync();
            return vendas.Cast<object>().ToList();
        }

        public async Task<List<Venda>> GetVendasPorClienteDetalhadaAsync(int clienteId)
        {
            return await _dbContext.Vendas
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .Where(v => v.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<List<object>> GetVendasPorClienteSumarizadaAsync(int clienteId)
        {
            var vendas = await _dbContext.Vendas
                .Where(v => v.ClienteId == clienteId)
                .GroupBy(v => v.Cliente)
                .Select(g => new
                {
                    ClienteNome = g.Key.Nome,
                    TotalPreco = g.Sum(v => v.QuantidadeVendida * v.PrecoUnitario),
                    Produtos = g.Select(v => new { v.Produto.Nome, v.QuantidadeVendida }).ToList()
                })
                .ToListAsync();
            return vendas.Cast<object>().ToList();
        }

        //Consultar produtos no deposito / estoque (sumarizada)
        public async Task<List<object>> GetProdutosNoDepositoAsync(int depositoId)
        {
            var result = await _dbContext.DepositoProdutos
                .Where(dp => dp.DepositoId == depositoId)
                .Select(dp => new
                {
                    ProdutoId = dp.ProdutoId,
                    NomeProduto = dp.Produto.Nome,
                    QuantidadeV = dp.Quantidade
                })
                .ToListAsync();

            return result.Cast<object>().ToList(); 
        }

        // Consultar a quantidade de um produto no depósito / estoque
        public async Task<object> GetQuantidadeProdutoNoDepositoAsync(int produtoId)
        {
            var result = await _dbContext.DepositoProdutos
                .Where(dp => dp.ProdutoId == produtoId)
                .Select(dp => new
                {
                    ProdutoId = dp.ProdutoId,
                    NomeProduto = dp.Produto.Nome,
                    Quantidade = dp.Quantidade
                })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}