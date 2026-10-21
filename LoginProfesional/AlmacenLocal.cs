using System.Security.Cryptography;
using System.Text.Json;

namespace LoginProfesionalA;

public record Perfil(string Nombre, string Correo);
public sealed class ErrorAcceso(string mensaje) : Exception(mensaje);
public sealed class CuentaLocal
{
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string SalClave { get; set; } = "";
    public string HashClave { get; set; } = "";
    public string SalRecuperacion { get; set; } = "";
    public string HashRecuperacion { get; set; } = "";
}

// Almacén del MODO DE PRÁCTICA. No sustituye al servidor de autenticación de producción.
public sealed class AlmacenLocal(string archivo)
{
    private readonly SemaphoreSlim _candado = new(1,1);
    private readonly Dictionary<string,(int Cantidad, DateTimeOffset Hasta)> _intentos = new();
    private static string Normalizar(string correo) => correo.Trim().ToLowerInvariant();
    private static string Hash(string texto, byte[] sal) => Convert.ToBase64String(
        Rfc2898DeriveBytes.Pbkdf2(texto, sal, 210_000, HashAlgorithmName.SHA256, 32));
    private static bool Coincide(string texto, string sal, string esperado) =>
        CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(Hash(texto, Convert.FromBase64String(sal))), Convert.FromBase64String(esperado));
    private async Task<List<CuentaLocal>> LeerAsync() => File.Exists(archivo)
        ? JsonSerializer.Deserialize<List<CuentaLocal>>(await File.ReadAllTextAsync(archivo)) ?? [] : [];
    private async Task GuardarAsync(List<CuentaLocal> cuentas)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(archivo)!);
        await File.WriteAllTextAsync(archivo + ".tmp", JsonSerializer.Serialize(cuentas));
        File.Move(archivo + ".tmp", archivo, true);
    }
    public async Task RegistrarAsync(string nombre, string correo, string clave, string palabra)
    {
        await _candado.WaitAsync();
        try
        {
            var cuentas = await LeerAsync(); correo = Normalizar(correo);
            if (cuentas.Any(x => x.Correo == correo)) throw new ErrorAcceso("Ya existe una cuenta con ese correo en esta práctica.");
            var salClave = RandomNumberGenerator.GetBytes(16);
            var salRecuperacion = RandomNumberGenerator.GetBytes(16);
            var cuenta = await Task.Run(() => new CuentaLocal {
                Nombre=nombre.Trim(), Correo=correo,
                SalClave=Convert.ToBase64String(salClave), HashClave=Hash(clave,salClave),
                SalRecuperacion=Convert.ToBase64String(salRecuperacion), HashRecuperacion=Hash(palabra.Trim().ToUpperInvariant(),salRecuperacion)
            });
            cuentas.Add(cuenta); await GuardarAsync(cuentas);
        }
        finally { _candado.Release(); }
    }
    public async Task<Perfil> EntrarAsync(string correo, string clave)
    {
        await _candado.WaitAsync();
        try
        {
            correo = Normalizar(correo);
            if (_intentos.TryGetValue(correo,out var intento) && intento.Cantidad >= 5 && intento.Hasta > DateTimeOffset.UtcNow)
                throw new ErrorAcceso("Hubo varios intentos. Espera un minuto para volver a entrar.");
            var cuenta = (await LeerAsync()).FirstOrDefault(x => x.Correo == correo);
            bool valido = cuenta is not null && await Task.Run(() => Coincide(clave,cuenta.SalClave,cuenta.HashClave));
            if (!valido)
            {
                _intentos[correo] = (intento.Hasta > DateTimeOffset.UtcNow ? intento.Cantidad+1 : 1, DateTimeOffset.UtcNow.AddMinutes(1));
                throw new ErrorAcceso("Correo o contraseña incorrectos.");
            }
            _intentos.Remove(correo); return new(cuenta!.Nombre,cuenta.Correo);
        }
        finally { _candado.Release(); }
    }
    public async Task RestablecerAsync(string correo, string palabra, string nuevaClave)
    {
        await _candado.WaitAsync();
        try
        {
            var cuentas = await LeerAsync(); correo = Normalizar(correo);
            var cuenta = cuentas.FirstOrDefault(x => x.Correo == correo);
            if (cuenta is null || !await Task.Run(() => Coincide(palabra.Trim().ToUpperInvariant(),cuenta.SalRecuperacion,cuenta.HashRecuperacion)))
                throw new ErrorAcceso("El correo o la palabra de recuperación no coinciden.");
            var sal = RandomNumberGenerator.GetBytes(16);
            cuenta.SalClave=Convert.ToBase64String(sal);
            cuenta.HashClave=await Task.Run(() => Hash(nuevaClave,sal));
            _intentos.Remove(correo); await GuardarAsync(cuentas);
        }
        finally { _candado.Release(); }
    }
}
