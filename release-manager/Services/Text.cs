using System.Text;

namespace ReleaseManager.Services
{
    /// <summary>Text helpers for showing messages that come from tools and other programs.</summary>
    public static class Text
    {
        /// <summary>
        /// The text without the invisible characters that rearrange or hide it: bidirectional marks,
        /// embeddings and isolates (an obfuscated name full of them turns the rest of the line
        /// right-to-left), zero-width characters and control characters. A run of them becomes "…".
        /// </summary>
        public static string Printable(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
            var builder = new StringBuilder(text.Length);
            bool dropped = false;
            foreach (var c in text)
            {
                bool invisible =
                    (c >= '‪' && c <= '‮') || (c >= '⁦' && c <= '⁩') ||
                    c == '‎' || c == '‏' || c == '؜' ||
                    (c >= '​' && c <= '‍') || c == '﻿' ||
                    (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t') ||
                    char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.PrivateUse ||
                    char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherNotAssigned;
                if (invisible)
                {
                    if (!dropped) builder.Append('…');
                    dropped = true;
                    continue;
                }
                dropped = false;
                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
