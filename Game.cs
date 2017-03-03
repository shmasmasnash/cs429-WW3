using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Game
{
    private const string Commands = @"help|end|mv|quit|print|capture";
    private static readonly Regex Command = new Regex(@"(" + Commands + @")");
    private static readonly Regex Move = new Regex(@"mv (\d+) (\d+),(\d+)");
    private static readonly Regex Capture = new Regex(@"capture (\d+)");
    private readonly World world;
    private int currentPlayer;
    private List<Player> players;

    public Game()
    {
        // initialize world
        world = new World("maps.csv");

        // set up players list. for now, just add two players.
        players = new List<Player>();
        players.Add(new Player());
        players.Add(new Player());
        players[0].AddArmy(new Army(new Pos(0, 0), 10));
        players[1].AddArmy(new Army(new Pos(1, 1), 10));
        currentPlayer = 0;
    }

    public static void Main(string[] args)
    {
        new Game().Launch();
    }

    public Player GetCurrentPlayer()
    {
        return players[currentPlayer];
    }

    public void AdvancePlayer()
    {
        currentPlayer = (currentPlayer + 1) % players.Count;
    }

    public void Launch()
    {
        var originals = new Dictionary<Army, Pos>();

        Console.WriteLine("Welcome to WW3 - type 'help' for instructions.");

        bool running = true;
        do
        {
            Console.Write(currentPlayer + "> ");
            var input = Console.ReadLine();
            var match = Command.Match(input);
            if (match.Success)
            {
                if (match.Value == "help")
                {
                    Console.WriteLine("fuck you");
                }
                else if (match.Value == "end")
                {
                    AdvancePlayer();
                    originals = new Dictionary<Army, Pos>();
                }
                else if (match.Value == "mv")
                {
                    var mv = Move.Match(input);
                    if (mv.Success)
                    {
                        var index = int.Parse(mv.Groups[1].Value);

                        var x = int.Parse(mv.Groups[2].Value);
                        var y = int.Parse(mv.Groups[3].Value);
                        var target = new Pos(x, y);

                        Console.WriteLine(" " + index + " " + x + " " + y);

                        var player = players[currentPlayer];
                        if (player.CanMoveArmy(index, target))
                        {
                            player.MoveArmy(index, target);
                        }
                        else
                        {
                            Console.WriteLine("Illegal Movement");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Command must match: mv [0-9]+ [0-9]+,[0-9]+");
                    }
                }
                else if (match.Value == "capture")
                {
                    var capture = Capture.Match(input);
                    if (capture.Success)
                    {
                        var index = int.Parse(capture.Groups[1].Value);

                        var player = players[currentPlayer];

                        if (player.ArmyExists(index))
                        {
                            var armyPosition = player.ArmyPosition(index);
                            var armyProvince = world.GetProvinceAt(armyPosition);
                            if (armyProvince.Owner != player)
                            {
                                armyProvince.Owner = player;
                                Console.WriteLine("Territory captured");
                            }
                            else
                            {
                                Console.WriteLine("Territory already controlled");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Army Index");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Command must match: capture [0-9]+");
                    }
                }
                else if (match.Value == "quit")
                {
                    running = false;
                }
                else if (match.Value == "print")
                {
                    Print();
                }
            }
            else
            {
                Console.WriteLine("Command must match: " + Commands);
            }
        }
        while (running);
    }

    public void Print()
    {
        for (int x = 0; x < World.WIDTH; x++)
        {
            for (int y = 0; y < World.HEIGHT; y++)
            {
                Province p = world.GetProvinceAt(new Pos(x, y));
                bool printed = false;
                foreach (var player in players)
                {
                    var armies = player.ArmyList;
                    for (int j = 0; j < armies.Count; ++j)
                    {
                        if (armies[j].Position.Equals(new Pos(x, y)))
                        {
                            Console.Write(j);
                            printed = true;
                        }
                    }
                }

                if (!printed)
                {
                    Console.Write(p.City?.Name.Substring(0, 1) ?? "*");
                }
            }

            Console.WriteLine(string.Empty);
        }
    }
}