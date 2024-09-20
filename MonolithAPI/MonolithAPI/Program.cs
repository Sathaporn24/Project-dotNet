using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonolithAPI;
using MonolithAPI.Filters;
using MonolithAPI.Helpers;
using MonolithAPI.Models;
using MonolithAPI.Senders;

var builder = WebApplication.CreateBuilder(args);

// ---------------- services section -----------------------
var services = builder.Services;
// add controllers
services.AddControllers();
// add database context
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
});
// add identity system and its db context
services.AddIdentity<UserModel, RoleModel>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
// add cross-origin resource sharing
services.AddCors(options =>
{
    options.AddPolicy("MyCors", config =>
    {
        config
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins")
        .Get<string[]>()!)
        .AllowAnyMethod().AllowAnyHeader();
    });
});
// add authentication for jwt
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecurityKey"]!))
    };
});
// add token helper
services.AddScoped<TokenHelper>();
services.AddScoped<IEmailSender<UserModel>, FakeEmailSender>();
// configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

// ---------------- app section -----------------------
var app = builder.Build();

// create image folder
var imageFolder = builder.Configuration.GetValue<string>("ImageSettings:UploadFolder");
var imageFolderPath = Path.Combine(builder.Environment.ContentRootPath, imageFolder!);
CreateFolder(imageFolderPath);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// redirect http request to https
app.UseHttpsRedirection();
// make images folder can access as static files
var imageRequestPath = builder.Configuration.GetValue<string>("ImageSettings:RequestPath");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imageFolderPath),
    RequestPath = imageRequestPath
});
// add CORS middleware
app.UseCors("MyCors");
// add authentication middleware
app.UseAuthentication();
// add authorization middleware
app.UseAuthorization();
// map enpoint for controller actions
app.MapControllers();

app.Run();

void CreateFolder(string folderPath)
{
    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }
}