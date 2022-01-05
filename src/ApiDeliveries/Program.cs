global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Hosting;
global using System;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapPost("/", (Func<string>)(() => "Hello World!"));
app.MapPut("/{id}", async (Guid id) =>
{
    return new StatusCodeResult(StatusCodes.Status404NotFound);
});
app.MapDelete("/{id}", async (Guid id) =>
{
    return new StatusCodeResult(StatusCodes.Status404NotFound);
});

await app.RunAsync();

public record Delivery(string Name, string Address, DateOnly Date);
