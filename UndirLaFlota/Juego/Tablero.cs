namespace UndirLaFlota.Juego;

public class Tablero
{
    public List<List<int>> TableroList { get; set; }
    private List<Barco> Barcos;
    private int aciertos = 0;
    private int TotalPuntos;
    public int Dim;

    public class Barco
    {
        private Tablero tablero;
        public int Tamaño { get; set; }
        public List<(int x, int y)> Posiciones { get; set; }
        
        public bool Hundido 
        { 
            get 
            {
                if (tablero == null || Posiciones == null || Posiciones.Count == 0) return false;
                return Posiciones.All(p => tablero.TableroList[p.x][p.y] == 3);
            }
        }

        public Barco(int tamaño, Tablero tablero)
        {
            Tamaño = tamaño;
            Posiciones = new List<(int x, int y)>();
            this.tablero = tablero;
        }
    }

    public Tablero()
    {
        Dim = 10;
        TableroList = new List<List<int>>();
        Barcos = new List<Barco>();
        TableroLimpio();
    }

    public bool TodosLosBarcoHundidos()
    {
        return Barcos.All(b => b.Hundido);
    }

    public void InicializarBarcos(int[] tamaños)
    {
        Barcos.Clear();
        TotalPuntos = 0;
        foreach (var tamaño in tamaños)
        {
            var barco = new Barco(tamaño, this);
            Barcos.Add(barco);
            TotalPuntos += tamaño;
        }
    }

    public void GenerarTablero()
    {
        foreach (var barco in Barcos)
        {
            GenerarBarco(barco);
        }
    }

    public bool ColocarBarco(int x, int y, int tamaño, bool horizontal)
    {
        if (Barcos.Count == 0) return false;
        
        var barco = Barcos.First(b => b.Tamaño == tamaño && b.Posiciones.Count == 0);
        List<(int x, int y)> posicionesPropuestas = new List<(int x, int y)>();

        for (int i = 0; i < tamaño; i++)
        {
            int currentX = x;
            int currentY = y;

            if (horizontal)
                currentY += i;
            else
                currentX += i;

            if (currentX < 0 || currentX >= Dim || currentY < 0 || currentY >= Dim)
                return false;

            // Verificar si hay barcos adyacentes
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int checkX = currentX + dx;
                    int checkY = currentY + dy;
                    if (checkX >= 0 && checkX < Dim && checkY >= 0 && checkY < Dim)
                    {
                        if (TableroList[checkX][checkY] == 2)
                            return false;
                    }
                }
            }

            posicionesPropuestas.Add((currentX, currentY));
        }

        // Si llegamos aquí, podemos colocar el barco
        barco.Posiciones = posicionesPropuestas;
        foreach (var pos in posicionesPropuestas)
        {
            TableroList[pos.x][pos.y] = 2;
        }

        return true;
    }

    private void GenerarBarco(Barco barco)
    {
        Random random = new Random();
        int x, y, dir;
        bool entra;

        do
        {
            entra = true;
            x = random.Next(0, Dim);
            y = random.Next(0, Dim);
            dir = random.Next(0, 4);

            List<(int x, int y)> posicionesPropuestas = new List<(int x, int y)>();
            int currentX = x;
            int currentY = y;

            for (int i = 0; i < barco.Tamaño; i++)
            {
                if (currentX < 0 || currentX >= Dim || currentY < 0 || currentY >= Dim)
                {
                    entra = false;
                    break;
                }

                // Verificar si hay barcos adyacentes
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int checkX = currentX + dx;
                        int checkY = currentY + dy;
                        if (checkX >= 0 && checkX < Dim && checkY >= 0 && checkY < Dim)
                        {
                            if (TableroList[checkX][checkY] == 2)
                            {
                                entra = false;
                                break;
                            }
                        }
                    }
                    if (!entra) break;
                }
                if (!entra) break;

                posicionesPropuestas.Add((currentX, currentY));

                switch (dir)
                {
                    case 0: currentX++; break; // Derecha
                    case 1: currentX--; break; // Izquierda
                    case 2: currentY++; break; // Abajo
                    case 3: currentY--; break; // Arriba
                }
            }

            if (entra)
            {
                barco.Posiciones = posicionesPropuestas;
                foreach (var pos in posicionesPropuestas)
                {
                    TableroList[pos.x][pos.y] = 2;
                }
            }
        } while (!entra);
    }

    private void TableroLimpio()
    {
        aciertos = 0;
        TableroList.Clear();
        for (int i = 0; i < Dim; i++)
        {
            TableroList.Add(new List<int>());
            for (int j = 0; j < Dim; j++)
            {
                TableroList[i].Add(0);
            }
        }
    }

    public bool EsBarcoHundido(int x, int y)
    {
        var barcoGolpeado = Barcos.FirstOrDefault(b => b.Posiciones.Contains((x, y)));
        return barcoGolpeado?.Hundido ?? false;
    }

    public String Jugada(int x, int y)
    {
        if (x < 0 || x >= Dim || y < 0 || y >= Dim)
            return null;

        if (TableroList[x][y] == 0)
        {
            TableroList[x][y] = 1;
            return "Agua";
        }
        else if (TableroList[x][y] == 1 || TableroList[x][y] == 3)
        {
            return null;
        }
        else if (TableroList[x][y] == 2)
        {
            aciertos++;
            TableroList[x][y] = 3;
            
            // Verificar si el barco está hundido
            var barcoGolpeado = Barcos.FirstOrDefault(b => b.Posiciones.Contains((x, y)));
            if (barcoGolpeado != null && barcoGolpeado.Hundido)
            {
                if (TodosLosBarcoHundidos())
                    return "Partida finalizada";
                return "Hundido";
            }
            
            return "Tocado";
        }

        return null;
    }
}