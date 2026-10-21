using HotChocolate.Diagnostics;

namespace Frameworker.Observability;

// O cartão chega escrito no documento da tokenizeCard (não só em variáveis), e CPF, telefone e e-mail chegam no
// placeOrder: a operação GraphQL vai para o trace só pelo tipo e pelo nome. Usado no gateway e nos subgraphs.
public static class GraphQLInstrumentation
{
    public static void WithoutSensitiveData(this InstrumentationOptionsBase options)
    {
        options.IncludeDocument = false;
        // O padrão também grava as variáveis e as extensões da requisição.
        options.RequestDetails = RequestDetails.OperationName;
        // A mensagem de um erro de validação pode repetir um valor do documento; fica só a contagem (graphql.error.count).
        options.MaxErrorEvents = 0;
        options.IncludeOperationNameInSpanName = true;
    }
}
