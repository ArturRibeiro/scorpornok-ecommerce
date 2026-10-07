using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Bogus;
using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Seeds;

public static class CatalogDbContextSeed
{
    private const int TotalProducts = 1000;

    private static readonly string[] Models = ["Runner", "Court", "Trail", "Low", "High", "Flex", "Glide", "Pro", "Lite", "Max"];

    // O f.Commerce.ProductMaterial() do Bogus gera "Wooden", "Concrete" etc.
    private static readonly string[] Materials = ["leather", "suede", "mesh", "canvas", "knit", "nubuck", "synthetic leather"];

    // Fotos de tênis do Unsplash, as mesmas do template do front-end.
    private static readonly string[] Pictures =
    [
        "https://images.unsplash.com/photo-1579338559194-a162d19bf842?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1608667508764-33cf0726b13a?q=80&w=880&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1465453869711-7e174808ace9?q=80&w=1176&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1512374382149-233c42b6a83b?q=80&w=735&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1608231387042-66d1773070a5?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1511556532299-8f662fc26c06?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1516767254874-281bffac9e9a?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1560769629-975ec94e6a86?q=80&w=764&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1621315271772-28b1f3a5df87?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        "https://images.unsplash.com/photo-1496202703211-aa28e9500c30?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
    ];

    public static async Task Seed(this ApplicationCatalogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        if (await dbContext.Products.AnyAsync()) return;

        await dbContext.Products.AddRangeAsync(GetProducts());
        await dbContext.SaveChangesAsync();
    }

    // UseSeed deixa o Bogus determinístico: o seed gera sempre os mesmos produtos.
    private static List<Product> GetProducts()
    {
        var titleCase = CultureInfo.InvariantCulture.TextInfo;

        return new Faker<Product>()
            .UseSeed(42)
            .CustomInstantiator(f =>
            {
                var color = titleCase.ToTitleCase(f.Commerce.Color());
                var material = f.PickRandom(Materials);

                return new Product(
                    $"{f.Commerce.ProductAdjective()} {color} {f.PickRandom(Models)}",
                    $"SKU-{f.IndexFaker + 1:D4}",
                    f.PickRandom(Pictures),
                    f.Random.Int(49, 199),
                    $"{color} sneakers made of {material}. {f.Company.CatchPhrase()}.");
            })
            .Generate(TotalProducts);
    }
}
