using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleBlog.API.Data;
using System.Net.Http.Json;
using Xunit;

public class PostsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PostsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BlogContext>));
                if (descriptor != null) services.Remove(descriptor);
                services.AddDbContext<BlogContext>(options =>
                {
                    options.UseInMemoryDatabase("TestBlog");
                });
            });
        });
    }

    [Fact]
    public async Task GetPosts_ReturnsPosts()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/posts");
        response.EnsureSuccessStatusCode();
        var posts = await response.Content.ReadFromJsonAsync<List<Post>>();
        Assert.NotNull(posts);
        Assert.True(posts.Count >= 2);
    }

    [Fact]
    public async Task GetPost_ExistingId_ReturnsPost()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/posts/1");
        response.EnsureSuccessStatusCode();
        var post = await response.Content.ReadFromJsonAsync<Post>();
        Assert.NotNull(post);
        Assert.Equal("Добро пожаловать в блог!", post.Title);
    }

    [Fact]
    public async Task CreatePost_ValidPost_CreatesSuccessfully()
    {
        var client = _factory.CreateClient();
        var newPost = new Post { Title = "Новый пост", Content = "Содержание", Author = "Тест" };
        var response = await client.PostAsJsonAsync("/api/posts", newPost);
        response.EnsureSuccessStatusCode();
        var createdPost = await response.Content.ReadFromJsonAsync<Post>();
        Assert.NotNull(createdPost);
        Assert.Equal("Новый пост", createdPost.Title);
    }
}
