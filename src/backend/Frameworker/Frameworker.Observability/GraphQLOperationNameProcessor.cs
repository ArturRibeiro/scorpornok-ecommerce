namespace Frameworker.Observability;

// Toda requisição GraphQL chega no mesmo POST /graphql: sem isto, a lista de traces mostraria só
// "POST /graphql/{**slug}". O span da operação (Hot Chocolate e Fusion) traz o tipo e o nome; o span HTTP que o
// contém passa a se chamar "mutation PlaceOrder". O span HTTP termina depois do da operação e depois de a
// instrumentação do ASP.NET Core o nomear pela rota, então o nome dado aqui prevalece.
public sealed class GraphQLOperationNameProcessor : BaseProcessor<Activity>
{
    private const string OperationName = "graphql.operation.display";

    public override void OnEnd(Activity activity)
    {
        if (activity.GetCustomProperty(OperationName) is string name)
        {
            activity.DisplayName = name;
            return;
        }

        if (activity.GetTagItem("graphql.processing.type") as string != "request"
            || activity.GetTagItem("graphql.operation.type") is not string type)
            return;

        var server = ServerAncestor(activity);
        if (server is null)
            return;
        var operation = activity.GetTagItem("graphql.operation.name") as string;
        server.SetCustomProperty(OperationName, string.IsNullOrEmpty(operation) ? type : $"{type} {operation}");
    }

    private static Activity? ServerAncestor(Activity activity)
    {
        for (var parent = activity.Parent; parent is not null; parent = parent.Parent)
            if (parent.Kind == ActivityKind.Server)
                return parent;
        return null;
    }
}
