using CKSA.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<CacheHelper>();
builder.Services.AddRouting(options =>
{
	options.LowercaseUrls = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

// Redirect uppercase URLs to lowercase
app.Use(async (context, next) =>
{
	var path = context.Request.Path.Value;

	if (path != null && path.Any(char.IsUpper))
	{
		var lowerPath = path.ToLowerInvariant();
		var query = context.Request.QueryString.Value;

		context.Response.Redirect($"{lowerPath}{query}", permanent: true);
		return;
	}

	await next();
});

// Force trailing slash on directory URLs
app.Use(async (context, next) =>
{
	var path = context.Request.Path.Value;

	// Ignore files (css, js, images, etc.)
	if (!string.IsNullOrEmpty(path) &&
		!path.EndsWith("/") &&
		!Path.HasExtension(path))
	{
		var query = context.Request.QueryString.Value;
		context.Response.Redirect($"{path}/{query}", permanent: true);
		return;
	}

	await next();
});

// URL normalization (lowercase + trailing slash)
app.Use(async (context, next) =>
{
	var path = context.Request.Path.Value;

	if (path != null && path.Any(char.IsUpper))
	{
		context.Response.Redirect(
			$"{path.ToLowerInvariant()}{context.Request.QueryString}",
			permanent: true);
		return;
	}

	if (!string.IsNullOrEmpty(path) &&
		!path.EndsWith("/") &&
		!Path.HasExtension(path))
	{
		context.Response.Redirect(
			$"{path}/{context.Request.QueryString}",
			permanent: true);
		return;
	}

	await next();
});

app.UseRouting();


app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
