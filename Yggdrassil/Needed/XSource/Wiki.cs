// Lic:
// Yggdrassil
// Wiki
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
// Version: 24.09.09
// EndLic





using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TrickyUnits;
using System.Windows.Ink;
using System.Linq;

namespace Yggdrassil.Needed.XSource {

	using WikiProfile = Dictionary<string, string>;

	class WikiPage {
		TGINI Data;
		public readonly Wiki ParentWiki;
		public Project Ancestor => ParentWiki.Parent;
		readonly string Name;
		string _Lang="";
		public string Lang { set { _Lang = value; }
			get {				
				Fout.NFAssert(_Lang, $"Request to language done, while this has not yet been set! ({Name})");
#if DEBUG
				/*
				try { throw new Exception("Langauge Fuck"); } catch(Exception e) { Debug.WriteLine(e.StackTrace); }
				//*/
#endif
				//Debug.Assert(_Lang != null && Lang.Length>0,"Lang failure (DEBUG)");
				return _Lang;
			}
		}
		public bool LangSet => _Lang.Length> 0;

		private string ContentTag => $"{Lang}.{Profile}.Content";
		public string Profile { get => Data.C($"{Lang}.PROFILE"); set { Data.D($"{Lang}.Profile", value); Modified(); } }
		public void Set(string Profile, string Key, string Value) => Data.D($"VARIABLE.{Lang}.{Profile}.{Key}", Value);
		public string Get(string Profile, string Key) => Data.C($"VARIABLE.{Lang}.{Profile}.{Key}");

		public List<string> Profiles {
			get {
				return Data.List("Profiles");
			}
		}
		public string Content {
			get {
				var ret = new StringBuilder(1);
				foreach(string l in Data.List(ContentTag)) {
					var dubbelepunt = l.IndexOf(':');
					string cmd, val;
					if (dubbelepunt<0) { cmd = "IGNORE"; val = l; }
					cmd = l.Substring(0, dubbelepunt).ToUpper();
					val = l.Substring(dubbelepunt + 1);
					for (int i = 0; i < 256; i++) if (val.IndexOf('\\') >= 0) val = val.Replace($"\\{i}", qstr.Chr(i));
					switch (cmd) {
						case "WL":
						case "WHITELINE":
							ret.Append("\n");
							break;
						case "IG":
						case "IGNORE":
							break;
						case "NL":
						case "ADD":
						case "NEWLINE":
							ret.Append($"{val}\n");
							break;
						default:
							Fout.Error($"Illegal instruction: {cmd}");
							break;
					}
				}
				return ret.ToString();
			}

			set {
				Data.CL(ContentTag, false);
				var s = new StringBuilder(1);
				for(int i = 0; i < value.Length; ++i) {
					var b = (byte)value[i];
					if (b>=32 && value[i]!='\\' && b<128) {
						s.Append(value[i]);
					} else {
						switch (b) {
							case 10:
								if (s.Length > 0) {
									Data.List(ContentTag).Add($"NL:{s.ToString()}");
								} else {
									Data.List(ContentTag).Add("WL: << White Line >>");
								}
								s.Clear();
								break;
							case 13:
								break; // Only Unix line breaks;
							default:
								s.Append($"\\{b}");
								break;
						}
					}
				}
				if (s.Length > 0) {
					Data.List(ContentTag).Add($"NL:{s.ToString()}");
				} else {
					Data.List(ContentTag).Add("WL: << White Line >>");
				}
				Modified();
				if (value != "" && (!Profiles.Contains(Profile))) { Profiles.Add(Profile); Profiles.Sort(); Debug.WriteLine($"Added profile {Profile}"); }
			}
		}

		public string WikiPageFile => $"{ParentWiki.WikiPageDir}/{Name}.GINI";

		public string MD5 {
			get {
				if (!File.Exists(WikiPageFile)) return "This page is probably on vacation!";
				return qstr.md5(QuickStream.LoadString(WikiPageFile));
			}
		}

		void Modified() {
			Data.D("MODIFIED.BY", Wiki.MW.TBox_WikiPageUser.Text);
			Data.D("MODIFIED.ON", $"{DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}");
		}

		internal string LastModified => Data.C("MODIFIED.ON");
		internal string LastModifiedBy => Data.C("MODIFIED.BY");

		public void Save() {
			Data.SaveSource(WikiPageFile);
		}

		public WikiPage(Wiki Ouwe, string PageName) {
			ParentWiki = Ouwe;
			Name = PageName;
			if (File.Exists(WikiPageFile))
				Data = GINI.ReadFromFile(WikiPageFile);
			else {
				Data = new TGINI();
				Data.D("CREATED.BY", Wiki.MW.TBox_WikiPageUser.Text);
				Data.D("CREATED.ON", $"{DateTime.Now.ToLongDateString()}; {DateTime.Now.ToLongTimeString()}");
				Modified();
				Save();
			}
			Debug.WriteLine($"Created wiki page: {PageName} (C# object)");
			Fout.Assert(Data,$"Data could not be properly set up on page {PageName}!");
		}
	}

	class Wiki {
		public static MainWindow MW;
		public readonly Project Parent;
		public readonly string WikiName;
		Dictionary<string, WikiProfile> Profiles = new Dictionary<string, WikiProfile>();
		Dictionary<string, WikiPage> Pages = new Dictionary<string, WikiPage>();
		TGINI Data = new TGINI();
		public string[] ProfileList { get { if (Data != null) return Data.List("Profiles").ToArray(); else return null; } }
		public List<string> ProfileListList => Data.List("Profiles");
		public string WikiFile => $"{Parent.WikiMainDir}/{WikiName}.Profiles.GINI";
		public string WikiPageDir => $"{Parent.WikiMainDir}/{WikiName}.Pages";

		public Wiki(Project Ouwe, string wikiName) {
			Parent = Ouwe;
			Parent.Wikis[wikiName] = this;
			WikiName = wikiName;
			Data = GINI.ReadFromFile(WikiFile);
			if (!Fout.NFAssert(Data, $"Wiki profile file {WikiFile} could not be properly read!")) return;
			Data.CL("Profiles");
			Data.List("Profiles").Sort();
			Debug.WriteLine($"Wiki {wikiName} has {Data.List("Profiles").Count} profile(s) => (Check {ProfileList.Length})");
		}

		public string[] Vars => Data.Vars();
		public string GetVar(string key) => Data.C($"{key}");
		public void SetVar(string key, string value) { Data.D($"{key}", value); Data.SaveSource(WikiFile); }

		public WikiPage GetWikiPage (string wp) {
			var pt = $"{wp}";
			if (!Pages.ContainsKey(pt)) {
				Pages[pt] = new WikiPage(this, pt);
			}
			return Pages[pt];
		}

		public void UpdateHash() {
			foreach (string key in Pages.Keys) Data.D($"MD5[{key}]", Pages[key].MD5);
		}

		public void UpdateHash( string key) {
			Data.D($"MD5[{key}]", GetWikiPage(key).MD5);
		}

		public WikiProfile GetProfile(string l,string p) {			
			var tag = $"{l}.{p}".ToUpper();
			if (!Profiles.ContainsKey(tag)) {
				var ret = new WikiProfile();
				Profiles[tag] = ret;
				foreach(var v in Vars) {
					//var prefix = $"VARIABLE.{tag.ToUpper()}.";
					var prefix = $"VAR.{p.ToUpper()}.";
					Debug.WriteLine($"GetProfile -> {prefix} -> {v} -> {qstr.Prefixed(v, prefix)}");
					if (qstr.Prefixed(v,prefix))
						ret[qstr.Right(v,v.Length-prefix.Length)] = GetVar(v);
				}

			}
			return Profiles[tag];
		}

		public WikiProfile GetProfile(WikiPage P) => GetProfile(P.Lang, P.Profile);

		public void Export() {
			Debug.WriteLine("Exporting Wiki");
			try {
				var Template = QuickStream.LoadString($"{Parent.TemplateDir}/{Parent.DefaultTemplate}");
				//var items = WikiPagePage.Items;
				var W = this; // Project.Current.GetWiki(CurrentWiki);
				var WPages = FileList.GetDir(W.WikiPageDir);
				var Langs = Parent.TransDict;
				if (!Fout.NFAssert(WPages, $"{W.WikiPageDir} could not be access somehow!")) return;
				var EntriesByProfile = new SortedDictionary<string,SortedDictionary<string,string>>();
				foreach (string p in WPages) {
					if (qstr.ExtractExt(p).ToUpper() == "GINI") {
						Debug.WriteLine($"Exporting {p}");
						foreach (var L in Langs) {
							Debug.WriteLine($"Exporting {p} - Lang {L.Key}");
							var Page = qstr.Left(p, p.Length - 5);
							var PData = GetWikiPage(Page);
							var OPL = ""; if (PData.LangSet) OPL = PData.Lang; PData.Lang = L.Key; Debug.WriteLine($"Lang set to: {PData.Lang}");
							if (PData.Profiles.Count == 0 && PData.Profile!="") { PData.Profiles.Add(PData.Profile); }
							Debug.WriteLine("Profile rollout");
							foreach (var _profile in PData.Profiles) {
								Debug.WriteLine($"Exporting {p} - Lang {L.Key} - Profile: {_profile}!");
								var HasContent = false;
								var XPage = new StringBuilder($"<!--- Wiki: {WikiName}; Page: {Page}; Langauge {L.Value} -->\n\n");
								var PVars = GetProfile(PData); //Profiles[_profile];							
								if (!EntriesByProfile.ContainsKey(_profile)) { EntriesByProfile[_profile] = new SortedDictionary<string, string>(); }
								EntriesByProfile[_profile][Page] = Page;
								XPage.Append($"<div style=\"float:right;\" width=\"20%\"><table><tr><td colspan=2>{Page}</td></tr>\n");
								foreach (var D in PVars) {
									var DP = D.Value.Split(':');
									var DType = DP[0].ToLower();
									var DName = DP[1];
									var DValue = PData.Get(_profile, D.Key);
									//Debug.WriteLine($"Checking profile var {D.Key} => {DValue}"); 
									if (DValue != "") {
										switch (DType) {
											case "sys":
												switch (D.Key.ToLower()) {
													case "screenname":
													case "screen_name":
														EntriesByProfile[_profile][Page] = DValue;
														break;
													default:
														Debug.WriteLine($"Unknown sys name! {DName}");
														break;
												}
												break;
											case "picture":
												HasContent = true;
												XPage.Append($"\t<tr valign=top><td style=\"text-align:center\" colspan=2><img src=\"{DValue}\" Alt=\"Picture: {DName}\" /></td></tr>\n");
												break;
											case "string":
												HasContent = true;
												XPage.Append($"\t<tr valign=top><td style=\"text-align:right\">{DName}:</td><td>{DValue}</td></tr>\n");
												break;
											default:
												Debug.WriteLine($"Unknown profile var type {DType} for var {D.Key} (ignored)");
												break;
										}
									}
								}
								HasContent = HasContent || PData.Content.Trim().Length > 0;
								XPage.Append("</table></div>\n\n");
								XPage.Append($"<h1>{EntriesByProfile[_profile][Page]}</h1>\n\n");
								XPage.Append($"{PData.Content}\n\n");
								XPage.Append($"<br /><br /><br /><br /><small>Last updated on {PData.LastModified} by {PData.LastModifiedBy}</small>");
								XPage.Append($"<br/><a href=\"WikiIndex_{WikiName}_{PData.Lang}.html\">Index</a>");
								Debug.WriteLine($"Wiki {WikiName}; Page {Page}\n\n{XPage}; Language {L.Value} ({L.Key}); Has content {HasContent}\n");
								if (HasContent) {
									var OFile = $"{Parent.OutputDir}/Wiki_{PData.Lang}_{_profile}_{qstr.StripExt(p)}.html";
									var ODir = qstr.ExtractDir(OFile);
									if (!Directory.Exists(ODir)) {
										Debug.WriteLine($"Creating dir: {ODir}");
										Directory.CreateDirectory(ODir);
									}
									Debug.WriteLine($"Saving: {OFile}");
									QuickStream.SaveString(OFile, Template.Replace("[[CONTENT]]", XPage.ToString()));
								}
								if (OPL != "") PData.Lang = OPL;
							}
						}
					}
				}
				Debug.WriteLine("Lang workout");
				foreach (var L in Langs) {
					var Wiki_Index = new StringBuilder("<table><caption>Wiki index</caption>\n");
					foreach (var WI_Profile in EntriesByProfile) {
						Wiki_Index.Append($"\t<tr valign=top><td style=\"text-align:right\">{WI_Profile.Key}</td><td>");
						foreach (var WI_Page in WI_Profile.Value) {
							var FFile = $"{Parent.OutputDir}/Wiki_{L.Key}_{WI_Profile.Key}_{WI_Page.Key}.html";
							if (File.Exists(FFile)) {
								Debug.WriteLine($"Adding {WI_Page.Key}");
								Wiki_Index.Append($"<a href=\"{qstr.StripDir(FFile)}\">{WI_Page.Value}</a><br/>");
							} else {
								Debug.WriteLine($"Skipping \"{FFile}\" {WI_Page} (file not found)");
							}
						}
						Wiki_Index.AppendLine("</td></tr>");
					}
					Wiki_Index.Append($"</table>\n<br /><small>Last updated {DateTime.Now}</small><br />\n\n");
					QuickStream.SaveString($"{Parent.OutputDir}/WikiIndex_{WikiName}_{L.Key}.html", Template.Replace("[[CONTENT]]", Wiki_Index.ToString()));
				}
				if (MW.Wiki_Git_Commit.IsChecked== true) {
					var cmtmessage = MW.Wiki_Git_Notes.Text.Trim();
					if (cmtmessage.Length == 0) cmtmessage = $"WIKI Commit {DateTime.Now}";
					Git.AddAndCommit(cmtmessage, "Wiki_*.html",$"WikiIndex_{WikiName}_*.html");
					if (MW.Wiki_Git_Push.IsChecked== true) {
						Git.Push(false);
					}
				}

			} catch (Exception ErrareHumanumEst) {
				Fout.Error(ErrareHumanumEst);
				Debug.WriteLine($".NET Error\n=> {ErrareHumanumEst.Message}\n\n{ErrareHumanumEst.StackTrace}\n");
			}
		}

		public void Save() {            
			try {
				Data.SaveSource(WikiFile);
			} catch (Exception ex) {
				Fout.Error(ex);
			}
		}
	}
}