namespace LoginProfesionalA;

public partial class RecuperacionPage : ContentPage
{
    private bool _ocupado;
    public RecuperacionPage()
    {
        InitializeComponent(); SolicitarButton.IsVisible=!App.Acceso.EsLocal;
        VerificadorLabel.Text=App.Acceso.EsLocal ? "Palabra de recuperación" : "Código recibido";
        VerificadorEntry.IsPassword=App.Acceso.EsLocal;
        AyudaLabel.Text=App.Acceso.EsLocal ? "Usa la palabra elegida al crear la cuenta. Este modo funciona sin servidor y no envía correos." : "Solicita el código a tu API y escribe el código recibido para cambiar la contraseña.";
    }
    private async void OnSolicitarClicked(object? sender,EventArgs e)
    {
        if(!Reglas.CorreoValido(CorreoEntry.Text)) { EstadoLabel.Text="Escribe un correo válido."; return; }
        await Ejecutar(async () => await App.Acceso.SolicitarAsync(CorreoEntry.Text));
    }
    private async void OnRestablecerClicked(object? sender,EventArgs e)
    {
        string? error=Reglas.Validar(CorreoEntry.Text,ClaveEntry.Text);
        if(error is null && string.IsNullOrWhiteSpace(VerificadorEntry.Text)) error="Completa el dato de recuperación.";
        if(error is null && ClaveEntry.Text!=ConfirmarEntry.Text) error="Las contraseñas no coinciden.";
        if(error is not null) { EstadoLabel.Text=error; return; }
        await Ejecutar(async () => {
            await App.Acceso.RestablecerAsync(CorreoEntry.Text,VerificadorEntry.Text,ClaveEntry.Text);
            VerificadorEntry.Text=ClaveEntry.Text=ConfirmarEntry.Text="";
            return "Contraseña actualizada. Regresa e inicia sesión.";
        });
    }
    private async Task Ejecutar(Func<Task<string>> accion)
    {
        if(_ocupado) return; _ocupado=true; Formulario.IsEnabled=false; Carga.IsVisible=Carga.IsRunning=true;
        try { EstadoLabel.Text=await accion(); }
        catch(ErrorAcceso ex) { EstadoLabel.Text=ex.Message; }
        catch(IOException) { EstadoLabel.Text="No se pudo actualizar la cuenta local."; }
        finally { _ocupado=false; Formulario.IsEnabled=true; Carga.IsVisible=Carga.IsRunning=false; SemanticScreenReader.Default.Announce(EstadoLabel.Text); }
    }
}
