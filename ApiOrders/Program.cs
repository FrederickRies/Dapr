using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/items/{item-id}", async (string itemId, CancellationToken cancellationToken) =>
{
    using var client = new DaprClientBuilder().Build();
    var result = client.CreateInvokeMethodRequest(HttpMethod.Get, "stocks", "items/" + itemId, cancellationToken);
    await client.InvokeMethodAsync(result);

});

await app.RunAsync();
