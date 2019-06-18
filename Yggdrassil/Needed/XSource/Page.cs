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
    }
}
