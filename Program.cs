using Microsoft.EntityFrameworkCore;
using AzureSqlWebApiSample.Data;
using AzureSqlWebApiSample.Models;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQL Server
var connString = builder.Configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(connString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger'ı prod'da da aç
app.UseSwagger();
app.UseSwaggerUI();

// Root -> bilgi sayfası veya Swagger'a yönlendir
app.MapGet("/", () => Results.Redirect("/swagger"));
// alternatif istersen: app.MapGet("/", () => "OK — /health, /db-ping, /api/todos");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// DB bağlantısını test eden uç
app.MapGet("/db-ping", async (AppDbContext db) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT 1";
        _ = await cmd.ExecuteScalarAsync();
        return Results.Ok(new { ok = true });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
});

// CRUD (TodoItem)
app.MapGet("/api/todos", async (AppDbContext db) => await db.Todos.OrderByDescending(t => t.Id).ToListAsync());

app.MapGet("/api/todos/{id:int}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is not null ? Results.Ok(todo) : Results.NotFound();
});

app.MapPost("/api/todos", async (TodoItem input, AppDbContext db) =>
{
    input.CreatedUtc = DateTime.UtcNow;
    db.Todos.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{input.Id}", input);
});

app.MapPut("/api/todos/{id:int}", async (int id, TodoItem input, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Title = input.Title;
    todo.IsDone = input.IsDone;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/todos/{id:int}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
