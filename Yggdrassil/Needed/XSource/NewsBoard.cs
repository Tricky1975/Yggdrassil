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
// Version: 19.06.16
// EndLic


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
        readonly public TGINI data;
        public string ItemDir => $"{Parent.NewsDir}/{id}";
        public string GINIFile => $"{Parent.NewsDir}/{id}.GINI";
        public NewsBoard(Project ouwe, string idcode) {
            Parent = ouwe;
            id = idcode;
            Directory.CreateDirectory(ItemDir);
        }
    }
}


