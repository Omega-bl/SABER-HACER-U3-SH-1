namespace LoginProfesionalA;

public partial class MainPage : ContentPage
{
    private bool _ocupado;
    public MainPage()
    {
        InitializeComponent(); NavigationPage.SetHasNavigationBar(this,false);
        ModoLabel.Text=App.Acceso.EsLocal ? "Modo de práctica local · sin servidor" : "Autenticación por API HTTPS";
        CorreoEntry.Text=Preferences.Default.Get("correo","");
        RecordarCheck.IsChecked=!string.IsNullOrEmpty(CorreoEntry.Text);
    }
    private void Mensaje(string texto,bool error=false)
    {
        EstadoLabel.Text=texto;
        EstadoLabel.TextColor=Color.FromArgb(error ? "#B12B40" : "#152445");
        SemanticScreenReader.Default.Announce(texto);
    }
    private void Ocupar(bool valor)
    {
        _ocupado=valor; Formulario.IsEnabled=SesionPanel.IsEnabled=!valor;
        Carga.IsVisible=Carga.IsRunning=valor;
    }
    private void OnMostrarClicked(object? sender,EventArgs e)
    {
        ClaveEntry.IsPassword=!ClaveEntry.IsPassword;
        MostrarButton.Text=ClaveEntry.IsPassword ? "Mostrar" : "Ocultar";
    }
    private async void OnEntrarClicked(object? sender,EventArgs e)
    {
        if(_ocupado) return;
        var error=Reglas.Validar(CorreoEntry.Text,ClaveEntry.Text);
        if(error is not null) { Mensaje(error,true); return; }
        Ocupar(true);
        try
        {
            var perfil=await App.Acceso.EntrarAsync(CorreoEntry.Text,ClaveEntry.Text);
            if(RecordarCheck.IsChecked) Preferences.Default.Set("correo",CorreoEntry.Text.Trim());
            else Preferences.Default.Remove("correo");
            NombreLabel.Text=perfil.Nombre; CorreoLabel.Text=perfil.Correo;
            Formulario.IsVisible=false; SesionPanel.IsVisible=true;
            Mensaje(App.Acceso.EsLocal ? "Acceso correcto en la práctica local." : "Acceso confirmado por la API.");
        }
        catch(ErrorAcceso ex) { Mensaje(ex.Message,true); }
        catch(IOException) { Mensaje("No se pudieron leer los datos locales. Intenta de nuevo.",true); }
        finally { ClaveEntry.Text=""; ClaveEntry.IsPassword=true; MostrarButton.Text="Mostrar"; Ocupar(false); }
    }
    private async void OnSalirClicked(object? sender,EventArgs e)
    {
        if(_ocupado) return; Ocupar(true);
        try { await App.Acceso.SalirAsync(); Mensaje("Sesión cerrada."); }
        catch(ErrorAcceso ex) { Mensaje("Sesión cerrada en este dispositivo. "+ex.Message,true); }
        finally { Formulario.IsVisible=true; SesionPanel.IsVisible=false; NombreLabel.Text=CorreoLabel.Text=""; Ocupar(false); }
    }
    private async void OnCrearClicked(object? sender,EventArgs e) => await Navigation.PushAsync(new RegistroPage());
    private async void OnRecuperarClicked(object? sender,EventArgs e) => await Navigation.PushAsync(new RecuperacionPage());
}
