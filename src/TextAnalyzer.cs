using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace CommentCleaner
{
	public static class TextAnalyzer
	{
		public class AnalysisResult
		{
			public float Letters { get; set; }
			public float WhiteSpace { get; set; }
			public float Numbers { get; set; }
			public float Punctuation { get; set; }
			public float Other { get; set; }

			public AnalysisResult ReduceRight(AnalysisResult other)
			{
				return new AnalysisResult()
				{
					Letters = (this.Letters + other.Letters) / 2,
					WhiteSpace = (this.WhiteSpace + other.WhiteSpace) / 2,
					Numbers = (this.Numbers + other.Numbers) / 2,
					Punctuation = (this.Punctuation + other.Punctuation) / 2,
					Other = (this.Other + other.Other) / 2
				};
			}
		}

		public static AnalysisResult Analyze(string text)
		{
			// todo: count the individual characters separately?
			float letters = 0,
				 whiteSpace = 0,
				 numbers = 0,
				 punctuation = 0,
				 other = 0;

			float denominator = text.Length;

			// we might not need the fixed stuff for small strings.
			// if the bounds-check is respected.
			for (int i = 0; i < text.Length; i++)
			{
				Char c = text[i];

				if (Char.IsLetter(c)) letters++;
				else if (Char.IsWhiteSpace(c)) whiteSpace++;
				else if (Char.IsNumber(c)) numbers++;
				else if (Char.IsPunctuation(c)) punctuation++;
				else other++;
			}

			return new AnalysisResult() 
			{
				Letters = letters / denominator,
				WhiteSpace = whiteSpace / denominator,
				Numbers = numbers / denominator,
				Punctuation = punctuation / denominator,
				Other = other / denominator
			};
		}
	}
}
