using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
using WorldviewShareServer.Hubs;
using WorldviewShareServer.Services;
namespace WorldviewShareServer;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SupportNonNullableReferenceTypes();
            options.AddSignalRSwaggerGen();
        });
        
        builder.Services.AddDbContext<WorldviewShareContext>(options =>
        {
            options.UseSqlite(@"Data Source=Data/WorldviewShareData.db");
        });
        
        builder.Services.AddSignalR(options =>
        {
            options.HandshakeTimeout = TimeSpan.FromSeconds(60*60*4);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(60*60*4);
        });

        builder.Services.AddSingleton<ChatHubCache>();
        
        builder.Services.AddScoped<UsersService>();
        builder.Services.AddScoped<TopicSessionsService>();
        builder.Services.AddScoped<MessagesService>();
        
        builder.Services.AddControllers();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.MapControllers();
        
        app.MapHub<ChatHub>("messages");
        
        app.Run();
    }
}