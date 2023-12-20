using Spectre.Console;
using System;
using System.Linq;

namespace Othello
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Player player1 = new Player(1, "Zdifah");
            Player player2 = new Player(2, "ShockTherapy");

            GameController othelloGame = new GameController();
            ConsoleUI consoleUI = new ConsoleUI(othelloGame.Board);

            othelloGame.OnDiscUpdate += consoleUI.UpdateBoard;
            othelloGame.OnPossibleMove += consoleUI.UpdatePossibelMove;

            othelloGame.AddPlayer(player1, Disc.Black);
            othelloGame.AddPlayer(player2, Disc.White);
            othelloGame.SetIntialTurn(Disc.Black);

            othelloGame.StartGame();
            consoleUI.ShowGame();

            while (othelloGame.GameStat == GameStatus.Start || othelloGame.GameStat == GameStatus.OnGoing)
            {
                var findLegalMoves = othelloGame.FindLegalMoves();
                consoleUI.ShowGame();

                Console.WriteLine($"Giliran disc {othelloGame.CurrentDisc}");

                foreach (var item in findLegalMoves)
                {
                    Console.Write($"Disc {othelloGame.CurrentDisc} ({item.Key.Row},{item.Key.Col}) bisa memakan Disc {othelloGame.CurrentDisc.Opponent()} : ");
                    foreach (var list in item.Value)
                    {
                        Console.Write($"({list.Row},{list.Col})\t");
                    }
                    Console.WriteLine("");
                }

                while (findLegalMoves.Count > 0)
                {
                    Console.Write("Pilih posisi untuk jalan dan input sesuai format posisi (row,col) : ");
                    var input = Console.ReadLine().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    Console.WriteLine("");

                    bool breakLoop = false;
                    foreach (var item in findLegalMoves.Keys)
                    {
                        if (input[0] == item.Row && input[1] == item.Col)
                        {
                            othelloGame.MakeMove(new Position(input[0], input[1]));
                            breakLoop = true;
                            break;
                        }
                    }

                    if (breakLoop)
                    {
                        break;
                    }
                }

                othelloGame.PassTurn();
                consoleUI.ShowGame();
            }

            consoleUI.ShowGame();
            Console.WriteLine($"Disk {Disc.Black}: {othelloGame.CountDisc[Disc.Black]}");
            Console.WriteLine($"Disk {Disc.White}: {othelloGame.CountDisc[Disc.White]}");
            Console.WriteLine($"Disk {othelloGame.Winner} MENANG!!!!!");
        }
    }
}
