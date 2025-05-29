using System;
using System.Collections.Generic;

namespace UndirLaFlota.Juego
{
    public class IA
    {
        private Tablero tableroJugador;
        private Random random;
        private List<(int x, int y)> ultimosAciertos;
        private List<(int x, int y)> celdasPendientes;

        public IA(Tablero tableroJugador)
        {
            this.tableroJugador = tableroJugador;
            random = new Random();
            ultimosAciertos = new List<(int x, int y)>();
            celdasPendientes = new List<(int x, int y)>();
            
            // Inicializar todas las celdas posibles
            for (int i = 0; i < tableroJugador.Dim; i++)
            {
                for (int j = 0; j < tableroJugador.Dim; j++)
                {
                    celdasPendientes.Add((i, j));
                }
            }
        }

        public (int x, int y) RealizarJugada()
        {
            // Si hay aciertos previos, intentar disparar alrededor
            if (ultimosAciertos.Count > 0)
            {
                var ultimoAcierto = ultimosAciertos[ultimosAciertos.Count - 1];
                var celdasAdyacentes = new List<(int x, int y)>
                {
                    (ultimoAcierto.x - 1, ultimoAcierto.y),
                    (ultimoAcierto.x + 1, ultimoAcierto.y),
                    (ultimoAcierto.x, ultimoAcierto.y - 1),
                    (ultimoAcierto.x, ultimoAcierto.y + 1)
                };

                foreach (var celda in celdasAdyacentes)
                {
                    if (EsCeldaValida(celda.x, celda.y) && celdasPendientes.Contains(celda))
                    {
                        celdasPendientes.Remove(celda);
                        return celda;
                    }
                }
            }

            // Si no hay aciertos o no se pueden usar las celdas adyacentes, elegir una celda aleatoria
            if (celdasPendientes.Count > 0)
            {
                int index = random.Next(celdasPendientes.Count);
                var celda = celdasPendientes[index];
                celdasPendientes.RemoveAt(index);
                return celda;
            }

            // No deberíamos llegar aquí si el juego está bien implementado
            return (-1, -1);
        }

        public void NotificarResultado(int x, int y, string resultado)
        {
            if (resultado == "Tocado")
            {
                ultimosAciertos.Add((x, y));
            }
            else if (resultado == "Agua")
            {
                // No necesitamos hacer nada especial
            }
            else if (resultado == "Hundido")
            {
                // Limpiar los aciertos ya que el barco está hundido
                ultimosAciertos.Clear();
            }
        }

        private bool EsCeldaValida(int x, int y)
        {
            return x >= 0 && x < tableroJugador.Dim && y >= 0 && y < tableroJugador.Dim;
        }
    }
} 