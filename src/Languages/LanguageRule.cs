namespace CommentCleaner.Languages
{
    public abstract class LanguageRule
    {
		public abstract void Evaluate(Comment comment);
    }
}