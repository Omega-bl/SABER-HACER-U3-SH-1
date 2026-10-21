namespace LoginProfesionalA;

public sealed class ServicioAcceso
{
    private readonly AlmacenLocal _local = new(Path.Combine(FileSystem.AppDataDirectory,"cuentas-practica.json"));
    private ClienteApi? _api;
    public bool EsLocal => string.IsNullOrWhiteSpace(ConfiguracionApi.UrlBase);
    private ClienteApi Api => _api ??= new ClienteApi(ConfiguracionApi.UrlBase);
    public Task<Perfil> EntrarAsync(string correo,string clave) => EsLocal ? _local.EntrarAsync(correo,clave) : Api.EntrarAsync(correo,clave);
    public Task RegistrarAsync(string nombre,string correo,string clave,string palabra) => EsLocal
        ? _local.RegistrarAsync(nombre,correo,clave,palabra) : Api.RegistrarAsync(nombre,correo,clave);
    public Task<string> SolicitarAsync(string correo) => EsLocal
        ? Task.FromResult("En esta práctica usa la palabra de recuperación que elegiste al crear tu cuenta. No se envían correos.")
        : Api.RecuperarAsync(correo);
    public Task RestablecerAsync(string correo,string verificador,string nuevaClave) => EsLocal
        ? _local.RestablecerAsync(correo,verificador,nuevaClave) : Api.RestablecerAsync(correo,verificador,nuevaClave);
    public Task SalirAsync() => EsLocal ? Task.CompletedTask : Api.SalirAsync();
}
