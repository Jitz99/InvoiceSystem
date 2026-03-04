using InvoiceSystem.Api.Middleware;
using InvoiceSystem.Application.Interfaces;
using InvoiceSystem.Application.Services;
using InvoiceSystem.Infrastructure.Persistence;
using InvoiceSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
//builder.Services.AddDbContext<InvoiceDbContext>(options =>
//	options.UseSqlite("Data Source=invoices.db"));

//builder.Services.AddScoped<IInvoiceRepository, SqliteInvoiceRepository>();
builder.Services.AddScoped<IInvoiceService,InvoiceService>();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
//app.UseHttpsRedirection();
app.MapControllers();

app.Run();