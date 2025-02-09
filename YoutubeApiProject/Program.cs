using YouTubeApiProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<YouTubeApiService>();  // Register YouTubeApiService

// ✅ Enable Session
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ✅ Use Session Middleware
app.UseSession();

app.UseAuthorization();

// Set the default route to YouTubeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=YouTube}/{action=Index}/{id?}"); // ✅ Tukar YouTubeController sebagai default


app.Run();
