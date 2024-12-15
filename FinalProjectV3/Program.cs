using FinalProjectV3.Context;
using FinalProjectV3.Helpers;
using FinalProjectV3.Models;
using FinalProjectV3.Services;
using FinalProjectV3.Validations;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using NLog;
using NLog.Web;
using FinalProjectV3.Middlewares;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders(); 
builder.Host.UseNLog();



builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()  
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountantService, AccountantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<AccountantRegisterDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidation>();
builder.Services.AddValidatorsFromAssemblyContaining<LoanRequestDtoValidation>();
builder.Services.AddTransient<ValidationHelper>();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseMiddleware<ForbiddenMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();

