using System.Collections.Generic;
using System.Linq;

namespace System.IO.Csv
{
    public class CsvWriter : IDisposable
    {
        private const char DefaultComment = '#';
        private const char DefaultDelimiter = ',';
        private const char DefaultQuote = '"';
        private const string ExceptionMessage = "Cannot set {0} while the file is being written.";
        private static readonly string CrLf = Environment.NewLine;

        private char _cmnt;
        private char _delim;
        private char _quot;
        private TextWriter stream;
        private bool writingFile;

        public CsvWriter(string fileName)
            : this(new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)),
                   DefaultDelimiter, DefaultQuote, DefaultComment) { }

        public CsvWriter(string fileName, char delimiter, char quote)
            : this(new StreamWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)),
                   delimiter, quote, DefaultComment) { }

        public CsvWriter(TextWriter stream)
            : this(stream, DefaultDelimiter, DefaultQuote, DefaultComment) { }

        public CsvWriter(TextWriter stream, char delimiter, char quote, char comment = DefaultComment)
        {
            this.stream = stream;
            this.writingFile = false;
            _delim = delimiter;
            _quot = quote;
            _cmnt = comment;
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
                if (!this.writingFile)
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
                if (!this.writingFile)
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
                if (!this.writingFile)
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
            this.stream.Flush();
            this.stream.Close();
            this.stream.Dispose();
        }

        /// <summary>Writes the specified data to the CSV file.</summary>
        /// <param name="data">The data to be written to the file.</param>
        public void WriteData(IEnumerable<IEnumerable<string>> data)
        {
            this.writingFile = true;

            foreach (var record in data)
            {
                this.stream.WriteLine(string.Join(_delim.ToString(),
                    from field in record
                    select this.FormatValue(field)));
            }

            this.stream.Flush();
            this.writingFile = false;
        }

        /// <summary>Writes the specified data to the CSV file.</summary>
        /// <param name="data">The data to be written to the file.</param>
        public void WriteData(IEnumerable<IEnumerable<object>> data)
        {
            this.WriteData(from record in data
                           select
                               from field in record
                               select field.ToString());
        }

        /// <summary>Writes the specified data row to the CSV file.</summary>
        /// <param name="dataRow">The data row to be written to the file.</param>
        public void WriteDataRow(IEnumerable<string> dataRow)
        {
            this.WriteData(new[] { dataRow });
        }

        /// <summary>Writes the specified data row to the CSV file.</summary>
        /// <param name="dataRow">The data row to be written to the file.</param>
        public void WriteDataRow(IEnumerable<object> dataRow)
        {
            this.WriteData(new[] { dataRow });
        }

        private string FormatValue(string value)
        {
            string result;
            if (value.IndexOfAny(new[] { _delim, _quot }) >= 0 || value.Contains(CsvWriter.CrLf))
            {
                result = string.Format("{0}{1}{0}", _quot, value.Replace(_quot.ToString(), new string(_quot, 2)));
            }
            else
            {
                result = value;
            }
            return result;
        }
    }
}