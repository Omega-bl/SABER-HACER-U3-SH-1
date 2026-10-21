using System.Net.Mail;
namespace LoginProfesionalA;

public static class Reglas
{
    public static bool CorreoValido(string? correo)
    {
        correo = correo?.Trim();
        return !string.IsNullOrWhiteSpace(correo) && correo.Length <= 254
            && MailAddress.TryCreate(correo,out var direccion)
            && direccion.Address == correo && direccion.Host.Contains('.');
    }
    public static string? Validar(string? correo,string? clave)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave)) return "Completa el correo y la contraseña.";
        if (!CorreoValido(correo)) return "Escribe un correo electrónico válido.";
        if (clave.Length is < 8 or > 128) return "La contraseña debe tener entre 8 y 128 caracteres.";
        return null;
    }
}
