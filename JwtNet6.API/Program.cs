using JwtNet6.Models.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

#region Conexión BD
builder.Services.AddDbContext<JwtContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ServidorDesarrollo")));
#endregion

#region Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Modify Password settings.   
    options.Password.RequireDigit = true; //Requerir un número entre 0 o 9 por contraseña
    options.Password.RequiredLength = 8; //Máximo 8 caracteres
    options.Password.RequireNonAlphanumeric = true; //Requirir 1 caracter no alfanumérico
    options.Password.RequireUppercase = true; //Requirir por lo menos una mayúscula
    options.Password.RequireLowercase = true; //Requirir por lo menos una minúscula
    options.Password.RequiredUniqueChars = 1; //Cantidad de caracteres unicos por contraseña

    // Default User settings.   
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false; //No permitir email repetidos

    // Default Lockout settings.                                         
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(10); //5 Minutos bloqueado
    options.Lockout.MaxFailedAccessAttempts = 2; //Máximo 5 Intentos fallidos
    options.Lockout.AllowedForNewUsers = true; //Si permitir para nuevos usuarios

    // Default SignIn settings.
    options.SignIn.RequireConfirmedEmail = false; //No confirmar email
    options.SignIn.RequireConfirmedPhoneNumber = false; //No confirmar numero

}).AddEntityFrameworkStores<JwtContext>().AddDefaultTokenProviders();
#endregion

#region Jwt
//Configurar la sección de JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(
options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
    options.Events = new JwtBearerEvents()
    {
        OnAuthenticationFailed = c =>
        {
            c.NoResult();
            c.Response.StatusCode = 500;
            c.Response.ContentType = "text/plain";
            return c.Response.WriteAsync(c.Exception.Message);
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new Response() { Message = "Usted no esta autorizado.", StatusCode = 401, Success = false});
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new Response() { Message = "Usted no tiene permisos sobre este recurso.", StatusCode = 403, Success = false });
            return context.Response.WriteAsync(result);
        }
    };

});
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

app.Run();
