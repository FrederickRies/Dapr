var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .Build();
});
await using var app = builder.Build();

app.MapPost("checkout", async (Basket basket, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    foreach (var item in basket.Items)
    {
        var request = daprClient.CreateInvokeMethodRequest(HttpMethod.Get, "apistocks-app", "items/" + item.Id, cancellationToken);
        var response = await daprClient.InvokeMethodWithResponseAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return Results.UnprocessableEntity(); 
        }
    }
    await daprClient.PublishEventAsync("orders", "order-accepted", basket, cancellationToken);
    return Results.Ok();
});

await app.RunAsync();

public record Item(string Id, int Quantity);
public record Basket(string Id, IEnumerable<Item> Items);