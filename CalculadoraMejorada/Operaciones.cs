using System.Globalization;

namespace CalculadoraMejoradaA;

public record ResultadoOperacion(bool Correcto, string Mensaje, double? Valor = null);

public static class Operaciones
{
    public static bool Leer(string? texto, out double valor)
    {
        valor = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        texto = texto.Trim();
        if (texto.Contains('.') && texto.Contains(',')) return false;
        return double.TryParse(texto.Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out valor) && double.IsFinite(valor);
    }

    public static ResultadoOperacion Calcular(string? primero, string? segundo, string operacion)
    {
        if (!Leer(primero, out var a)) return new(false, "Escribe un primer número válido.");
        bool unaria = operacion is "cuadrado" or "raiz" or "inversa";
        double b = 0;
        if (!unaria && !Leer(segundo, out b)) return new(false, "Escribe un segundo número válido.");
        if ((operacion is "/" or "resto") && b == 0 || operacion == "inversa" && a == 0)
            return new(false, "No se puede dividir entre cero.");
        if (operacion == "raiz" && a < 0) return new(false, "La raíz cuadrada requiere un número no negativo.");
        double resultado;
        switch (operacion)
        {
            case "+": resultado = a + b; break;
            case "-": resultado = a - b; break;
            case "*": resultado = a * b; break;
            case "/": resultado = a / b; break;
            case "potencia": resultado = Math.Pow(a,b); break;
            case "cuadrado": resultado = a * a; break;
            case "raiz": resultado = Math.Sqrt(a); break;
            case "inversa": resultado = 1 / a; break;
            case "porcentaje": resultado = (a / 100) * b; break;
            case "resto": resultado = a % b; break;
            default: return new(false, "Selecciona una operación válida.");
        }
        if (!double.IsFinite(resultado)) return new(false, "El resultado está fuera del rango real permitido.");
        return new(true, "Resultado: " + resultado.ToString("G12", CultureInfo.InvariantCulture), resultado);
    }
}
