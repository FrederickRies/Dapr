var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .Build();
});
await using var app = builder.Build();

// Initialise the product catalog
app.MapPost(
    "products",
    async (
        IEnumerable<Product> products,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        foreach (var product in products)
        {
            await daprClient.SaveStateAsync<Product>(
                "catalog",
                product.Id,
                product,
                null,
                null,
                cancellationToken);
        }
        return Results.Ok();
    });

// Disable a product
app.MapPut(
    "products/{productId}/disable",
    async (
        string productId,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        var product = await daprClient.GetStateAsync<Product>(
            "catalog",
            productId,
            ConsistencyMode.Strong,
            null,
            cancellationToken);
        if (product is null)
        {
            return Results.NotFound();
        }
        product.IsDisable = true;
        await daprClient.SaveStateAsync<Product>(
            "catalog",
            productId,
            product,
            null,
            null,
            cancellationToken);
        return Results.Ok(product);
    });

// Get a product
app.MapGet(
    "products/{productId}",
    async (
        string productId,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        var product = await daprClient.GetStateAsync<Product>(
            "catalog",
            productId,
            null,
            null,
            cancellationToken);
        if (product is null)
        {
            return Results.NotFound();
        }
        return Results.Ok(product);
    });

await app.RunAsync();

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public bool IsDisable { get; set; }
}