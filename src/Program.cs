namespace CommentCleaner
{
	public class Program
	{		
		public static void Main (string[] args)
		{
			var processor = new ConsoleProcessor();

			processor.ProcessArgs(args).Wait();
		}
	}
}