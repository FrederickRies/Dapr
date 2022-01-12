using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("items/{objectId}", (string objectId) => new Item(objectId, "abcd", 4));

await app.RunAsync();

public record Item(string Id, string Code, int Quantity);