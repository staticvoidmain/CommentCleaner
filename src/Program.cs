using System;
using Mono.Options;
using System.Text.RegularExpressions;
using System.IO;

namespace CommentCleaner
{
	public class Program
	{
		private static readonly OptionSet options;
		private static string _directory;
		private static bool _verbose;
        private static Regex ignorePattern;

        // todo: Test the behavior of console arguments?

		private static TextWriter _out;

		static Program()
		{
			options = new OptionSet()
			{
				{ "d|dir=", "Directory to begin parsing.", (d) => _directory = d },
				{ "v|verbose", "Output verbose rule information.", (v) => _verbose = bool.Parse(v) },
				{ "t|threshold=", "Minimum score at which the comment is flagged as code.", (v) => _verbose = bool.Parse(v) },
                { "i|ignore=", "Ignore pattern.", i => ParseIgnorePattern(i) },
				{ "rs|report-style=", "Output type of the report (xml|text)", (v) => _verbose = bool.Parse(v) },
				{ "o|out=", "Output file for report mode.", (v) => _verbose = bool.Parse(v) },
				{ "h|?|help", "Display help information.", (_) => { ShowHelp(); Environment.Exit(-1); } },
			};
		}

		public Program() : this(Console.Out)
		{

		}

		public Program(TextWriter sysOut)
		{
			_out = sysOut;
		}

        private static void ParseIgnorePattern(string ignore)
        {
			if (!string.IsNullOrEmpty(ignore))
			{
				ignorePattern = new Regex(ignore);
			}
        }
		
		public static void Main (string[] args)
		{
			var errors = options.Parse(args);

			if (errors.Count == 0)
			{
				ValidateArgs();
				
			}
			else
			{
				foreach (var error in errors)
				{
					Console.WriteLine(error);
				}

				ShowHelp();
			}
		}

		private static void ValidateArgs()
		{
			if (string.IsNullOrEmpty(_directory))
			{
				
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("CommentCleaner v1.0");
			options.WriteOptionDescriptions(Console.Out);

			Console.WriteLine("\r\nExamples:");
			Console.WriteLine("\tCommentCleaner.exe --dir=c:\\dev --report-style=xml --out=report.xml --verbose");
			Console.WriteLine("\tCommentCleaner.exe --dir=c:\\dev --kill --report-style=text --out=report.txt");
 		}
	}
}