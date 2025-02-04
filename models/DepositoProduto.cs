 namespace loja.models
 {
    public class DepositoProduto{
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }
        public int DepositoId { get; set; }
        public Deposito Deposito { get; set; }
        public int Quantidade { get; set; }
    }
}