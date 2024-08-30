using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Mocca;

namespace MoccaProxy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //
        // // Add services to the container.
        // builder.Services.AddAuthorization();

        builder.Services.AddMocca(options =>
        {
            builder.Configuration.GetSection("Mocca").Bind(options);
        });

        var app = builder.Build();

        // app.UseHttpsRedirection();
        //
        // app.UseAuthorization();

        app.MapGet("/weatherforecast", () =>
        {
            return Task.FromResult(Results.Ok("Success"));
        });
        
        app
            .UseMoccaScribe()
            .UseMoccaOverwrite()
            .UseMoccaProxy();

        app.Run();
    }
    
}