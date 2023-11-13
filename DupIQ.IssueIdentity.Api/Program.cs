using DupIQ.IssueIdentity;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
