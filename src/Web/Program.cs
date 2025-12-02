using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Infrastructure.Repositories;
using CustomerSnapshot.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();

// Services
builder.Services.AddScoped<ICustomerKpiService, CustomerKpiService>();
builder.Services.AddScoped<IPersonalityProfileService, PersonalityProfileService>();
builder.Services.AddScoped<ITalkingPointsService, TalkingPointsService>();
builder.Services.AddScoped<INextBestActionService, NextBestActionService>();
builder.Services.AddScoped<ICustomerSnapshotService, CustomerSnapshotService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=CustomerSnapshot}/{action=Index}/{customerId?}");

app.Run();
