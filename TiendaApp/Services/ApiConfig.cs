namespace TiendaApp.Services;

public static class ApiConfig
{
	// API publicada en Somee. El plan gratuito no tiene HTTPS, por eso se usa http
	// (ver Platforms/Android/Resources/xml/network_security_config.xml).
	// Para probar con la API corriendo en la PC desde el emulador: "http://10.0.2.2:<puerto>/"
	public const string BaseUrl = "http://tp5poncekern.somee.com/";

	// Las imágenes vienen como ruta relativa ("uploads/producto_1_xxx.png")
	public static string? UrlImagen(string? rutaRelativa) =>
		string.IsNullOrWhiteSpace(rutaRelativa) ? null : BaseUrl + rutaRelativa.TrimStart('/');
}
