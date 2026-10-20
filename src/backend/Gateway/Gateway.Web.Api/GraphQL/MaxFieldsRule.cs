namespace Gateway.Web.Api.GraphQL;

/// <summary>
/// Recusa a operação com mais campos que <c>GraphQL:MaxFields</c>. Os fragmentos entram expandidos: um
/// fragmento usado 100 vezes conta 100 vezes, para não servir de atalho. Roda na validação do gateway,
/// então a consulta recusada não chega aos subgraphs.
/// </summary>
public sealed class MaxFieldsRule(int maxFields) : IDocumentValidatorRule
{
    public const string ErrorCode = "MAX_FIELDS_EXCEEDED";

    public ushort Priority => ushort.MaxValue;

    // O resultado só depende do documento e do limite.
    public bool IsCacheable => true;

    public void Validate(DocumentValidatorContext context, DocumentNode document)
    {
        var fragments = document.Definitions.OfType<FragmentDefinitionNode>()
            .GroupBy(f => f.Name.Value)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var operation in document.Definitions.OfType<OperationDefinitionNode>())
        {
            var count = Count(operation.SelectionSet, fragments, [], maxFields);
            if (count > maxFields)
            {
                context.ReportError(ErrorBuilder.New()
                    .SetMessage($"A consulta tem mais de {maxFields} campos.")
                    .SetCode(ErrorCode)
                    .SetExtension("maxFields", maxFields)
                    .Build());
                return;
            }
        }
    }

    // Para de contar ao passar do limite: um documento enorme não custa nada além disso.
    private static int Count(SelectionSetNode selectionSet, IReadOnlyDictionary<string, FragmentDefinitionNode> fragments,
        HashSet<string> visiting, int limit)
    {
        var count = 0;
        foreach (var selection in selectionSet.Selections)
        {
            if (count > limit) break;
            switch (selection)
            {
                case FieldNode field:
                    count++;
                    if (field.SelectionSet is { } children)
                        count += Count(children, fragments, visiting, limit - count);
                    break;
                case InlineFragmentNode inline:
                    count += Count(inline.SelectionSet, fragments, visiting, limit - count);
                    break;
                // Um ciclo de fragmentos já é recusado pelas regras padrão; aqui só não entra em loop.
                case FragmentSpreadNode spread
                    when fragments.TryGetValue(spread.Name.Value, out var fragment) && visiting.Add(spread.Name.Value):
                    count += Count(fragment.SelectionSet, fragments, visiting, limit - count);
                    visiting.Remove(spread.Name.Value);
                    break;
            }
        }
        return count;
    }
}
