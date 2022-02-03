var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .Build();
});
await using var app = builder.Build();

// Add or replace an item in the stock
app.MapPost(
    "items",
    async (
        string itemId,
        IEnumerable<Item> items,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        foreach (var item in items)
        {
            await daprClient.SaveStateAsync<Item>(
                "stocks",
                item.Id,
                item,
                new StateOptions()
                {
                    Concurrency = ConcurrencyMode.LastWrite,
                    Consistency = ConsistencyMode.Strong
                },
                null,
                cancellationToken);
        }
        return Results.Ok();
    });

// Add or replace an item in the stock
app.MapPut(
    "items/{itemId}", 
    async (
        string itemId, 
        Item item, 
        DaprClient daprClient, 
        CancellationToken cancellationToken) =>
{
    await daprClient.SaveStateAsync<Item>(
        "stocks",
        itemId,
        item,
        new StateOptions()
        {
            Concurrency = ConcurrencyMode.LastWrite,
            Consistency = ConsistencyMode.Strong
        }, 
        null, 
        cancellationToken);
    return Results.Ok(item);
});

// Change the quantity of a specific item in the stock
app.MapPut(
    "items/{itemId}/quantity/{quantity:int}",
    async (
        string itemId,
        int quantity,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        var item = await daprClient.GetStateAsync<Item>(
            "stocks",
            itemId, 
            ConsistencyMode.Strong, 
            null, 
            cancellationToken);
        if (item is null)
        {
            return Results.NotFound();
        }
        item.Quantity += quantity;
        await daprClient.SaveStateAsync<Item>(
            "stocks",
            itemId,
            item,
            new StateOptions()
            {
                Concurrency = ConcurrencyMode.LastWrite,
                Consistency = ConsistencyMode.Strong
            },
            null,
            cancellationToken);
        if (item.Quantity < 1)
        {
            var request = daprClient.CreateInvokeMethodRequest(HttpMethod.Put, "apicatalog-app", $"product/{item.Id}/disable", cancellationToken);
            var response = await daprClient.InvokeMethodWithResponseAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Results.UnprocessableEntity();
            }
        }
        return Results.Ok(item);
    });

// Get the stock of a specific item
app.MapGet(
    "items/{itemId}",
    async (
        string itemId,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
{
    var item = await daprClient.GetStateAsync<Item>(
        "stocks",
        itemId, 
        ConsistencyMode.Strong, 
        null, 
        cancellationToken);
    if (item is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item);
});

await app.RunAsync();

public class Item
{
    public string Id { get; set; }
    public int Quantity { get; set; }
}