namespace LoginProfesionalA;

public partial class RegistroPage : ContentPage
{
    private bool _ocupado;
    public RegistroPage()
    {
        InitializeComponent(); RecuperacionPanel.IsVisible=App.Acceso.EsLocal;
        AyudaLabel.Text=App.Acceso.EsLocal ? "La cuenta se guardará solo en esta app. Recuerda tu palabra para recuperar el acceso." : "La cuenta se registrará mediante la API configurada.";
    }
    private async void OnGuardarClicked(object? sender,EventArgs e)
    {
        if(_ocupado) return;
        string? error=string.IsNullOrWhiteSpace(NombreEntry.Text) ? "Escribe tu nombre." : Reglas.Validar(CorreoEntry.Text,ClaveEntry.Text);
        if(error is null && ClaveEntry.Text!=ConfirmarEntry.Text) error="Las contraseñas no coinciden.";
        if(error is null && App.Acceso.EsLocal && (PalabraEntry.Text?.Trim().Length ?? 0)<4) error="Elige una palabra de recuperación de al menos 4 caracteres.";
        if(error is not null) { EstadoLabel.Text=error; return; }
        _ocupado=true; Formulario.IsEnabled=false; Carga.IsVisible=Carga.IsRunning=true;
        try
        {
            await App.Acceso.RegistrarAsync(NombreEntry.Text,CorreoEntry.Text,ClaveEntry.Text,PalabraEntry.Text ?? "");
            EstadoLabel.Text="Cuenta creada. Regresa para iniciar sesión.";
        }
        catch(ErrorAcceso ex) { EstadoLabel.Text=ex.Message; }
        catch(IOException) { EstadoLabel.Text="No se pudo guardar la cuenta local."; }
        finally
        {
            ClaveEntry.Text=ConfirmarEntry.Text=PalabraEntry.Text="";
            _ocupado=false; Formulario.IsEnabled=true; Carga.IsVisible=Carga.IsRunning=false;
            SemanticScreenReader.Default.Announce(EstadoLabel.Text);
        }
    }
}
