using System.Text.Json;

namespace UndirLaFlota.Juego;

public class HistorialManager
{
    private const string HISTORIAL_KEY = "historial_partidas";

    public static void GuardarPartida(PartidaHistorial partida)
    {
        var partidas = ObtenerPartidas();
        partidas.Add(partida);
        
        // Mantener solo las últimas 10 partidas
        if (partidas.Count > 10)
        {
            partidas.RemoveAt(0);
        }

        string jsonString = JsonSerializer.Serialize(partidas);
        Preferences.Default.Set(HISTORIAL_KEY, jsonString);
    }

    public static List<PartidaHistorial> ObtenerPartidas()
    {
        string jsonString = Preferences.Default.Get(HISTORIAL_KEY, "[]");
        try
        {
            return JsonSerializer.Deserialize<List<PartidaHistorial>>(jsonString) ?? new List<PartidaHistorial>();
        }
        catch
        {
            return new List<PartidaHistorial>();
        }
    }

    public static void LimpiarHistorial()
    {
        Preferences.Default.Remove(HISTORIAL_KEY);
    }
} 