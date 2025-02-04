 using System.Collections.Generic;
 using System.Text.Json.Serialization;
 
 namespace loja.models
 {
 public class Deposito
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        [JsonIgnore]
        public ICollection<DepositoProduto> DepositoProdutos { get; set; }

    }
 }