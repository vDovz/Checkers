using System.Collections.Generic;
using System.Collections.ObjectModel;
using Checkers.Model;
using Checkers.ViewModel;
using System;
using System.Windows;

namespace Checkers.Logic
{
    class GameLogic
    {
        private static int BoardSize = 8;

        private ObservableCollection<CheckersPiece> field;
        public Player CurrentPlayer;
        public Info Selected = new Info(false);
        public bool AttackContinued = false;

        List<Move> alternativeJump = new List<Move>();
        List<Move> maxjump = new List<Move>();
        List<Move> jumps = new List<Move>();

        public GameLogic(ObservableCollection<CheckersPiece> field, Player player)
        {
            this.field = field;
            CurrentPlayer = player;
        }

        private void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
        }

        private bool IsInBoard(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }

        private static int PosToCol(int value)
        {
            if ((value / 4) % 2 != 0)
            {
                return (value % 4) * 2 + 1;
            }
            return (value % 4) * 2;

        }

        private static int PosToRow(int value)
        {
            return value / 4;
        }

        private static int ColRowToPos(int col, int line)
        {
            if (line % 2 == 0)
                return line * 4 + (col + 1) / 2;
            return line * 4 + col / 2;
        }

        private void MakeQueen(int index)
        {
            if (CurrentPlayer == Player.White && field[index].Pos.Y == BoardSize - 1)
                field[index].Type = PieceType.Queen;
            if (CurrentPlayer == Player.Black && field[index].Pos.Y == 0)
                field[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            field[selected.Index].Player = item2.Player;
            field[selected.Index].Type = item2.Type;
            field[index2].Player = selected.Player;
            field[index2].Type = selected.Type;
        }

        private void RemoveDensePawn(Info selected, CheckersPiece item)
        {
            if (selected.Type == PieceType.Pawn)
            {
                var player = item.Pos.Y > selected.Pos.Y ? 1 : -1;
                var value = ((int)item.Pos.Y % 2 == 0) ? (player == 1 ? 3 : 4) : (player == 1 ? 4 : 3);
                var indexx = item.Pos.X < selected.Pos.X ? (player == 1 ? 0 : 1) : (player == 1 ? 1 : 0);
                RemovePawn(selected.Index + player * (value + indexx));
            }
            if (selected.Type == PieceType.Queen)
            {
                int x = (int)item.Pos.X;
                int y = (int)item.Pos.Y;
                int vertically = y < selected.Pos.Y ? 1 : -1;
                int horizontally = x < selected.Pos.X ? 1 : -1;

                x += horizontally;
                y += vertically;

                while (IsInBoard(x + horizontally, y + vertically) && field[ColRowToPos(x, y)].Player == Player.None)
                {
                    x += horizontally;
                    y += vertically;
                }
                RemovePawn(ColRowToPos(x, y));
            }
        }

        private void RemovePawn(int index)
        {
            field[index].Type = PieceType.Free;
            field[index].Player = Player.None;
        }

        public bool MovePlayer(CheckersPiece item, int index)
        {
            BestAttack();
            var from = Selected.Index;
            var valid = IsValidMove(from, index);
            var column = PosToCol(index);
            var row = PosToRow(index);
            Move temp = new Move() { Column = column, Row = row };
            if (valid != -1)
            {
                if (valid == 1)
                {
                    if (!maxjump.Contains(temp) && !alternativeJump.Contains(temp))
                    {
                        return false;
                    }
                    ChangeStates(Selected, item, index);
                    RemoveDensePawn(Selected, item);
                    MakeQueen(index);
                    if (CanJump(column, row))
                    {
                        AttackContinued = true;
                        item.IsSelected = true;
                        Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
                        return false;
                    }
                }
                if (valid == 0)
                {
                    ChangeStates(Selected, item, index);
                }
                item.IsSelected = false;
                Selected.ChangeSelected();
                MakeQueen(index);
                ChangePlayer();
                AttackContinued = false;
                return true;
            }
            return false;
        }

        public bool CanJump(int column, int row, int dirX, int dirY)
        {
            int newColumn = column + dirX;
            int newRow = row + dirY;

            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            if (!IsInBoard(newColumn, newRow))
            {
                return false;
            }

            if (field[ColRowToPos(newColumn, newRow)].Player == enemy
                && field[ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                int endRow = newRow + dirY;
                int endColumn = newColumn + dirX;
                return (IsInBoard(endColumn, endRow) && field[ColRowToPos(endColumn, endRow)].Type == PieceType.Free);
            }
            return false;
        }

        public bool CanJump(int column, int row)
        {
            CheckersPiece piece = field[ColRowToPos(column, row)];
            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            return CanJump(column, row, 1, -1)
               || CanJump(column, row, -1, 1)
               || CanJump(column, row, -1, -1)
               || CanJump(column, row, 1, 1)
               || CanJumpQueen(column, row, piece, enemy);
        }

        public bool CanJump()
        {
            int col;
            int row;
            for (int i = 0; i < 32; i++)
            {
                col = PosToCol(i);
                row = PosToRow(i);
                if ((field[i].Type == PieceType.Pawn || field[i].Type == PieceType.Queen)
                    && CanJump(col, row))
                    return true;
            }
            return false;
        }

        public bool CanJumpQueen(int column, int row, CheckersPiece piece, Player enemy)
        {
            if (piece.Type == PieceType.Queen && CurrentPlayer == piece.Player)
            {
                for (int i = column + 1, j = row + 1; i < BoardSize - 1 && j < BoardSize - 1; i++, j++)
                {
                    if (field[ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (field[ColRowToPos(i, j)].Player == enemy && IsInBoard(i + 1, j + 1))
                            if (field[ColRowToPos(i + 1, j + 1)].Type == PieceType.Free)
                                return true;
                            else break;
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column - 1, j = row + 1; i > 0 && j < BoardSize; i--, j++)
                {
                    if ((field[ColRowToPos(i, j)].Player != Player.None))
                    {
                        if (field[ColRowToPos(i, j)].Player == enemy && IsInBoard(i - 1, j + 1))
                            if (field[ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
                                return true;
                            else break;
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column + 1, j = row - 1; i < BoardSize && j > 0; i++, j--)
                {
                    if (field[ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (field[ColRowToPos(i, j)].Player == enemy && IsInBoard(i + 1, j - 1))
                            if (field[ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
                                return true;
                            else
                                break;
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column - 1, j = row - 1; i > 0 && j > 0; i--, j--)
                {
                    if (field[ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (field[ColRowToPos(i, j)].Player == enemy && IsInBoard(i - 1, j - 1))
                            if (field[ColRowToPos(i - 1, i - 1)].Type == PieceType.Free)
                                return true;
                            else
                                break;
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        public int IsValidMove(int from, int to)
        {
            CheckersPiece pieceFrom = field[from];
            CheckersPiece pieceTo = field[to];

            int startCol = PosToCol(from);
            int startRow = PosToRow(from);

            int endCol = PosToCol(to);
            int endRow = PosToRow(to);

            int forward = (CurrentPlayer == Player.White) ? 1 : -1;
            int backward = (pieceTo.Type != PieceType.Queen) ? 0 : (CurrentPlayer == Player.White) ? 1 : -1;
            int backwardAtack = (CurrentPlayer == Player.White) ? -1 : 1;

            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            if (!IsInBoard(startCol, startRow) || !IsInBoard(endCol, endRow))
            {
                return -1;
            }
            if (pieceTo.Type != PieceType.Free)
            {
                return -1;
            }

            if (pieceFrom.Type == PieceType.Queen)
            {
                int dirX = endCol > startCol ? 1 : -1;
                int dirY = endRow > startRow ? 1 : -1;

                if (!CanJump())
                {
                    //Queen Move
                    if (dirX > 0 && dirY > 0)
                        for (int i = startCol + 1, j = startRow + 1; i < endCol && j < endRow; i++, j++)
                        {
                            if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX < 0 && dirY > 0)
                        for (int i = startCol - 1, j = startRow + 1; i > endCol && j < endRow; i--, j++)
                        {
                            if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX > 0 && dirY < 0)
                        for (int i = startCol + 1, j = startRow - 1; i < endCol && j > endRow; i++, j--)
                        {
                            if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX < 0 && dirY < 0)
                        for (int i = startCol - 1, j = startRow - 1; i > endCol && j > endRow; i--, j--)
                        {
                            if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    return 0;
                }
                else
                {
                    //Queen atack
                    return 1;
                }
            }
            if (Math.Abs(endRow - startRow) == 1)
            {
                if (CanJump())
                {
                    return -1;
                }

                if ((Math.Abs(endCol - startCol) == 1) && (startRow + forward == endRow || startRow + backward == endRow))
                {
                    return 0;
                }
            }
            else if (Math.Abs(endRow - startRow) == 2)
            {
                //PawnAttack
                return 1;
            }
            return -1;
        }

        public void BestAttack()
        {
            int col;
            int row;
            maxjump = new List<Move>();
            for (int i = 0; i < 32; i++)
            {
                jumps = new List<Move>();
                col = PosToCol(i);
                row = PosToRow(i);
                var tempJumps = AttackWay(col, row, field);
                if (tempJumps.Count > maxjump.Count)
                {
                    maxjump = new List<Move>(tempJumps);
                    alternativeJump = new List<Move>();
                }
                if (tempJumps.Count == maxjump.Count)
                {
                    foreach (var item in tempJumps)
                    {
                        alternativeJump.Add(item);
                    }
                }
            }
        }

        public List<Move> AttackWay(int column, int row, ObservableCollection<CheckersPiece> fld)
        {
            List<Move> maxjumps = new List<Move>();
            foreach (var item in AttackMove(column, row, fld))
            {
                int index = ColRowToPos(column, row);
                int indexRes = ColRowToPos(item.Column, item.Row);
                int indexDelPawn = ColRowToPos((item.Column + column) / 2, (item.Row + row) / 2);
                ObservableCollection<CheckersPiece> tmpboard = CopyBoard(fld);
                CheckersPiece pawn = fld[index];
                tmpboard[indexRes] = new CheckersPiece() { Player = pawn.Player, Type = pawn.Type, Pos = new Point(item.Column, item.Row) };
                tmpboard[index].Player = Player.None;
                tmpboard[index].Type = PieceType.Free;
                if (pawn.Type == PieceType.Pawn)
                {
                    tmpboard[indexDelPawn].Player = Player.None;
                    tmpboard[indexDelPawn].Type = PieceType.Free;
                }
                if (pawn.Type == PieceType.Queen)
                {
                    int dirX = item.Column > column ? 1 : -1;
                    int dirY = item.Row > row ? 1 : -1;

                    column += dirX;
                    row += dirY;

                    while (IsInBoard(column + dirX, row + dirY) && field[ColRowToPos(column, row)].Player == Player.None)
                    {
                        column += dirX;
                        row += dirY;
                    }
                    tmpboard[ColRowToPos(column, row)].Player = Player.None;
                    tmpboard[ColRowToPos(column, row)].Type = PieceType.Free;
                }

                if (!jumps.Contains(item))
                {
                    jumps.Add(item);
                }
                AttackWay(item.Column, item.Row, tmpboard);
                if (jumps.Count > maxjumps.Count)
                {
                    maxjumps = new List<Move>(jumps);
                }
            }
            return maxjumps;
        }

        public List<Move> AttackMove(int column, int row, ObservableCollection<CheckersPiece> field)
        {
            List<Move> list = new List<Move>();
            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            if (field[ColRowToPos(column, row)].Type == PieceType.Queen
               && field[ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                for (int i = column + 1, j = row + 1; i < BoardSize && j < BoardSize; i++, j++)
                {
                    if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[ColRowToPos(i, j)].Player == enemy)
                           && IsInBoard(i + 1, j + 1)
                           && field[ColRowToPos(i + 1, j + 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i + 1, Row = j + 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column + 1, j = row - 1; i < BoardSize && j >= 0; i++, j--)
                {
                    if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[ColRowToPos(i, j)].Player == enemy)
                           && IsInBoard(i + 1, j - 1)
                           && field[ColRowToPos(i + 1, j - 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i + 1, Row = j - 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column - 1, j = row + 1; i >= 0 && j < BoardSize; i--, j++)
                {
                    if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[ColRowToPos(i, j)].Player == enemy)
                           && IsInBoard(i - 1, j + 1)
                           && field[ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i - 1, Row = j + 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                for (int i = column - 1, j = row - 1; i >= 0 && j >= 0; i--, j--)
                {
                    if (!(field[ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[ColRowToPos(i, j)].Player == enemy)
                           && IsInBoard(i - 1, j - 1)
                           && field[ColRowToPos(i - 1, j - 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i - 1, Row = j - 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }

            if (field[ColRowToPos(column, row)].Type == PieceType.Pawn
                && field[ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                if (IsInBoard(column + 2, row + 2)
                    && field[ColRowToPos(column + 1, row + 1)].Player == enemy
                    && field[ColRowToPos(column + 2, row + 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column + 2, Row = row + 2 });
                }
                if (IsInBoard(column - 2, row + 2)
                    && field[ColRowToPos(column - 1, row + 1)].Player == enemy
                    && field[ColRowToPos(column - 2, row + 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column - 2, Row = row + 2 });
                }
                if (IsInBoard(column + 2, row - 2)
                    && field[ColRowToPos(column + 1, row - 1)].Player == enemy
                    && field[ColRowToPos(column + 2, row - 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column + 2, Row = row - 2 });
                }
                if (IsInBoard(column - 2, row - 2)
                    && field[ColRowToPos(column - 1, row - 1)].Player == enemy
                    && field[ColRowToPos(column - 2, row - 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column - 2, Row = row - 2 });
                }
            }
            return list;
        }

        private ObservableCollection<CheckersPiece> CopyBoard(ObservableCollection<CheckersPiece> field)
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
