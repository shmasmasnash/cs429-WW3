using System.Threading;
using OpenTK;

public class GameRunner
{
    public static void Main(string[] args)
    {
        var game = new Game();
        var window = new Window(1000, 600, game);
        var repl = new REPL(game, window);
        var task = new Thread(() =>
        {
            repl.Launch();
        });

        task.Start();
        using (GameWindow w = window) // necessary idiom for IDisposable
        {
            w.Run(60.0); // hand off execution to OpenTK, 60fps
        }

        task.Join(); // will execute after window.Exit() function is called
    }
}