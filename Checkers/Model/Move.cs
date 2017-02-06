namespace Checkers.Logic
{
    public class Move
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public override int GetHashCode()
        {
            return Column.GetHashCode() + Row.GetHashCode(); 
        }
        public override bool Equals(object obj)
        {
            Move obj1 = (Move) obj;
            return Column == obj1.Column && Row == obj1.Row;
        }
    }
}