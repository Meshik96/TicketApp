using Application.Services.Events;
using Application.Services.Seats;
using Application.Services.Reservations;
using Application.Interfaces.Persistence.Events;
using Application.Interfaces.Persistence.Seats;
using Application.Interfaces.Persistence.Reservations;
using Application.Interfaces.Services;
using Infrastructure.Persistence.Events;
using Infrastructure.Persistence.Seats;
using Infrastructure.Persistence.Reservations;
using Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==================== PERSISTENCE LAYER ====================
builder.Services.AddScoped<IEventQueries, EventQueries>();
builder.Services.AddScoped<ISeatQueries, SeatQueries>();
builder.Services.AddScoped<IReservationCommands, ReservationCommands>();

// ==================== APPLICATION LAYER ====================
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
