// Lic:
// Yggdrassil
// Wiki
// 
// 
// 
// (c) Jeroen P. Broks, 2019
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
// Version: 21.12.11
// EndLic





using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TrickyUnits;



namespace Yggdrassil.Needed.XSource {

    using WikiProfile = Dictionary<string, string>;

    class WikiPage {
        TGINI Data;
        public readonly Wiki ParentWiki;
        public Project Ancestor => ParentWiki.Parent;
        readonly string Name;
        string _Lang;
        public string Lang { set { _Lang = value; }
            get {
                Fout.NFAssert(_Lang, "Request to language done, while this has not yet been set!");
                return _Lang;
            }
        }
        private string ContentTag => $"{Lang}.{Profile}.Content";
        public string Profile { get => Data.C($"{Lang}.PROFILE"); set { Data.D($"{Lang}.Profile", value); Modified(); } }
        public void Set(string Profile, string Key, string Value) => Data.D($"VARIABLE.{Lang}.{Profile}.{Key}", Value);
        public string Get(string Profile, string Key) => Data.C($"VARIABLE.{Lang}.{Profile}.{Key}");
        public string Content {
            get {
                var ret = new StringBuilder(1);
                foreach(string l in Data.List(ContentTag)) {
                    var dubbelepunt = l.IndexOf(':');
                    string cmd, val;
                    if (dubbelepunt<0) { cmd = "IGNORE"; val = l; }
                    cmd = l.Substring(0, dubbelepunt).ToUpper();
                    val = l.Substring(dubbelepunt + 1);
                    for (int i = 0; i < 256; i++) if (val.IndexOf('\\') >= 0) val = val.Replace($"\\{i}", qstr.Chr(i));
                    switch (cmd) {
                        case "WL":
                        case "WHITELINE":
                            ret.Append("\n");
                            break;
                        case "IG":
                        case "IGNORE":
                            break;
                        case "NL":
                        case "ADD":
                        case "NEWLINE":
                            ret.Append($"{val}\n");
                            break;
                        default:
                            Fout.Error($"Illegal instruction: {cmd}");
                            break;
                    }
                }
                return ret.ToString();
            }

            set {
                Data.CL(ContentTag, false);
                var s = new StringBuilder(1);
                for(int i = 0; i < value.Length; ++i) {
                    var b = (byte)value[i];
                    if (b>=32 && value[i]!='\\' && b<128) {
                        s.Append(value[i]);
                    } else {
                        switch (b) {
                            case 10:
                                if (s.Length > 0) {
                                    Data.List(ContentTag).Add($"NL:{s.ToString()}");
                                } else {
                                    Data.List(ContentTag).Add("WL: << White Line >>");
                                }
                                s.Clear();
                                break;
                            case 13:
                                break; // Only Unix line breaks;
                            default:
                                s.Append($"\\{b}");
                                break;
                        }
                    }
                }
                if (s.Length > 0) {
                    Data.List(ContentTag).Add($"NL:{s.ToString()}");
                } else {
                    Data.List(ContentTag).Add("WL: << White Line >>");
                }
            Modified();
            }
        }

        public string WikiPageFile => $"{ParentWiki.WikiPageDir}/{Name}.GINI";

        public string MD5 {
            get {
                if (!File.Exists(WikiPageFile)) return "This has is probably on vacation!";
                return qstr.md5(QuickStream.LoadString(WikiPageFile));
            }
        }

        void Modified() {
            Data.D("MODIFIED.BY", Wiki.MW.TBox_WikiPageUser.Text);
            Data.D("MODIFIED.ON", $"{DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}");
        }

        public void Save() {
            Data.SaveSource(WikiPageFile);
        }

        public WikiPage(Wiki Ouwe, string PageName) {
            ParentWiki = Ouwe;
            Name = PageName;
            if (File.Exists(WikiPageFile))
                Data = GINI.ReadFromFile(WikiPageFile);
            else {
                Data = new TGINI();
                Data.D("CREATED.BY", Wiki.MW.TBox_WikiPageUser.Text);
                Data.D("CREATED.ON", $"{DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}");
                Modified();
                Save();
            }
            Debug.WriteLine($"Created wiki page: {PageName}");
            Fout.Assert(Data,$"Data could not be properly set up on page {PageName}!");
        }
    }

    class Wiki {
        public static MainWindow MW;
        public readonly Project Parent;
        public readonly string WikiName;
        Dictionary<string, WikiProfile> Profiles = new Dictionary<string, WikiProfile>();
        Dictionary<string, WikiPage> Pages = new Dictionary<string, WikiPage>();
        TGINI Data = new TGINI();
        public string[] ProfileList { get { if (Data != null) return Data.List("Profiles").ToArray(); else return null; } }
        public List<string> ProfileListList => Data.List("Profiles");
        public string WikiFile => $"{Parent.WikiMainDir}/{WikiName}.Profiles.GINI";
        public string WikiPageDir => $"{Parent.WikiMainDir}/{WikiName}.Pages";

        public Wiki(Project Ouwe, string wikiName) {
            Parent = Ouwe;
            Parent.Wikis[wikiName] = this;
            WikiName = wikiName;
            Data = GINI.ReadFromFile(WikiFile);
            if (!Fout.NFAssert(Data, $"Wiki profile file {WikiFile} could not be properly read!")) return;
            Data.CL("Profiles");
            Data.List("Profiles").Sort();
            Debug.WriteLine($"Wiki {wikiName} has {Data.List("Profiles").Count} profile(s) => (Check {ProfileList.Length})");
        }

        public string[] Vars => Data.Vars();
        public string GetVar(string key) => Data.C($"{key}");
        public void SetVar(string key, string value) { Data.D($"{key}", value); Data.SaveSource(WikiFile); }

        public WikiPage GetWikiPage (string wp) {
            var pt = $"{wp}";
            if (!Pages.ContainsKey(pt)) {
                Pages[pt] = new WikiPage(this, pt);
            }
            return Pages[pt];
        }

        public void UpdateHash() {
            foreach (string key in Pages.Keys) Data.D($"MD5[{key}]", Pages[key].MD5);
        }

        public void UpdateHash( string key) {
            Data.D($"MD5[{key}]", GetWikiPage(key).MD5);
        }

        public void Save() {            
            try {
                Data.SaveSource(WikiFile);
            } catch (Exception ex) {
                Fout.Error(ex);
            }
        }
    }
}