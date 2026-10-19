namespace Gateway.Web.Api.GraphQL;

/// <summary>
/// Limites das consultas que chegam ao gateway (seção <c>GraphQL</c>). Uma consulta acima deles é recusada
/// na validação, antes de o gateway chamar qualquer API.
/// </summary>
public sealed class GatewayLimits
{
    public const string Section = "GraphQL";

    /// <summary>Profundidade máxima da consulta (campos aninhados).</summary>
    public int MaxDepth { get; init; } = 8;

    /// <summary>
    /// Custo máximo, medido em campos por operação, com os fragmentos expandidos. O Fusion 16 não calcula
    /// custo no gateway; sem este limite, uma consulta cara só seria barrada pelo subgraph, já encaminhada.
    /// </summary>
    public int MaxFields { get; init; } = 200;

    public static GatewayLimits From(IConfiguration configuration)
    {
        var limits = configuration.GetSection(Section).Get<GatewayLimits>() ?? new GatewayLimits();
        if (limits.MaxDepth <= 0 || limits.MaxFields <= 0)
            throw new InvalidOperationException($"{Section}:MaxDepth e {Section}:MaxFields devem ser positivos.");
        return limits;
    }
}
