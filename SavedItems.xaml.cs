using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MiDic
{
    /// <summary>
    /// Interaction logic for SavedItems.xaml
    /// </summary>
    public partial class SavedItems : Window
    {
        public SavedItems()
        {
            InitializeComponent();

            using (var db = new LiteDatabase(@"MyData.db"))
            {
                var col = db.GetCollection<Word>("words");

                var results = col.Query().ToList();
                ItemsList.ItemsSource = results;
                
            }
        }
    }
}
