using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HotSLogsTool
{
	class SiteReader
	{
		public void Run()
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var paths = GetHeroPaths();
			var tasks = new List<Task<HeroStats>>();
			foreach (var path in paths)
			{
				var task = new Task<HeroStats>(() => ReadHeroStats(path));
				task.Start();
				tasks.Add(task);
			}

			var allHeroStats = tasks.Select((task) =>
			{
				task.Wait();
				return task.Result;
			}).ToList();

			stopwatch.Stop();
			Console.WriteLine("Done downloading content after {0} ms\n", stopwatch.ElapsedMilliseconds);

			var levels = new int[] { 1, 4, 7 };
			foreach (var level in levels)
				CalculateStats(level, allHeroStats);
		}

		private void CalculateStats(int level, List<HeroStats> allHeroStats)
		{
			decimal minimumPopularityPercentage = 5.0M;

			var adjustedHeroes = new List<dynamic>();
			foreach (var heroStats in allHeroStats)
			{
				var matchingSkills = heroStats.HeroSkillStats.Where(stats => stats.Level == level && stats.PopularityPercentage >= minimumPopularityPercentage);
				var winningSkill = matchingSkills.OrderByDescending(stats => stats.WinPercentage).First();
				adjustedHeroes.Add(new
				{
					Name = heroStats.Name,
					WinPercentage = winningSkill.WinPercentage
				});
			}
			adjustedHeroes = adjustedHeroes.OrderByDescending(hero => hero.WinPercentage).ToList();
			Console.WriteLine("Adjusted win percentages at level {0}:", level);
			foreach (var hero in adjustedHeroes)
			{
				Console.WriteLine("{0}: {1}%", hero.Name, hero.WinPercentage);
			}
			Console.WriteLine("");
		}

		private HtmlDocument GetDocument(string path)
		{
			string uri = string.Format("https://www.hotslogs.com{0}", path);
			var request = WebRequest.Create(uri);
			using (var response = (HttpWebResponse)request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					var document = new HtmlDocument();
					document.Load(stream);
					return document;
				}
			}
		}

		private List<string> GetHeroPaths()
		{
			var document = GetDocument("/Default");
			var links = document.DocumentNode.SelectNodes("//a[starts-with(@href, '/Sitewide/HeroDetails?Hero=')]");
			var paths = links.Select(link => link.Attributes["href"].Value);
			paths = paths.Where(path => !path.Contains("Auto Select"));
			paths = paths.Distinct();
			return paths.ToList();
		}

		private HeroStats ReadHeroStats(string path)
		{
			var document = GetDocument(path);
			var rows = document.DocumentNode.SelectNodes("//tr[starts-with(@id, 'ctl00_MainContent_RadGridHeroTalentStatistics_ctl00__')]");
			int? level = null;
			string heroName = document.DocumentNode.SelectSingleNode("//option[@selected]").Attributes["value"].Value;
			var heroStats = new HeroStats(heroName);
			foreach (var row in rows)
			{
				var precedingRow = row.PreviousSibling;
				if (precedingRow.Attributes["class"].Value == "rgGroupHeader")
				{
					var levelString = precedingRow.SelectSingleNode(".//p").InnerText;
					var levelPattern = new Regex(@"\d+");
					var levelMatch = levelPattern.Match(levelString);
					level = int.Parse(levelMatch.Value);
				}
				var children = row.ChildNodes;
				string name = children[4].InnerText;
				decimal popularityPercentage = GetPercentage(children[7].InnerText);
				decimal winPercentage = GetPercentage(children[8].InnerText);
				var skillStats = new HeroSkillStats(level.Value, name, popularityPercentage, winPercentage);
				heroStats.HeroSkillStats.Add(skillStats);
			}
			return heroStats;
		}

		private decimal GetPercentage(string input)
		{
			var pattern = new Regex(@"\d+\.\d+");
			var match = pattern.Match(input);
			decimal percentage = decimal.Parse(match.Value);
			return percentage;
		}
	}
}
