using System.Collections.Generic;
using System;
using System.Linq;

namespace Othello
{
    public class GameController
    {
        // size row board
        public int rows = 8;
        
        // size colomn board
        public const int cols = 8;
        
        // list player and pick colour disc
        public Dictionary<Disc, IPlayer> Players { get; private set; }
        
        // array of position disc on board will contain disc white, black and none
        public Disc[,] Board { get; }
        
        //curent player turn
        public IPlayer CurrentPlayer { get { return Players[CurrentDisc]; }}
        
        // curent colour disc turn
        public Disc CurrentDisc { get; private set; }
        
        // simpan total disc masing-masing warna
        public Dictionary<Disc, int> CountDisc { get; }
        
        // simpan jumlah kemungkinan legalmove
        private Dictionary<Disc, int> _countPossibelMove = new Dictionary<Disc, int>();
        
        // for who is winner
        public Disc Winner { get; private set; }
        
        // store which position can move disc and who enemy disc will outflanked
        public Dictionary<Position, List<Position>> LegalMoves { get; private set; }
        
        public GameStatus GameStat { get; private set; }

        private List<Position> _outflanked = new List<Position>();

        public Action<IPlayer, Disc> OnPlayerUpdate;
        
        public Action<Disc, Position, List<Position>, List<Position>> OnDiscUpdate;

        public Action<List<Position>> OnPossibleMove;

        public GameController()
        {
            Players = new Dictionary<Disc, IPlayer>();

            // put disc on board
            Board = new Disc[rows, cols];
            Board[3, 3] = Disc.White;
            Board[3, 4] = Disc.Black;
            Board[4, 3] = Disc.Black;
            Board[4, 4] = Disc.White;

            // intial condition on early game
            CountDisc = new Dictionary<Disc, int>()
            {
                { Disc.White, 0 },
                { Disc.Black, 0 }
            };

            LegalMoves = new Dictionary<Position, List<Position>>(); 

            GameStat = GameStatus.NoReady;
            CurrentDisc = Disc.None;
            _countPossibelMove[Disc.Black] = 0;
            _countPossibelMove[Disc.White] = 0;

        }

        public bool AddPlayer(IPlayer player, Disc disc)
        {
            if (GameStat == GameStatus.NoReady && disc != Disc.None)
            {
                if (Players.Count > 0 && Players[disc.Opponent()].Id != player.Id)
                {
                    Players[disc] = player;
                    return true;
                }
                else if (Players.Count == 0)
                {
                    Players[disc] = player;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public Disc CheckPlayer(IPlayer player)
        {
            foreach (var disc in Players)
            {
                if (disc.Value == player)
                {
                    return disc.Key;
                }
            }

            return Disc.None;
        }

        public IPlayer CheckDisc(Disc disc)
        {
            if (Players.ContainsKey(disc))
            {
                return Players[disc];

            }
            else
            {
                return null;
            }
        }

        public bool SetIntialTurn(Disc disc)
        {
            if (GameStat == GameStatus.NoReady && Players.Count > 1 && disc != Disc.None)
            {
                CurrentDisc = disc;
                return true;
            }
            else
            {
                return false;
            }
        }

        private GameStatus StatusGame()
        {
            // check untuk mid game
            if (CountDisc[Disc.Black] > 0 || CountDisc[Disc.White] > 0)
            {
                GameStat = GameStatus.OnGoing;
                return GameStat;
            }
            // check untuk awal game
            else if (CurrentDisc != Disc.None && Players.Count > 1) 
            {
                GameStat = GameStatus.Start;
                return GameStat;
            }
            else
            {
                return GameStat;
            }
        }

        public bool StartGame()
        {
            if (StatusGame() == GameStatus.Start)
            {
                CountDisc[Disc.White] = 2;
                CountDisc[Disc.Black] = 2;
                GameStat = GameStatus.Start;
                return true;
            }
            else
            {
                return false;
            }
        }

        public IPlayer GetCurrentPlayer()
        {
            if (CurrentDisc != Disc.None)
            {
                return Players[CurrentDisc];
            }
            else
            {
                return null;
            }
        }

        public Disc GetCurrentDisc()
        {
            return CurrentDisc;
        }

        public bool ChangeTurn()
        {
            if (CurrentDisc != Disc.None)
            {
                CurrentDisc = CurrentDisc.Opponent();
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ChangeTurn(Disc disc)
        {
            if (CurrentDisc != Disc.None && disc != Disc.None)
            {
                CurrentDisc = disc;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ChangeTurn(IPlayer player)
        {
            if (Players.ContainsValue(player))
            {
                CurrentDisc = Players.FirstOrDefault(x => x.Value == player).Key;
                return true;
            }
            else
            {
                return false;
            }
        }

        public IPlayer GetNextTurn()
        {
            if (CurrentDisc != Disc.None && Players.Count > 1)
            {
                return Players[CurrentDisc.Opponent()];
            }
            else
            {
                return null;
            }
        }

        private bool IsInsideBoard(int r, int c)
        {
            return r >= 0 && r < rows && c >= 0 && c < cols;
        }

        private List<Position> OutflankedInDir(Position pos, Disc disc, int rDelta, int cDelta)
        {
            List<Position> outflanked = new List<Position>();
            int r = pos.Row + rDelta;
            int c = pos.Col + cDelta;

            while (IsInsideBoard(r, c) && Board[r, c] != Disc.None)
            {
                if (Board[r, c] == disc.Opponent())
                {
                    outflanked.Add(new Position(r, c));
                    r += rDelta;
                    c += cDelta;
                }
                else
                {
                    return outflanked;
                }
            }

            return new List<Position>();
        }

        private List<Position> Outflanked(Position pos, Disc disc)
        {
            List<Position> outflanked = new List<Position>();

            for (int rDelta = -1; rDelta <= 1; rDelta++)
            {
                for (int cDelta = -1; cDelta <= 1; cDelta++)
                {
                    if (rDelta == 0 && cDelta == 0)
                    {
                        continue;
                    }
                    else
                    {
                        outflanked.AddRange(OutflankedInDir(pos, disc, rDelta, cDelta));
                    }
                }
            }

            return outflanked;
        }

        public bool IsMoveLegal(Disc disc, Position position)
        {
            if (Board[position.Row, position.Col] != Disc.None)
            {
                return false;
            }
            else
            {
                _outflanked = Outflanked(position, disc);
                return _outflanked.Count > 0;
            }
        }

        public Dictionary<Position, List<Position>> FindLegalMoves()
        {
            LegalMoves = new Dictionary<Position, List<Position>>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Position position = new Position(r, c);
                    if (IsMoveLegal(CurrentDisc, position) && (GameStat == GameStatus.Start || GameStat == GameStatus.OnGoing))
                    {
                        LegalMoves[position] = _outflanked;
                    }
                }
            }

            _countPossibelMove[CurrentDisc] = LegalMoves.Count;
            OnPossibleMove.Invoke(LegalMoves.Keys.ToList());
            return LegalMoves;
        }

        public List<Position> GetLegalMove()
        {
            return LegalMoves.Keys.ToList();
        }

        public bool TryMove(Position position)
        {
            foreach (var item in GetLegalMove())
            {
                if (position.Row == item.Row && position.Col == item.Col)
                {
                    return true;
                }
            }

            return false;
        }

        public bool MakeMove(Position position)
        {
            StatusGame();
            if (!LegalMoves.ContainsKey(position))
            {
                return false;
            }

            _outflanked = LegalMoves[position];
            Board[position.Row, position.Col] = CurrentDisc;
            FlipDiscs();
            UpdateCountDisc(_outflanked.Count);

            OnDiscUpdate.Invoke(CurrentDisc, position, GetLegalMove(),_outflanked);
            return true;
        }

        private void FlipDiscs()
        {
            foreach (Position pos in _outflanked)
            {
                Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
            }
        }

        private void UpdateCountDisc(int outflanked)
        {
            CountDisc[CurrentDisc] += outflanked + 1;
            CountDisc[CurrentDisc.Opponent()] -= outflanked;

        }

        private Disc FindWinner()
        {
            if (CountDisc[Disc.Black] > CountDisc[Disc.White])
            {
                return Disc.Black;
            }
            else if (CountDisc[Disc.Black] < CountDisc[Disc.White])
            {
                return Disc.White;
            }
            else
            {
                return Disc.None;
            }
        }

        public bool PassTurn()
        {
            if (_countPossibelMove[Disc.White] > 0 || _countPossibelMove[Disc.Black] > 0)
            {
                CurrentDisc = CurrentDisc.Opponent();
                return true;
            }
            else
            {
                CurrentDisc = Disc.None;
                Winner = FindWinner();
                GameStat = GameStatus.End;
                return false;
            }
        }

    }
}