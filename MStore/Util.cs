using System;
using System.Collections.Generic;
using System.Text;

namespace MStore
{
    public static class MUtil
    {
        public static string GetStringToSpecialChar(string str, char specialChar)
        {
            string value = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == specialChar) return value;
                value += str[i];
            }

            return value;
        }

        /// <summary>
        /// Returns string from special character ( without it )
        /// </summary>
        /// <param name="str"></param>
        /// <param name="specialChar"></param>
        /// <param name="strToSpecialChar"></param>
        /// <returns></returns>
        public static string GetStringToSpecialCharAndDelete(string str, char specialChar, out string strToSpecialChar)
        {
            strToSpecialChar = GetStringToSpecialChar(str, specialChar);

            if(strToSpecialChar.Length >= str.Length)
            {
                return "";
            }
            return str.Remove(0, strToSpecialChar.Length + 1);
        }

        public static bool AskUserYesNo(string action = "do this")
        {
            ConsoleColor orColor = Console.ForegroundColor;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Are you sure you want to " + action + "? Y - yes, N - no");
                Console.ForegroundColor = orColor;

                ConsoleKeyInfo info = Console.ReadKey();
                if(info.Key == ConsoleKey.Y)
                {
                    return true;
                }
                if(info.Key == ConsoleKey.N)
                {
                    return false;
                }
            }


            return false;

        }

        public static List<string> RemoveEmptyLines(List<string> lines, bool removeAlsoNLandCR = true)
        {
            

            //List<string> linesList = new List<string>(lines);
            for(int i = 0;i< lines.Count;i++)
            {
                Debug.Log("Checking " + i + ", it's " + lines[i].Length + " chars long");
                if(lines[i].Length == 0)
                {
                    Debug.LogWarning("Removing at " + i);
                    lines.RemoveAt(i);
                    i--;
                    continue;
                }
                if(removeAlsoNLandCR)
                {
                    if(lines[i] == "\n" || lines[i] == "\r")
                    {
                        Debug.LogWarning("Removing at " + i);
                        lines.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }

            

            return lines;
        }

        public static string[] RemoveEmptyLines(string[] lines)
        {
            Debug.Log("Size before cutting : " + lines.Length);
            string[] lns = RemoveEmptyLines(new List<string>(lines)).ToArray();
            Debug.Log("Size after cutting: " + lns.Length);
            return lns;
        }

        public static string[] StringToStringArray(string input)
        {
            List<string> array = new List<string>();
            string actualLine = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\n' || input[i] == '\r')
                {
                    Debug.Log("Actual line: \"" + actualLine + "\" lenght: " + actualLine.Length);
                    array.Add(actualLine);


                    actualLine = "";
                    continue;
                }

                actualLine += input[i];
            }

            if (actualLine.Length != 0)
            {
                array.Add(actualLine);
                Debug.Log("Actual line: \"" + actualLine + "\" lenght: " + actualLine.Length);
            }

            return array.ToArray();
        }

        public static string ByteArrayToString(byte[] array)
        {
            return Convert.ToBase64String(array);
        }

        public static byte[] StringToByteArray(string input)
        {
            return Convert.FromBase64String(input);
        }
    }
}
