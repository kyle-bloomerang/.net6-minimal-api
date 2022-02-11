using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

#region setup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AlbumDb>(opt => opt.UseInMemoryDatabase("AlbumList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
});
var app = builder.Build();
#endregion

#region Endpoints
app.MapGet("/", () => "Hello World!");
app.MapGet("/albums", async (AlbumDb db) => await db.Albums.ToListAsync());
app.MapGet("/albums/{id}", async (int id, AlbumDb db) =>
    await db.Albums.FindAsync(id)
        is Album album
        ? Results.Ok(album)
        : Results.NotFound());
app.MapPost("/albums", async (Album album, AlbumDb db) =>
{
    db.Albums.Add(album);
    await db.SaveChangesAsync();

    return Results.Created($"/albums/{album.Id}", album);
});

app.MapPut("/albums/{id}", async (int id, Album inputAlbum, AlbumDb db) =>
{
    var album = await db.Albums.FindAsync(id);

    if (album is null) return Results.NotFound();

    album.Title = inputAlbum.Title;
    album.Artist = inputAlbum.Artist;
    album.Price = inputAlbum.Price;

    await db.SaveChangesAsync();

    return Results.Ok(album);
});

app.MapDelete("/albums/{id}", async (int id, AlbumDb db) =>
{
    if (await db.Albums.FindAsync(id) is Album album)
    {
        db.Albums.Remove(album);
        await db.SaveChangesAsync();
        return Results.Ok(album);
    }

    return Results.NotFound();
});
#endregion

app.Run();

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public double Price { get; set; }
}
class AlbumDb : DbContext
{
    public AlbumDb(DbContextOptions<AlbumDb> options)
        : base(options)
    {
        if (!Albums.Any())
        {
            var albums = new Album[]
            {
                new Album() { Id = 1, Title = "Blue Train", Artist = "John Coltrane", Price = 56.99 },
                new Album() { Id = 2, Title = "Jeru", Artist = "Gerry Mulligan", Price = 17.99 },
                new Album() { Id = 3, Title = "Sarah Vaughan and Clifford Brown", Artist = "Sarah Vaughan", Price = 39.99 },
            };
            AddRange(albums);
            SaveChanges();
        }
    }

    public DbSet<Album> Albums => Set<Album>();
}