namespace CalculadoraBasicaA;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        
    }
    private void OnOperationClicked(object? sender, EventArgs e)
    {
        if (sender is not Button boton || boton.CommandParameter is not string operacion) return;
        
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
        
        FirstNumberEntry.Focus();
    }

}
