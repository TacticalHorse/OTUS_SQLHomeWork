using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLHomeWork
{
    class DrawTableTool
    {


        public static void Draw(List<string[]> rows)
        {
            if (rows == null || rows.Count == 0) return;

            int[] maxColSize = GetMaxColSizes(rows);
            int maxRowLength = GetMaxRowLength(maxColSize);
            Console.WriteLine();
            PrintHeadLine(maxRowLength);
            PrintRows(rows, maxColSize);
            PrintFootLine(maxRowLength);
            Console.WriteLine();
        }

        static int GetMaxRowLength(int[] cols)
        {
            int output = 1;
            foreach (var col in cols)
            {
                output += col + 1;
            }
            return output;
        }

        static int[] GetMaxColSizes(List<string[]> rows)
        {
            int[] output = new int[rows[0].Length];

            foreach (var row in rows)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (output[i] < (row[i].Length+1)) output[i] = (row[i].Length+1);
                }
            }
            return output;
        }

        static void PrintHeadLine(int Length)
        {
            if (Length < 3) return;
            Console.WriteLine(new string('_', Length));
        }

        static void PrintRows(List<string[]> Rows, int[] ColSizes)
        {
            foreach (var Row in Rows)
            {
                string rowString = "|";
                for (int i = 0; i < ColSizes.Length; i++)
                {
                    rowString += Row[i].PadRight(ColSizes[i], ' ')+"|";
                }
                Console.WriteLine(rowString);
            }
        }
        static void PrintFootLine(int Length)
        {
            if (Length < 3) return;
            Console.WriteLine("|" + new string('_', Length - 2) + "|");
        }
    }
}
