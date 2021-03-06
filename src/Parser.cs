﻿using System;
namespace CommentCleaner.Languages
{
    public abstract class Parser
    {
        public abstract void ParseChunk(Document document, char[] buffer, Action<Comment> onComment);
    }
}