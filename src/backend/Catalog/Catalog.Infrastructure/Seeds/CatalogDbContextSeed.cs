using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Seeds;

public static class CatalogDbContextSeed
{
    private const int TotalProducts = 1000;

    public static async Task Seed(this ApplicationCatalogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();

        // Insere só os produtos que faltam, comparando pelo SKU: um banco
        // criado com menos produtos é completado sem duplicar os existentes.
        var existingSkus = (await dbContext.Products.Select(x => x.Sku).ToListAsync()).ToHashSet();
        var missing = GetProducts().Where(x => !existingSkus.Contains(x.Sku)).ToList();
        if (missing.Count == 0) return;

        await dbContext.Products.AddRangeAsync(missing);
        await dbContext.SaveChangesAsync();
    }

    private static List<Product> GetProducts()
    {
        var featured = GetFeaturedProducts();
        return [.. featured, .. GenerateProducts(featured, TotalProducts - featured.Count)];
    }

    // Variações dos produtos em destaque, com a foto e a descrição do produto
    // base. O Random tem semente fixa, então o seed gera sempre os mesmos dados.
    // As combinações de nome (12 x 10 x 9 = 1080) não se repetem até 1080 produtos.
    private static IEnumerable<Product> GenerateProducts(IReadOnlyList<Product> featured, int count)
    {
        string[] brands = ["Air", "Urban", "Volt", "Zenith", "Nova", "Pulse", "Core", "Aero", "Swift", "Terra", "Metro", "Apex"];
        string[] models = ["Runner", "Court", "Trail", "Low", "High", "Flex", "Glide", "Pro", "Lite", "Max"];
        string[] colors = ["Black", "White", "Navy", "Red", "Grey", "Olive", "Sand", "Blue", "Green"];
        var random = new Random(42);

        for (var i = 0; i < count; i++)
        {
            var baseProduct = featured[i % featured.Count];
            var color = colors[i / (brands.Length * models.Length) % colors.Length];
            var name = $"{brands[i % brands.Length]} {models[i / brands.Length % models.Length]} {color}";
            var sku = $"SKU-{featured.Count + i + 1:D4}";
            var price = random.Next(49, 200);

            yield return new Product(name, sku, baseProduct.PictureUri, price,
                $"{color} edition. {baseProduct.Description}");
        }
    }

    // Mesmos produtos que o front-end usava no data/products.json, na mesma ordem:
    // assim os ids gerados (1 a 10) batem com carrinhos já salvos no navegador.
    private static List<Product> GetFeaturedProducts()
    {
        return
        [
            new Product("AirFlex Runner", "SKU-0001",
                "https://images.unsplash.com/photo-1579338559194-a162d19bf842?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                89m, "Lightweight running sneakers designed for speed and comfort. Breathable mesh and durable sole."),
            new Product("Urban Street Pro", "SKU-0002",
                "https://images.unsplash.com/photo-1608667508764-33cf0726b13a?q=80&w=880&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                99m, "Minimalist sneakers for everyday wear. Premium leather with a modern urban look."),
            new Product("Classic Court 90s", "SKU-0003",
                "https://images.unsplash.com/photo-1465453869711-7e174808ace9?q=80&w=1176&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                79m, "Retro-inspired sneakers with a tennis court vibe. Perfect balance between comfort and style."),
            new Product("Volt Edge", "SKU-0004",
                "https://images.unsplash.com/photo-1512374382149-233c42b6a83b?q=80&w=735&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                119m, "Performance sneakers with bold details. Responsive cushioning for all-day energy."),
            new Product("Zenith Flow", "SKU-0005",
                "https://images.unsplash.com/photo-1608231387042-66d1773070a5?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                129m, "Premium lifestyle sneakers blending high-quality knit material and futuristic design."),
            new Product("Street Vibe Low", "SKU-0006",
                "https://images.unsplash.com/photo-1511556532299-8f662fc26c06?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                69m, "Casual low-top sneakers with a timeless silhouette. Built for versatility and comfort."),
            new Product("Nova Horizon", "SKU-0007",
                "https://images.unsplash.com/photo-1516767254874-281bffac9e9a?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                109m, "High-top sneakers crafted with suede and mesh. Perfect mix of streetwear and performance."),
            new Product("Pulse React", "SKU-0008",
                "https://images.unsplash.com/photo-1560769629-975ec94e6a86?q=80&w=764&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                99m, "Dynamic sneakers with responsive cushioning. Designed for training and everyday comfort."),
            new Product("Core Street Retro", "SKU-0009",
                "https://images.unsplash.com/photo-1621315271772-28b1f3a5df87?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                85m, "Old-school sneakers inspired by 80s basketball. Durable construction with vintage vibes."),
            new Product("AeroFlex Lite", "SKU-0010",
                "https://images.unsplash.com/photo-1496202703211-aa28e9500c30?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                75m, "Ultra-light sneakers designed for everyday mobility. Breathable and flexible design.")
        ];
    }
}
