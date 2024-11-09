using Microsoft.EntityFrameworkCore;
using WorldviewShareServer.Data;
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
        });
        
        builder.Services.AddDbContext<WorldviewShareContext>(options =>
        {
            options.UseSqlite(@"Data Source=Data/WorldviewShareData.db");
        });
        
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
        
        app.Run();
    }
}