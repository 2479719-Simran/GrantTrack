using System.Text;
using GrantTrack.Domain.Entities;
using GrantTrack.Repository;
using GrantTrack.Repository.ApplicationRepositories;
using GrantTrack.Repository.Interface;
using GrantTrack.Repository.ProgramRepository;
using GrantTrack.Service;
using GrantTrack.Service.ApplicationServices;
using GrantTrack.Service.AuthServices;
using GrantTrack.Service.Interfaces;
using GrantTrack.Service.ProgramServices;
using GrantTrack.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using DotNetEnv;
// using Microsoft.OpenApi.Models; // Change this
// using Microsoft.AspNetCore.Authentication.JwtBearer; // Ensure this is present
Env.Load();
var DefaultConnection = Environment.GetEnvironmentVariable("DefaultConnection");
Console.WriteLine(DefaultConnection);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<GrantTrackDbContext>(options => options.UseSqlServer(
DefaultConnection));

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IProgramRepository, ProgramRepository>();

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authentication using Bearer scheme"
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {new OpenApiSecuritySchemeReference("Bearer",doc),new List<string>()}
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
