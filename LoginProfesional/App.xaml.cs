namespace LoginProfesionalA;

public partial class App : Application
{
    public static ServicioAcceso Acceso { get; } = new();
    public App() => InitializeComponent();
    protected override Window CreateWindow(IActivationState? activationState) => new(new NavigationPage(new MainPage()));
}
