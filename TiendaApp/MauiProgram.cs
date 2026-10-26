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

		// HttpClient único apuntando a la API
		builder.Services.AddSingleton(new HttpClient
		{
			BaseAddress = new Uri(ApiConfig.BaseUrl),
			Timeout = TimeSpan.FromSeconds(30)
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
