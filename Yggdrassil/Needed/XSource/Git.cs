// Lic:
// Yggdrassil
// Git handler
// 
// 
// 
// (c) Jeroen P. Broks, 2019, 2024
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
// Version: 24.02.02
// EndLic



using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using TrickyUnits;


namespace Yggdrassil.Needed.XSource {
	class Git {
		static MainWindow MainWindow;
		static public void Register(MainWindow MW) { MainWindow = MW; }
		static Project Prj => Project.Current;

		static bool Call(string cmd, string parameters, bool cls = true) {
			if (Prj == null) return false;
			if (Prj.OutputGit == "") {
				Debug.WriteLine($"Git request denied! No git repository");
				return false;
			}
			var output = new StringBuilder($"{Prj.OutputDir}$ {cmd} {parameters}\n\n");
			QuickStream.PushDir();
			Debug.WriteLine($"Going to dir: {Prj.OutputGit}");
			Directory.SetCurrentDirectory(Prj.OutputGit);
			var pgit = new Process();
			pgit.StartInfo.FileName = cmd;
			pgit.StartInfo.Arguments = parameters;
			pgit.StartInfo.CreateNoWindow = true;
			pgit.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			pgit.StartInfo.RedirectStandardOutput = true;
			pgit.StartInfo.UseShellExecute = false;
			pgit.Start();
			while (!pgit.StandardOutput.EndOfStream) {
				var p = pgit.StandardOutput.ReadLine();
				output.Append($"{p}\n");
				Debug.WriteLine($"GITEXE>{p}");
			}
			pgit.WaitForExit();
			output.Append($"\n\n\nDone! Exit code {pgit.ExitCode}\n\n");
			if (cls)
				MainWindow.GitOutput.Text = output.ToString();
			else
				MainWindow.GitOutput.Text += output.ToString();
			if (pgit.ExitCode != 0) {
				Fout.Error($"git call returned exit code {pgit.ExitCode}\n\n");
				return false;
			}
			return true;
		}

		static bool GIT(string parameters,bool cls=true) {
			return Call("git", parameters,cls);
		}

		static public void AddAndCommit(string commitmessage,params string[] files) {
			if (GIT($"add {string.Join(" ", files)}"))
				Commit(commitmessage, files);
		}

		static public void Commit(string commitmessage,params string[] files) {
			GIT($"commit -m \"{commitmessage.Replace("\"", "'")}\" {string.Join(" ", files)}");
		}

		static public void Push(bool cls=true) {
			GIT("push",cls);
		}
	}
}