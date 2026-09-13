using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Parkking.Api.Mapping;
using Parkking.Infrastructure.Authentication;
using Parkking.Infrastructure.Persistence;
using Parkking.Infrastructure.Security;
using Parkking.Infrastructure.Tenant;
using Parkking.Repositories;
using Parkking.Services;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

MapsterConfig.Register();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IEstacionamientoContext, TenantProvider>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<AbonoRepository>();
builder.Services.AddScoped<CocheraRepository>();
builder.Services.AddScoped<CajaMensualRepository>();
builder.Services.AddScoped<PagoRepository>();
builder.Services.AddScoped<ReciboRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<TarifaMensualRepository>();
builder.Services.AddScoped<TipoVehiculoRepository>();
builder.Services.AddScoped<MetodoDePagoRepository>();
builder.Services.AddScoped<GrupoRepository>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<EstacionamientoRepository>();
builder.Services.AddScoped<EstacionamientoService>();

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<AbonoService>();
builder.Services.AddScoped<AbonoPrecioService>();
builder.Services.AddScoped<CocheraService>();
builder.Services.AddScoped<CajaMensualService>();
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<ReciboService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<TarifaMensualService>();
builder.Services.AddScoped<TipoVehiculoService>();
builder.Services.AddScoped<MetodoDePagoService>();
builder.Services.AddScoped<GrupoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReportesService>();

builder.Services.AddDbContext<EstacionamientoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
