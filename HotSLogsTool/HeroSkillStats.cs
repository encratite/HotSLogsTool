namespace HotSLogsTool
{
	class HeroSkillStats
	{
		public int Level { get; private set; }

		public string Name { get; private set; }

		public decimal PopularityPercentage { get; private set; }

		public decimal WinPercentage { get; private set; }

		public HeroSkillStats(int level, string name, decimal popularityPercentage, decimal winPercentage)
		{
			Level = level;
			Name = name;
			PopularityPercentage = popularityPercentage;
			WinPercentage = winPercentage;
		}
	}
}
