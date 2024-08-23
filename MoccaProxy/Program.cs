using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MoccaProxy.Interfaces;
using MoccaProxy.Services;

namespace MoccaProxy;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure options
        builder.Services.Configure<MoccaOptions>(
            builder.Configuration.GetSection("Mocca"));
        
        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddHttpClient("ProxyClient", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MoccaOptions>>().Value;
            client.BaseAddress = new Uri(options.ForwardTo);
        });

        builder.Services.AddScoped<IMoccaRepository, MoccaJsonRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // app.UseHttpsRedirection();
        //
        // app.UseAuthorization();

        app.UseMoccaScribe();

        app.UseMoccaProxy();

        app.Run();
    }
}