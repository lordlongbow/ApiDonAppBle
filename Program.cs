using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ApiDonAppBle.Models;
using System.Text;
using ApiDonAppBle.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));

//, "https://*:5001"
builder.WebHost.UseUrls("http://localhost:5156", "http://*:5156");

builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// Configurar autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($" Error de autenticación: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine(" Token JWT validado correctamente.");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($" Token recibido: {context.Token}");
                return Task.CompletedTask;
            }
        };
    });

// Configurar autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Agregar servicios de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API DonAppBle", Version = "v1" });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddSignalR();


var app = builder.Build();

// Configurar el middleware de la aplicación.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapControllers();

//Cors que permita todos los orgigenes
app.UseCors("AllowAllOrigins");


//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
