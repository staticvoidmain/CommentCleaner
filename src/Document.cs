using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommentCleaner
{
	public sealed class Document
	{
		public string FileName { get; private set; }
		public List<Comment> Comments { get; private set; }

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
			if (commentBegin != -1)
			{
				Debugger.Launch();
			}

			commentBegin = lineCount;
		}

		internal void MarkCommentEnd()
		{
			if (this.Comments == null)
			{
				this.Comments = new List<Comment>();
			}

			this.Comments.Add(new Comment()
			{
				Text = commentBuffer.ToString(),
				Start = commentBegin,
				End = lineCount
			});

			commentBuffer.Clear();
			commentBegin = -1;
		}

		internal void AppendChar(char c)
		{
			commentBuffer.Append(c);
		}
	}
}