using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapPost("checkout", async (Basket basket, CancellationToken cancellationToken) =>
{
    using var client = new DaprClientBuilder().Build();
    foreach (var item in basket.Items)
    {
        var request = client.CreateInvokeMethodRequest(HttpMethod.Get, "stocks", "items/" + item.Id, cancellationToken);
        var response = await client.InvokeMethodWithResponseAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return Results.UnprocessableEntity(); 
        }
    }
    await client.PublishEventAsync("events", "order-accepted", basket, cancellationToken);
    return Results.Ok();
});

await app.RunAsync();

public record Item(string Id, int Quantity);
public record Basket(string Id, IEnumerable<Item> Items);