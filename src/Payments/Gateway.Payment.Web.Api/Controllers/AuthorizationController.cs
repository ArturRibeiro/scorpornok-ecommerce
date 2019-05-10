using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gateway.Payment.Web.Api.App.eRede.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Code.Bus;

namespace Gateway.Payment.Web.Api.Controllers
{
    /// <summary>
    /// Uma transação pode ser enviada com captura automática ou manual.
    /// Na captura automática é enviada a autorização e a captura na mesma mensagem.
    /// Com isso, a cobrança será realizada na hora em que a transação é aprovada.
    /// Esse tipo de autorização é o mais utilizado.
    /// A captura manual ocorre em casos que o estabelecimento não quer debitar imediatamente da conta do comprador.
    /// O caso mais comum para esse caso é o de aluguéis, reservas e pré-pagos.
    /// Um exemplo prático, seria a reserva de um hotel. O cliente faz a reserva (autorização) através do site, 
    /// porém a cobrança (captura) só é realizada no check-in.
    /// </summary>
    [Route("api/v1/[controller]"), ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IMediatorHandler mediatorHandler;

        public AuthorizationController(IMediatorHandler mediatorHandler)
            => this.mediatorHandler = mediatorHandler;

        /// <summary>
        /// A autorização é o primeiro passo para realizar uma transação.
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("Authorize")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Authorize([FromBody] AuthorizationCommand message, [FromHeader(Name = "CardOperator")] string operadora)
        {
            await this.mediatorHandler.Send(message);
            return Ok();

        }
    }
}