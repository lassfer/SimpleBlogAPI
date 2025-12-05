using Microsoft.EntityFrameworkCore;
using SimpleBlog.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BlogContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        options.UseInMemoryDatabase("TestBlog");
    else
        options.UseSqlite("Data Source=blog.db");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlogContext>();
    context.Database.EnsureCreated();
}

app.Run();

public partial class Program {}
