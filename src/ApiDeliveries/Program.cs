var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .Build();
});
await using var app = builder.Build();

await app.RunAsync();

