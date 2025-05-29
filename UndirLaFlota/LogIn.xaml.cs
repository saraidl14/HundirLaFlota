namespace UndirLaFlota;

public partial class LogIn : ContentPage
{
	public LogIn()
	{
		InitializeComponent();
	}
    private void OnClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new AppShell();
        // Navegar a la página principal
        Shell.Current.GoToAsync("//NuevoTest");
    }
}