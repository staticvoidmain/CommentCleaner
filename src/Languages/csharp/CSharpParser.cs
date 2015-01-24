#define USE_UNSAFE
using System;

namespace CommentCleaner.Languages
{
	public class CSharpParser : Parser
	{
		// parser states
		private const int PARSING = 0;
		private const int INLINE_COMMENT = 1;
		private const int BLOCK_COMMENT = 2;
		private const int CHAR_LITERAL = 3;
		private const int STRING_LITERAL = 4;
		private const int VERBATIM_STRING = 5;
		private const int AWAITING_COMMENT_COMPLETION = 6;
		private const int AWAITING_VERBATIM_STRING = 7;
		private const int LITERAL_ESCAPE_SEQUENCE = 8;
		private const int AWAITING_BLOCK_COMMENT_COMPLETION = 9;
		private const int VERBATIM_ESCAPE_QUOTE = 10;

		public override void ParseChunk(Document document, char[] block, Action<Comment> onComment)
		{
			int index = 0;
			int len = block.Length;
			int state = document.State;

#if USE_UNSAFE
			// nice work, buffer overruns are now possible
			// are you proud of yourself?

			unsafe
			{
				fixed (char* buffer = block)
				{
#else
			char[] buffer = block;
#endif
					for (; index <= len; index++)
					{
						// the bounds checks are real.
						Char c = buffer[index];

						switch (state)
						{
							case PARSING:
								switch (c)
								{
									case '/': state = AWAITING_COMMENT_COMPLETION; break;
									case '"': state = STRING_LITERAL; break;
									case '\'': state = CHAR_LITERAL; break; 
									case '@': state = AWAITING_VERBATIM_STRING; break;
									case '\n': document.IncrementLineCount(); break;
									default: break; // still parsing
								} break;

							case STRING_LITERAL:
								if (c == '"') state = PARSING;// close out the string and continue.
								else if (c == '\\') state = LITERAL_ESCAPE_SEQUENCE;

								break;

							case AWAITING_COMMENT_COMPLETION:
								switch (c)
								{
									case '/':
										state = INLINE_COMMENT;
										document.MarkCommentBegin();
										break;
									case '*':
										state = BLOCK_COMMENT;
										document.MarkCommentBegin();
										break;
									default: state = PARSING; break;
								} break;

							case BLOCK_COMMENT:
								if (c == '*')
								{
									state = AWAITING_BLOCK_COMMENT_COMPLETION;
								}
								else
								{
									document.AppendChar(c);
								}

								break;

							case AWAITING_BLOCK_COMMENT_COMPLETION:

								if (c == '/')
								{
									onComment(document.CloseComment());
									state = PARSING;
								}
								else
								{
									// we have no completed the block.
									document.AppendChar('*');
									document.AppendChar(c);
									state = BLOCK_COMMENT;
								}

								break;

							case INLINE_COMMENT:
							{
								if (c == '\n')
								{
									// edge: line consisting of only a comment.
									onComment(document.CloseComment());
									document.IncrementLineCount();
									state = PARSING;
								}
								else
								{
									document.AppendChar(c);
								}
								break;
							}

							case LITERAL_ESCAPE_SEQUENCE:
							{
								// what was escaped? do we actually care?
								// doesn't handle 
								state = STRING_LITERAL;
								break;
							}
								
							case AWAITING_VERBATIM_STRING:
							{
								if (c == '"') state = VERBATIM_STRING;
								else state = PARSING;
								break;
							}

							case VERBATIM_STRING:
								switch (c)
								{
									case '"': state = VERBATIM_ESCAPE_QUOTE; break;
									case '\n': document.IncrementLineCount(); break;
									default: break;
								} break;

							case VERBATIM_ESCAPE_QUOTE:
								if (c == '"')
									state = VERBATIM_STRING;
								else state = PARSING;
								// in this case though, should we increment?
								break;

							case CHAR_LITERAL:
							default: throw new ApplicationException();
						}
					}

					document.State = state;
#if USE_UNSAFE
				}
			}
#endif
		}
	}
}