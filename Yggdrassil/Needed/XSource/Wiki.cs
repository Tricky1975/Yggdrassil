// Lic:
// Yggdrassil
// Wiki
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TrickyUnits;



namespace Yggdrassil.Needed.XSource {

    using WikiProfile = Dictionary<string, string>;

    class Wiki {
        public static MainWindow MW;
        public readonly Project Parent;
        public readonly string WikiName;
        Dictionary<string, WikiProfile> Profiles = new Dictionary<string, WikiProfile>();
        TGINI Data = new TGINI();
        public string[] ProfileList { get { if (Data != null) return Data.List("Profiles").ToArray(); else return null; } }
        public string WikiFile => $"{Parent.WikiMainDir}/{WikiName}.Profiles.GINI";

        public Wiki(Project Ouwe,string wikiName) {
            Parent = Ouwe;
            Parent.Wikis[wikiName] = this;
            WikiName = wikiName;            
            Data = GINI.ReadFromFile(WikiFile);
            if (!Fout.NFAssert(Data, $"Wiki profile file {WikiFile} could not be properly read!")) return;
            Data.CL("Profiles");
            Data.List("Profiles").Sort();
            Debug.WriteLine($"Wiki {wikiName} has {Data.List("Profiles").Count} profile(s) => (Check {ProfileList.Length})");
        }
    }
}


