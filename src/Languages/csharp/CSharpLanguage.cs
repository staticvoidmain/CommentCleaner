using CommentCleaner.Languages;
using System;

namespace CommentCleaner
{
	public class CSharpLanguage : Language
	{
		public CSharpLanguage()
		{
			Name = "CSharp";
			Pattern = "*.cs";
            Parser = new CSharpParser();
		}

		public override string Name { get; set; }
		public override string Pattern { get; set; }
		public override string[] ReservedWords { get; set; }
		public override Tuple<Char, double>[] Frequencies { get; set; }
	}
}