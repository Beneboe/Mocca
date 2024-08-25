using Microsoft.AspNetCore.Builder;
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

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        // builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddSwaggerGen();

        builder.Services.AddMocca(options =>
        {
            builder.Configuration.GetSection("Mocca").Bind(options);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        // app.UseHttpsRedirection();
        //
        // app.UseAuthorization();
        
        app
            .UseMoccaScribe()
            .UseMoccaProxy();

        app.Run();
    }
}