using TodoApi;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


void ConfigureServices(IServiceCollection services)
    {
        // הוספת אימות JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key")) // כדאי להחליף למפתח סודי אמיתי בסביבת ההפקה
                };
            });

        services.AddAuthorization();
        
        // הוספת שירותים נוספים והגדרת ה-API המינימלי
    }

    void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // הוספת Middleware עבור אימות והפשטה
        app.UseAuthentication();
        app.UseAuthorization();
        
        // הוספת Middleware והגדרת ה-Endpoints
    }
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddDbContext<ToDoDbContext>(options =>
{
    options.UseMySql("server=localhost;user=root;password=sdfg;database=ToDoDB", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.3.0-mysql"));
});


var app = builder.Build();
//  app.Run("http://localhost:5210");


app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());



 app.UseSwagger();
     app.UseSwaggerUI(c =>
     {
         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Name");
     });


app.MapGet("/tasks", async (ToDoDbContext dbContext) =>
{
    var tasks = await dbContext.Items.ToListAsync();
    return Results.Ok(tasks);
});

app.MapPost("/tasks", async (Item item, ToDoDbContext dbContext) =>
{
    item.IsComplete=false;
    dbContext.Items.Add(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
});

app.MapPut("/tasks/{id}", async (int id, Item newItem, ToDoDbContext dbContext) =>
{
    var existingItem = await dbContext.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound();
    }

    
    existingItem.IsComplete = newItem.IsComplete;

    await dbContext.SaveChangesAsync();
    return Results.Ok(existingItem);
});

app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item == null)
    {
        return Results.NotFound();
    }

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();



