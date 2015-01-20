using CommentCleaner.Languages;
using System;
using System.Collections.Generic;

namespace CommentCleaner
{
	/// <summary>
	/// Abstract base class for all language profiles.
	/// </summary>
	public abstract class Language
	{
		/// <summary>
		/// Name of the language represented.
		/// </summary>
		public virtual string Name { get; set; }

		/// <summary>
		/// File-system pattern for matching source files. ex: *.cs
		/// </summary>
		public virtual string Pattern { get; set; }

		// TODO: Rules?
        public virtual ICollection<LanguageRule> Rules { get; set; }
		
		/// <summary>
		/// List of all reserved words in the language.
        /// 
        /// todo: do we actually care about this?
		/// </summary>
		public virtual string[] ReservedWords { get; set; }

		/// <summary>
		/// Array of character frequencies within representative documents.
		/// </summary>
		public virtual Tuple<Char, double>[] Frequencies { get; set; }

		/// <summary>
		/// Returns an antlr Lexer for the specified language.
		/// </summary>
		/// <returns></returns>
        public virtual Parser Parser { get; protected set; }
	}
}