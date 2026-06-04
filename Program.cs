using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MODELO.Contexto;
using Parkking_backend.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch(
    "Npgsql.EnableLegacyTimestampBehavior",
    true
);

// Necesario para IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Servicios
// Contexto
builder.Services.AddScoped<IEstacionamientoContext, EstacionamientoContextService>();

// Utilitarios
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<UserService>();

// Negocio
builder.Services.AddScoped<AbonoCocheraService>();
builder.Services.AddScoped<CajaMensualService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<CocheraService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EstacionamientoService>();
builder.Services.AddScoped<GrupoService>();
builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<TarifaMensualService>();
builder.Services.AddScoped<TipoVehiculoService>();
// DbContext
builder.Services.AddDbContext<EstacionamientoContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token =
                    context.Request.Cookies["access_token"];

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey!)
                    ),

                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");

app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllers();

app.Run();