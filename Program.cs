using System.Text;
using api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Supabase;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configure connection to database
var URL = config["SUPABASE:URL"]!;
var KEY = config["SUPABASE:KEY"]!;

var options = new SupabaseOptions { AutoConnectRealtime = true };
var supabase = new Client(URL, KEY, options);
await supabase.InitializeAsync();

// Add jwt authentication
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JWT:SECRET_KEY"]!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddSingleton(supabase);
builder.Services.AddSingleton<IConfiguration>(conf => config);
builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())*/
/*{*/
app.UseSwagger();
app.UseSwaggerUI();

/*}*/

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
