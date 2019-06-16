using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TrickyUnits;

namespace Yggdrassil.Needed.XSource {

    class NewsItem {
        readonly public NewsBoard Parent;
        readonly string id;
        public readonly DateTime Created;
        public DateTime Modified { get; private set; }
        TGINI Data;
        public string Subject {
            get => Data.C("Subject");
            set { Data.D("Subject",value); Modified = DateTime.Now; Data.D("Modified", Modified.ToLongDateString()); }
        }        
        public string Content {
            get => Data.ListToString("Content");
            set { Data.StringToList("Content", value); Modified = DateTime.Now; Data.D("Modified", Modified.ToLongDateString()); }
        }
        public string FileName => $"{Parent.ItemDir}/{id}.GINI";


        public NewsItem(NewsBoard ouwe,string idcode) {
            Created = DateTime.Now;
            Modified = Created;
            Data.D("Modified", Modified.ToLongDateString());
            Data.D("Created", Created.ToLongDateString());
            Parent = ouwe;
            id = idcode;
            if (id == "*NEW*") {
                int i = -1;
                do {
                    i++;
                    id = qstr.Right($"000000000{i}", 9);
                } while(File.Exists(FileName));
                Data.SaveSource(id);
                return;
            }
            try {
                Data = GINI.ReadFromFile(FileName);
            } catch(Exception crap) {
                Fout.Error($"Error loading: {FileName}\n{crap.Message}\n\nThings may not be loaded correctly (if it is loaded at all)");
            }
        }
    }

    class NewsBoard {
        readonly public Project Parent;
        readonly public string id;
        public string ItemDir => $"{Parent.NewsDir}/NewsItems";
        public NewsBoard(Project ouwe, string idcode) {
            Parent = ouwe;
            id = idcode;
            Directory.CreateDirectory(ItemDir);
        }
    }
}
