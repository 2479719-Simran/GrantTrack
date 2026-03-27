using GrantTrack.Repository;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GrantTrack.Repository.Interface;
using GrantTrack.Service;
using GrantTrack.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddDbContext<GrantTrackDbContext>(options => options.UseSqlServer(
builder.Configuration.GetConnectionString("GrantTrackConnectionString")));   

builder.Services.AddOpenApi();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();




app.Run();
