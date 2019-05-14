using Catalog.Queries.Products;
using Ecommerce.Integration.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Scenario.CartFlow
{
    [TestFixture]
    public class CarrinhoDeCompraTests
    {
        private readonly HttpServiceClientCatalog _catalogoServiceClient;

        public CarrinhoDeCompraTests()
        {
            _catalogoServiceClient = (HttpServiceClientCatalog)NativeInjectorBootStrapper.GetInstanceHttpServiceClient<HttpServiceClientCatalog>();
        }

        /// <summary>
        /// Passo 1: Escolher o produto
        /// Passo 2: Adicionar o produto escolhido no carrinho
        /// Passo 3: Fechar o Pedido
        /// Passo 4: Escolher Forma de Pagamento
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Executar()
        {
            Func<int, ProductItemMessageResponse> PassoEscolherProduto = (int arg) =>
            {
                var result = _catalogoServiceClient.GetAllProducts();
                result.Wait();
                return result.Result.Data.ElementAt(arg);
            };

            var produtoSelecionado = PassoEscolherProduto(RandomNumber(1, 20));

            //result.IsSuccessStatusCode.Should().BeTrue();
        }

        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

    }
}
