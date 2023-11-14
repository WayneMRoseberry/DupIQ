using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(o =>
{
	o.DefaultAuthenticateScheme = JwtBearerDefaults.
AuthenticationScheme;
	o.DefaultChallengeScheme = JwtBearerDefaults.
AuthenticationScheme;
	o.DefaultScheme = JwtBearerDefaults.
AuthenticationScheme;
}).AddJwtBearer(o =>
{
	o.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = false,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey
(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	};
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var securityScheme = new OpenApiSecurityScheme()
{
	Name = "Authorization",
	Type = SecuritySchemeType.ApiKey,
	Scheme = "Bearer",
	BearerFormat = "JWT",
	In = ParameterLocation.Header,
	Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
				{
					{
					 new OpenApiSecurityScheme
					 {
					 Reference = new OpenApiReference
					 {
					 Type = ReferenceType.SecurityScheme,
					 Id = "Bearer"
					 }
					 },
					 new string[] {}
					}
				};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
	o.AddSecurityDefinition("Bearer", securityScheme);
	o.AddSecurityRequirement(securityReq);
});

var app = builder.Build();

app.UseSwagger();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(exceptionHanderApp =>
{
    exceptionHanderApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if( exceptionHandlerFeature?.Error is IssueDoesNotExistException || exceptionHandlerFeature?.Error is IssueReportDoesNotExistException)
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent; 
        }
        if( exceptionHandlerFeature?.Error is ArgumentNullException || exceptionHandlerFeature?.Error is ArgumentException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "plain-text";
            context.Response.WriteAsync($"{exceptionHandlerFeature.Error.Message}");
        }
    });
});

app.MapControllers();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();

app.Logger.LogInformation("Starting app.");

app.MapGet("/", (HttpContext context) => context.Response.Redirect("/swagger") );

app.Run();
