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
        private GameBoard board;
        private GameLogic Rules;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            StartGame();
        }

        private void StartGame()
        {
            GameBoard board = new GameBoard(new ObservableCollection<CheckersPiece>());
            CheckersBoard.ItemsSource = GameBoard.Field;
            Rules = new GameLogic(GameBoard.Field, Player.White);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckersPiece item = ((FrameworkElement)sender).DataContext as CheckersPiece;
            int index = GameBoard.Field.IndexOf(item);

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
            Save.SaveToFile(GameBoard.Field, Rules.CurrentPlayer);
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Load.LoadFromFile(GameBoard.Field, ref Rules.CurrentPlayer);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}