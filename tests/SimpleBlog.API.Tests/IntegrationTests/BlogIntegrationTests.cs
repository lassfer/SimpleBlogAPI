using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleBlog.API.Data;
using System.Net.Http.Json;
using Xunit;

public class BlogIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BlogIntegrationTests(WebApplicationFactory<Program> factory)
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
    public async Task FullFlow_CreatePostAndAddComment_WorksCorrectly()
    {
        var client = _factory.CreateClient();

        var newPost = new Post { 
            Title = "Интеграционный тест", 
            Content = "Тестируем поток", 
            Author = "Тестер" 
        };
        var postResponse = await client.PostAsJsonAsync("/api/posts", newPost);
        postResponse.EnsureSuccessStatusCode();
        var createdPost = await postResponse.Content.ReadFromJsonAsync<Post>();

        var newComment = new Comment { 
            Author = "Читатель", 
            Content = "Отличный пост!" 
        };
        var commentResponse = await client.PostAsJsonAsync(
            $"/api/posts/{createdPost.Id}/comments", newComment);
        commentResponse.EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"/api/posts/{createdPost.Id}");
        var postWithComments = await getResponse.Content.ReadFromJsonAsync<Post>();

        Assert.NotNull(postWithComments);
        Assert.Single(postWithComments.Comments);
        Assert.Equal("Отличный пост!", postWithComments.Comments[0].Content);
    }
}
