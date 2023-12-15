using System.Collections.Generic;

namespace Othello
{
    public class MoveInfo
    {
        //which player that turn
        public Disc Disc { get; private set; }
        // which disc will move
        public Position Position { get; private set; }
        // which enemy disc will fliped
        public List<Position> Outflanked { get; private set; }

        public MoveInfo()
        {
            
        }

        public MoveInfo(Disc disc, Position position, List<Position> outflanked)
        {
            Disc = disc;
            Position = position;
            Outflanked = new List<Position>();
            Outflanked = outflanked;
        }
    }
}