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
        private static int _boardSize = 8;

        private ObservableCollection<CheckersPiece> _field;
        public Player CurrentPlayer;
        public Info Selected = new Info(false);
        public bool AttackContinued = false;

        List<Move> alternativeJump = new List<Move>();
        List<Move> maxjump = new List<Move>();
        List<Move> jumps = new List<Move>();

        public GameLogic(ObservableCollection<CheckersPiece> field, Player player)
        {
            this._field = field;
            CurrentPlayer = player;
        }

        private void ChangePlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.Black ? Player.White : Player.Black;
        }

        private void MakeQueen(int index)
        {
            if (CurrentPlayer == Player.White && _field[index].Pos.Y == _boardSize - 1)
                _field[index].Type = PieceType.Queen;
            if (CurrentPlayer == Player.Black && _field[index].Pos.Y == 0)
                _field[index].Type = PieceType.Queen;
        }

        private void ChangeStates(Info selected, CheckersPiece item2, int index2)
        {
            _field[selected.Index].Player = item2.Player;
            _field[selected.Index].Type = item2.Type;
            _field[index2].Player = selected.Player;
            _field[index2].Type = selected.Type;
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

                while (GameBoard.IsInBoard(x + horizontally, y + vertically) && _field[GameBoard.ColRowToPos(x, y)].Player == Player.None)
                {
                    x += horizontally;
                    y += vertically;
                }
                RemovePawn(GameBoard.ColRowToPos(x, y));
            }
        }

        private void RemovePawn(int index)
        {
            _field[index].Type = PieceType.Free;
            _field[index].Player = Player.None;
        }

        public bool MovePlayer(CheckersPiece item, int index)
        {
            BestAttack();
            var from = Selected.Index;
            var valid = IsValidMove(from, index);
            var column = GameBoard.PosToCol(index);
            var row = GameBoard.PosToRow(index);
            Move temp = new Move() { Column = column, Row = row };
            if (valid != -1)
            {
                if (valid == 1)
                {
                    if ((!maxjump.Contains(temp) && !alternativeJump.Contains(temp)) || maxjump.Count == 0)
                    {
                        return false;
                    }
                    ChangeStates(Selected, item, index);
                    RemoveDensePawn(Selected, item);
                    MakeQueen(index);
                    if (maxjump.Count > 1)
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

            if (!GameBoard.IsInBoard(newColumn, newRow))
            {
                return false;
            }

            if (_field[GameBoard.ColRowToPos(newColumn, newRow)].Player == enemy
                && _field[GameBoard.ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                int endRow = newRow + dirY;
                int endColumn = newColumn + dirX;
                return (GameBoard.IsInBoard(endColumn, endRow) && _field[GameBoard.ColRowToPos(endColumn, endRow)].Type == PieceType.Free);
            }
            return false;
        }

        public bool CanJump(int column, int row)
        {
            CheckersPiece piece = _field[GameBoard.ColRowToPos(column, row)];
            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            return CanJump(column, row, 1, -1)
               || CanJump(column, row, -1, 1)
               || CanJump(column, row, -1, -1)
               || CanJump(column, row, 1, 1)
               || CanJumpQueen(column, row, piece, enemy);
        }


        public bool CanJumpQueen(int column, int row, CheckersPiece piece, Player enemy)
        {
            if (piece.Type == PieceType.Queen && CurrentPlayer == piece.Player)
            {
                for (int i = column + 1, j = row + 1; i < _boardSize - 1 && j < _boardSize - 1; i++, j++)
                {
                    if (_field[GameBoard.ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (_field[GameBoard.ColRowToPos(i, j)].Player == enemy && GameBoard.IsInBoard(i + 1, j + 1))
                            if (_field[GameBoard.ColRowToPos(i + 1, j + 1)].Type == PieceType.Free)
                                return true;
                            else break;
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column - 1, j = row + 1; i > 0 && j < _boardSize; i--, j++)
                {
                    if ((_field[GameBoard.ColRowToPos(i, j)].Player != Player.None))
                    {
                        if (_field[GameBoard.ColRowToPos(i, j)].Player == enemy && GameBoard.IsInBoard(i - 1, j + 1))
                            if (_field[GameBoard.ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
                                return true;
                            else break;
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column + 1, j = row - 1; i < _boardSize && j > 0; i++, j--)
                {
                    if (_field[GameBoard.ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (_field[GameBoard.ColRowToPos(i, j)].Player == enemy && GameBoard.IsInBoard(i + 1, j - 1))
                            if (_field[GameBoard.ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
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
                    if (_field[GameBoard.ColRowToPos(i, j)].Player != Player.None)
                    {
                        if (_field[GameBoard.ColRowToPos(i, j)].Player == enemy && GameBoard.IsInBoard(i - 1, j - 1))
                            if (_field[GameBoard.ColRowToPos(i - 1, i - 1)].Type == PieceType.Free)
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
            CheckersPiece pieceFrom = _field[from];
            CheckersPiece pieceTo = _field[to];

            int startCol = GameBoard.PosToCol(from);
            int startRow = GameBoard.PosToRow(from);

            int endCol = GameBoard.PosToCol(to);
            int endRow = GameBoard.PosToRow(to);

            int forward = (CurrentPlayer == Player.White) ? 1 : -1;
            int backward = (pieceTo.Type != PieceType.Queen) ? 0 : (CurrentPlayer == Player.White) ? 1 : -1;
            int backwardAtack = (CurrentPlayer == Player.White) ? -1 : 1;

            var enemy = (CurrentPlayer == Player.White) ? Player.Black : Player.White;

            if (!GameBoard.IsInBoard(startCol, startRow) || !GameBoard.IsInBoard(endCol, endRow))
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

                if (maxjump.Count == 0)
                {
                    //Queen Move
                    if (dirX > 0 && dirY > 0)
                        for (int i = startCol + 1, j = startRow + 1; i < endCol && j < endRow; i++, j++)
                        {
                            if (!(_field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX < 0 && dirY > 0)
                        for (int i = startCol - 1, j = startRow + 1; i > endCol && j < endRow; i--, j++)
                        {
                            if (!(_field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX > 0 && dirY < 0)
                        for (int i = startCol + 1, j = startRow - 1; i < endCol && j > endRow; i++, j--)
                        {
                            if (!(_field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                            {
                                return -1;
                            }
                        }
                    else if (dirX < 0 && dirY < 0)
                        for (int i = startCol - 1, j = startRow - 1; i > endCol && j > endRow; i--, j--)
                        {
                            if (!(_field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
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
                if (maxjump.Count > 0)
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
            alternativeJump = new List<Move>();
            for (int i = 0; i < 32; i++)
            {
                jumps = new List<Move>();
                col = GameBoard.PosToCol(i);
                row = GameBoard.PosToRow(i);
                var tempJumps = AttackWay(col, row, _field);
                if (tempJumps.Count > maxjump.Count)
                {
                    maxjump = new List<Move>(tempJumps);
                    alternativeJump = new List<Move>();
                }
                if (tempJumps.Count == maxjump.Count)
                {
                    foreach (var item in tempJumps)
                    {
                        if(!maxjump.Contains(item))
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
                int index = GameBoard.ColRowToPos(column, row);
                int indexRes = GameBoard.ColRowToPos(item.Column, item.Row);
                int indexDelPawn = GameBoard.ColRowToPos((item.Column + column) / 2, (item.Row + row) / 2);
                ObservableCollection<CheckersPiece> tmpboard = GameBoard.CopyBoard(fld);
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

                    while (GameBoard.IsInBoard(column + dirX, row + dirY) && _field[GameBoard.ColRowToPos(column, row)].Player == Player.None)
                    {
                        column += dirX;
                        row += dirY;
                    }
                    tmpboard[GameBoard.ColRowToPos(column, row)].Player = Player.None;
                    tmpboard[GameBoard.ColRowToPos(column, row)].Type = PieceType.Free;
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

            if (field[GameBoard.ColRowToPos(column, row)].Type == PieceType.Queen
               && field[GameBoard.ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                for (int i = column + 1, j = row + 1; i < _boardSize && j < _boardSize; i++, j++)
                {
                    if (!(field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[GameBoard.ColRowToPos(i, j)].Player == enemy)
                           && GameBoard.IsInBoard(i + 1, j + 1)
                           && field[GameBoard.ColRowToPos(i + 1, j + 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i + 1, Row = j + 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column + 1, j = row - 1; i < _boardSize && j >= 0; i++, j--)
                {
                    if (!(field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[GameBoard.ColRowToPos(i, j)].Player == enemy)
                           && GameBoard.IsInBoard(i + 1, j - 1)
                           && field[GameBoard.ColRowToPos(i + 1, j - 1)].Type == PieceType.Free)
                        {
                            list.Add(new Move() { Column = i + 1, Row = j - 1 });
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = column - 1, j = row + 1; i >= 0 && j < _boardSize; i--, j++)
                {
                    if (!(field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[GameBoard.ColRowToPos(i, j)].Player == enemy)
                           && GameBoard.IsInBoard(i - 1, j + 1)
                           && field[GameBoard.ColRowToPos(i - 1, j + 1)].Type == PieceType.Free)
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
                    if (!(field[GameBoard.ColRowToPos(i, j)].Type == PieceType.Free))
                    {
                        if ((field[GameBoard.ColRowToPos(i, j)].Player == enemy)
                           && GameBoard.IsInBoard(i - 1, j - 1)
                           && field[GameBoard.ColRowToPos(i - 1, j - 1)].Type == PieceType.Free)
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

            if (field[GameBoard.ColRowToPos(column, row)].Type == PieceType.Pawn
                && field[GameBoard.ColRowToPos(column, row)].Player == CurrentPlayer)
            {
                if (GameBoard.IsInBoard(column + 2, row + 2)
                    && field[GameBoard.ColRowToPos(column + 1, row + 1)].Player == enemy
                    && field[GameBoard.ColRowToPos(column + 2, row + 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column + 2, Row = row + 2 });
                }
                if (GameBoard.IsInBoard(column - 2, row + 2)
                    && field[GameBoard.ColRowToPos(column - 1, row + 1)].Player == enemy
                    && field[GameBoard.ColRowToPos(column - 2, row + 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column - 2, Row = row + 2 });
                }
                if (GameBoard.IsInBoard(column + 2, row - 2)
                    && field[GameBoard.ColRowToPos(column + 1, row - 1)].Player == enemy
                    && field[GameBoard.ColRowToPos(column + 2, row - 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column + 2, Row = row - 2 });
                }
                if (GameBoard.IsInBoard(column - 2, row - 2)
                    && field[GameBoard.ColRowToPos(column - 1, row - 1)].Player == enemy
                    && field[GameBoard.ColRowToPos(column - 2, row - 2)].Type == PieceType.Free)
                {
                    list.Add(new Move() { Column = column - 2, Row = row - 2 });
                }
            }
            return list;
        }

    }
}
