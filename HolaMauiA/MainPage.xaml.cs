namespace HolaMauiA;

public partial class MainPage : ContentPage
{
    public MainPage() => InitializeComponent();
    private void OnSaludarClicked(object? sender, EventArgs e)
    {
        string nombre = NombreEntry.Text?.Trim() ?? "";
        MensajeLabel.Text = nombre.Length == 0
            ? "Escribe tu nombre para continuar."
            : $"¡Hola, {nombre}! Bienvenido a MAUI.";
        MensajeLabel.TextColor = Color.FromArgb(nombre.Length == 0 ? "#B12B40" : "#152445");
        SemanticScreenReader.Default.Announce(MensajeLabel.Text);
    }
    private void OnLimpiarClicked(object? sender, EventArgs e)
    {
        NombreEntry.Text = "";
        MensajeLabel.Text = "Aquí aparecerá tu saludo";
        MensajeLabel.TextColor = Color.FromArgb("#152445");
        NombreEntry.Focus();
    }
}
