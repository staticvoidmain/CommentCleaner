using System;
using CommentCleaner.Languages;

namespace CommentCleaner
{
    public class LanguageFactory
    {
        public static Language CreateLanguage(string name)
        {
            // todo: lookup language profile on disk or something
            // probe?
            // MEF?
            // dafuh?

            return new CSharpLanguage();
        }
    }
}