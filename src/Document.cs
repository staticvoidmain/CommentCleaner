using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommentCleaner
{
	public sealed class Document
	{
		public string FileName { get; private set; }

		private int lineCount;
		private readonly StringBuilder commentBuffer = new StringBuilder(256);

		// parser_state
		public int State { get; set; }

		public Document(string file)
		{
			this.FileName = file;
		}

		internal void IncrementLineCount()
		{
			this.lineCount++;
		}

		private int commentBegin = -1;

		internal void MarkCommentBegin()
		{
			commentBegin = lineCount;
		}

		internal Comment CloseComment()
		{
			var comment = new Comment()
			{
				Text = commentBuffer.ToString(),
				Start = commentBegin,
				End = lineCount
			};

			commentBuffer.Clear();
			commentBegin = -1;

			return comment;
		}

		internal void AppendChar(char c)
		{
			commentBuffer.Append(c);
		}
	}
}