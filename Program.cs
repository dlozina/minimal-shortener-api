var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(_ => new LiteDatabase("shorten-service.db"));
var app = builder.Build();

Hashids _hashIds = new Hashids("This is my shortener", 6);

app.MapPost("/shorten", (Url url, ILiteDatabase _context) =>
{
    var db = _context.GetCollection<Url>(BsonAutoId.Int32);
    var id = db.Insert(url);
    return Results.Created("ShortURL: ", _hashIds.Encode(id));
});

app.MapGet("/{shortUrl}", (string shortUrl, ILiteDatabase _context) =>
{
    var id = _hashIds.Decode(shortUrl);
    var tempId = id[0];
    var db = _context.GetCollection<Url>();
    var entry = db.Query().Where(x => x.Id.Equals(tempId)).ToList().FirstOrDefault();
    if (entry != null) return Results.Ok(entry.longUrl);
    return Results.NoContent();
});

app.Run("http://localhost:4000");

public record Url(int Id, string longUrl);
