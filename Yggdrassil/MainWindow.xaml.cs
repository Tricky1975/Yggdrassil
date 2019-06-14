// Lic:
// Yggdrassil
// Main UI
// 
// 
// 
// (c) Jeroen P. Broks, 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// Please note that some references to data like pictures or audio, do not automatically
// fall under this licenses. Mostly this is noted in the respective files.
// 
// Version: 19.06.14
// EndLic




using System;
using System.Diagnostics;
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
using TrickyUnits;

namespace Yggdrassil {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            Debug.WriteLine("Loading main window");
            MKL.Lic    ("Yggdrassil - MainWindow.xaml.cs","GNU General Public License 3");
            MKL.Version("Yggdrassil - MainWindow.xaml.cs","19.06.14");
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = $"Yggdrassil version {MKL.Newest}";
            Debug.WriteLine(MKL.All());
            VersionDetails.Content = MKL.All();
        }

        private void CenterWindowOnScreen() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}




