using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PocketFlow.Data;
using PocketFlow.Repositories;
using PocketFlow.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<PocketFlow.Models.WebPushOptions>(
    builder.Configuration.GetSection(PocketFlow.Models.WebPushOptions.WebPush));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PocketFlow.Repositories.IUserRepository, PocketFlow.Repositories.UserRepository>();
builder.Services.AddScoped<PocketFlow.Services.IAuthService, PocketFlow.Services.AuthService>();

builder.Services.AddHttpClient<PocketFlow.Services.ISupabaseExternalAuthService, PocketFlow.Services.SupabaseExternalAuthService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "PocketFlow.Auth";
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddScoped<IAppClock, AppClock>();
builder.Services.AddScoped<IAuthenticationSessionService, AuthenticationSessionService>();
builder.Services.AddScoped<IFinancialCalculationService, FinancialCalculationService>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPiggyBankRepository, PiggyBankRepository>();
builder.Services.AddScoped<IMonthlyPlanRepository, MonthlyPlanRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

builder.Services.AddScoped<IOnboardingStateService, OnboardingStateService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IAccountContextService, AccountContextService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IPaydayService, PaydayService>();
builder.Services.AddScoped<IMonthlyTransitionService, MonthlyTransitionService>();
builder.Services.AddScoped<IMonthlyHistoryService, MonthlyHistoryService>();
builder.Services.AddScoped<IPiggyBankService, PiggyBankService>();
builder.Services.AddScoped<IWebPushNotificationService, WebPushNotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
