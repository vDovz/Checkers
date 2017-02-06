using Checkers.Model;
using Checkers.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Checkers
{
    class Load
    {
        public static void LoadFromFile(ObservableCollection<CheckersPiece> field, ref Player current)
        {
            string content = File.ReadAllText("UserSave.txt");

            string[] arr = content.Split('\n');

            for (int i = 0; i < field.Count; i++)
            {
                string[] arr2 = arr[i].Split(';');
                field[i].Pos = new Point(double.Parse(arr2[0]), double.Parse(arr2[1]));
                field[i].Player = (Player)Enum.Parse(typeof(Player), arr2[2]);
                field[i].Type = (PieceType)Enum.Parse(typeof(PieceType), arr2[3]);
                field[i].IsSelected = bool.Parse(arr2[4]);
            }
            current = (Player)Enum.Parse(typeof(Player), arr[32]);
            MessageBox.Show("Game load");
        }
    }
}
