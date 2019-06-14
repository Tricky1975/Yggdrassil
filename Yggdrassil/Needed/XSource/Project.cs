using System;
using System.Collections.Generic;
using System.Linq;
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

        static void Load(string name) {
            var P = new Project();
            PrjDict[name.ToUpper()] = P;
            P.Load();
        }

        static Project Get(string name,bool setcurrent=false) {
            name = name.ToUpper();
            if (setcurrent) CrPrjName = name;
            if (name == "") return null;
            if (!PrjDict.ContainsKey(name)) return null;
            return PrjDict[name];
        }

        static Project SetCurrent(string name) => Get(name, true);
        #endregion

        #region Actual project data
        string Name = "";
        string Dir => $"{Config.ProjectsDir}/{Name}";
        string GlobalFile => $"{Dir}/{Name}.Global.GINI";
        TGINI Global;

        public void Load() {
            try {
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
