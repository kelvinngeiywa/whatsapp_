using Scalar.AspNetCore;
using whatsapp_cloud_api.Services;
using whatsapp_cloud_api.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<IHubSignature, XHubSignature>();
builder.Services.AddScoped<IWhatsAppMessage,WhatsAppMessageService>();
builder.Services.AddScoped<IMessageProcessing,MessageProcessing>();
builder.Services.AddScoped<IMpesa, MpesaServices>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
