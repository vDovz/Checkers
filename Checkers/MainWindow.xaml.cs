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
        private GameLogic _logic;

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
            _logic = new GameLogic(GameBoard.Field, Player.White);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckersPiece item = ((FrameworkElement)sender).DataContext as CheckersPiece;
            int index = GameBoard.Field.IndexOf(item);

            if (_logic.Selected.IsSelected)
            {
                if (item.Player == _logic.CurrentPlayer && !_logic.AttackContinued)
                {
                    UnSelect(item, index);
                }
                else
                {
                    if (!_logic.MovePlayer(item, index))
                        return;
                }
            }
            else
            {
                if (item.Player != _logic.CurrentPlayer || _logic.AttackContinued)
                {
                    return;
                }
                item.IsSelected = true;
                _logic.Selected.ChangeFields(item.Player, item.Type, item.Pos, index, true);
            }
        }

        private void UnSelect(CheckersPiece item, int index)
        {
            if (_logic.Selected.Index == index)
            {
                item.IsSelected = false;
                _logic.Selected.ChangeSelected();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save.SaveToFile(GameBoard.Field, _logic.CurrentPlayer);
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            Load.LoadFromFile(GameBoard.Field, ref _logic.CurrentPlayer);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}