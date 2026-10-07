using Brokerage.Web.Hubs;
using Brokerage.Web.Middleware;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Services

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<JwtAuthorizationHandler>();


// JWT settings

var jwtSettings =
    builder.Configuration.GetSection("JwtSettings");


// Authentication

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {


        options.Events =
            new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    string? token =
                        context.HttpContext.Request
                            .Cookies["EstateFlow.Auth"];

                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };


        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,


                ValidIssuer =
                    jwtSettings["Issuer"],


                ValidAudience =
                    jwtSettings["Audience"],


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings["SecretKey"]!
                        )
                    ),



                RoleClaimType =
                    ClaimTypes.Role,

                NameClaimType =
                    ClaimTypes.Name
            };
    });


// Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "SellerOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole("Seller");
        });
});
builder.Services.AddRazorPages(options =>
{

    options.Conventions.AuthorizeAreaFolder(
        "Seller",
        "/",
        "SellerOnly");
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "AdminOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole("Admin");
        });
});
builder.Services.AddRazorPages(options =>
{

    options.Conventions.AuthorizeAreaFolder(
        "Admin",
        "/",
        "AdminOnly");
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "BuyerOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole("Buyer");
        });
});
builder.Services.AddRazorPages(options =>
{

    options.Conventions.AuthorizeAreaFolder(
        "Buyer",
        "/",
        "BuyerOnly");
});

// Razor pages




// Http client

builder.Services.AddHttpClient(
    "BrokerageApi",
    client =>
    {
        client.BaseAddress =
            new Uri(
                builder.Configuration[
                    "ApiSettings:BaseUrl"]!
            );

        client.Timeout =
            TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<JwtAuthorizationHandler>();
builder.Services
    .AddHttpClient("BrokeragePublicApi", client =>
    {
        client.BaseAddress =
            new Uri(
                builder.Configuration[
                    "ApiSettings:BaseUrl"]!);
    });
builder.Services.AddScoped<HomeApiClient>();
builder.Services.AddScoped<PropertyApiClient>();
builder.Services.AddScoped<AdminApiClient>();
builder.Services.AddScoped<NotificationApiClient>();
builder.Services.AddScoped<TransactionApiClient>();
builder.Services.AddScoped<ChatApiClient>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<RoleLandingPageService>();


// Application

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();


// Authentication MUST come first.
app.UseAuthentication();

app.UseMiddleware<
    AuthenticatedEntryRedirectMiddleware>();

// Authorization comes after authentication.
app.UseAuthorization();


app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();
app.MapHub<ChatHub>("/hubs/chat");


app.Run();