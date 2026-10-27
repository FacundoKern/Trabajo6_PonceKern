using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using TiendaApp.Services;

namespace TiendaApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

		// Sesión del usuario: Blazor la usa para [Authorize] y <AuthorizeView Roles="...">
		builder.Services.AddAuthorizationCore();
		builder.Services.AddCascadingAuthenticationState();
		builder.Services.AddSingleton<SesionService>();
		builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<SesionService>());

		// HttpClient único apuntando a la API; ApiHandler le agrega el token a cada pedido
		builder.Services.AddSingleton(sp => new HttpClient(new ApiHandler(sp.GetRequiredService<SesionService>()))
		{
			BaseAddress = new Uri(ApiConfig.BaseUrl),
			Timeout = TimeSpan.FromSeconds(30)
		});
		builder.Services.AddSingleton<AuthService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
