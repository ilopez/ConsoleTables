using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleTables.Core
{
    public class ConsoleTable
    {
        public bool ShowRowDivider = true;
        public bool ShowEndColumnDivider = true;
        public bool ShowCount = true;
        public int MaxTableWidth = 80; // Dont do anything with this yet.
        public bool ShowMiddleColumnDivider = true;

        public IList<object> Columns { get; protected set; }
        public IList<object[]> Rows { get; protected set; }

        public ConsoleTable(params string[] columns)
        {
            Columns = new List<object>(columns);
            Rows = new List<object[]>();
        }

        public ConsoleTable AddColumn(IEnumerable<string> names)
        {
            foreach (var name in names)
                Columns.Add(name);

            return this;
        }

        public ConsoleTable AddRow(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (!Columns.Any())
                throw new Exception("Please set the columns first");

            if (Columns.Count != values.Length)
                throw new Exception(string.Format("The number columns in the row ({0}) does not match the values ({1}", Columns.Count, values.Length));

            Rows.Add(values);
            return this;
        }

        public static ConsoleTable From<T>(IEnumerable<T> values)
        {
            var table = new ConsoleTable();

            var columns = typeof(T).GetProperties().Select(x => x.Name).ToArray();
            table.AddColumn(columns);

            foreach (var propertyValues in values.Select(value => columns.Select(column => typeof(T).GetProperty(column).GetValue(value, null))))
                table.AddRow(propertyValues.ToArray());

            return table;
        }

        public void PrintToConsole()
        {
            Console.WriteLine(this.ToString());
        }
        public void PrintToConsoleColor(ConsoleColor ForeGround, ConsoleColor Background)
        {
            var oBG = Console.BackgroundColor;
            var oFG = Console.ForegroundColor;
            Console.ForegroundColor = ForeGround;
            Console.BackgroundColor = Background;
            Console.WriteLine(this.ToString());
            Console.ForegroundColor = oFG;
            Console.BackgroundColor = oBG;
        }
        public void PrintFirstLineColor(ConsoleColor Header)
        {
            var cls = this.ToString();
            var lines = cls.Split('\n');
            int count = 0;
            ConsoleColor oFG = Console.ForegroundColor;
            foreach (var s in lines)
            {
                if (count == 0)
                {
                    Console.ForegroundColor = Header;
                    Console.WriteLine(s);
                    Console.ForegroundColor = oFG;
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(s);
                }
                count++;
            }
                
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = Columns.Select(x => x.ToString().Length).ToList();

            // create the string format with padding
            var format = Enumerable.Range(0, Columns.Count)
                .Select(i =>
                    (i == 0 && !ShowEndColumnDivider ? "" : (!ShowMiddleColumnDivider ? " " : " | ")) + "{"
                    + i 
                    + ", -" 
                    + columnLengths[i] 
                    + " }")
                    .Aggregate((s, a) => s + a) + (!ShowEndColumnDivider ? "" : " |" ) ;

            var results = new List<string>();

            // find the longest formatted line
            var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
            var columnHeaders = string.Format(format, Columns.ToArray());
            
            // longest line is greater of formatted columnHeader and longest row
            var longestLine = Math.Max(maxRowLength, columnHeaders.Length);
            
            // add each row
            Array.ForEach(Rows.Select(row =>  string.Format(format, row)  ).ToArray(), results.Add);

            // create the divider
            var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";

            if (ShowRowDivider)
            {
                builder.AppendLine(divider);
            }
            builder.AppendLine(columnHeaders.Substring(0, (columnHeaders.Length < MaxTableWidth ? columnHeaders.Length : MaxTableWidth)));

            foreach (var row in results)
            {
                if (ShowRowDivider)
                {
                    builder.AppendLine(divider);
                }
                builder.AppendLine(row.Substring(0, (row.Length < MaxTableWidth ? row.Length : MaxTableWidth)));
            }
            if (ShowRowDivider)
            {
                builder.AppendLine(divider);
            }
            if (ShowCount)
            {
                builder.AppendLine("");
                builder.AppendFormat(" Count: {0}", Rows.Count);
            }

            return builder.ToString();
        }

        public void Write()
        {
            Console.WriteLine(ToString());
        }
    }
}
