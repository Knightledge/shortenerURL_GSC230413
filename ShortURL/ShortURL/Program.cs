using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using ShortURL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Enable controllers with views so MVC views are served
builder.Services.AddControllersWithViews();

// Configure the database context. We are using SQLite for simplicity.
// The connection string "Data Source=urlshortener.db" will create a file in the project root.
builder.Services.AddDbContext<URLDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("URLConnection")));

// Add Swagger/OpenAPI support for API documentation.
// This is helpful for testing and understanding your API endpoints.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // In development, use Swagger UI to browse the API.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Serve static files (css/js)
app.UseStaticFiles();

app.UseRouting();

// Enables authorization middleware.
app.UseAuthorization();

// Map default MVC route for views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Minimal API endpoint handling the short redirect at /r/{code}
app.MapGet("/r/{code}", async (string code, [FromServices] URLDbContext context) =>
{
    // Find the original URL in the database based on the provided code.
    var url = await context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);

    // If the code is not found, return a 404 Not Found response.
    if (url == null)
    {
        return Results.NotFound();
    }

    // Update click metrics
    url.ClickCount += 1;
    url.LastAccessedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();

    // If found, redirect the user's browser to the original URL.
    return Results.Redirect(url.OriginalUrl);
});

// Automatically apply database migrations on startup. This is great for development environments.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<URLDbContext>();
    dbContext.Database.Migrate();
}

app.Run();