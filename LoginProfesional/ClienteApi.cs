using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
namespace LoginProfesionalA;

public sealed class ClienteApi
{
    private readonly HttpClient _http;
    private string? _token;
    public ClienteApi(string direccion)
    {
        if (!Uri.TryCreate(direccion.TrimEnd('/')+"/",UriKind.Absolute,out var uri) || uri.Scheme != "https")
            throw new ErrorAcceso("Configura una URL HTTPS válida en ConfiguracionApi.cs.");
        _http=new HttpClient { BaseAddress=uri, Timeout=TimeSpan.FromSeconds(15) };
    }
    private async Task<T> EnviarAsync<T>(string ruta, object? datos=null, bool autenticar=false)
    {
        try
        {
            using var peticion=new HttpRequestMessage(HttpMethod.Post,ruta);
            if(datos is not null) peticion.Content=JsonContent.Create(datos,datos.GetType());
            if(autenticar) peticion.Headers.Authorization=new AuthenticationHeaderValue("Bearer",_token);
            using var respuesta=await _http.SendAsync(peticion);
            if(!respuesta.IsSuccessStatusCode)
            {
                MensajeApi? detalle=null;
                try { detalle=await respuesta.Content.ReadFromJsonAsync<MensajeApi>(); } catch(JsonException) { }
                throw new ErrorAcceso(detalle?.Mensaje ?? "La API rechazó la solicitud. Revisa tus datos.");
            }
            return await respuesta.Content.ReadFromJsonAsync<T>() ?? throw new ErrorAcceso("La API devolvió una respuesta vacía.");
        }
        catch(HttpRequestException) { throw new ErrorAcceso("No se pudo conectar con la API. Revisa la conexión y la dirección configurada."); }
        catch(TaskCanceledException) { throw new ErrorAcceso("La API tardó demasiado. Intenta de nuevo."); }
        catch(JsonException) { throw new ErrorAcceso("La respuesta de la API no coincide con el contrato esperado."); }
    }
    public async Task<Perfil> EntrarAsync(string correo,string clave)
    {
        var respuesta=await EnviarAsync<SesionApi>("api/login",new { correo=correo.Trim(),clave });
        if(string.IsNullOrWhiteSpace(respuesta.Token)) throw new ErrorAcceso("La API no devolvió una sesión válida.");
        _token=respuesta.Token; return respuesta.Usuario;
    }
    public async Task RegistrarAsync(string nombre,string correo,string clave) =>
        await EnviarAsync<MensajeApi>("api/registro",new { nombre=nombre.Trim(),correo=correo.Trim(),clave });
    public async Task<string> RecuperarAsync(string correo) =>
        (await EnviarAsync<MensajeApi>("api/recuperar",new {correo=correo.Trim()})).Mensaje;
    public async Task RestablecerAsync(string correo,string codigo,string nuevaClave) =>
        await EnviarAsync<MensajeApi>("api/restablecer",new {correo=correo.Trim(),codigo=codigo.Trim(),nuevaClave});
    public async Task SalirAsync()
    {
        try { if(_token is not null) await EnviarAsync<MensajeApi>("api/logout",autenticar:true); }
        finally { _token=null; }
    }
    private record MensajeApi(string Mensaje);
    private record SesionApi(string Token,DateTimeOffset Expira,Perfil Usuario);
}
