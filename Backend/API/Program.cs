using Application.Services.Events;
using Application.Services.Seats;
using Application.Services.Reservations;
using Application.Services.Users;
using Application.Interfaces.Persistence.Events;
using Application.Interfaces.Persistence.Seats;
using Application.Interfaces.Persistence.Reservations;
using Application.Interfaces.Services;
using Application.Interfaces.Persistence.Users;
using Infrastructure.Persistence.Events;
using Infrastructure.Persistence.Seats;
using Infrastructure.Persistence.Reservations;
using Infraestructure.Persistence.Users;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



builder.Services.AddScoped<IUserService, UserService>();

// Database
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==================== PERSISTENCE LAYER ====================
builder.Services.AddScoped<IEventQueries, EventQueries>();
builder.Services.AddScoped<ISeatQueries, SeatQueries>();
builder.Services.AddScoped<IReservationCommands, ReservationCommands>();


// USERS
builder.Services.AddScoped<IUserQueries, UserQueries>();
builder.Services.AddScoped<IUserCommands, UserCommands>();



// ==================== APPLICATION LAYER ====================
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// USERS
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // MIDDLEWARE X-API-VERSION
    app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Api-version", "1.0");
    await next();
});
}

app.UseHttpsRedirection();
app.UseAuthorization();
//CORS ACTIVACIÓN
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
