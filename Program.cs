using Microsoft.EntityFrameworkCore;
using AzureSqlWebApiSample.Data;
using AzureSqlWebApiSample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core DbContext with SQL Server
var connString = builder.Configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(connString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-create database schema (demo purpose only). In production, use migrations.

app.MapGet("/db-ping", async (AzureSqlWebApiSample.Data.AppDbContext db) =>
{
    try
    {
        await db.Database.OpenConnectionAsync();
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT 1";
        var _ = await cmd.ExecuteScalarAsync();
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

//using (var scope = app.Services.CreateScope())
// {
   // var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  //  await db.Database.EnsureCreatedAsync();
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// CRUD endpoints for TodoItem
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
