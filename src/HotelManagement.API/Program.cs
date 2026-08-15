using HotelManagement.Infrastructure.Data;
using HotelManagement.Infrastructure.Repositories;
using HotelManagement.Application.Interfaces;
using HotelManagement.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using HotelManagement.API.Exceptions;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Lines for Swagger Ui
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Exception Handling services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//Connecting Sql Server from connectionstring
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
//Room repository and services
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

var app = builder.Build();

app.UseExceptionHandler();

//Lines for Swagger Ui
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.MapControllers();


app.Run();