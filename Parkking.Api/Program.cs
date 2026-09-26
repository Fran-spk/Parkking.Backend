using System.Text;
using DotNetEnv;
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
using Parkking.Services.Mensajeria;
using Parkking.Services.Documentos;

var builder = WebApplication.CreateBuilder(args);

// .env junto al csproj (ContentRoot). Env.Load + re-scan para que Email:*
// pise appsettings aunque el cwd no sea Parkking.Api.
var envFile = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envFile))
{
    Env.Load(envFile);
    builder.Configuration.AddEnvironmentVariables();
}

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

MapsterConfig.Register();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IEstacionamientoContext, TenantProvider>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<AbonoRepository>();
builder.Services.AddScoped<CocheraRepository>();
builder.Services.AddScoped<PagoRepository>();
builder.Services.AddScoped<ReciboRepository>();
builder.Services.AddScoped<MensajeRepository>();
builder.Services.AddScoped<DocumentoRepository>();
builder.Services.AddScoped<CategoriaRepository>();
builder.Services.AddScoped<TarifaMensualRepository>();
builder.Services.AddScoped<TipoVehiculoRepository>();
builder.Services.AddScoped<MetodoDePagoRepository>();
builder.Services.AddScoped<GrupoFinancieroRepository>();
builder.Services.AddScoped<ReglaAsignacionRepository>();
builder.Services.AddScoped<TipoGastoRepository>();
builder.Services.AddScoped<FinanzasMovimientoRepository>();
builder.Services.AddScoped<CuentaCorrienteRepository>();
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
    builder.Services.AddScoped<ContratoService>();
builder.Services.AddScoped<CocheraService>();
builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<ReciboService>();
builder.Services.AddScoped<MensajeService>();
builder.Services.AddScoped<DocumentoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<TarifaMensualService>();
builder.Services.AddScoped<TipoVehiculoService>();
builder.Services.AddScoped<MetodoDePagoService>();
builder.Services.AddScoped<GrupoFinancieroService>();
builder.Services.AddScoped<ReglaAsignacionService>();
builder.Services.AddScoped<TipoGastoService>();
builder.Services.AddScoped<OperacionFinancieraService>();
builder.Services.AddScoped<CuentaCorrienteService>();
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<GrupoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReportesService>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.Configure<DocumentStorageOptions>(
    builder.Configuration.GetSection(DocumentStorageOptions.SectionName));
builder.Services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();

{
    var emailCfg = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();
    Console.WriteLine(
        $"[Email] .env={(File.Exists(envFile) ? "ok" : "missing")} enabled={emailCfg?.Enabled} host={emailCfg?.Host} user={emailCfg?.UserName} from={emailCfg?.FromEmail}");
}

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
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
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
