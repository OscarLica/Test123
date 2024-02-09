using Application;
using Application.Auth.Tokens;
using Application.Auth.Users;
using Application.Common.Interfaces;
using Application.Core.Organizations;
using Application.Core.Products;
using Domain;
using Infrastructure;
using Infrastructure.Common.Services;
using Infrastructure.Core;
using MediatR;
using Microsoft.OpenApi.Models;
using MultiTenant.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddInfrastructure(builder.Configuration);  
builder.Services.AddApplication();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISerializerService, NewtonSoftService>();
builder.Services.AddScoped<IRequestHandler<GetTokenRequest, TokenResponse>, GetTokenRequestHandler>();
builder.Services.AddScoped<IRequestHandler<GetOrganizationsRequest, IEnumerable<Organization>>, GetOrganizationsHandler>();
builder.Services.AddScoped<IRequestHandler<GetProductByIdRequest, Product>, GetProductByIdRequestHandler>();
builder.Services.AddScoped<IRequestHandler<GetUserByIdRequest, UserDetailsDto>, GetUserByIdRequestHandler>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

await app.Services.InitializeDatabasesAsync();

app.UseInfrastructure(builder.Configuration);


app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
