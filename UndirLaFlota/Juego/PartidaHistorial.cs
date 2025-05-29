namespace UndirLaFlota.Juego;

public class PartidaHistorial
{
    public DateTime Fecha { get; set; }
    public int Puntuacion { get; set; }
    public int BarcosHundidos { get; set; }

    public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
} 