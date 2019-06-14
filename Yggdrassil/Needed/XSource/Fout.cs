// Lic:
// Yggdrassil
// Error Menagement!
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


#undef FOUT_DoNotCrash

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using TrickyUnits;


namespace Yggdrassil.Needed.XSource {
    static class Fout {
        static Fout() {
            MKL.Lic    ("Yggdrassil - Fout.cs","GNU General Public License 3");
            MKL.Version("Yggdrassil - Fout.cs","19.06.14");
        }
        public static void Crash(string foutmelding) {
            MessageBox.Show($"FATAL ERROR!\n\n{foutmelding}", "That doesn't work!", MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine($"FATAL ERROR:> {foutmelding}");

#if !FOUT_DoNotCrash
            Environment.Exit(1);
#endif
        }


        public static void Crash(Exception foutmelding) => Crash(foutmelding.Message);

        public static void Error(string foutmelding) {
            MessageBox.Show($"ERROR!\n\n{foutmelding}", "That doesn't work!",MessageBoxButton.OK,MessageBoxImage.Error);
            Debug.WriteLine($"ERROR:> {foutmelding}");
        }

        public static void Assert(bool voorwaarde,string foutmelding) {
            if (!voorwaarde) Crash(foutmelding);               
        }

        public static void Assert(int voorwaarde, string foutmelding) => Assert(voorwaarde != 0, foutmelding);
        public static void Assert(string voorwaarde, string foutmelding) => Assert(voorwaarde.Length, foutmelding);
        public static void Assert(object voorwaarde, string foutmelding) => Assert(voorwaarde != null, foutmelding);



        public static bool NFAssert(bool voorwaarde, string foutmelding) {
            if (!voorwaarde) Error(foutmelding);
            return voorwaarde;
        }

        public static bool NFAssert(int voorwaarde, string foutmelding) => NFAssert(voorwaarde != 0, foutmelding);
        public static bool NFAssert(string voorwaarde, string foutmelding) => NFAssert(voorwaarde.Length, foutmelding);
        public static bool NFAssert(object voorwaarde, string foutmelding) => NFAssert(voorwaarde != null, foutmelding);


    }
}


