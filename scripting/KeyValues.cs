using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF2MConfigRewrite
{
    public class KeyValues
    {
        public int currentIndex = 0;
        public string fileName = string.Empty;
        public string GetFloat(string key, int position = -1)
        {
            string brokenKey;
            string newKey = "\"" + key + "\"";
            StringBuilder builder = new StringBuilder();
            char[] arr;
            int quoteCheck = 0;
            List<string>? line;
            line = Program.globalLine;
            if (position != -1 && line != null)
            {
                string tempKey = line[position].Replace(newKey, "");
                arr = tempKey.ToCharArray();
            }
            else
            {
                arr = key.ToCharArray();
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (position == -1)
                {
                    if (arr[i] == '\"')
                    {
                        quoteCheck++;
                    }
                    if (quoteCheck < 2)
                    {
                        continue;
                    }
                }
                if (char.IsDigit(arr[i]) || arr[i] == '.')
                {
                    builder.Append(arr[i]);
                }
            }
            brokenKey = builder.ToString();

            return brokenKey;
        }

        public int GetNum(string key, int position = -1)
        {
            string brokenKey;
            string newKey = "\"" + key + "\"";
            StringBuilder builder = new StringBuilder();
            char[] arr;
            int quoteCheck = 0;
            List<string>? line;
            line = Program.globalLine;
            if (position != -1 && line != null)
            {
                string tempKey = line[position].Replace(newKey, "");
                arr = tempKey.ToCharArray();
            }
            else
            {
                arr = key.ToCharArray();
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (position == -1)
                {
                    if (arr[i] == '\"')
                    {
                        quoteCheck++;
                    }
                    if (quoteCheck < 2)
                    {
                        continue;
                    }
                }
                if (char.IsDigit(arr[i]))
                {
                    builder.Append(arr[i]);
                }
            }
            brokenKey = builder.ToString(); 
            if (string.IsNullOrEmpty(brokenKey))
            {
                return 0;
            }

            return int.Parse(brokenKey);
        }

        public string GetString(string key, int position = -1, string optionalFile = "")
        {
            string newFile = fileName;
            if (optionalFile != "")
            {
                newFile = optionalFile;
            }
            string brokenKey;
            string newKey = "\"" + key + "\"";
            StringBuilder builder = new StringBuilder();
            char[] arr;
            int quoteCheck = 0;
            List<string>? line = Program.globalLine;
            if (position != -1 && line != null)
            {
                string tempKey = line[position].Replace(newKey, "");
                arr = tempKey.ToCharArray();
            }
            else
            {
                arr = key.ToCharArray();
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (position == -1)
                {
                    if (arr[i] == '\"')
                    {
                        quoteCheck++;
                    }
                    if (quoteCheck < 2)
                    {
                        continue;
                    }
                }
                if (char.IsLetterOrDigit(arr[i]) || arr[i] == '.' || arr[i] == '_' || arr[i] == '-' || ((arr[i] == '/' || arr[i] == '\\') && arr[i] != '\t'))
                {
                    builder.Append(arr[i]);
                }
            }
            brokenKey = builder.ToString();

            return brokenKey;
        }
    }
}
