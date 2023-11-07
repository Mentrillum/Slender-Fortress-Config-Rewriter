using Microsoft.VisualBasic;
using Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SF2MConfigRewriteV2.Keys
{
	public class KeyValues
	{
		public string Name;
		private List<string>[] Keys;
		private List<bool> IsSection;
		private List<int> Indexes;
		Stack<KeyValues> Root;
		List<KeyValues> ParentKeys;

		public KeyValues(string name = "")
		{
			this.Name = name;
			this.Keys = new List<string>[2];
			for (int i = 0; i < 2; i++)
			{
				this.Keys[i] = new List<string>();
			}
			this.IsSection = new List<bool>();
			this.Indexes = new List<int>();
			Root = new Stack<KeyValues>();
			this.ParentKeys = new List<KeyValues>();
		}

		~KeyValues()
		{
			this.Keys = null;
			this.IsSection = null;
			this.Indexes = null;
		}

		public KeyValues FindKey(string key)
		{
			if (key == string.Empty || key.Length == 0)
			{
				return null;
			}

			return null;
		}

		public bool ReadFromFile(List<string> content)
		{
			while (this.Root.Count > 0)
			{
				this.Root.Pop();
			}
			this.IsSection.Clear();
			this.Indexes.Clear();
			this.Keys[0].Clear();
			this.Keys[1].Clear();
			this.ParentKeys.Clear();
			bool wasConditional = false;
			KeyValues nextKeys = null;
			KeyValues currentKeys = this;
			Stack<KeyValues> previousKeys = new Stack<KeyValues>();
			string section = string.Empty;
			int bracket = 0;
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < content.Count; i++)
			{
				builder.Clear();
				char[] arr;
				int quoteCheck = 0;
				arr = content[i].ToCharArray();
				if (i == content.Count - 2)
				{

				}
				for (int i2 = 0; i2 < arr.Length; i2++)
				{
					if ((arr[i2] == '\\' || arr[i2] == '/') && quoteCheck % 2 == 0)
					{
						if (i2 + 1 < arr.Length && (arr[i2 + 1] == '\\' || arr[i2 + 1] == '/'))
						{
							break;
						}
					}
					if (arr[i2] == '\"')
					{
						quoteCheck++;
					}
					if (char.IsWhiteSpace(arr[i2]) && quoteCheck % 2 != 1)
					{
						continue;
					}
					builder.Append(arr[i2]);
				}
				string s = builder.ToString();
				if (s.Length == 0 || s == string.Empty || s == "")
				{
					continue;
				}
				// Let's break some keyvalues down
				if (s[0] == '\"')
				{
					int splits = s.Split('\"').Length - 1;
					if (splits % 2 != 0 || splits > 4)
					{
						return false;
					}

					string[] splittedKeys = s.Split('\"');
					int index = 0;
					for (int i2 = 0; i2 < splittedKeys.Length; i2++)
					{
						if (splittedKeys[i2].Length == 0 || splittedKeys[i2] == string.Empty || splittedKeys[i2] == "" || splittedKeys[i2].Contains("//"))
						{
							continue;
						}
						currentKeys.Keys[index].Add(splittedKeys[i2]);
						index++;

						if (splittedKeys.Length == 3)
						{
							currentKeys.Keys[index].Add("");
							currentKeys.IsSection.Add(true);
							section = splittedKeys[i2];
						}
						else
						{
							if (i2 == splittedKeys.Length - 2)
							{
								currentKeys.IsSection.Add(false);
							}
						}
					}
					if (splittedKeys.Length > 3 && index == 1)
					{
						currentKeys.Keys[index].Add("");
						currentKeys.IsSection.Add(false);
					}
					currentKeys.Indexes.Add(i);
				}

				// Now let's split up some sections
				if (s[0] == '{')
				{
					bracket++;
					previousKeys.Push(currentKeys);
					nextKeys = new KeyValues();
					nextKeys.Name = section;
					currentKeys.ParentKeys.Add(nextKeys);
					currentKeys = nextKeys;
				}

				if (s[0] == '}')
				{
					bracket--;
					currentKeys = previousKeys.Pop();
				}
			}

			if (bracket != 0)
			{
				return false;
			}
			return this.GotoFirstSubKey();
		}

		public bool ReadFromFile(string content)
		{
			List<string> strings = new List<string>();
			strings = File.ReadAllLines(content).ToList<string>();

			return this.ReadFromFile(strings);
		}

		string RetrieveKeyValue(string key, out bool success)
		{
			for (int i = 0; i < this.Root.Peek().Keys[0].Count; i++)
			{
				if (i < this.Root.Peek().IsSection.Count)
				{
					if (this.Root.Peek().IsSection[i])
					{
						continue;
					}
				}

				if (this.Root.Peek().Keys[0][i] == key)
				{
					success = true;
					return this.Root.Peek().Keys[1][i];
				}
			}

			success = false;
			return string.Empty;
		}

		public float GetKeyValue(string key, float def)
		{
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return def;
			}

			return float.Parse(value);
		}

		public int GetKeyValue(string key, int def)
		{
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return def;
			}

			return int.Parse(value);
		}

		public string GetKeyValue(string key, string def)
		{
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return def;
			}

			return value;
		}

		public bool GetKeyValue(string key, bool def)
		{
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return def;
			}

			return int.Parse(value) != 0;
		}

		public void GetKeyValue(string key, out float[] inp, float[] def)
		{
			inp = def;
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return;
			}
			string[] subKey = value.Split(' ');
			int i = 0;
			foreach (string index in subKey)
			{
				inp[i] = float.Parse(index);
				i++;
			}
		}

		public void GetKeyValue(string key, out int[] inp, int[] def)
		{
			inp = def;
			bool success = false;
			string value = RetrieveKeyValue(key, out success);
			if (!success)
			{
				return;
			}
			string[] subKey = value.Split(' ');
			int i = 0;
			foreach (string index in subKey)
			{
				inp[i] = int.Parse(index);
				i++;
			}
		}

		public string GetProfileKeyWithDifficultySuffix(string key, Difficulty difficulty)
		{
			string value = string.Empty;
			if (difficulty < Difficulty.Easy || difficulty > Difficulty.Max)
			{
				return string.Empty;
			}

			string[] suffixes = new string[] { "easy", "", "hard", "insane", "nightmare", "apollyon" };
			if (difficulty != Difficulty.Normal)
			{
				value = key + "_" + suffixes[(int)difficulty];
			}
			else
			{
				value = key;
			}

			return value;
		}

		public void GetDifficultyValues(string baseKey, out float[] values, float[] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					values[i] = this.GetKeyValue(key, defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out int[] values, int[] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					values[i] = this.GetKeyValue(key, defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out bool[] values, bool[] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					values[i] = this.GetKeyValue(key, defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out string[] values, string[] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					values[i] = this.GetKeyValue(key, defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out float[][] values, float[][] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					this.GetKeyValue(key, out values[i], defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out int[][] values, int[][] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				bool success = false;
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string value = RetrieveKeyValue(key, out success);
				if (success)
				{
					this.GetKeyValue(key, out values[i], defaultValues[i]);
				}
				else
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		public bool JumpToKey(string name)
		{
			for (int i = 0; i < this.Root.Peek().ParentKeys.Count; i++)
			{
				if (this.Root.Peek().ParentKeys[i].Name == name)
				{
					this.Root.Push(this.Root.Peek().ParentKeys[i]);
					return true;
				}
			}
			return false;
		}

		public bool GoBack()
		{
			if (this.Root.Count() == 0)
			{
				return false;
			}
			this.Root.Pop();
			return true;
		}

		public string GetSectionName()
		{
			return this.Root.Count() == 0 ? this.Name : this.Root.Peek().Name;
		}

		public bool GotoFirstSubKey()
		{
			if (this.Root.Count != 0 && this.Root.Peek().ParentKeys.Count == 0)
			{
				return false;
			}

			if (this.Root.Count == 0)
			{
				this.Root.Push(this.ParentKeys[0]);
			}
			else
			{
				this.Root.Push(this.Root.Peek().ParentKeys[0]);
			}
			return true;
		}

		public bool GotoNextKey()
		{
			KeyValues kv = this.Root.Peek();
			if (kv == null)
			{
				return false;
			}

			this.Root.Pop();
			int index = this.Root.Peek().ParentKeys.IndexOf(kv);
			if (index == -1 || index + 1 >= this.Root.Peek().ParentKeys.Count)
			{
				// Fuck let's go back
				this.Root.Push(kv);
				return false;
			}

			this.Root.Push(this.Root.Peek().ParentKeys[index + 1]);
			return true;
		}

		public int GetSectionIndex(string name)
		{
			if (this.Root.Peek().Name == name)
			{
				return this.Root.Peek().Indexes[0] - 1;
			}
			int index = 0;
			do
			{
				index = this.Root.Peek().Keys[0].IndexOf(name);
			}
			while (!this.Root.Peek().IsSection[index]);
			index = this.Root.Peek().Indexes[index];
			return index;
		}

		public int GetKeyIndex(string name)
		{
			int index = 0;
			do
			{
				index = this.Root.Peek().Keys[0].IndexOf(name);
			}
			while (this.Root.Peek().IsSection[index]);
			index = this.Root.Peek().Indexes[index];
			return index;
		}

		public string FormatFloat(float value)
		{
			string result = value.ToString();
			if (!result.Contains('.'))
			{
				result += ".0";
			}
			else
			{
				if (!char.IsNumber(result[result.Length - 1]))
				{
					result += "0";
				}
			}
			return result;
		}

		public string FormatBool(bool value)
		{
			if (value)
			{
				return "1";
			}
			return "0";
		}

		public List<KeyValues> GetParentKeys()
		{
			return this.Root.Peek().ParentKeys;
		}
	}
}
