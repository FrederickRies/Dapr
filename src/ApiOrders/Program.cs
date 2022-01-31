using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .Build();
});
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapPost("checkout", async (Basket basket, DaprClient daprClient, CancellationToken cancellationToken) =>
{
    foreach (var item in basket.Items)
    {
        var request = daprClient.CreateInvokeMethodRequest(HttpMethod.Get, "stocks", "items/" + item.Id, cancellationToken);
        var response = await daprClient.InvokeMethodWithResponseAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return Results.UnprocessableEntity(); 
        }
    }
    await daprClient.PublishEventAsync("events", "order-accepted", basket, cancellationToken);
    return Results.Ok();
});

await app.RunAsync();

public record Item(string Id, int Quantity);
public record Basket(string Id, IEnumerable<Item> Items);