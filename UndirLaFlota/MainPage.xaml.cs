using UndirLaFlota.Juego;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace UndirLaFlota
{
    public partial class MainPage : ContentPage
    {
        private Tablero tableroJugador;
        private Tablero tableroIA;
        private IA ia;
        private int puntuacion = 0;
        private int barcosPorColocar = 5;
        private bool juegoIniciado = false;
        private bool orientacionHorizontal = true;
        private int[] tamañosBarcos = new int[] { 6, 5, 3, 3, 2 };
        private int barcoActual = 0;
        private ObservableCollection<Juego.PartidaHistorial> historialPartidas;

        public MainPage()
        {
            InitializeComponent();
            historialPartidas = new ObservableCollection<Juego.PartidaHistorial>(HistorialManager.ObtenerPartidas());
            IniciarNuevaPartida();
        }

        private void IniciarNuevaPartida()
        {
            tableroJugador = new Tablero();
            tableroIA = new Tablero();
            tableroJugador.InicializarBarcos(tamañosBarcos);
            tableroIA.InicializarBarcos(tamañosBarcos);
            ia = new IA(tableroJugador);
            puntuacion = 0;
            barcosPorColocar = 5;
            barcoActual = 0;
            juegoIniciado = false;
            orientacionHorizontal = true;
            IniciarPartidaBtn.IsEnabled = false;
            RotarBarcoBtn.IsEnabled = true;
            EstadoJuegoLabel.Text = $"Coloca el primer barco (Tamaño: {tamañosBarcos[0]})";
            ActualizarContadores();
            CrearTableros();
            tableroIA.GenerarTablero(); // La IA coloca sus barcos automáticamente
        }

        private void CrearTableros()
        {
            CrearTablero(TableroJugadorGrid, true);
            CrearTablero(TableroIAGrid, false);
        }

        private void CrearTablero(Grid grid, bool esJugador)
        {
            // Limpiar botones existentes
            var botonesAEliminar = grid.Children.Where(x => x is Button).ToList();
            foreach (var boton in botonesAEliminar)
            {
                grid.Children.Remove(boton);
            }

            // Agregar etiquetas de coordenadas
            for (int i = 1; i <= 10; i++)
            {
                grid.Add(new Label
                {
                    Text = ((char)('A' + i - 1)).ToString(),
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }, 0, i);

                grid.Add(new Label
                {
                    Text = i.ToString(),
                    TextColor = Colors.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }, i, 0);
            }

            // Crear nuevos botones
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var button = new Button
                    {
                        Text = "",
                        BackgroundColor = Color.Parse("#232f3e"),
                        TextColor = Colors.White,
                        CornerRadius = 5,
                        Margin = new Thickness(1),
                        CommandParameter = new Tuple<int, int>(i, j)
                    };

                    if (esJugador)
                    {
                        button.Clicked += ColocarBarco_Clicked;
                    }
                    else
                    {
                        button.Clicked += RealizarDisparo_Clicked;
                        button.IsEnabled = false;
                    }

                    grid.Add(button, j + 1, i + 1);
                }
            }
        }

        private void ActualizarContadores()
        {
            ScoreLabel.Text = $"Puntuación: {puntuacion}";
            ShipsLabel.Text = $"Barcos por colocar: {barcosPorColocar}";
        }

        private async void ColocarBarco_Clicked(object sender, EventArgs e)
        {
            if (juegoIniciado || barcoActual >= tamañosBarcos.Length) return;

            var btn = sender as Button;
            var position = (Tuple<int, int>)btn.CommandParameter;
            int tamaño = tamañosBarcos[barcoActual];

            if (tableroJugador.ColocarBarco(position.Item1, position.Item2, tamaño, orientacionHorizontal))
            {
                // Actualizar la visualización del barco
                for (int i = 0; i < tamaño; i++)
                {
                    int x = position.Item1;
                    int y = position.Item2;

                    if (orientacionHorizontal)
                        y += i;
                    else
                        x += i;

                    var boton = TableroJugadorGrid.Children
                        .Cast<View>()
                        .FirstOrDefault(v => Grid.GetRow(v) == x + 1 && Grid.GetColumn(v) == y + 1) as Button;
                    if (boton != null)
                    {
                        boton.BackgroundColor = Colors.Green;
                    }
                }

                barcoActual++;
                barcosPorColocar--;
                ActualizarContadores();

                if (barcosPorColocar == 0)
                {
                    IniciarPartidaBtn.IsEnabled = true;
                    RotarBarcoBtn.IsEnabled = false;
                    EstadoJuegoLabel.Text = "¡Pulsa 'Iniciar Partida' para comenzar!";
                }
                else
                {
                    EstadoJuegoLabel.Text = $"Coloca el siguiente barco (Tamaño: {tamañosBarcos[barcoActual]})";
                }
            }
            else
            {
                await DisplayAlert("Error", "No se puede colocar el barco en esa posición", "OK");
            }
        }

        private async void RealizarDisparo_Clicked(object sender, EventArgs e)
        {
            if (!juegoIniciado) return;

            var btn = sender as Button;
            var position = (Tuple<int, int>)btn.CommandParameter;
            
            // Turno del jugador
            string resultado = tableroIA.Jugada(position.Item1, position.Item2);
            if (resultado == null) return;

            if (resultado == "Agua")
            {
                btn.BackgroundColor = Colors.Blue;
                btn.Text = "💧";
                puntuacion -= 1;
            }
            else if (resultado == "Tocado" || resultado == "Hundido")
            {
                btn.BackgroundColor = Colors.Red;
                btn.Text = "💥";
                puntuacion += 5;

                if (resultado == "Hundido")
                {
                    puntuacion += 10;
                    await DisplayAlert("¡Barco hundido!", "¡Has hundido un barco enemigo!", "OK");
                }
            }
            
            if (resultado == "Partida finalizada")
            {
                await FinalizarPartida("¡Has ganado! Has hundido toda la flota enemiga.");
                return;
            }

            // Turno de la IA
            var jugadaIA = ia.RealizarJugada();
            resultado = tableroJugador.Jugada(jugadaIA.x, jugadaIA.y);
            ia.NotificarResultado(jugadaIA.x, jugadaIA.y, resultado);

            var botonJugador = TableroJugadorGrid.Children
                .Cast<View>()
                .FirstOrDefault(v => Grid.GetRow(v) == jugadaIA.x + 1 && Grid.GetColumn(v) == jugadaIA.y + 1) as Button;

            if (botonJugador != null)
            {
                if (resultado == "Agua")
                {
                    botonJugador.Text = "💧";
                }
                else if (resultado == "Tocado" || resultado == "Hundido")
                {
                    botonJugador.Text = "💥";
                    if (resultado == "Hundido")
                    {
                        await DisplayAlert("¡Barco hundido!", "¡La IA ha hundido uno de tus barcos!", "OK");
                    }
                }
                else if (resultado == "Partida finalizada")
                {
                    await FinalizarPartida("¡Has perdido! La IA ha hundido toda tu flota.");
                    return;
                }
            }

            ActualizarContadores();
        }

        private async Task FinalizarPartida(string mensaje)
        {
            juegoIniciado = false;
            var partida = new Juego.PartidaHistorial
            {
                Fecha = DateTime.Now,
                Puntuacion = puntuacion,
                BarcosHundidos = tableroIA.TodosLosBarcoHundidos() ? 5 : tableroJugador.TodosLosBarcoHundidos() ? 0 : 5 - barcosPorColocar
            };

            historialPartidas.Add(partida);
            HistorialManager.GuardarPartida(partida);

            await DisplayAlert("Fin de la partida", $"{mensaje}\nPuntuación final: {puntuacion}", "OK");
            IniciarNuevaPartida();
        }

        private void RotarBarco_Clicked(object sender, EventArgs e)
        {
            orientacionHorizontal = !orientacionHorizontal;
            EstadoJuegoLabel.Text = $"Orientación: {(orientacionHorizontal ? "Horizontal" : "Vertical")} - Tamaño: {tamañosBarcos[barcoActual]}";
        }

        private void IniciarPartida_Clicked(object sender, EventArgs e)
        {
            juegoIniciado = true;
            IniciarPartidaBtn.IsEnabled = false;
            
            // Habilitar botones del tablero de la IA
            foreach (var child in TableroIAGrid.Children)
            {
                if (child is Button button)
                {
                    button.IsEnabled = true;
                }
            }

            // Deshabilitar botones del tablero del jugador
            foreach (var child in TableroJugadorGrid.Children)
            {
                if (child is Button button)
                {
                    button.Clicked -= ColocarBarco_Clicked;
                }
            }

            EstadoJuegoLabel.Text = "¡Dispara contra la flota enemiga!";
        }

        private void NuevaPartida_Clicked(object sender, EventArgs e)
        {
            IniciarNuevaPartida();
        }

        private async void Historial_Clicked(object sender, EventArgs e)
        {
            var partidas = HistorialManager.ObtenerPartidas();
            if (partidas.Count == 0)
            {
                await DisplayAlert("Historial", "No hay partidas registradas", "Cerrar");
                return;
            }

            string mensaje = "Historial de partidas:\n\n";
            foreach (var partida in partidas)
            {
                mensaje += $"Fecha: {partida.FechaFormateada}\n";
                mensaje += $"Puntuación: {partida.Puntuacion}\n";
                mensaje += $"Barcos hundidos: {partida.BarcosHundidos}\n\n";
            }
            
            await DisplayAlert("Historial", mensaje, "Cerrar");
        }
    }
}