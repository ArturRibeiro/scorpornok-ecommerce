using Shared.Code.Commands;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Gateway.Payment.Web.Api.App.eRede.Commands
{
    [DataContract(Namespace = "Epimed")]
    public class AuthorizationCommand : CommandBase
    {
        /// <summary>
        /// Define se a transação terá captura automática ou posterior. 
        /// O não envio desse campo será considerado a captura automática (true)
        /// </summary>
        [DataMember] public bool Caputre { get; set; }

        /// <summary>
        /// Tipo de transação a ser realizada
        /// </summary>
        [DataMember] public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Código do pedido gerado pelo estabelecimento
        /// </summary>
        [Required]
        [MaxLength(16)]
        [DataMember] public string Reference { get; set; }

        /// <summary>
        /// Valor total da transação sem separador de milhar e decimal
        /// Exemplos: 
        ///     R$ 10,00 = 1000 
        ///     R$ 0,50 = 50
        /// </summary>
        //[Required]
        //[MaxLength(10)]
        [DataMember] public decimal Amount { get; set; }

        /// <summary>
        /// Número de parcelas em que a transação será autorizada. 
        /// De 2 a 12 O não envio desse campo será considerado à vista.
        /// </summary>
        [DataMember] public short Installments { get; set; }

        /// <summary>
        /// Nome do portador impresso no cartão.
        /// </summary>
        [Required]
        [DataMember] public string CardHolderName { get; set; }

        /// <summary>
        /// Número do cartão
        /// </summary>
        [Required]
        [MaxLength(19)]
        [DataMember] public string CardNumber { get; set; }

        /// <summary>
        /// Mês de vencimento do cartão. De 1 a 12.
        /// </summary>
        [Required]
        [DataMember] public short ExpirationMonth { get; set; }

        /// <summary>
        /// Ano de vencimento do cartão
        /// Ex.: 2021 ou 21
        /// </summary>
        [Required]
        [DataMember] public int ExpirationYear { get; set; }

        /// <summary>
        /// Código de segurança do cartão (geralmente localizado no verso do cartão)
        /// O envio desse parâmetro garante maior possibilidade de aprovação da transação.
        /// </summary>
        [MaxLength(4)]
        [DataMember] public string SecurityCode { get; set; }

        /// <summary>
        /// Frase personalizada que será impressa na fatura do portador
        /// </summary>
        [MaxLength(13)]
        [DataMember] public string SoftDescriptor { get; set; }

        /// <summary>
        /// Informa ao emissor se a transação é proveniente de uma recorrência. Se transação for uma recorrência, enviar true. Caso contrário, enviar false.
        /// O não envio desse campo será considerado o valor false.
        /// A Rede não gerencia os agendamentos de recorrência, apenas permite aos lojistas indicarem se a transação originada é de um plano recorrente.
        /// </summary>
        [DataMember] public bool Subscription { get; set; }

        /// <summary>
        /// Identifica a origem da transação
        /// </summary>
        [DataMember] public Origin Origin { get; set; }

        /// <summary>
        /// Número de filiação do distribuidor (PV)
        /// </summary>
        [MaxLength(9)]
        [DataMember] public string DistributorAffiliation { get; set; }
    }
}
