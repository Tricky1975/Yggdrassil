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
// Version: 19.06.14
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

        static public Project Current {
            get {
                if (CrPrjName == "") return null;
                if (!PrjDict.ContainsKey(CrPrjName)) return null;
                return PrjDict[CrPrjName];
            }
        }

        static public void Load(string name) {
            var P = new Project();
            PrjDict[name.ToUpper()] = P;
            P.Name = name.ToUpper();
            P.Load();
        }

        static public Project Get(string name,bool setcurrent=false) {
            name = name.ToUpper();
            if (setcurrent) CrPrjName = name;
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
        #endregion
    }
}

