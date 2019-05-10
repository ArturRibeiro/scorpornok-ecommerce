using Shared.Code.Commands;
using Shared.Code.Notifications;
using Gateway.Payment.Web.Api.App.eRede.Commands;
using MediatR;
using Shared.Code.Bus;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Payment.Web.Api.App.eRede
{
    public sealed class eRedeCommandHandler : CommandHandler,
        IRequestHandler<AuthorizationCommand>
    {
        public eRedeCommandHandler(IMediatorHandler mediator, INotificationHandler<DomainNotification> notificationHandler)
            : base(mediator, notificationHandler)
        {
        }

        /// <summary>
        /// A autorização é o primeiro passo para realizar uma transação.
        /// O valor da transação sensibiliza o limite do cartão do portador, porém não gera cobrança na fatura enquanto não houver a confirmação (captura).
        /// Caso a autorização não seja capturada no prazo máximo de acordo com o ramo do estabelecimento
        /// </summary>
        /// <param name="message">Representa uma autorização <see cref="AuthorizationCommand"/></param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        public async Task<Unit> Handle(AuthorizationCommand message, CancellationToken cancellationToken)
        {

            //await _mediator.RaiseEvent(DomainNotification.Factory.Create(message, "message error"));

            return Unit.Value;
        }
    }
}
