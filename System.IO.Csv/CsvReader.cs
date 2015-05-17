using System.Collections.Generic;
using System.Linq;
using Regex = System.Text.RegularExpressions.Regex;

namespace System.IO.Csv
{
    public class CsvReader : IDisposable
    {
        private const char DefaultComment = '#';
        private const char DefaultDelimiter = ',';
        private const char DefaultQuote = '"';
        private const string ExceptionMessage = "Cannot set {0} while the file is being parsed.";
        private static readonly string CrLf = Environment.NewLine;

        private char _cmnt;
        private char _delim;
        private char _quot;
        private bool? isBackslashEsc;
        private bool parsingFile;
        private TextReader stream;

        public CsvReader(string fileName)
            : this(new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)),
                   DefaultDelimiter, DefaultQuote, DefaultComment) { }

        public CsvReader(string fileName, char delimiter, char quote)
            : this(new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)),
                   delimiter, quote, DefaultComment) { }

        public CsvReader(TextReader stream)
            : this(stream, DefaultDelimiter, DefaultQuote, DefaultComment) { }

        public CsvReader(TextReader stream, char delimiter, char quote, char comment = DefaultComment)
        {
            this.stream = stream;
            this.parsingFile = false;
            _delim = delimiter;
            _quot = quote;
            _cmnt = comment;
            this.isBackslashEsc = null;
        }

        /// <summary>Gets or sets the comment character used in the CSV file.</summary>
        /// <value>The comment character used in the CSV file.</value>
        /// <exception cref="T:System.InvalidOperationException">
        /// Attempted to change the comment character while the file is being read.
        /// </exception>
        public char CommentMark
        {
            get
            {
                return _cmnt;
            }
            set
            {
                if (!this.parsingFile)
                {
                    _cmnt = value;
                    return;
                }
                throw new InvalidOperationException(string.Format(ExceptionMessage, "comment mark"));
            }
        }

        /// <summary>Gets or sets the delimiter used in the CSV file.</summary>
        /// <value>The delimiter used in the CSV file.</value>
        /// <exception cref="T:System.InvalidOperationException">
        /// Attempted to change the delimiter while the file is being read.
        /// </exception>
        public char Delimiter
        {
            get
            {
                return _delim;
            }
            set
            {
                if (!this.parsingFile)
                {
                    _delim = value;
                    return;
                }
                throw new InvalidOperationException(string.Format(ExceptionMessage, "delimiter"));
            }
        }

        /// <summary>Gets or sets the quote mark used in the CSV file.</summary>
        /// <value>The quote mark used in the CSV file.</value>
        /// <exception cref="T:System.InvalidOperationException">
        /// Attempted to change the quote mark while the file is being read.
        /// </exception>
        public char QuoteMark
        {
            get
            {
                return _quot;
            }
            set
            {
                if (!this.parsingFile)
                {
                    _quot = value;
                    return;
                }
                throw new InvalidOperationException(string.Format(ExceptionMessage, "quote mark"));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.stream.Close();
            this.stream.Dispose();
        }

        /// <summary>Reads the data from the CSV file.</summary>
        public IEnumerable<List<string>> ReadData()
        {
            this.parsingFile = true;

            string buffer = "";

            foreach (string line in this.ReadLines())
            {
                if (buffer != "" || line.TrimStart()[0] != _cmnt)
                {
                    var result = this.ParseLine(buffer == "" ? line : buffer + CrLf + line);
                    if (result.Item1)
                    {
                        buffer = "";
                        yield return result.Item2;
                    }
                    else
                    {
                        buffer = line;
                    }
                }
            }

            this.parsingFile = false;
            yield break;
        }

        private bool IsQuoteComplete(string text)
        {
            string backSlashQuote = "\\" + _quot.ToString();
            bool result;

            if (this.isBackslashEsc.HasValue && this.isBackslashEsc.Value)
            {
                string pattern = "(?<bs>\\\\*)(?<qt>" + _quot.ToString() + "+)$";
                var grps = Regex.Match(text, pattern).Groups;
                result = (grps["bs"].Value.Length == 0 && grps["qt"].Value.Length == 1) ||  // no backslash; single quote completes field
                         (grps["bs"].Value.Length % 2 != grps["qt"].Value.Length % 2);      // backslash; odd backslash, even quotes or vice-versa
            }
            else
            {
                string pattern = _quot.ToString() + "+$";
                result = (Regex.Match(text, pattern).Value.Length % 2 == 1);    // unescaped; field ends with an odd number of quotes
            }
            return result;
        }

        private Tuple<bool, List<string>> ParseLine(string line)
        {
            var parts = new Queue<string>(line.Split(new[] { _delim }));
            var fields = new List<string>();

            bool inQuote = false;
            bool quoteEnd = false;

            // quotation mark used in fields
            string fieldQuote = (isBackslashEsc.HasValue && isBackslashEsc.Value) ?
                                 "\\" + _quot.ToString() : new string(_quot, 2);

            string field = "";
            while (parts.Any<string>())
            {
                field += parts.Dequeue();
                inQuote = (inQuote || field[0] == _quot);
                quoteEnd = (inQuote && this.IsQuoteComplete(field));

                if (inQuote == quoteEnd) // was in quote and quote has closed
                {
                    if (inQuote)
                    {
                        // strip off quotes
                        field = field.Substring(1, field.Length - 2);
                        // replace double- or escaped quotes
                        field = field.Replace(fieldQuote, _quot.ToString());
                    }

                    fields.Add(field);
                    field = "";
                    inQuote = false;
                    quoteEnd = false;
                }
                else
                {
                    field += _delim;
                }
            }

            return Tuple.Create(inQuote == quoteEnd, fields);
        }

        private IEnumerable<string> ReadLines()
        {
            while (this.stream.Peek() != -1)
            {
                yield return this.stream.ReadLine();
            }

            yield break;
        }
    }
}