using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
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

// Add or replace an item in the stock
app.MapPut(
    "items/{objectId}", 
    async (
        string objectId, 
        Item item, 
        DaprClient daprClient, 
        CancellationToken cancellationToken) =>
{
    await daprClient.SaveStateAsync<Item>(
        "stocks", 
        objectId,
        item,
        new StateOptions()
        {
            Concurrency = ConcurrencyMode.LastWrite,
            Consistency = ConsistencyMode.Strong
        }, 
        null, 
        cancellationToken);
    return Results.Ok();
});

// Change the quantity of a specific item in the stock
app.MapPut(
    "items/{objectId}/quantity/{quantity:int}",
    async (
        string objectId,
        int quantity,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
    {
        var item = await daprClient.GetStateAsync<Item>(
            "stocks", 
            objectId, 
            ConsistencyMode.Strong, 
            null, 
            cancellationToken);
        if (item is null)
        {
            return Results.NotFound();
        }
        var uodatedItem = item with { Quantity = item.Quantity + quantity };
        await daprClient.SaveStateAsync<Item>(
            "stocks",
            objectId,
            uodatedItem,
            new StateOptions()
            {
                Concurrency = ConcurrencyMode.LastWrite,
                Consistency = ConsistencyMode.Strong
            },
            null,
            cancellationToken);
        return Results.Ok(item);
    });

// Get the stock of a specific item
app.MapGet(
    "items/{objectId}",
    async (
        string objectId,
        DaprClient daprClient,
        CancellationToken cancellationToken) =>
{
    var item = await daprClient.GetStateAsync<Item>(
        "stocks", 
        objectId, 
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

public record Item(string Id, int Quantity);