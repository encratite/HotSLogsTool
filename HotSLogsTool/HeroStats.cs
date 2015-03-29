using System.Collections.Generic;

namespace HotSLogsTool
{
	class HeroStats
	{
		public string Name { get; private set; }

		public List<HeroSkillStats> HeroSkillStats { get; private set; }

		public HeroStats(string name)
		{
			Name = name;
			HeroSkillStats = new List<HeroSkillStats>();
        }
	}
}
