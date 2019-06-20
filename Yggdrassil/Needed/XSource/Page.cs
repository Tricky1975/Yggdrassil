// Lic:
// Yggdrassil
// Page manager
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
// Version: 19.06.20
// EndLic




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrickyUnits;

namespace Yggdrassil.Needed.XSource {
    class Page {
        Project Parent;
        TGINI Data;        

        static MainWindow MW;
        static public void Register(MainWindow RMW) { MW = RMW; }
        static string LanguageSelectJS = QuickStream.StringFromEmbed("LanguageSelect.js");


        readonly string id;
        public string clanguage {
            get {
                try {                    
                    if (!Fout.NFAssert(MW.PageLanguage.SelectedItem, "Language detection resulted into null value!\n\nThis is likely an internal error! Please report it!")) return "";
                    return MW.PageLanguage.SelectedItem.ToString();
                } catch (Exception huh) {
                    Fout.Error($"INTERNAL ERROR:\n\n{huh.Message}\n\nTraceback:{huh.StackTrace}\n\nPlease report this, as this is very likely the result of a bug!");
                    return "";
                }
            }
        }

        public Page(Project Ouwe,string page) {
            Parent = Ouwe;
            Data = GINI.ReadFromFile($"{Ouwe.PageDir}/Page_{page.Trim().ToUpper()}.GINI");
            id = page;
        }

        public string Content {
            get {
                if (!Fout.NFAssert(clanguage, "I cannot seek content without a language!")) return "";
                return Data.ListToString($"CONTENT.{clanguage}");
            }
            set {
                if (!Fout.NFAssert(clanguage, "I cannot define content without a language!")) return;
                Data.StringToList($"CONTENT.{clanguage}", value);
                Data.D($"ModifyDate", MW.TBox_PageUser.Text);                
            }
        }

        public void Save() {
            System.Diagnostics.Debug.WriteLine($"Saving page: {id}");
            Data.SaveSource($"{Parent.PageDir}/Page_{id.Trim().ToUpper()}.GINI");
        }
        public string Template { get => qstr.OrText(Data.C("Template"),"*DEFAULT*"); set { Data.D("Template", qstr.OrText(value, "*DEFAULT*")); Save(); } }

        public void Generate() {
            var js = new StringBuilder($"// This JavaScript file was generated by Yggdrassil on {DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}\n\nlet PageLangs = {"{}"}\nPageLangs.List=[]\nPageLangs.Names={"{}"}\n\n");
            var Temp = Template;
            if (Template == "" || Template == "*DEFAULT*") Temp = Parent.DefaultTemplate;
            if (!(Fout.NFAssert(Temp, "No Template") && Fout.NFAssert(Parent.OutputDir, "No output dir configured!"))) return;
            var TemplateString = "";
            TemplateString = QuickStream.LoadString($"{Parent.TemplateDir}/{Temp}");
            //var humanlanguageselector = new StringBuilder("<select id='ygg_lang_select'><option value='---'>Select a language for human translatioon</option>");
            foreach (string lng in Project.Current.Language.Keys) {
                var content = Data.ListToString($"Content.{lng}");                
                if (content.Trim() != "") {
                    var outhtml = new StringBuilder($"<!-- Generated by Yggdrassil on {DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()} -->\n\n");
                    outhtml.Append(content);
                    outhtml.Append("\n\n<div id='LangSelector'></div>");
                    outhtml.Append($"<script src='PageLangs_{id}.js'></script>\n");
                    outhtml.Append("<script src='PageLangsSelector.js'></script>\n");
                    outhtml.Append($"<script>ShowSelector('{lng}','Page_+LANG+_{id}.html');</script>\n\n");
                    js.Append($"PageLangs.List[PageLangs.List.length] = '{lng}'\n");
                    js.Append($"PageLangs.Names['{lng}']='{Project.Current.Language[lng]}'\n\n");
                    //humanlanguageselector.Append($"<option value='{lng}'>{Project.Current.Language[lng]}</option>");
                    QuickStream.SaveString($"{Project.Current.OutputDir}/Page_{lng}_{id}.html",TemplateString.Replace("[[CONTENT]]",outhtml.ToString()));
                }
                //humanlanguageselector.Append("</select>");
            }            
            QuickStream.SaveString($"{Project.Current.OutputDir}/PageLangs_{id}.js",js.ToString());
            QuickStream.SaveString($"{Project.Current.OutputDir}/PageLangsSelector.js", LanguageSelectJS);


            Git.AddAndCommit(qstr.OrText(MW.TBox_PageCommit.Text.Trim(), $"Update of page {id}"),$"*/Page_*_{id}.html","*/PageLangsSelector.js",$"*/PageLangs_{id}.js");
            MW.TBox_PageCommit.Text = "";
        }
    }
}



