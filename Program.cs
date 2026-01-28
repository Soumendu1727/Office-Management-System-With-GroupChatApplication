using ClientServerCommunication.Models;
using ClientServerCommunication.Services;
using Microsoft.AspNetCore.DataProtection;


var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register AuthService AFTER builder creation
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<GroupService>();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("ClientServerCommunication");

var app = builder.Build();

// ---------------- PIPELINE ----------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// OPTIONAL: Seed Admin (temporary – move later)
var users = JsonFileService.Read<User>("Data/users.json");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
