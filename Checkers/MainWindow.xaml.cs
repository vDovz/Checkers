using System;
using Checkers.Logic;
using Checkers.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Checkers.ViewModel;

namespace Checkers
{

    public partial class MainWindow
    {
        private int BoardSize = 8;
        private ObservableCollection<CheckersPiece> field;
        private GameLogic Rules;
         
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            StartGame();
        }

        private void StartGame()
        {
            field = new ObservableCollection<CheckersPiece>();
            InitField();
            CheckersBoard.ItemsSource = field;
            Rules = new GameLogic(field, Player.White);
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
                            field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.White });
                        if (i > 4)
                            field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Pawn, Player = Player.Black });
                        if (i >= 3 && i <= 4)
                            field.Add(new CheckersPiece { Pos = new Point(j, i), Type = PieceType.Free, Player = Player.None });
                    }

                }
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckersPiece item = ((FrameworkElement)sender).DataContext as CheckersPiece;
            int index = field.IndexOf(item);

            if (Rules.Selected.IsSelected)
            {
                if (item.Player == Rules.CurrentPlayer && !Rules.AttackContinued)
                {
                    UnSelect(item, index);
                }
                else
                {
                    if (!Rules.MovePlayer(item, index))
                        return;
                }
            }
            else
            {
                if (item.Player != Rules.CurrentPlayer || Rules.AttackContinued)
                {
                    return;
                }
                item.IsSelected = true;
                Rules.Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
            }
        }

        private void UnSelect(CheckersPiece item, int index)
        {
            if (Rules.Selected.Index == index)
            {
                item.IsSelected = false;
                Rules.Selected.ChangeSelected();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save.SaveToFile(field, Rules.CurrentPlayer);
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Load.LoadFromFile(field, ref Rules.CurrentPlayer);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}