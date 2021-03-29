using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Objectiks.Json.Models
{
    public class DocumentText : IDisposable
    {
        public string Content { get; set; }

        public DocumentText() { }

        internal DocumentText(string content)
        {
            Content = content;
        }

        private string GetContent(string content)
        {
            if (!String.IsNullOrWhiteSpace(content))
            {
                //Id = ExtractToken(content, DocumentDefaults.ContentRegexKeyForId);
                //Language = ExtractToken(content, DocumentDefaults.ContentRegexKeyForLanguage);

                int length = content.Length;
                int separatorL = DocumentDefaults.ContentInfoSeparator.Length;
                int indexOf = content.IndexOf(DocumentDefaults.ContentInfoSeparator);

                return content.Substring(indexOf + separatorL, length - (indexOf + separatorL));
            }
            return content;
        }

        private string ExtractToken(string content, string expressionPattern)
        {
            var reqular = new Regex(expressionPattern);
            var match = reqular.Match(content);

            if (match.Success)
            {
                return match.Value.Replace("\"", "");
            }

            return string.Empty;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
