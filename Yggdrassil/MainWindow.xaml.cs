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
// Version: 19.06.21
// EndLic



using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        List<ComboBox> LanguageCombo = new List<ComboBox>();
        List<Image> Avatars = new List<Image>();
        List<TextBox> Users = new List<TextBox>();
        List<UIElement> NeedsPage = new List<UIElement>();
        List<UIElement> NeedsWiki = new List<UIElement>();
        List<UIElement> NeedsWikiProfile = new List<UIElement>();
        public bool AutoAdept = true;

        public MainWindow() {
            Debug.WriteLine("Loading main window");
            MKL.Lic    ("Yggdrassil - MainWindow.xaml.cs","GNU General Public License 3");
            MKL.Version("Yggdrassil - MainWindow.xaml.cs","19.06.21");
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Title = $"Yggdrassil version {MKL.Newest}";
            Debug.WriteLine(MKL.All());
            VersionDetails.Content = MKL.All();
            Copyrightlabel.Content = $"(c) {MKL.CYear(2019)} Jeroen P. Broks, released under the terms of the GPL3";
            NeedsProject.Add(ConfigTab);
            NeedsProject.Add(NewsTab);
            NeedsProject.Add(GitPush);
            NeedsProject.Add(Tab_Page);
            NeedsProject.Add(Tab_Wiki);
            NeedsNewsBoard.Add(DoRemoveNewsPage);
            NeedsNewsBoard.Add(TBox_NewsTemplate);
            NeedsNewsBoard.Add(TBox_PreNewsText);
            NeedsNewsBoard.Add(CB_NewsEdit);
            NeedsNewsBoard.Add(TBox_NewsSubject);
            NeedsNewsBoard.Add(NewsCommit);
            NeedsNewsBoard.Add(TBox_NewsContent);
            NeedsNewsBoard.Add(SaveNewsItem);
            NeedsPage.Add(PageContentGroup);
            NeedsPage.Add(DeletePage);
            NeedsWiki.Add(WikiSubTab);
            NeedsWikiProfile.Add(WikiProfileSelectType);
            NeedsWikiProfile.Add(WikiProfileVar);
            NeedsWikiProfile.Add(WikiProfileVariableShow);
            NeedsWikiProfile.Add(AddVariable);
            LanguageCombo.Add(PageLanguage);
            Avatars.Add(Avatar_NewsItem);
            Avatars.Add(Page_Avatar);
            Users.Add(TBox_NewsItem_User);
            Users.Add(TBox_PageUser);
            Project.RegisterMainWindow(this);
            Needed.XSource.Page.Register(this);
            Wiki.MW = this;
            Git.Register(this);
            RefreshProjectList();
        }

        bool HaveProject => ListProjects.SelectedItem != null;
        bool HaveNewsBoard => HaveProject && ListNewsBoards.SelectedItem != null;
        bool HavePage => HaveProject && Pages.SelectedItem != null && PageLanguage.SelectedItem != null && TBox_PageUser.Text.Trim()!="";
        bool HaveWiki => HaveProject && List_Wikis.SelectedItem != null;
        bool HaveProfile => HaveWiki && List_WikiProfile.SelectedItem != null;

        void AutoEnable(List<UIElement> GadgetList,bool condition) {
            foreach (UIElement Elem in GadgetList) {
                Elem.IsEnabled = condition;
            }

        }

        void EnableElements() {
            AutoEnable(NeedsProject, HaveProject);
            AutoEnable(NeedsNewsBoard, HaveNewsBoard);
            AutoEnable(NeedsPage, HavePage);
            AutoEnable(NeedsWiki, HaveWiki);
            AutoEnable(NeedsWikiProfile, HaveProfile);
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
            var l = FileList.GetDir(Project.Current.NewsDir, 0);
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
                for (int i = nnb.ainii; i > 0; --i) {
                    var tid = qstr.Right($"000000000{i}", 9);
                    if (File.Exists($"{nnb.ItemDir}/{tid}.GINI")) {
                        if (nnb.Items.ContainsKey(tid)) {
                            CB_NewsEdit.Items.Add($"{tid}; {nnb.Items[tid].Subject}");
                        } else {
                            CB_NewsEdit.Items.Add($"{tid}; << NOT LOADED >>");
                        }
                    }
                }
            }
            AutoAdept = true;
        }

        public void ReSelectNews(string id) {
            var idx = -1;
            foreach (var it in CB_NewsEdit.Items) {
                idx++;
                var istr = it.ToString();
                if (qstr.Prefixed(istr, id)) CB_NewsEdit.SelectedIndex = idx;
            }
        }

        public void ReSelectNews(int id) => ReSelectNews(qstr.Right($"000000000{id}", 9));

        

        void RefreshLanguages() {
            foreach(ComboBox Lng in LanguageCombo) {
                Lng.Items.Clear();
                foreach (string code in Project.Current.Language.Keys) Lng.Items.Add(code);
                Lng.SelectedIndex = 0;
            }
        }

        public void UpdateUI() {
            AutoAdept = false;
            TBox_OutputFolder.Text = Project.Current.OutputDir;
            TBox_DefaultTemplate.Text = Project.Current.DefaultTemplate;
            TBox_Languages.Text = Project.Current.Translations;
            TBox_Users.Text = Project.Current.Users;
            TBox_NewsItem_User.Text = Project.Current.LastUser;
            TBox_PageUser.Text = Project.Current.LastUser;
            RefreshNewsBoards();
            RefreshPages();
            RefreshLanguages();
            RefreshWikis();
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

            // Generate News
            nb.Generate();

            // Git call
            var cmm = NewsCommit.Text;
            if (cmm == "") cmm = "Update news";
            Git.AddAndCommit(cmm, "*");
            NewsCommit.Text = "";

            // Cleanup after new!
            var nin = CB_NewsEdit.SelectedItem.ToString();
            if (nin=="*NEW*") {
                TBox_NewsContent.Text = "";
                TBox_NewsSubject.Text = "";
            }
            // Update news if an item has been written or edited
            RefreshNewsEntries();
        }

        private void GitPush_Click(object sender, RoutedEventArgs e) {
            Git.Push();
        }

        private void TBox_Languages_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) Project.Current.Translations = TBox_Languages.Text;
        }

        private void TBox_Users_TextChanged(object sender, TextChangedEventArgs e) {
            if (AutoAdept) Project.Current.Users = TBox_Users.Text;
        }

        void UpdateAvatars(string nu) {
            if (Project.Current.Avatar.ContainsKey(nu)) {
                Debug.WriteLine($"Avatar found for user {nu} -- Loading: {Project.Current.Avatar[nu]}");
                BitmapImage bi3 = new BitmapImage();
                bi3.BeginInit();
                bi3.UriSource = new Uri(Project.Current.Avatar[nu]); //, UriKind.Relative);
                bi3.EndInit();
                foreach (Image av in Avatars) {
                    av.Stretch = Stretch.Fill;
                    av.Source = bi3;
                }
            } else {
                Debug.WriteLine($"No avatar for user {nu} so skipping that request!");
            }

        }

        void SyncUsers(TextBox u) {
            var aa = AutoAdept;
            AutoAdept = false;
            foreach(TextBox iu in Users) {
                if (iu != u) iu.Text = u.Text;
            }
            AutoAdept = aa;
        }

        private void TBox_NewsItem_User_TextChanged(object sender, TextChangedEventArgs e) {
            var nu = TBox_NewsItem_User.Text;
            if (AutoAdept) {
                Project.Current.LastUser = nu;
                SyncUsers(TBox_NewsItem_User);
            }
            UpdateAvatars(nu);
        }

        void SyncLanguages(ComboBox caller) {
            foreach(ComboBox Lng in LanguageCombo) {
                if (Lng != caller) Lng.SelectedIndex = caller.SelectedIndex;
            }
            PageContent_Update();
        }

        private void PageLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SyncLanguages(PageLanguage);            
        }

        private void TBox_PageUser_TextChanged(object sender, TextChangedEventArgs e) {
            var nu = TBox_PageUser.Text;
            if (AutoAdept) {
                Project.Current.LastUser = nu;
                SyncUsers(TBox_PageUser);
            }
            UpdateAvatars(nu);

        }

        void RefreshPages() {
            Pages.Items.Clear();
            if (Directory.Exists(Project.Current.PageDir)) {
                foreach (string f in FileList.GetDir(Project.Current.PageDir)) {
                    if (qstr.Suffixed(f, ".GINI") && qstr.Prefixed(f,"Page_")) Pages.Items.Add(qstr.Left(f, f.Length - 5).Substring(5)                        );
                }
            }
        }

        private void CreatePage_Click(object sender, RoutedEventArgs e) {
            var newpage = NameNewPage.Text.Trim().ToUpper();
            var cp = Project.Current;
            var lng = PageLanguage.SelectedItem.ToString();
            var file = $"{cp.PageDir}/Page_{newpage}.GINI";
            var user = TBox_PageUser.Text.ToUpper();            
            if (!(
                Fout.NFAssert(newpage, "The new page needs a name!") &&
                Fout.NFAssert(!File.Exists(file), "That page already exists!")
                )) return;
            var np = new TGINI();
            np.D("CreationDate", DateTime.Now.ToLongDateString());
            np.D("ModifyDate", DateTime.Now.ToLongDateString());
            np.D("CreationUser", user);
            np.D("ModifyUser", user);
            Debug.WriteLine($"Creating: {file}");
            try {
                Directory.CreateDirectory(cp.PageDir);
                np.SaveSource(file);
            } catch (Exception X) {
                Fout.Error(X);
            }
            RefreshPages();
        }

        void PageContent_Update() {
            if (HavePage) PageContent.Text = Project.Current.GetPage(Pages.SelectedItem.ToString()).Content;
        }

        private void Pages_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            PageContent_Update();
            EnableElements();
        }

        private void PageContent_TextChanged(object sender, TextChangedEventArgs e) {
            Project.Current.GetPage(Pages.SelectedItem.ToString()).Content = PageContent.Text;
        }

        private void Button_Save_page_Click(object sender, RoutedEventArgs e) {
            var pageid = Pages.SelectedItem.ToString();
            var page = Project.Current.GetPage(Pages.SelectedItem.ToString());
            page.Save();
            page.Generate();
        }

        private void CB_NewsEdit_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!Fout.NFAssert(CB_NewsEdit.SelectedItem,"INTERNAL ERROR!\n\nFor some silly reasons the news selector returns 'null'.")) return;
            var eid = CB_NewsEdit.SelectedItem.ToString();
            if (eid=="*NEW*") {
                TBox_NewsSubject.Text = "";
                TBox_NewsContent.Text = "";
            } else {
                var puntkomma = eid.IndexOf(";"); Fout.NFAssert(puntkomma > 0, "HUH?");
                var gid = eid.Substring(0, puntkomma); Debug.WriteLine($"eid={eid}; puntkomma={puntkomma}; gid={gid}");
                var cnb = ListNewsBoards.SelectedItem.ToString();
                var NEB = Project.Current.GetNewsBoard(cnb);
                var NEI = new NewsItem(NEB, gid);
                TBox_NewsSubject.Text = NEI.Subject;
                TBox_NewsContent.Text = NEI.Content;
            }
        }

        void RefreshWikis() {
            Directory.CreateDirectory($"{Project.Current.WikiMainDir}");
            var wikil = FileList.GetDir(Project.Current.WikiMainDir);
            var suf = ".Profiles.GINI";
            List_Wikis.Items.Clear();
            foreach (string wk in wikil) {
                if (qstr.Suffixed(wk,suf)) {
                    List_Wikis.Items.Add(wk.Substring(0, wk.Length - suf.Length));
                }
            }
        }

        private void DoAddWiki_Click(object sender, RoutedEventArgs e) {
            var nw = AddWikiName.Text;
            if (!(
                Fout.NFAssert(nw,"I need a NAME!") &&
                Fout.NFAssert(!File.Exists($"{Project.Current.WikiMainDir}/{nw}.Profiles.GINI"),"That wiki already exists") &&
                Fout.NFAssert(!Directory.Exists($"{Project.Current.WikiMainDir}/{nw}.Articles"),"Article directory exists, yet profile directory doesn't")
                )) return;
            try {
                QuickStream.SaveString($"{Project.Current.WikiMainDir}/{nw}.Profiles.GINI", "[rem]\nThere is no life in the void!");
                Directory.CreateDirectory($"{Project.Current.WikiMainDir}/{nw}.Articles");
                AddWikiName.Text = "";
            } catch (Exception FoutjeBedankt) {
                Fout.Error(FoutjeBedankt);
            } finally {
                RefreshWikis();
            }
        }

        private void DoRemoveWiki_Click(object sender, RoutedEventArgs e) {

        }

        private void DoAddWikiProfile_Click(object sender, RoutedEventArgs e) {
            if (CurrentWiki == "") return;
            Debug.WriteLine($"Checking profiles for wiki {CurrentWiki}");
            var W = Project.Current.GetWiki(CurrentWiki);
            var WPL = W.ProfileListList;
            var WPName = TBox_NewWikiProfileName.Text;
            if (!(
                Fout.NFAssert(WPName,"Profile needs a NAME!") &&
                Fout.NFAssert(!WPL.Contains(WPName),"That profile already exists!")
                )) return;
            WPL.Add(WPName);
            WPL.Sort();
            W.Save();
            RefreshWikiProfiles();
        }

        private void DoRemoveWikiProfile_Click(object sender, RoutedEventArgs e) {

        }

        private void List_Wikis_SelectionChanged(object sender, SelectionChangedEventArgs e) {            
            EnableElements();
            RefreshWikiProfiles();
        }

        string CurrentWiki {
            get {
                if (List_Wikis.SelectedItem == null) return "";
                return List_Wikis.SelectedItem.ToString();
            }
        }

        string CurrentWikiProfile {
            get {
                if (List_WikiProfile.SelectedItem == null) return "";
                return List_WikiProfile.SelectedItem.ToString();

            }
        }

        void RefreshWikiProfiles() {
            List_WikiProfile.Items.Clear();
            if (CurrentWiki == "") return;
            Debug.WriteLine($"Loading profiles for wiki {CurrentWiki}");
            var W = Project.Current.GetWiki(CurrentWiki);
            foreach (string wkp in W.ProfileList) {
                Debug.WriteLine($"Added profile: {wkp}");
                List_WikiProfile.Items.Add(wkp);
            }
        }
        
        void RefreshWpVars() {
            var r = new StringBuilder(1);
            var W = Project.Current.GetWiki(CurrentWiki);
            if (CurrentWikiProfile == "") {
                OverviewProfileVariables.Text = "Alright, move along!\nThere's nothing to see here!";
                return;
            }
            foreach (string vr in W.Vars) {
                if (qstr.Prefixed(vr, $"VAR.{CurrentWikiProfile}.")) {
                    var v = qstr.RemPrefix(vr,$"VAR.{CurrentWikiProfile}.");
                    var dsplit = W.GetVar(v).Split(':');
                    if (Fout.NFAssert(dsplit.Length>0,$"Invalid definition for {v}")) {
                        r.Append(dsplit[0].ToLower());
                        for (int i = dsplit[0].Length; i <= 10; i++) r.Append(" ");
                        r.Append(vr);
                        if (dsplit[1].Trim() != "") {
                            if (vr.Length < 12)
                                for (int i = vr.Length; i <= 11; i++) r.Append(" ");
                            else {
                                r.Append("\r");
                                for (int i = 1; i <= 21; i++) r.Append(" ");
                            }
                            r.Append($" // {dsplit[1]}");
                        }
                        r.Append("\r");
                    }
                }
            }
            OverviewProfileVariables.Text = r.ToString();
        }

        private void List_WikiProfile_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            EnableElements();
            RefreshWpVars();
        }
    }
}













