using loja.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace loja.data{
    public class LojaDbContext : DbContext{
        public LojaDbContext(DbContextOptions<LojaDbContext> options) : base(options){}
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<Deposito> Depositos { get; set; }
        //public DbSet<ProdutoDeposito> ProdutoDepositos { get; set; }
        public DbSet<DepositoProduto> DepositoProdutos { get; set; }


         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DepositoProduto>()
                .HasKey(pd => new { pd.ProdutoId, pd.DepositoId }); // Definindo a chave composta

            base.OnModelCreating(modelBuilder);
        }
    }
}