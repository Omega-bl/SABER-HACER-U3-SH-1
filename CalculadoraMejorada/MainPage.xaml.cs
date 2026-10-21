namespace CalculadoraMejoradaA;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        _campoActivo = FirstNumberEntry;
    }
    private void OnOperationClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not string operacion) return;
        _ultimaOperacion = operacion;
        Resolver(operacion);
    }
    private void Resolver(string operacion)
    {
        var respuesta = Operaciones.Calcular(FirstNumberEntry.Text, SecondNumberEntry.Text, operacion);
        ResultLabel.Text = respuesta.Mensaje;
        ResultLabel.TextColor = Color.FromArgb(respuesta.Correcto ? "#152445" : "#B12B40");
        SemanticScreenReader.Default.Announce(respuesta.Mensaje);
    }
    private void OnClearClicked(object? sender, EventArgs e) => Limpiar();
    private void Limpiar()
    {
        FirstNumberEntry.Text = SecondNumberEntry.Text = "";
        ResultLabel.Text = "Resultado:";
        ResultLabel.TextColor = Color.FromArgb("#152445");
        _campoActivo = FirstNumberEntry; _ultimaOperacion = "+";
        FirstNumberEntry.Focus();
    }

    private Entry _campoActivo = null!;
    private string _ultimaOperacion = "+";
    private void OnNumeroFocused(object? sender, FocusEventArgs e)
    {
        if (sender is Entry campo) _campoActivo = campo;
    }
    private void OnTeclaClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not string tecla) return;
        var campo = _campoActivo;
        var texto = campo.Text ?? "";
        switch (tecla)
        {
            case "C": Limpiar(); return;
            case "A/B":
                _campoActivo = campo == FirstNumberEntry ? SecondNumberEntry : FirstNumberEntry;
                _campoActivo.Focus(); return;
            case "=": Resolver(_ultimaOperacion); return;
            case "±":
                campo.Text = texto.StartsWith('-') ? texto[1..] : "-" + texto; return;
            case "⌫":
                campo.Text = texto.Length == 0 ? "" : texto[..^1]; return;
            case ".":
                if (texto.Contains('.') || texto.Contains(',')) return;
                if (texto is "" or "-") texto += "0";
                break;
        }
        if (texto.Length < 40) campo.Text = texto + tecla;
    }

}
