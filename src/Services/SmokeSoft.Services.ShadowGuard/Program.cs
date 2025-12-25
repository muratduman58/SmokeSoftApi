using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using SmokeSoft.Infrastructure.Caching;
using SmokeSoft.Infrastructure.Services;
using SmokeSoft.Services.ShadowGuard.Configuration;
using SmokeSoft.Services.ShadowGuard.Data;
using SmokeSoft.Services.ShadowGuard.Filters;
using SmokeSoft.Services.ShadowGuard.Services;
using SmokeSoft.Services.ShadowGuard.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/shadowguard-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ShadowGuard API",
        Version = "v1",
        Description = "ShadowGuard Service API for AI Identity Management"
    });

    // Add XML comments to Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add file upload support
    c.OperationFilter<FileUploadOperationFilter>();

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<ShadowGuardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure MemoryCache (instead of Redis)
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Add WebSocket support for JWT
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ws"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAIIdentityService, AIIdentityService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IScreenCustomizationService, ScreenCustomizationService>();

// Register security & ElevenLabs services
builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();
builder.Services.AddScoped<IQuotaEnforcementService, QuotaEnforcementService>();
builder.Services.AddScoped<IVoiceSlotManager, VoiceSlotManager>();

// Register ElevenLabs services
builder.Services.AddHttpClient<IElevenLabsVoiceService, ElevenLabsVoiceService>();

// Register WebSocket services
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddScoped<WebSocketHandler>();
builder.Services.AddScoped<VoiceWebSocketHandler>();
builder.Services.AddScoped<ElevenLabsConversationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShadowGuard API v1");
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Enable WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// WebSocket endpoints
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    
    // ElevenLabs conversation endpoint: /ws/conversation/{conversationId}
    if (path.StartsWithSegments("/ws/conversation"))
    {
        var segments = path.Value?.Split('/') ?? Array.Empty<string>();
        if (segments.Length >= 4 && context.WebSockets.IsWebSocketRequest)
        {
            var conversationId = segments[3];
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            
            var handler = context.RequestServices
                .GetRequiredService<ElevenLabsConversationHandler>();
            
            await handler.HandleConversationAsync(
                context,
                webSocket,
                conversationId
            );
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    // Legacy voice streaming endpoint: /ws/voice
    else if (path == "/ws/voice")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            // Get user ID from token
            var token = context.Request.Query["access_token"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Extract user ID from token
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    context.Response.StatusCode = 401;
                    return;
                }

                var voiceHandler = context.RequestServices.GetRequiredService<VoiceWebSocketHandler>();
                await voiceHandler.HandleWebSocketAsync(context, userId);
            }
            catch
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "ShadowGuard",
    timestamp = DateTime.UtcNow
}));

// Apply migrations on startup (only in development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShadowGuardDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
    }
}

Log.Information("ShadowGuard Service starting...");

app.Run();
