using System;
using Mono.Options;
using System.Text.RegularExpressions;
using System.IO;

namespace CommentCleaner
{
	public class Program
	{		
		public static void Main (string[] args)
		{
			var processor = new ConsoleProcessor();

			processor.ProcessArgs(args);
		}
	}
}