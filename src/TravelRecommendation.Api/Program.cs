using TravelRecommendation.Api.Extensions;
using TravelRecommendation.Api.Middleware;
using TravelRecommendation.Infrastructure;
using TravelRecommendation.Infrastructure.Identity;
using TravelRecommendation.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);

const string AngularDevCorsPolicy = "AngularDevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DevelopmentAdminSeeder.SeedAsync(app.Services);
    await DevelopmentDemoDataSeeder.SeedAsync(app.Services);
    app.UseCors(AngularDevCorsPolicy);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
