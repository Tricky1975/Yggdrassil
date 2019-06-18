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
// Version: 19.06.18
// EndLic










using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
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
using Yggdrassil.Needed.XSource;

namespace Yggdrassil {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        List<UIElement> NeedsProject = new List<UIElement>();
        List<UIElement> NeedsNewsBoard = new List<UIElement>();
        public bool AutoAdept = true;

        public MainWindow() {
            Debug.WriteLine("Loading main window");
            MKL.Lic    ("Yggdrassil - MainWindow.xaml.cs","GNU General Public License 3");
            MKL.Version("Yggdrassil - MainWindow.xaml.cs","19.06.18");
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = $"Yggdrassil version {MKL.Newest}";
            Debug.WriteLine(MKL.All());
            VersionDetails.Content = MKL.All();
            Copyrightlabel.Content = $"(c) {MKL.CYear(2019)} Jeroen P. Broks, released under the terms of the GPL3";
            NeedsProject.Add(ConfigTab);
            NeedsProject.Add(NewsTab);
            NeedsProject.Add(GitPush);
            NeedsNewsBoard.Add(DoRemoveNewsPage);
            NeedsNewsBoard.Add(TBox_NewsTemplate);
            NeedsNewsBoard.Add(TBox_PreNewsText);
            NeedsNewsBoard.Add(CB_NewsEdit);
            NeedsNewsBoard.Add(TBox_NewsSubject);
            NeedsNewsBoard.Add(NewsCommit);
            NeedsNewsBoard.Add(TBox_NewsContent);
            NeedsNewsBoard.Add(SaveNewsItem);
            Project.RegisterMainWindow(this);
            Git.Register(this);
            RefreshProjectList();
        }

        bool HaveProject => ListProjects.SelectedItem != null;
        bool HaveNewsBoard => HaveProject && ListNewsBoards.SelectedItem != null;

        void EnableElements() {
            foreach(UIElement Elem in NeedsProject) {
                Elem.IsEnabled = HaveProject;
            }
            foreach(UIElement Elem in NeedsNewsBoard) {
                Elem.IsEnabled = HaveNewsBoard;
            }
        }

        void RefreshProjectList() {
            var l = FileList.GetDir(Config.ProjectsDir, 2);
            ListProjects.Items.Clear();
            foreach (string p in l)
                ListProjects.Items.Add(p);
            EnableElements();
        }

        void RefreshNewsBoards() {
            Debug.WriteLine("Retrieving news boards");
            if (Project.Current == null) {
                Debug.WriteLine("= REQUEST DENIED!\tNo project loaded yet!");
                return;
            }
            Directory.CreateDirectory(Project.Current.NewsDir);
            var l = FileList.GetDir(Project.Current.NewsDir, 1);
            ListNewsBoards.Items.Clear();
            foreach (string p in l) {
                if (qstr.ExtractExt(p.ToUpper()) == "GINI")
                    ListNewsBoards.Items.Add(qstr.RemSuffix(p, ".GINI"));
                else
                    Debug.WriteLine($"= Denied file {p}\tNot a GINI file! What is it doing here?");
            }
            EnableElements();
            RefreshNewsEntries();
        }

        void RefreshNewsEntries() {
            Debug.WriteLine("Refresh news entries");
            var cp = Project.Current;
            if (cp == null) {
                Debug.WriteLine("= REJECTED!\tNo project");
                return;
            }                
            var nbchosen = ListNewsBoards.SelectedItem;
            if (nbchosen == null) {
                Debug.WriteLine("= REJECTED!\tNo newsboard");
                return;
            }
            AutoAdept = false;
            var nb = nbchosen.ToString();
            var newsdir = $"{cp.NewsDir}/{nb}";
            Directory.CreateDirectory(newsdir);
            CB_NewsEdit.Items.Clear();
            CB_NewsEdit.Items.Add("*NEW*");
            CB_NewsEdit.SelectedIndex = 0;

            if (ListNewsBoards.SelectedItem != null) {
                var nnb = Project.Current.GetNewsBoard(ListNewsBoards.SelectedItem.ToString());
                TBox_NewsTemplate.Text = nnb.Template;
                TBox_PreNewsText.Text = nnb.PreText;
            }
            AutoAdept = true;
        }

        public void UpdateUI() {
            AutoAdept = false;
            TBox_OutputFolder.Text = Project.Current.OutputDir;
            TBox_DefaultTemplate.Text = Project.Current.DefaultTemplate;
            TBox_Languages.Text = Project.Current.Translations;
            RefreshNewsBoards();
            AutoAdept = true;
        }

        private void CenterWindowOnScreen() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e) {
            var prj = TBox_NewProject.Text;
            if (!(Fout.NFAssert(!Project.Exists(prj), $"Project '{prj}' already exists") ||
                Fout.NFAssert(!File.Exists($"{Config.ProjectsDir}/{prj}"), $"There is a file name '{prj}' in the projects directory\n(no files should be there. Please remove it!)")))
                return;
            Debug.WriteLine($"Creating Project: {prj}");
            Project.Load(prj);
            RefreshProjectList();
        }

        private void ListProjects_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var prj = ListProjects.SelectedItem.ToString();
            Project.Load(prj);
            Project.SetCurrent(prj);
            UpdateUI();
            EnableElements();
        }

        private void TBox_OutputFolder_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) {
                Project.Current.OutputDir = TBox_OutputFolder.Text.Replace("\\","/");
            }
            GitRepoLabel.Content = Project.Current.OutputGit;
        }

        private void BrowseOutputFolder_Click(object sender, RoutedEventArgs e) {
            TBox_OutputFolder.Text = FFS.RequestDir();
        }

        private void DoAddNewsPage_Click(object sender, RoutedEventArgs e) {
            var np = AddNewsBoardName.Text.Trim();
            var npf = $"{Project.Current.NewsDir}/{np}.GINI";
            if (!(Fout.NFAssert(np, "Sorry, but the newsboards needs a name!") && Fout.NFAssert(!File.Exists(npf),"That newsboard already exists!"))) return;
            var npg = new TGINI();
            npg.D("NAME", np);
            npg.D("TEMPLATE", "$DEFAULT$");
            Debug.WriteLine($"Saving: {npf}");
            npg.SaveSource(npf);
            AddNewsBoardName.Text = "";
            RefreshNewsBoards();
        }

        private void DoRemoveNewsPage_Click(object sender, RoutedEventArgs e) {
            var rp = ListNewsBoards.SelectedItem.ToString();
            var rpf = $"{Project.Current.NewsDir}/{rp}.GINI";
            var r = MessageBox.Show($"Do you really want to remove news board `{rp}`?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r==MessageBoxResult.Yes) {
                try {
                    File.Delete(rpf);
                    RefreshNewsBoards();
                } catch (Exception ex) {
                    Fout.Error(ex);
                }
            }
        }

        private void ListNewsBoards_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            RefreshNewsEntries();
            //UpdateUI();
            EnableElements();
        }

        private void TBox_DefaultTemplate_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) {
                Project.Current.DefaultTemplate = TBox_DefaultTemplate.Text.Replace("\\", "/");
            }

        }

        private void TBox_NewsTemplate_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) {
                var cp = ListNewsBoards.SelectedItem.ToString();
                Project.Current.GetNewsBoard(cp).Template=TBox_NewsTemplate.Text;
            }
        }

        private void TBox_PreNewsText_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) {
                var cp = ListNewsBoards.SelectedItem.ToString();
                Project.Current.GetNewsBoard(cp).PreText = TBox_PreNewsText.Text;
            }
        }

        private void SaveNewsItem_Click(object sender, RoutedEventArgs e) {
            var cp = ListNewsBoards.SelectedItem.ToString();
            var nb = Project.Current.GetNewsBoard(cp);
            // Update news if an item has been written or edited
            // TODO: This comes later!

            // Generate News
            nb.Generate();

            // Git call
            var cmm = NewsCommit.Text;
            if (cmm == "") cmm = "Update news";
            Git.Commit(cmm, "-a");
        }

        private void GitPush_Click(object sender, RoutedEventArgs e) {
            Git.Push();
        }

        private void TBox_Languages_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) Project.Current.Translations = TBox_Languages.Text;
        }
    }
}










