using APITEST.Core.InMemoryStore;
using APITEST.Extensions;
using APITEST.Infrastructure.Database;
using APITEST.Infrastructure.IServices;
using APITEST.Infrastructure.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var allowOrigins = builder.Configuration["AllowOrigins"];
System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", $"Extensions\\firebase-auth.json");

//var firebaseCredential = GoogleCredential.FromFile($"Extensions\\firebase-auth.json");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    //options.AddPolicy("AllowHeaders", builder =>
    //{
    //    builder.WithOrigins(allowOrigins)
    //            .WithHeaders(HeaderNames.ContentType, HeaderNames.Server, HeaderNames.AccessControlAllowHeaders, HeaderNames.AccessControlExposeHeaders, "x-custom-header", "x-path", "x-record-in-use", HeaderNames.ContentDisposition);
    //});
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vietsoft.HRM.Api", Version = "v1" });
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    c.DocInclusionPredicate((_, _) => true);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
});

builder.Services.AddTransient<IDapperService, DapperService>();
builder.Services.AddDistributedMemoryCache();
var sessionTimeout = builder.Configuration["SessionTimeout"];
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromHours(Convert.ToDouble(sessionTimeout));
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
{
	//x.RequireHttpsMetadata = false;
	//x.SaveToken = true;
	x.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWTConfig:SecretKey"))),
		ValidateIssuer = false,
		ValidIssuer = builder.Configuration.GetValue<string>("JWTConfig:Iss"),
		ValidateAudience = false,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.FromSeconds(30)

	};
});


builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSevices();


// Thêm cấu hình Firebase Admin SDK
builder.Services.AddSingleton<FirebaseApp>(sp =>
{
	var config = sp.GetRequiredService<IConfiguration>();
	var path = config.GetValue<string>("Firebase_Config"); // Đường dẫn đến tệp tin JSON

	return FirebaseApp.Create(new AppOptions
	{
		Credential = GoogleCredential.FromJson(builder.Configuration.GetValue<string>("Firebase_Config"))
	});
});

var app = builder.Build();

var firebaseApp = app.Services.GetRequiredService<FirebaseApp>();
var firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseSession();

app.UseCors("AllowAll");

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run("http://192.168.2.114:7174");
