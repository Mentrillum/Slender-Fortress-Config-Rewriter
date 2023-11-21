
using SF2MConfigRewriteV2.Keys;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Stocks
{
	public enum Difficulty
	{
		Easy = 0,
		Normal,
		Hard,
		Insane,
		Nightmare,
		Apollyon,
		Max
	};

	public static class Stock
	{
		public static void InsertKeyValue(ref List<string> lines, ref int index, string key)
		{
			lines.Insert(index, key);
			index++;
		}

		public static void InsertAttackIndexes(ref List<int> indexes, string value)
		{
			indexes.Clear();
			if (value.Contains(' '))
			{
				string[] subKey = value.Split(' ');
				foreach (string str in subKey)
				{
					indexes.Add(int.Parse(str));
				}
			}
			else
			{
				char[] arr;
				arr = value.ToCharArray();
				for (int k = 0; k < arr.Length; k++)
				{
					indexes.Add(arr[k] - '0');
				}
			}
		}

		public static void InsertAnimationSection(ref List<string> lines, ref int index, string section, List<ProfileAnimation> animations, KeyValues kv)
		{
			InsertKeyValue(ref lines, ref index, "\"" + section + "\"");
			InsertKeyValue(ref lines, ref index, "{");

			float cycle = 0.0f;
			float rate = 1.0f;
			float duration = 0.0f;
			float footstep = 0.0f;
			string name = string.Empty;
			for (int i = 0; i < animations.Count; i++)
			{
				InsertKeyValue(ref lines, ref index, "\"" + (i + 1) + "\"");
				InsertKeyValue(ref lines, ref index, "{");

				for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
				{
					if (animations[i].Animations[i2] != name)
					{
						name = animations[i].Animations[i2];
						InsertKeyValue(ref lines, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("name", (Difficulty)i2) + "\" \"" + name + "\"");
					}
				}

				for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
				{
					if (animations[i].Playbackrates[i2] != rate)
					{
						rate = animations[i].Playbackrates[i2];
						InsertKeyValue(ref lines, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("rate", (Difficulty)i2) + "\" \"" + kv.FormatFloat(rate) + "\"");
					}
				}

				for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
				{
					if (animations[i].Cycles[i2] != cycle)
					{
						cycle = animations[i].Cycles[i2];
						InsertKeyValue(ref lines, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("cycle", (Difficulty)i2) + "\" \"" + kv.FormatFloat(cycle) + "\"");
					}
				}

				for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
				{
					if (animations[i].Durations[i2] != duration)
					{
						duration = animations[i].Durations[i2];
						InsertKeyValue(ref lines, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("duration", (Difficulty)i2) + "\" \"" + kv.FormatFloat(duration) + "\"");
					}
				}

				for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
				{
					if (animations[i].FootstepIntervals[i2] != footstep)
					{
						footstep = animations[i].FootstepIntervals[i2];
						InsertKeyValue(ref lines, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("footstepinterval", (Difficulty)i2) + "\" \"" + kv.FormatFloat(footstep) + "\"");
					}
				}

				InsertKeyValue(ref lines, ref index, "}");
				InsertKeyValue(ref lines, ref index, "");
			}

			InsertKeyValue(ref lines, ref index, "}");
		}

		public static void StoreAnimationData(ref List<ProfileAnimation> animations, KeyValues kv)
		{
			if (kv.GotoFirstSubKey())
			{
				do
				{
					ProfileAnimation animation = new ProfileAnimation();
					kv.GetDifficultyValues("name", out animation.Animations, animation.Animations);
					kv.GetDifficultyValues("playbackrate", out animation.Playbackrates, animation.Playbackrates);
					kv.GetDifficultyValues("cycle", out animation.Cycles, animation.Cycles);
					kv.GetDifficultyValues("footstepinterval", out animation.FootstepIntervals, animation.FootstepIntervals);
					kv.GetDifficultyValues("duration", out animation.Durations, animation.Durations);
					animations.Add(animation);
				}
				while (kv.GotoNextKey());

				kv.GoBack();
			}
			kv.GoBack();
		}

		public static void ReplaceAnimationNames(string fileName, string text, string keyName, bool ignoreDifficulty = false)
		{
			text = File.ReadAllText(fileName);
			text = text.Replace("\"" + keyName + "\"", "\"name\"");

			text = text.Replace("\"" + keyName + "_playbackrate\"", "\"playbackrate\"");

			text = text.Replace("\"" + keyName + "_footstepinterval\"", "\"footstepinterval\"");

			text = text.Replace("\"" + keyName + "_cycle\"", "\"cycle\"");

			if (!ignoreDifficulty)
			{
				text = text.Replace("\"" + keyName + "_hard\"", "\"name_hard\"");

				text = text.Replace("\"" + keyName + "_hard_playbackrate\"", "\"playbackrate_hard\"");

				text = text.Replace("\"" + keyName + "_hard_footstepinterval\"", "\"footstepinterval_hard\"");

				text = text.Replace("\"" + keyName + "_hard_cycle\"", "\"cycle_hard\"");

				text = text.Replace("\"" + keyName + "_insane\"", "\"name_insane\"");

				text = text.Replace("\"" + keyName + "_insane_playbackrate\"", "\"playbackrate_insane\"");

				text = text.Replace("\"" + keyName + "_insane_footstepinterval\"", "\"footstepinterval_insane\"");

				text = text.Replace("\"" + keyName + "_insane_cycle\"", "\"cycle_insane\"");

				text = text.Replace("\"" + keyName + "_nightmare\"", "\"name_nightmare\"");

				text = text.Replace("\"" + keyName + "_nightmare_playbackrate\"", "\"playbackrate_nightmare\"");

				text = text.Replace("\"" + keyName + "_nightamre_footstepinterval\"", "\"footstepinterval_nightmare\"");

				text = text.Replace("\"" + keyName + "_nightmare_cycle\"", "\"cycle_nightmare\"");

				text = text.Replace("\"" + keyName + "_apollyon\"", "\"name_apollyon\"");

				text = text.Replace("\"" + keyName + "_apollyon_playbackrate\"", "\"playbackrate_apollyon\"");

				text = text.Replace("\"" + keyName + "_apollyon_footstepinterval\"", "\"footstepinterval_apollyon\"");

				text = text.Replace("\"" + keyName + "_apollyon_cycle\"", "\"cycle_apollyon\"");
			}
			File.WriteAllText(fileName, text);
		}
	}

	/*public class KeyValues
	{
		public List<string> file;
		~KeyValues()
		{
			file = null;
		}
		string BreakKeyValueDown(string value)
		{
			string broken = string.Empty;
			StringBuilder builder = new StringBuilder();
			char[] arr;
			int quoteCheck = 0;
			arr = value.ToCharArray();
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] == '\"')
				{
					quoteCheck++;
				}
				if (quoteCheck < 3)
				{
					continue;
				}
				else if (quoteCheck >= 3 && arr[i] == '\"')
				{
					continue;
				}
				builder.Append(arr[i]);
			}
			broken = builder.ToString();
			return broken;
		}

		public string GetProfileKeyWithDifficultySuffix(string key, Difficulty difficulty)
		{
			string value = string.Empty;
			if (difficulty < Difficulty.Easy || difficulty > Difficulty.Max)
			{
				return string.Empty;
			}

			string name = this.GetName(key);
			if (name == "")
			{
				name = key;
			}

			string[] suffixes = new string[] { "easy", "", "hard", "insane", "nightmare", "apollyon" };
			if (difficulty != Difficulty.Normal)
			{
				value = name + "_" + suffixes[(int)difficulty];
			}
			else
			{
				value = name;
			}

			return value;
		}

		// Float
		public void GetDifficultyValues(string baseKey, out float[] values, float[] defaultValues)
		{
			values = defaultValues;

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					float value = float.Parse(this.GetFloat(result));
					values[i] = value;
				}
				else // Remember this for Sourcepawn
				{
					if (i > 0)
					{
						values[i] = values[i - 1];
					}
				}
			}
		}

		// Int
		public void GetDifficultyValues(string baseKey, out int[] values, int[] defaultValues)
		{
			values = defaultValues;

			if (this.GetName(baseKey) != string.Empty)
			{
				int value = this.GetNum(baseKey);
				for (int i = 0; i < (int)Difficulty.Max; i++)
				{
					values[i] = value;
				}
			}

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					int value = this.GetNum(key);
					values[i] = value;
				}
			}
		}

		// Bool
		public void GetDifficultyValues(string baseKey, out bool[] values, bool[] defaultValues)
		{
			values = defaultValues;

			if (this.GetName(baseKey) != string.Empty)
			{
				bool value = this.GetBool(baseKey);
				for (int i = 0; i < (int)Difficulty.Max; i++)
				{
					values[i] = value;
				}
			}

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					bool value = this.GetBool(key);
					values[i] = value;
				}
			}
		}

		// String
		public void GetDifficultyValues(string baseKey, out string[] values, string[] defaultValues)
		{
			values = defaultValues;

			if (this.GetName(baseKey) != string.Empty)
			{
				string value = this.GetString(baseKey);
				for (int i = 0; i < (int)Difficulty.Max; i++)
				{
					values[i] = value;
				}
			}

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					string value = this.GetString(key);
					values[i] = value;
				}
			}
		}

		// Array
		public void GetDifficultyValues(string baseKey, out float[][] values, float[][] defaultValues)
		{
			values = defaultValues;
			float[] temp = new float[3];

			if (this.GetName(baseKey) != string.Empty)
			{
				this.GetArray(baseKey, out temp, temp);
				for (int i = 0; i < (int)Difficulty.Max; i++)
				{
					values[i] = temp;
				}
			}

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					this.GetArray(baseKey, out temp, temp);
					values[i] = temp;
				}
			}
		}

		public void GetDifficultyValues(string baseKey, out int[][] values, int[][] defaultValues)
		{
			values = defaultValues;
			int[] temp = new int[3];

			if (this.GetName(baseKey) != string.Empty)
			{
				this.GetArray(baseKey, out temp, temp);
				for (int i = 0; i < (int)Difficulty.Max; i++)
				{
					values[i] = temp;
				}
			}

			string key;

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				key = GetProfileKeyWithDifficultySuffix(baseKey, (Difficulty)i);
				string result = this.file.FirstOrDefault(s => s.Contains(key)) ?? string.Empty;
				if (result != string.Empty)
				{
					this.GetArray(baseKey, out temp, temp);
					values[i] = temp;
				}
			}
		}

		public string GetFloat(string key, string def = "0.0")
		{
			if (key == string.Empty)
			{
				return def;
			}
			string result = this.BreakKeyValueDown(key);
			if (!result.Contains('.'))
			{
				result += ".0";
			}
			return result;
		}

		public int GetNum(string key, int def = 0)
		{
			if (key == string.Empty)
			{
				return def;
			}
			return int.Parse(this.BreakKeyValueDown(key));
		}

		public bool GetBool(string key, bool def = false)
		{
			if (key == string.Empty)
			{
				return def;
			}
			return int.Parse(this.BreakKeyValueDown(key)) != 0;
		}

		public string GetString(string key, string def = "")
		{
			if (key == string.Empty)
			{
				return def;
			}
			return this.BreakKeyValueDown(key);
		}

		// That's right, we can get 2D vectors, vectors, colors, or whatever
		public void GetArray(string key, out float[] values, float[] def)
		{
			values = def;
			if (key == string.Empty)
			{
				values = def;
				return;
			}
			string value = this.BreakKeyValueDown(key);
			string[] subKey = value.Split(' ');
			int i = 0;
			foreach (string index in subKey)
			{
				values[i] = float.Parse(index);
				i++;
			}
		}

		public void GetArray(string key, out int[] values, int[] def)
		{
			values = def;
			if (key == string.Empty)
			{
				values = def;
				return;
			}
			string value = this.BreakKeyValueDown(key);
			string[] subKey = value.Split(' ');
			int i = 0;
			foreach (string index in subKey)
			{
				values[i] = int.Parse(index);
				i++;
			}
		}

		public string GetName(string key)
		{
			string broken = string.Empty;
			StringBuilder builder = new StringBuilder();
			char[] arr;
			int quoteCheck = 0;
			arr = key.ToCharArray();
			for (int i = 0; i < arr.Length; i++)
			{
				if (arr[i] == '\"')
				{
					quoteCheck++;
				}
				if (quoteCheck == 0)
				{
					continue;
				}
				if (quoteCheck >= 2)
				{
					break;
				}
				if (arr[i] == '\"')
				{
					continue;
				}
				builder.Append(arr[i]);
			}
			broken = builder.ToString();
			return broken;
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
	}*/

	public enum DamageType
	{
		Invalid = -1,
		Jarate = 0,
		Milk,
		Gas,
		Mark,
		Ignite,
		Stun,
		Bleed,
		Smite,
		Random
	};

	public class ProfileSound
	{
		public List<string> Sounds;
		public int Channel = 0;
		public float Volume = 1.0f;
		public int Flags = 0;
		public int Level = 90;
		public int Pitch = 100;
		public float CooldownMin = 1.5f;
		public float CooldownMax = 1.5f;
		public int PitchRandomMin = 100;
		public int PitchRandomMax = 100;
		public float Radius = 850.0f;
		public float Chance = 1.0f;

		public ProfileSound()
		{
			this.Sounds = new List<string>();
		}

		~ProfileSound()
		{
			this.Sounds = null;
		}

		public void GetValues(KeyValues kv)
		{
			this.Channel = kv.GetKeyValue("channel", this.Channel);
			this.Volume = kv.GetKeyValue("volume", this.Volume);
			this.Flags = kv.GetKeyValue("flags", this.Flags);
			this.Level = kv.GetKeyValue("level",	this.Level);
			this.Pitch = kv.GetKeyValue("pitch", this.Pitch);
			this.CooldownMin = kv.GetKeyValue("cooldown_min", this.CooldownMin);
			this.CooldownMax = kv.GetKeyValue("cooldown_max", this.CooldownMax);
			this.PitchRandomMin = kv.GetKeyValue("pitch_random_min", this.Pitch);
			this.PitchRandomMax = kv.GetKeyValue("pitch_random_max", this.Pitch);
			this.Chance = kv.GetKeyValue("chance", this.Chance);
			kv.JumpToKey("paths");
			for (int i = 1; ; i++)
			{
				string path = kv.GetKeyValue(i.ToString(), string.Empty);
				if (path == string.Empty)
				{
					break;
				}

				this.Sounds.Add(path);
			}
			kv.GoBack();
		}

		void InsertKeyValue(ref List<string> lines, ref int index, string key)
		{
			lines.Insert(index, key);
			index++;
		}

		public void InsertSection(string section, ref List<string> lines, ref int index, KeyValues kv)
		{
			InsertKeyValue(ref lines, ref index, "\"" + section + "\"");
			InsertKeyValue(ref lines, ref index, "{");
			bool whiteSpace = false;
			if (this.Channel != 0)
			{
				InsertKeyValue(ref lines, ref index, "\"channel\" \"" + this.Channel + "\"");
				whiteSpace = true;
			}

			if (this.Volume != 1.0f)
			{
				InsertKeyValue(ref lines, ref index, "\"volume\" \"" + kv.FormatFloat(this.Volume) + "\"");
				whiteSpace = true;
			}

			if (this.Flags != 0)
			{
				InsertKeyValue(ref lines, ref index, "\"volume\" \"" + this.Flags + "\"");
				whiteSpace = true;
			}

			if (this.Level != 90)
			{
				InsertKeyValue(ref lines, ref index, "\"level\" \"" + this.Level + "\"");
				whiteSpace = true;
			}

			if (this.Pitch != 100)
			{
				InsertKeyValue(ref lines, ref index, "\"pitch\" \"" + this.Pitch + "\"");
				whiteSpace = true;
			}

			if (this.CooldownMin != 1.5f)
			{
				InsertKeyValue(ref lines, ref index, "\"cooldown_min\" \"" + kv.FormatFloat(this.CooldownMin) + "\"");
				whiteSpace = true;
			}

			if (this.CooldownMax != 1.5f)
			{
				InsertKeyValue(ref lines, ref index, "\"cooldown_max\" \"" + kv.FormatFloat(this.CooldownMax) + "\"");
				whiteSpace = true;
			}

			if (this.PitchRandomMin != this.Pitch || this.PitchRandomMax != this.Pitch)
			{
				InsertKeyValue(ref lines, ref index, "\"pitch_random_min\" \"" + this.PitchRandomMin + "\"");
				InsertKeyValue(ref lines, ref index, "\"pitch_random_max\" \"" + this.PitchRandomMax + "\"");
				whiteSpace = true;
			}

			if (this.Chance != 1.5f)
			{
				InsertKeyValue(ref lines, ref index, "\"chance\" \"" + kv.FormatFloat(this.Chance) + "\"");
				whiteSpace = true;
			}

			if (whiteSpace)
			{
				InsertKeyValue(ref lines, ref index, "");
			}
			InsertKeyValue(ref lines, ref index, "\"paths\"");
			InsertKeyValue(ref lines, ref index, "{");
			for (int i = 0; i < this.Sounds.Count; i++)
			{
				InsertKeyValue(ref lines, ref index, "\"" + (i + 1) + "\" \"" + this.Sounds[i] + "\"");
			}
			InsertKeyValue(ref lines, ref index, "}");

			InsertKeyValue(ref lines, ref index, "}");
		}
	}

	public class ProfileAnimation
	{
		public string[] Animations = new string[(int)Difficulty.Max];
		public float[] Playbackrates = new float[(int)Difficulty.Max];
		public float[] FootstepIntervals = new float[(int)Difficulty.Max];
		public float[] Cycles = new float[(int)Difficulty.Max];
		public float[] Durations = new float[(int)Difficulty.Max];
	}

	public class DamageEffectData
	{
		public DamageType Type;
		public bool[] Enabled = new bool[(int)Difficulty.Max];
		public string Particle;
		public string Sound;
		public bool AttachParticle;
		public bool Beam;
		public float[] Duration = new float[(int)Difficulty.Max];
		public List<int> AttackIndexex;
		public string Search, SearchAlt;

		public bool MarkSilent;

		public string StunFlag;
		public float[] StunSlowdown = new float[(int)Difficulty.Max];

		public float[] SmiteDamage = new float[(int)Difficulty.Max];
		public int[] SmiteDamageType = new int[(int)Difficulty.Max];
		public int[] SmiteColor = { 255, 255, 255, 255 };
		public bool SmiteMessage;
		public string SmiteSound;

		public DamageEffectData(DamageType type)
		{
			this.Type = type;
			this.SearchAlt = string.Empty;
			this.Search = string.Empty;
			this.Sound = string.Empty;
			this.Particle = string.Empty;
			this.AttackIndexex = new List<int>();
			this.AttackIndexex.Add(1);
			this.AttachParticle = true;
			this.SmiteSound = ")ambient/explosions/explode_9.wav";
			this.StunFlag = "slow";

			for (int i = 0; i < 4; i++)
			{
				this.SmiteColor[i] = 255;
			}

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				this.Enabled[i] = true;
				this.Duration[i] = 8.0f;
				this.StunSlowdown[i] = 0.5f;
				this.SmiteDamage[i] = 9001.0f;
				this.SmiteDamageType[i] = 1048576;
			}
		}

		~DamageEffectData()
		{
			this.AttackIndexex = null;
		}
	}

	public class ShockwaveData
	{
		public bool Enabled = false;
		public float[] Height = new float[(int)Difficulty.Max];
		public float[] Range = new float[(int)Difficulty.Max];
		public float[] DrainAmount = new float[(int)Difficulty.Max];
		public float[] Force = new float[(int)Difficulty.Max];
		public bool Stun;
		public float[] StunDuration = new float[(int)Difficulty.Max];
		public float[] StunSlowdown = new float[(int)Difficulty.Max];
		public float Width1;
		public float Width2;
		public float Amplitude;
		public int[] Color1 = new int[4];
		public int[] Color2 = new int[4];
		public string BeamSprite;
		public string HaloSprite;

		public List<int> AttackIndexex;

		public ShockwaveData()
		{
			for (int i = 0; i < 3; i++)
			{
				this.Color1[i] = 128;
				this.Color2[i] = 255;
			}

			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				this.Height[i] = 80;
				this.Range[i] = 200;
				this.Force[i] = 600;
				this.DrainAmount[i] = 0;
				this.StunDuration[i] = 2;
				this.StunSlowdown[i] = 0.7f;
			}

			this.Width1 = 40;
			this.Width2 = 20;

			this.Amplitude = 5;

			this.Color1[3] = 255;
			this.Color2[3] = 255;
			this.BeamSprite = "sprites/laser.vmt";
			this.HaloSprite = "sprites/halo01.vmt";
			this.AttackIndexex = new List<int>();
			this.AttackIndexex.Add(1);
		}

		~ShockwaveData()
		{
			this.AttackIndexex = null;
		}
	}

	public class CloakData
	{
		public bool Enabled = false;
		public float[] CloakRange = new float[(int)Difficulty.Max];
		public float[] DecloakRange = new float[(int)Difficulty.Max];
		public float[] CloakDuration = new float[(int)Difficulty.Max];
		public float[] Cooldown = new float[(int)Difficulty.Max];
		public float[] SpeedMultiplier = new float[(int)Difficulty.Max];

		public int[] RenderColor = new int[4];
		public int RenderMode = 1;
		public string CloakParticle = "drg_cow_explosioncore_charged_blue";
		public string CloakOnSound = ")weapons/medi_shield_deploy.wav";
		public string CloakOffSound = ")weapons/medi_shield_retract.wav";

		public CloakData()
		{
			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				this.CloakRange[i] = 350.0f;
				this.DecloakRange[i] = 150.0f;
				this.CloakDuration[i] = 10.0f;
				this.Cooldown[i] = 8.0f;
				this.SpeedMultiplier[i] = 1.0f;
			}
			for (int i = 0; i < 4; i++)
			{
				this.RenderColor[i] = 0;
			}
		}
	}

	public class RageData
	{
		public float PercentThreshold = 0.75f;
		public bool IncreaseDifficulty = true;

		public bool Heal = false;
		public bool CloakToHeal = false;
		public float FleeRange = 1024.0f;
		public float HealAmount = 0.5f;
		public float HealDelay = 0.0f;
		public float HealDuration = 1.0f;

		public ProfileSound StartSounds;
		public ProfileSound HealSounds;

		public RageData()
		{
			StartSounds = new ProfileSound();
			HealSounds = new ProfileSound();
		}
	}

	public class CopyData
	{
		public bool[] Enabled = new bool[(int)Difficulty.Max];
		public int[] MaxCopies = new int[(int)Difficulty.Max];
		public float[] TeleportDistance = new float[(int)Difficulty.Max];
		public bool[] Fakes = new bool[(int)Difficulty.Max];

		public CopyData()
		{
			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				this.Enabled[i] = false;
				this.MaxCopies[i] = 1;
				this.TeleportDistance[i] = 800.0f;
				this.Fakes[i] = false;
			}
		}
	}

	public class AutoChaseData
	{
		public bool[] Enabled = new bool[(int)Difficulty.Max];
		public int[] Threshold = new int[(int)Difficulty.Max];
		public bool[] Sprinters = new bool[(int)Difficulty.Max];
		public int[] AddOnStateChange = new int[(int)Difficulty.Max];
		public int[] AddFootsteps = new int[(int)Difficulty.Max];
		public int[] AddLoudFootsteps = new int[(int)Difficulty.Max];
		public int[] AddQuietFootsteps = new int[(int)Difficulty.Max];
		public int[] AddVoice = new int[(int)Difficulty.Max];
		public int[] AddWeapon = new int[(int)Difficulty.Max];

		public AutoChaseData()
		{
			for (int i = 0; i < (int)Difficulty.Max; i++)
			{
				this.Enabled[i] = false;
				this.Threshold[i] = 100;
				this.Sprinters[i] = false;
				this.AddOnStateChange[i] = 0;
				this.AddFootsteps[i] = 2;
				this.AddLoudFootsteps[i] = 2;
				this.AddQuietFootsteps[i] = 0;
				this.AddVoice[i] = 8;
				this.AddWeapon[i] = 4;
			}
		}
	}
}