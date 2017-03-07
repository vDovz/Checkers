using Checkers.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Checkers.Model
{
    class GameBoard
    {
        private static int BoardSize = 8;

        public ObservableCollection<CheckersPiece> Field { get; set; }

        public GameBoard(ObservableCollection<CheckersPiece> field)
        {
            Field = field;
            InitField();
        }
        private void InitField()
        {
            for (var i = 0; i < BoardSize; i++)
            {
                for (var j = 0; j < BoardSize; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        if (i < 3)
                            Field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.White });
                        if (i > 4)
                            Field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.Black });
                        if (i >= 3 && i <= 4)
                            Field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Free, Player = Player.None });
                    }
                }
            }
        }

        public ObservableCollection<CheckersPiece> GetField()
        {
            InitField();
            return Field;
        }

        public static bool IsInBoard(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }

        public static int PosToCol(int value)
        {
            if ((value / 4) % 2 != 0)
            {
                return (value % 4) * 2 + 1;
            }
            return (value % 4) * 2;

        }

        public static int PosToRow(int value)
        {
            return value / 4;
        }

        public static int ColRowToPos(int col, int line)
        {
            if (line % 2 == 0)
                return line * 4 + (col + 1) / 2;
            return line * 4 + col / 2;
        }

        public static ObservableCollection<CheckersPiece> CopyBoard(ObservableCollection<CheckersPiece> field)
        {
            ObservableCollection<CheckersPiece> result = new ObservableCollection<CheckersPiece>();
            for (int i = 0; i < field.Count; i++)
            {
                result.Add(new CheckersPiece { Pos = field[i].Pos, Type = field[i].Type, Player = field[i].Player });
            }
            return result;
        }
    }
}
