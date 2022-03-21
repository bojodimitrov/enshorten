using Enshorten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IUrlShortenHandler, EnshortenHandler>();
builder.Services.AddTransient<IEnshortenRepository, EnshortenRepository>();
builder.Services.AddTransient<GetFullUrlQuery>();
builder.Services.AddTransient<InsertShortenedUrlCommand>();
builder.Services.AddTransient<OpenAddressingProbingQuery>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("", async (HttpContext context, IUrlShortenHandler urlShortener) =>
{
    var url = await ReadRawDataAsync(context);

    var result = await urlShortener.ShortenUrlAsync(url);

    return $"{app.Urls.First()}/{result}";
});

app.MapGet("/{shortenedUrl}", async (string shortenedUrl, IUrlShortenHandler urlShortener) =>
{
    return Results.Redirect(await urlShortener.GetShortenedUrlAsync(shortenedUrl));
});

app.Run();

async Task<string> ReadRawDataAsync(HttpContext context)
{
    string body;
    using (var streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8))
    {
        body = await streamReader.ReadToEndAsync();
    }

    return body;
}