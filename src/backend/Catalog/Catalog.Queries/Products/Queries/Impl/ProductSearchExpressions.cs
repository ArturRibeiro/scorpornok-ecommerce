namespace Catalog.Queries.Products.Queries.Impl;

// Monta, palavra a palavra, as expressões da busca sobre um único parâmetro, para o EF traduzi-las
// numa consulta só: cada palavra vira ILIKE '%palavra%' sobre unaccent do campo, sem diferenciar
// maiúsculas e acentos dos dois lados.
internal static class ProductSearchExpressions
{
    private const string Escape = "\\";

    private static readonly ParameterExpression Product = Expression.Parameter(typeof(Product), "x");

    // Os grupos da ordem: todas as palavras no nome, todas no nome ou na marca, e o resto (descrição).
    public const int InName = 0;
    public const int InNameOrBrand = 1;
    public const int InDescription = 2;

    public static Expression<Func<Product, bool>> Filter(IReadOnlyList<string> words)
        => Lambda<bool>(AllWords(words, AllFields));

    // Sem palavras, todos ficam no mesmo grupo, e vale a ordem do catálogo.
    public static Expression<Func<Product, int>> Rank(IReadOnlyList<string> words)
    {
        if (words.Count == 0) return Lambda<int>(Expression.Constant(InName));

        return Lambda<int>(Expression.Condition(AllWords(words, NameOnly), Expression.Constant(InName),
            Expression.Condition(AllWords(words, NameAndBrand), Expression.Constant(InNameOrBrand),
                Expression.Constant(InDescription))));
    }

    private static readonly Expression<Func<Product, string>>[] NameOnly = [x => x.Name];
    private static readonly Expression<Func<Product, string>>[] NameAndBrand = [x => x.Name, x => x.Brand];
    private static readonly Expression<Func<Product, string>>[] AllFields = [x => x.Name, x => x.Brand, x => x.Description];

    // Cada palavra precisa aparecer em algum dos campos.
    private static Expression AllWords(IReadOnlyList<string> words, Expression<Func<Product, string>>[] fields)
        => words
            .Select(word => fields.Select(field => Matches(field, Pattern(word))).Aggregate(Expression.OrElse))
            .Aggregate((Expression)Expression.Constant(true), Expression.AndAlso);

    private static Expression Matches(Expression<Func<Product, string>> field, string pattern)
    {
        // O padrão vai como membro de um objeto, para o EF o enviar como parâmetro da consulta.
        var value = Expression.Property(Expression.Constant(new SearchPattern(pattern)), nameof(SearchPattern.Value));
        Expression<Func<string, string, bool>> like =
            (text, p) => EF.Functions.ILike(EF.Functions.Unaccent(text), EF.Functions.Unaccent(p), Escape);
        var fieldBody = new ReplaceParameter(field.Parameters[0], Product).Visit(field.Body);
        return new ReplaceParameter(like.Parameters[1], value).Visit(
            new ReplaceParameter(like.Parameters[0], fieldBody).Visit(like.Body));
    }

    // %, _ e a barra do termo valem como texto, não como curinga.
    private static string Pattern(string word)
        => "%" + word.Replace(Escape, Escape + Escape).Replace("%", Escape + "%").Replace("_", Escape + "_") + "%";

    private static Expression<Func<Product, T>> Lambda<T>(Expression body)
        => Expression.Lambda<Func<Product, T>>(body, Product);

    private sealed record SearchPattern(string Value);

    private sealed class ReplaceParameter(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
