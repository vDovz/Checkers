using Checkers.Model;
using Checkers.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Checkers
{
    class Save
    { 
        public static void SaveToFile(ObservableCollection<CheckersPiece> field, Player currentPlayer)
        {
            using (File.Create("UserSave.txt")) ;
            string[] arr = new string[field.Count() + 1];
            for (int i = 0; i < field.Count(); i++)
            {
                string str = field[i].Pos.X.ToString() + ";" + field[i].Pos.Y + ";" + field[i].Player + ";" + field[i].Type + ";" + field[i].IsSelected;
                arr[i] = str;
            }
            arr[field.Count()] = currentPlayer.ToString();
            File.AppendAllLines("UserSave.txt", arr);
            MessageBox.Show("Game save");
        }

    }
}
