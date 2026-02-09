using ckLib;
using CKSA.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped<CookieHelper>();
builder.Services.AddSingleton<CacheHelper>();
builder.Services.AddRouting(options =>
{
	options.LowercaseUrls = true;
});

builder.Services.AddControllersWithViews() // or .AddRazorPages()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
	});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.Use(async (context, next) =>
{
	var path = context.Request.Path.Value;

	// Fast exit: null or empty
	if (string.IsNullOrEmpty(path))
	{
		await next();
		return;
	}

	// Ignore files (css, js, images, etc.)
	if (Path.HasExtension(path))
	{
		await next();
		return;
	}

	bool needsLowercase = false;
	bool needsTrailingSlash = !path.EndsWith('/');

	// Single scan of the path
	for (int i = 0; i < path.Length; i++)
	{
		char c = path[i];
		if (c >= 'A' && c <= 'Z')
		{
			needsLowercase = true;
			break;
		}
	}

	// Nothing to do continue pipeline
	if (!needsLowercase && !needsTrailingSlash)
	{
		await next();
		return;
	}

	// Build canonical path once
	var normalizedPath = needsLowercase
		? path.ToLowerInvariant()
		: path;

	if (needsTrailingSlash)
	{
		normalizedPath += "/";
	}

	context.Response.Redirect(
		normalizedPath + context.Request.QueryString,
		permanent: true);
});

app.UseRouting();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
