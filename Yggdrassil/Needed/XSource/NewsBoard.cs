// Lic:
// Yggdrassil
// News Board Data
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
using System.IO;
using System.Text;
//using System.Linq;
using TrickyUnits;

namespace Yggdrassil.Needed.XSource {

    class NewsItem {
        readonly public NewsBoard Parent;
        readonly public string id;
        public readonly DateTime Created;
        public DateTime Modified { get; private set; }
        internal TGINI Data = new TGINI();
        public string StrCreated => Data.C("Created");
        public string Subject {
            get => Data.C("Subject");
            set { Data.D("Subject",value); Modified = DateTime.Now; Data.D("Modified", Modified.ToLongDateString()); }
        }        
        public string Content {
            get => Data.ListToString("Content");
            set { Data.StringToList("Content", value); Modified = DateTime.Now; Data.D("Modified", Modified.ToLongDateString()); }
        }
        public string Author {
            get => Data.C("Author");
            set {
                Data.D("Author", value);
                Modified = DateTime.Now;
                Data.D("Modified", Modified.ToLongDateString());
                // To Do: Autoset: Avatar/Gravatar
            }

        }
        public string FileName => $"{Parent.ItemDir}/{id}.GINI";

        public void Save() => Data.SaveSource(FileName);
        


        public NewsItem(NewsBoard ouwe,string idcode) {
            Created = DateTime.Now;
            Modified = Created;
            Data.D("Modified", Modified.ToLongDateString());
            Data.D("Created", Created.ToLongDateString());
            Parent = ouwe;
            id = idcode;
            if (id == "*NEW*") {
                /*
                int i = -1;
                do {
                    i++;
                    id = qstr.Right($"000000000{i}", 9);
                } while(File.Exists(FileName));
                Data.SaveSource(id);
                */
                var pid = "";
                if (idcode == "*NEW*") {
                    do {
                        Parent.ainii++;
                        id = qstr.Right($"000000000{Parent.ainii}", 9);
                        pid = $"{id}.GINI";
                    } while (File.Exists($"{Parent.ItemDir}/{pid}"));
                }
                Parent.Items[id] = this;
                return;
            }
            try {
                Parent.Items[id] = this;
                Data = GINI.ReadFromFile(FileName);
                Fout.NFAssert(Data, $"Something didn't go well in loading {FileName}!\n\nCrashes can be expected from this point!");
            } catch(Exception crap) {
                Fout.Error($"Error loading: {FileName}\n{crap.Message}\n\nThings may not be loaded correctly (if it is loaded at all)");
            }
        }
    }

    class NewsBoard {
        readonly public Project Parent;
        readonly public string id;
        readonly public TGINI data = new TGINI();
        public string ItemDir => $"{Parent.NewsDir}/{id}";
        public string GINIFile => $"{Parent.NewsDir}/{id}.GINI";
        public string Template { get => data.C("Template"); set { data.D("Template", qstr.OrText(value,"*DEFAULT*")); Save(); } }
        public string PreText { get => data.ListToString("PreText"); set { data.StringToList("PreText", value); Save(); } }
        readonly public SortedDictionary<string, NewsItem> Items = new SortedDictionary<string, NewsItem>();
        public int ainii { get => qstr.ToInt(data.C("Auto_Increment_News_Item_Index")); set { data.D("Auto_Increment_News_Item_Index", $"{value}"); data.SaveSource(GINIFile); } }
        public string POST_Subject => Project.MW.TBox_NewsSubject.Text;
        public string POST_Content => Project.MW.TBox_NewsContent.Text;
        public string POST_ID {
            get {
                var r = Project.MW.CB_NewsEdit.SelectedItem.ToString();
                if (r != "*NEW*") {
                    var pk = r.IndexOf(';');
                    r = r.Substring(0, pk);
                }
                return r;
            }
        }
        public bool POST => POST_Content != "" && POST_Subject != "";

        public NewsBoard(Project ouwe, string idcode) {
            Parent = ouwe;
            id = idcode;
            Directory.CreateDirectory(ItemDir);
            if (File.Exists(GINIFile)) data = GINI.ReadFromFile(GINIFile);
        }

        void SavePOST() {
            var Item = new NewsItem(this, POST_ID);
            Debug.WriteLine($"Saving news item: {Item.id}");
            if (Item.Author == "") Item.Author = Project.MW.TBox_NewsItem_User.Text;
            Item.Subject = POST_Subject;
            Item.Content = POST_Content;
            Item.Save();
        }

        void Save() {
            Debug.WriteLine($"Saving newsboard: {GINIFile}");
            data.SaveSource(GINIFile);            
        }

        public void Generate() {
            // Create
            var content = new StringBuilder($"<!-- Generated by Yggdrassil on {DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()} -->\n\n");

            // Pre-Text
            content.Append($"<p>{PreText}</p>\n\n");

            // News Items
            if (POST) SavePOST();
            var countdown = 10;
            content.Append("<table id='NewsTable' width='100%'>\n");
            for(int idx=ainii;idx>0 && countdown > 0; idx--) {
                var nid = qstr.Right($"000000000{idx}", 9);
                if (File.Exists($"{ItemDir}/{nid}.GINI")) {
                    countdown--;
                    var bericht = new NewsItem(this, nid);
                    content.Append("\t<tr valign=top><td width='100'>");
                    if (Parent.Avatar.ContainsKey(bericht.Author))
                        content.Append($"<img src=\"{Parent.Avatar[bericht.Author]}\" width=\"100\" alt=\"{bericht.Author}\">");
                    content.Append($"</td><td><h1>{bericht.Subject}</h1><small>By: {bericht.Author}<br>{bericht.StrCreated}</td></tr>");
                    content.Append($"<tr valign=top><td colspan=2>{bericht.Content}</td></tr>\n");
                }
            }
            content.Append("</table>\n\n");

            // Convert to HTML
            var Temp = Template;
            if (Template == "" || Template == "*DEFAULT*") Temp = Parent.DefaultTemplate;
            if (!(Fout.NFAssert(Temp, "No Template") && Fout.NFAssert(Parent.OutputDir,"No output dir configured!"))) return;
            var TemplateString = "";
            var Output = "";
            try {
                TemplateString = QuickStream.LoadString($"{Parent.TemplateDir}/{Temp}");
                if (!Fout.NFAssert(TemplateString, "Template empty or not properly loaded!")) return;
                Output = TemplateString.Replace("[[CONTENT]]", content.ToString());
                var OutFile = $"{Parent.OutputDir}/News_{id}.html";
                Debug.WriteLine($"Saving: {OutFile}");
                QuickStream.SaveString(OutFile, Output);
            } catch (Exception Ex) {
                Fout.Error(Ex);
                return;
            }

        }
    }
}




