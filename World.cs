using System.IO;

public class World
{
    public const int WIDTH = 100;

    public const int HEIGHT = 50;

    public const float MAXLAT = 90f;

    public const float MINLAT = -90f;

    public const float MAXLONG = 180f;

    public const float MINLONG = -180f;

    private Province[,] provinceGrid = new Province[WIDTH, HEIGHT];

    public World()
    {
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                provinceGrid[i, j] = new Province();
            }
        }
    }

    /**
     * Create a grid from the specified csv file
     */
    public World(string filePath)
        : this()
    {
        using (StreamReader file = File.OpenText(filePath))
        {
            while (!file.EndOfStream)
            {
                string cityString = file.ReadLine();
                AddCity(cityString);
            }
        }
    }

    public Province GetProvinceAt(Pos pos)
    {
        if (pos.X >= 0 && pos.X < provinceGrid.GetLength(0) && pos.Y >= 0 && pos.Y < provinceGrid.GetLength(1))
        {
            return provinceGrid[pos.X, pos.Y];
        }
        else
        {
            return null;
        }
    }

    public void Tick()
    {
        for (int i = 0; i < WIDTH; ++i)
        {
            for (int j = 0; j < HEIGHT; ++j)
            {
                provinceGrid[i, j].Tick();
            }
        }
    }

    private void AddCity(string cityString)
    {
        const int NAME_IDX = 1;
        const int LAT_IDX = 2;
        const int LONG_IDX = 3;
        const int POP_IDX = 4;

        string[] cityData = cityString.Split(new char[] { ',' });
        if (cityData.Length != 9)
        {
            // for now, if the city parsed is invalid, just silently fail to add it to the map
            return;
        }

        var city = new City(cityData[NAME_IDX], (int)float.Parse(cityData[POP_IDX]));
        float latitude = float.Parse(cityData[LAT_IDX]);
        float longitude = float.Parse(cityData[LONG_IDX]);

        Pos pos = ConvertGridCoords(latitude, longitude);
        AddCity(city, pos);
    }

    private Pos ConvertGridCoords(float latitude, float longitude)
    {
        int x = (int)((longitude - MINLONG) * WIDTH / (MAXLONG - MINLONG));
        int y = (int)((latitude - MINLAT) * HEIGHT / (MAXLAT - MINLAT));

        return new Pos(x, y);
    }

    private void AddCity(City city, Pos pos)
    {
        provinceGrid[pos.X, pos.Y].City = city;
    }
}