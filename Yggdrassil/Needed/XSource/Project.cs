// Lic:
// Yggdrassil
// Project data
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using TrickyUnits;


namespace Yggdrassil.Needed.XSource {
    class Project {

        #region Static part
        static Dictionary<string, Project> PrjDict = new Dictionary<string, Project>();
        static public string CrPrjName { get; private set; } = "";
        static MainWindow MW;

        static public void RegisterMainWindow(MainWindow AMW) { MW = AMW; }

        static public Project Current {
            get {
                if (CrPrjName == "") return null;
                if (!PrjDict.ContainsKey(CrPrjName)) return null;
                return PrjDict[CrPrjName];
            }
        }

        static public void Load(string name) {
            if (PrjDict.ContainsKey(name.ToUpper()))
                return;
            var P = new Project();
            PrjDict[name.ToUpper()] = P;
            P.Name = name.ToUpper();
            P.Load();
        }

        static public Project Get(string name,bool setcurrent=false) {
            name = name.ToUpper();
            if (setcurrent) {
                CrPrjName = name;
                MW.Title = $"Yggdrasil version {MKL.Newest}; Project: {name}";
            }
            if (name == "") return null;
            if (!PrjDict.ContainsKey(name)) return null;
            return PrjDict[name];
        }

        static public Project SetCurrent(string name) => Get(name, true);

        static public bool Exists(string name) => Directory.Exists($"{Config.ProjectsDir}/{name}");
        
        #endregion

        #region Actual project data
        string Name = "";
        string Dir => $"{Config.ProjectsDir}/{Name}";
        string GlobalFile => $"{Dir}/{Name}.Global.GINI";
        
        TGINI Global;

        public string OutputDir { get => Global.C("OUTPUTDIR"); set { Global.D("OUTPUTDIR", value); SaveGlobal(); } }
        public string DefaultTemplate { get => Global.C("DEFAULTTEMPLATE"); set { Global.D("DEFAULTTEMPLATE", value); SaveGlobal(); } }
        public string NewsDir { get => $"{Dir}/NewsBoards"; }
        public string TemplateDir => $"{Dir}/Templates";
        public Dictionary<string, NewsBoard> NewsBoards = new Dictionary<string, NewsBoard>();



        public string OutputGit {
            get {
                var s = OutputDir.Split('/');
                for (int i = s.Length - 1; i > 0; i--) {
                    var ts = "";
                    for(int j = 0; j <= i; j++) {
                        if (ts != "") ts += "/";
                        ts += s[j];
                    }
                    if (Directory.Exists($"{ts}/.git")) return ts;
                }
                return "";
            }
        }
        public void Load() {
            try {
                Debug.WriteLine($"Loading project: {Name}");
                Directory.CreateDirectory(Dir);                
                if (File.Exists(GlobalFile))
                    Global = GINI.ReadFromFile(GlobalFile);
                else
                    Global = new TGINI();
            } catch (Exception OhJee) {
                Fout.Crash(OhJee);
            }
        }
        public string Translations {
            get => Global.ListToString("Translations");
            set {
                var k = value.Replace("\r","");
                k = k.Replace(";", "\n");
                Global.StringToList("Translations", k);
                SaveGlobal();
            }
        }


        public string Users {
            get => Global.ListToString("Users");
            set {
                var k = value.Replace("\r", "");
                AvRetOld = null;
                k = k.Replace(";", "\n");
                Global.StringToList("Users", k);
                SaveGlobal();
            }
        }

        Dictionary<string, string> AvRetOld = null;
        public Dictionary<string,string> Avatar {
            get {
                if (AvRetOld != null) return AvRetOld;
                var ret = new Dictionary<string, string>();
                foreach (string lin in Global.List("Users")) {
                    string cmd, value,avatar;
                    int isteken = lin.IndexOf('=');
                    if (Fout.NFAssert(isteken > 0, "Illegal line in avatar bindings!\n\n{lin}")) {
                        cmd = lin.Substring(0, isteken);
                        value = lin.Substring(isteken);  /* 123456789 */
                        avatar = value;
                        if (qstr.Prefixed(value.ToUpper(), "GRAVATAR:")) {
                            // The odd HTTP thing was to prevent links in my Syntax Highlight, as this spooks things up and I don't like that!
                            avatar = $"{"https://"}secure.gravatar.com/avatar/{qstr.md5(value.Substring(9))}?s=200&r=pg"; // &d=https%3A%2F%2Fb6d3e9q9.ssl.hwcdn.net%2Fimg%2Fno-avatar-3.png";
                        }
                        ret[cmd] = avatar;
                    }
                }
                AvRetOld = ret;
                return ret;
            }
        }

        public void SaveGlobal() {
            try {
                Global.SaveSource(GlobalFile);
            } catch (Exception OhJee) {
                Fout.Crash(OhJee);
            }
        }

        public NewsBoard GetNewsBoard(string id) {
            if (!NewsBoards.ContainsKey(id)) {
                NewsBoards[id] = new NewsBoard(this, id);
            }
            return NewsBoards[id];
        }
        #endregion
    }
}






