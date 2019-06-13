// Lic:
// Yggdrassil
// Configuration
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
// Version: 19.06.13
// EndLic


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using TrickyUnits;

namespace Yggdrassil.Needed.XSource {
    static class Config {
        static TGINI config;
        static string Dir => Dirry.C("$AppSupport$").Replace("\\","/");
        static string File => $"{Dir}/Yggdrassil_MainConfig.GINI";

        static void Print(params string[] s) {
            foreach (string qs in s) Debug.Write($"{qs}\t");
            Debug.WriteLine("");
        }


        static public void Load() {
            MKL.Version("Yggdrassil - Config.cs","19.06.13");
            MKL.Lic    ("Yggdrassil - Config.cs","GNU General Public License 3");
            GINI.Hello();
            Print("Searching for:", File);
            Fout.Assert(System.IO.File.Exists(File), $"Configuration file \"{File}\" not found!");
            Print("Loading");
            config = GINI.ReadFromFile(File);
            Print("Ready"); // Yeah, I did use a Commodore 64, long ago!

        }
    }
}


