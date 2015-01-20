using System;
using Mono.Options;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommentCleaner.Languages;
using System.Diagnostics;

namespace CommentCleaner
{
	public class ConsoleProcessor
	{
		private readonly TextWriter _error;
		private readonly OptionSet _options;

		// allows redirection
		private TextWriter _out;

		private string _language;
		private ProgramMode _mode;
		private string _directory;
		private bool _verbose;
		private Regex _ignorePattern;
		private double _threshold;

		public ConsoleProcessor()
			: this(Console.Out, Console.Error)
		{

		}

		public ConsoleProcessor(TextWriter output, TextWriter error)
		{
			this._out = output;
			this._error = error;

			this._options = new OptionSet()
			{
				{ "d=|dir=", "Directory to begin parsing.", (dir) => _directory = dir },
				{ "m=|mode=", "Program Mode (Profile|Report|Kill)", mode => ParseProgramMode(mode) }, 
				{ "l|language=", "Language to use.", lang => _language = lang },
				{ "v:|verbose:", "Output verbose rule information.", (verbose) => _verbose = bool.Parse(verbose) },
				{ "t:|threshold:", "Minimum score at which the comment is flagged as code.", (v) => _threshold = double.Parse(v) },
                { "i:|ignore:", "Ignore pattern.", i => ParseIgnorePattern(i) },
				{ "rs:|report-style:", "Output type of the report (xml|text)", (v) => _verbose = bool.Parse(v) },
				{ "o|out:", "Output file for report mode.", (o) => RedirectOut(o) },
				{ "a|attach", "", (_) => Debugger.Launch() },
				{ "h|?|help", "Display help information.", (_) => { ShowHelp(); Environment.Exit(-1); } },
			};
		}

		private void RedirectOut(string output)
		{
			this._out = new StreamWriter(output);
		}

		private void ParseProgramMode(string m)
		{
			Enum.TryParse<ProgramMode>(m, out _mode);
		}

		private void ParseIgnorePattern(string ignore)
		{
			if (!string.IsNullOrEmpty(ignore))
			{
				this._ignorePattern = new Regex(ignore);
			}
		}

		public void ProcessArgs(string[] args)
		{
			if (args.Length == 0)
			{
				ShowHelp();
			}
			else
			{
				var errors = _options.Parse(args);

				if (errors.Count == 0)
				{
					if (ValidateOptions())
					{
						var watch = Stopwatch.StartNew();
						
						Language language = LanguageFactory.CreateLanguage(_language);

						using (BlockingCollection<string> files = new BlockingCollection<string>(new ConcurrentQueue<string>(), 500))
						{
							// producer
							Task producer = Task.Run(() =>
							{
								foreach (var file in Directory.EnumerateFiles(_directory, language.Pattern, SearchOption.AllDirectories))
								{
									if (_ignorePattern == null || _ignorePattern.IsMatch(file) == false)
									{
										// todo: signal that we're ready to start processing?
										files.Add(file);
									}
								}

								files.CompleteAdding();
							});

							const int parserThreads = 8;

							Task[] consumers = new Task[parserThreads];
							Parser parser = language.Parser;
							const int bufferSize = 1024 * 8;

							for (int i = 0; i < parserThreads; i++)
							{
								consumers[i] = Task.Run(async () =>
								{
									while (!files.IsCompleted)
									{
										// should this be tryTake?
										string file = files.Take();

										var document = new Document(file);

										using (var sr = new StreamReader(file, true))
										{
											char[] buffer = new char[bufferSize];

											while (true)
											{
												int bytesRead = await sr.ReadBlockAsync(buffer, 0, bufferSize).ConfigureAwait(false);

												if (bytesRead == 0)
													break;

												parser.ParseChunk(document, buffer);
											}
										}
									}
								});
							}

							Task.WaitAll(producer);
							Task.WaitAll(consumers);

							watch.Stop();
							_out.WriteLine("elapsed: " + watch.ElapsedMilliseconds + "ms");
						}
					}
				}
				else
				{
					foreach (var error in errors)
					{
						_error.WriteLine("Invalid Argument: {0}", error);
					}
				}
			}

			_out.WriteLine();

			_error.Flush();
			_out.Flush();
		}

		private bool ValidateOptions()
		{
			bool valid = true;
			if (string.IsNullOrEmpty(_directory))
			{
				_error.WriteLine("--directory is required.");
				valid = false;
			}

			if (string.IsNullOrEmpty(_language))
			{
				_error.WriteLine("--directory is required.");
				valid = false;
			}

			return valid;
		}

		private void ShowHelp()
		{
			this._out.WriteLine("CommentCleaner v1.0");
			_options.WriteOptionDescriptions(this._out);

			this._out.WriteLine("\r\nExamples:");
			this._out.WriteLine("\tCommentCleaner.exe --dir=c:\\dev --report-style=xml --out=report.xml --verbose");
			this._out.WriteLine("\tCommentCleaner.exe --dir=c:\\dev --kill --report-style=text --out=report.txt");
		}
	}
}