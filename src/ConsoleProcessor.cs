using System;
using Mono.Options;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommentCleaner.Languages;
using System.Diagnostics;
using System.Text;

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
				{ "d=|directory=", "Directory to begin parsing.", (dir) => _directory = dir },
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

		public async Task ProcessArgs(string[] args)
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

						const int bufferSize = 1024 * 8;
						bool done = false;
						const int parserThreads = 8;
						ConcurrentQueue<string>[] buckets = InitializeQueueTiles(parserThreads);

						var producer = Task.Factory.StartNew(() =>
						{
							int index = 0;

							foreach (var file in Directory.EnumerateFiles(_directory, language.Pattern, SearchOption.AllDirectories))
							{
								if (_ignorePattern == null || _ignorePattern.IsMatch(file) == false)
								{
									var tile = buckets[index++ % parserThreads];

									tile.Enqueue(file);
								}
							}

							done = true;
						});

						Task[] consumers = new Task[parserThreads];
						Parser parser = language.Parser;

						for (int i = 0; i < parserThreads; i++)
						{
							var tile = buckets[i];

							consumers[i] = Task.Factory.StartNew(async () =>
							{
								while (!done)
								{
									string file;

									if (!tile.TryDequeue(out file))
										continue;

									Document document = new Document(file);

									using (var sr = new StreamReader(file, Encoding.UTF8, true, bufferSize))
									{
										char[] buffer = new char[bufferSize];

										while (true)
										{
											int bytesRead = await sr.ReadBlockAsync(buffer, 0, bufferSize).ConfigureAwait(false);

											if (bytesRead == 0)
												break;

											// todo: pass bytesRead into the parser?
											// maybe the bounds check would get eliminated?
											parser.ParseChunk(document, buffer, (comment) =>
											{
												var result = TextAnalyzer.Analyze(comment.Text);

												double score = Score(comment, result);

												if (score > 0)
												{
													lock (_out)
													{
														_out.WriteLine(comment.Text + " Score: " + score.ToString());
													}
												}
											});
										}
									}
								}
							}, TaskCreationOptions.PreferFairness);
						}

						await producer;
						await Task.WhenAll(consumers);

						watch.Stop();
						_out.WriteLine("elapsed: " + watch.ElapsedMilliseconds + "ms");
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

		private static ConcurrentQueue<string>[] InitializeQueueTiles(int tiles)
		{
			ConcurrentQueue<string>[] buckets = new ConcurrentQueue<string>[tiles];

			for (int tileIndex = 0; tileIndex < tiles; tileIndex++)
			{
				buckets[tileIndex] = new ConcurrentQueue<string>();
			}

			return buckets;
		}

		private double Score(Comment comment, TextAnalyzer.AnalysisResult result)
		{
			double score = 0;

			if (result.WhiteSpace == 1)
			{
				// whitespace only skip it.
			}
			else
			{
				Char endsWith = comment.Text[comment.Text.Length - 1];

				if (endsWith == ';')
				{
					score += 10;
				}
				else if (endsWith == '.')
				{
					// c# lines (in general) shouldn't end with a period.
					score -= 5;
				}

				if (result.WhiteSpace > 0.5)
				{
					score += 5;
				}

				if (result.Letters > 0.8)
				{
					score -= 5;
				}

				if (result.Punctuation > 0.3)
				{
					score += 3;
				}
			}

			return score;
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
				_error.WriteLine("--language is required.");
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