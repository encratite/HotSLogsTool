namespace HotSLogsTool
{
	class Application
	{
		static void Main(string[] args)
		{
			var reader = new SiteReader();
			reader.Run();
		}
	}
}
