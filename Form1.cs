using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Stocks;
using static System.Net.Mime.MediaTypeNames;
using static Stocks.Stock;
using SF2MConfigRewriteV2.Keys;
using System.Xml.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

namespace SF2MConfigRewriteV2
{
	public partial class FormMain : Form
	{
		List<string> files = new List<string>();
		public FormMain()
		{
			InitializeComponent();
		}

		private void openButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = "c:\\";
			ofd.Filter = "Config files (*.cfg)|*.cfg";
			ofd.FilterIndex = 0;
			ofd.RestoreDirectory = true;
			ofd.Multiselect = true;
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				files.Clear();
				foreach (string s in ofd.FileNames)
				{
					files.Add(s);
				}
				foreach (string s in ofd.SafeFileNames)
				{
					configsList.Items.Add(s);
				}
			}
		}

		private void rewriteButton_Click(object sender, EventArgs e)
		{
			if (configsList.Items.Count <= 0)
			{
				return;
			}

			KeyValues kv = new KeyValues();
			List<string> globalLine;
			int index = 0;
			bool success = true;
			for (int file = 0; file < configsList.Items.Count; file++)
			{
				string fileName = files[file];
				progressBox.Text = "Rewriting " + configsList.Items[file];
				globalLine = File.ReadAllLines(fileName).ToList<string>();
				// Delete any unused key values
				for (int i = 0; i < globalLine.Count; i++)
				{
					if (globalLine[i].Contains("\"jump_speed\"") || globalLine[i].Contains("\"airspeed\"") || globalLine[i].Contains("\"jump_cooldown\"") ||
						globalLine[i].Contains("\"random_attacks\"") || globalLine[i].Contains("\"enable_boss_tilting\"") || globalLine[i].Contains("\"think_time_min\"")
						|| globalLine[i].Contains("\"think_time_max\"") || globalLine[i].Contains("\"anger_start\"") || globalLine[i].Contains("\"anger_page_time_diff\"") || globalLine[i].Contains("\"anger_page_add\"") || globalLine[i].Contains("\"appear_chance_threshold\"") || globalLine[i].Contains("\"appear_chance_min\"")
						 || globalLine[i].Contains("\"appear_chance_max\"") || globalLine[i].Contains("\"proxies_teleport_enabled\"")
						 || globalLine[i].Contains("\"attack_props\"") || globalLine[i].Contains("\"attack_damageforce\"") || globalLine[i].Contains("\"attack_damage_vs_props\"") || globalLine[i].Contains("\"use_engine_sounds\"") || globalLine[i].Contains("\"difficulty_affects_animations\"")
						 || globalLine[i].Contains("\"multi_miss_sounds\"") || globalLine[i].Contains("\"multi_hit_sounds\"") || globalLine[i].Contains("\"multi_attack_sounds\"") ||
						 globalLine[i].Contains("\"speed_max\"") || globalLine[i].Contains("\"walkspeed_max\"") || globalLine[i].Contains("\"use_alert_walking_animation\"") ||
						 globalLine[i].Contains("\"spawn_animation\"") || globalLine[i].Contains("\"use_chase_initial_animation\"")
						 || globalLine[i].Contains("\"chase_persistency_time_init\"") || globalLine[i].Contains("\"chase_persistency_time_init_attack\"")
						 || globalLine[i].Contains("\"chase_persistency_time_add_attack\"") || globalLine[i].Contains("\"chase_persistency_time_init_newtarget\"")
						 || globalLine[i].Contains("\"chase_persistency_time_add_newtarget\"") || globalLine[i].Contains("\"chase_persistency_time_add_visible_min\"")
						 || globalLine[i].Contains("\"chase_persistency_time_add_visible_max\"") || globalLine[i].Contains("\"chase_persistency_time_init_stun\"")
						 || globalLine[i].Contains("\"chase_persistency_time_add_stun\"") || globalLine[i].Contains("\"walkspeed_max_hard\"")
						  || globalLine[i].Contains("\"walkspeed_max_insane\"") || globalLine[i].Contains("\"walkspeed_max_nightmare\"")
						   || globalLine[i].Contains("\"walkspeed_max_apollyon\"") || globalLine[i].Contains("\"speed_max_hard\"") || globalLine[i].Contains("\"speed_max_insane\"")
							|| globalLine[i].Contains("\"speed_max_nightmare\"") || globalLine[i].Contains("\"speed_max_apollyon\""))
					{
						globalLine.RemoveAt(i);
						i--;
						if (!globalLine[i].Contains('\"') && !globalLine[i].Contains('/') && !globalLine[i].Contains('{') && !globalLine[i].Contains('}'))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}
				}

				for (int i = 0; i < globalLine.Count; i++)
				{
					if (globalLine[i].Contains("\"cancel_distance\""))
					{
						globalLine[i] = globalLine[i].Replace("\"cancel_distance\"", "\"cancel_distance_max\"");
					}
				}
				File.WriteAllLines(fileName, globalLine);

				string text = File.ReadAllText(fileName);

				ReplaceAnimationNames(fileName, text, "animation_idle");

				ReplaceAnimationNames(fileName, text, "animation_walk");

				ReplaceAnimationNames(fileName, text, "animation_walkalert");

				ReplaceAnimationNames(fileName, text, "animation_run");

				ReplaceAnimationNames(fileName, text, "animation_attack");

				ReplaceAnimationNames(fileName, text, "animation_stun");

				ReplaceAnimationNames(fileName, text, "animation_shoot");

				ReplaceAnimationNames(fileName, text, "animation_deathcam");

				ReplaceAnimationNames(fileName, text, "animation_chaseinitial");

				ReplaceAnimationNames(fileName, text, "animation_spawn");

				ReplaceAnimationNames(fileName, text, "animation_crawlwalk");

				ReplaceAnimationNames(fileName, text, "animation_crawlrun");

				ReplaceAnimationNames(fileName, text, "animation_heal");

				ReplaceAnimationNames(fileName, text, "animation_fleestart");

				ReplaceAnimationNames(fileName, text, "animation_rage");

				ReplaceAnimationNames(fileName, text, "animation_jump");

				ReplaceAnimationNames(fileName, text, "animation_death");
				globalLine = File.ReadAllLines(fileName).ToList<string>();

				bool read = kv.ReadFromFile(fileName);
				if (!read)
				{
					progressBox.Text = "Failed to rewrite " + configsList.Items[file];
					success = false;
					break;
				}

				if (kv.JumpToKey("animations"))
				{
					if (kv.JumpToKey("walkalert"))
					{
						List<ProfileAnimation> walkAnimations = new List<ProfileAnimation>();
						StoreAnimationData(ref walkAnimations, kv);

						int bracket = 0, endIndex = 0;
						index = 0;
						index = kv.GetSectionIndex("walkalert");
						endIndex = index;
						while (!globalLine[endIndex].Contains('{'))
						{
							endIndex++;
						}
						endIndex++;
						while (bracket >= 0)
						{
							if (globalLine[endIndex].Contains('}'))
							{
								bracket--;
							}
							else if (globalLine[endIndex].Contains('{'))
							{
								bracket++;
							}
							endIndex++;
						}

						while (index != endIndex)
						{
							globalLine.RemoveAt(index);
							endIndex--;
						}
						kv.GoBack();

						index = kv.GetSectionIndex("animations");
						InsertKeyValue(ref globalLine, ref index, "\"postures\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"alert\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"conditions\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"on_alert\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");

						InsertKeyValue(ref globalLine, ref index, "\"animations\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertAnimationSection(ref globalLine, ref index, "walk", walkAnimations, kv);

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "");

						File.WriteAllLines(fileName, globalLine);
						kv.ReadFromFile(fileName);
					}
					else
					{
						kv.GoBack();
					}
				}

				bool copiesAlert = false, companionsAlert = false;
				if (kv.JumpToKey("attributes"))
				{
					if (kv.JumpToKey("alert companions"))
					{
						companionsAlert = true;
						kv.GoBack();
						index = kv.GetSectionIndex("alert companions");
						do
						{
							globalLine.RemoveAt(index);
						}
						while (!globalLine[index].Contains('}'));
						globalLine.RemoveAt(index);
						kv.ReadFromFile(globalLine);
						kv.JumpToKey("attributes");
					}

					if (kv.JumpToKey("alert copies"))
					{
						copiesAlert = true;
						kv.GoBack();
						index = kv.GetSectionIndex("alert copies");
						do
						{
							globalLine.RemoveAt(index);
						}
						while (!globalLine[index].Contains('}'));
						globalLine.RemoveAt(index);
						kv.ReadFromFile(globalLine);
						kv.JumpToKey("attributes");
					}

					kv.GoBack();

					if (copiesAlert || companionsAlert)
					{
						index = kv.GetSectionIndex("attributes");

						InsertKeyValue(ref globalLine, ref index, "\"chase\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"chase_together\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertKeyValue(ref globalLine, ref index, "\"enabled\" \"1\"");
						if (copiesAlert)
						{
							InsertKeyValue(ref globalLine, ref index, "\"copies\" \"1\"");
						}
						if (companionsAlert)
						{
							InsertKeyValue(ref globalLine, ref index, "\"companions\" \"1\"");
						}
						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "");
					}

					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("shockwave", false))
				{
					ShockwaveData shockwaveData = new ShockwaveData();
					shockwaveData.Enabled = true;
					kv.GetDifficultyValues("shockwave_height", out shockwaveData.Height, shockwaveData.Height);
					kv.GetDifficultyValues("shockwave_range", out shockwaveData.Range, shockwaveData.Range);
					kv.GetDifficultyValues("shockwave_drain", out shockwaveData.DrainAmount, shockwaveData.DrainAmount);
					kv.GetDifficultyValues("shockwave_force", out shockwaveData.Force, shockwaveData.Force);
					shockwaveData.Stun = kv.GetKeyValue("shockwave_stun", false);
					kv.GetDifficultyValues("shockwave_stun_duration", out shockwaveData.StunDuration, shockwaveData.StunDuration);
					kv.GetDifficultyValues("shockwave_stun_slowdown", out shockwaveData.StunSlowdown, shockwaveData.StunSlowdown);
					InsertAttackIndexes(ref shockwaveData.AttackIndexex, kv.GetKeyValue("shockwave_attack_index", "1"));
					shockwaveData.Width1 = kv.GetKeyValue("shockwave_width_1", shockwaveData.Width1);
					shockwaveData.Width2 = kv.GetKeyValue("shockwave_width_2", shockwaveData.Width2);
					shockwaveData.Amplitude = kv.GetKeyValue("shockwave_amplitude", shockwaveData.Amplitude);
					float[] color3 = new float[3];
					for (int i2 = 0; i2 < 3; i2++)
					{
						color3[i2] = shockwaveData.Color1[i2];
					}
					kv.GetKeyValue("shockwave_color_1", out color3, color3);
					for (int i2 = 0; i2 < 3; i2++)
					{
						shockwaveData.Color1[i2] = Convert.ToInt32(Math.Round(color3[i2]));
					}
					for (int i2 = 0; i2 < 3; i2++)
					{
						color3[i2] = shockwaveData.Color2[i2];
					}
					kv.GetKeyValue("shockwave_color_2", out color3, color3);
					for (int i2 = 0; i2 < 3; i2++)
					{
						shockwaveData.Color2[i2] = Convert.ToInt32(Math.Round(color3[i2]));
					}
					shockwaveData.Color1[3] = kv.GetKeyValue("shockwave_alpha_1", shockwaveData.Color1[3]);
					shockwaveData.Color2[3] = kv.GetKeyValue("shockwave_alpha_2", shockwaveData.Color2[3]);
					shockwaveData.BeamSprite = kv.GetKeyValue("shockwave_beam_sprite", shockwaveData.BeamSprite);
					shockwaveData.HaloSprite = kv.GetKeyValue("shockwave_halo_sprite", shockwaveData.HaloSprite);
					if (kv.JumpToKey("attacks"))
					{
						int atkIndex = 0;
						if (kv.GotoFirstSubKey())
						{
							do
							{
								atkIndex++;
								if (!shockwaveData.AttackIndexex.Contains(atkIndex))
								{
									continue;
								}
								string section = kv.GetSectionName();
								index = kv.GetSectionIndex(section);
								int bracket = 0, endIndex = index;
								while (!globalLine[endIndex].Contains('{'))
								{
									endIndex++;
								}
								endIndex++;
								while (bracket >= 0)
								{
									if (globalLine[endIndex].Contains('}'))
									{
										bracket--;
									}
									else if (globalLine[endIndex].Contains('{'))
									{
										bracket++;
									}
									endIndex++;
								}
								endIndex--;
								InsertKeyValue(ref globalLine, ref endIndex, "");
								InsertKeyValue(ref globalLine, ref endIndex, "\"shockwave\"");
								InsertKeyValue(ref globalLine, ref endIndex, "{");

								float height = 80.0f, range = 200.0f, force = 600.0f, drain = 0.0f;
								for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
								{
									if (shockwaveData.Height[i2] != height)
									{
										height = shockwaveData.Height[i2];
										InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("height", (Difficulty)i2) + "\" \"" + kv.FormatFloat(height) + "\"");
									}
								}

								for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
								{
									if (shockwaveData.Range[i2] != range)
									{
										range = shockwaveData.Range[i2];
										InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("range", (Difficulty)i2) + "\" \"" + kv.FormatFloat(range) + "\"");
									}
								}

								for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
								{
									if (shockwaveData.Force[i2] != force)
									{
										force = shockwaveData.Force[i2];
										InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("force", (Difficulty)i2) + "\" \"" + kv.FormatFloat(force) + "\"");
									}
								}

								for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
								{
									if (shockwaveData.DrainAmount[i2] != drain)
									{
										drain = shockwaveData.DrainAmount[i2];
										InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("battery_drain", (Difficulty)i2) + "\" \"" + kv.FormatFloat(drain) + "\"");
									}
								}

								if (shockwaveData.Stun)
								{
									InsertKeyValue(ref globalLine, ref endIndex, "");
									InsertKeyValue(ref globalLine, ref endIndex, "\"apply_conditions\"");
									InsertKeyValue(ref globalLine, ref endIndex, "{");

									InsertKeyValue(ref globalLine, ref endIndex, "\"stun\"");
									InsertKeyValue(ref globalLine, ref endIndex, "{");

									float duration = 8.0f, slowdown = 0.5f;
									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (shockwaveData.StunDuration[i2] != duration)
										{
											duration = shockwaveData.StunDuration[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("duration", (Difficulty)i2) + "\" \"" + kv.FormatFloat(duration) + "\"");
										}
									}

									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (shockwaveData.StunSlowdown[i2] != slowdown)
										{
											slowdown = shockwaveData.StunSlowdown[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("slow_multiplier", (Difficulty)i2) + "\" \"" + kv.FormatFloat(slowdown) + "\"");
										}
									}
									InsertKeyValue(ref globalLine, ref endIndex, "}");

									InsertKeyValue(ref globalLine, ref endIndex, "}");
								}

								InsertKeyValue(ref globalLine, ref endIndex, "");
								InsertKeyValue(ref globalLine, ref endIndex, "\"effects\"");
								InsertKeyValue(ref globalLine, ref endIndex, "{");

								InsertKeyValue(ref globalLine, ref endIndex, "\"ring_1\"");
								InsertKeyValue(ref globalLine, ref endIndex, "{");
								InsertKeyValue(ref globalLine, ref endIndex, "\"type\" \"te_beamring\"");
								InsertKeyValue(ref globalLine, ref endIndex, "");
								string insert = string.Empty;
								for (int i2 = 0; i2 < shockwaveData.Color1.Length; i2++)
								{
									insert += shockwaveData.Color1[i2].ToString();
									if (i2 != shockwaveData.Color1.Length - 1)
									{
										insert += " ";
									}
								}
								InsertKeyValue(ref globalLine, ref endIndex, "\"color\" \"" + insert + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"beam_sprite\" \"" + shockwaveData.BeamSprite + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"halo_sprite\" \"" + shockwaveData.HaloSprite + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"origin\" \"0 0 5\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"width\" \"" + kv.FormatFloat(shockwaveData.Width1) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"end_radius\" \"" + kv.FormatFloat(shockwaveData.Range[1]) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"amplitude\" \"" + kv.FormatFloat(shockwaveData.Amplitude) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"framerate\" \"30\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"lifetime\" \"0.2\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"speed\" \"" + Convert.ToInt32(Math.Floor(shockwaveData.Range[1])) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "}");
								InsertKeyValue(ref globalLine, ref endIndex, "");

								InsertKeyValue(ref globalLine, ref endIndex, "\"ring_2\"");
								InsertKeyValue(ref globalLine, ref endIndex, "{");
								InsertKeyValue(ref globalLine, ref endIndex, "\"type\" \"te_beamring\"");
								InsertKeyValue(ref globalLine, ref endIndex, "");
								insert = string.Empty;
								for (int i2 = 0; i2 < shockwaveData.Color2.Length; i2++)
								{
									insert += shockwaveData.Color2[i2].ToString();
									if (i2 != shockwaveData.Color2.Length - 1)
									{
										insert += " ";
									}
								}
								InsertKeyValue(ref globalLine, ref endIndex, "\"color\" \"" + insert + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"beam_sprite\" \"" + shockwaveData.BeamSprite + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"halo_sprite\" \"" + shockwaveData.HaloSprite + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"origin\" \"0 0 5\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"width\" \"" + kv.FormatFloat(shockwaveData.Width2) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"end_radius\" \"" + kv.FormatFloat(shockwaveData.Range[1]) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"amplitude\" \"" + kv.FormatFloat(shockwaveData.Amplitude) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"framerate\" \"30\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"lifetime\" \"0.2\"");
								InsertKeyValue(ref globalLine, ref endIndex, "\"speed\" \"" + Convert.ToInt32(Math.Floor(shockwaveData.Range[1])) + "\"");
								InsertKeyValue(ref globalLine, ref endIndex, "}");

								InsertKeyValue(ref globalLine, ref endIndex, "}");

								InsertKeyValue(ref globalLine, ref endIndex, "}");
								kv.ReadFromFile(globalLine);
								kv.JumpToKey("attacks");
								kv.JumpToKey(section);
							}
							while (kv.GotoNextKey());

							kv.GoBack();
						}
						kv.GoBack();
					}
					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"shockwave\" \"1\"") || globalLine[i].Contains("\"shockwave_"))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}
					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("player_damage_effects", false))
				{
					List<DamageEffectData> datas = new List<DamageEffectData>();
					int stunType = kv.GetKeyValue("player_stun_type", 0);
					bool attach = kv.GetKeyValue("player_attach_particle", true);
					if (kv.GetKeyValue("player_jarate_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Jarate);
						effect.Particle = "peejar_impact";
						effect.Sound = ")weapons/jar_single.wav";
						kv.GetDifficultyValues("player_jarate_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_jarate_attack_indexs", "1"));
						effect.Particle = kv.GetKeyValue("player_jarate_particle", effect.Particle);
						effect.Beam = kv.GetKeyValue("player_jarate_beam_particle", false);
						effect.Sound = kv.GetKeyValue("player_jarate_sound", effect.Sound);
						effect.AttachParticle = attach;
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_milk_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Milk);
						effect.Particle = "peejar_impact";
						effect.Sound = ")weapons/jar_single.wav";
						kv.GetDifficultyValues("player_milk_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_milk_attack_indexs", "1"));
						effect.Particle = kv.GetKeyValue("player_milk_particle", effect.Particle);
						effect.Beam = kv.GetKeyValue("player_milk_beam_particle", false);
						effect.Sound = kv.GetKeyValue("player_milk_sound", effect.Sound);
						effect.AttachParticle = attach;
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_gas_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Gas);
						effect.Particle = "peejar_impact";
						effect.Sound = ")weapons/jar_single.wav";
						kv.GetDifficultyValues("player_gas_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_gas_attack_indexs", "1"));
						effect.Particle = kv.GetKeyValue("player_gas_particle", effect.Particle);
						effect.Beam = kv.GetKeyValue("player_gas_beam_particle", false);
						effect.Sound = kv.GetKeyValue("player_gas_sound", effect.Sound);
						effect.AttachParticle = attach;
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_mark_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Mark);
						kv.GetDifficultyValues("player_mark_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_mark_attack_indexs", "1"));
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_silent_mark_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Mark);
						kv.GetDifficultyValues("player_silent_mark_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_silent_mark_attack_indexs", "1"));
						effect.MarkSilent = true;
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_ignite_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Ignite);
						kv.GetDifficultyValues("player_ignite_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_ignite_attack_indexs", "1"));
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_bleed_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Bleed);
						kv.GetDifficultyValues("player_bleed_duration", out effect.Duration, effect.Duration);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_bleed_attack_indexs", "1"));
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_smite_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Smite);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_smite_attack_indexs", "1"));
						effect.SmiteMessage = kv.GetKeyValue("player_smite_message", false);
						kv.GetDifficultyValues("player_smite_damage", out effect.SmiteDamage, effect.SmiteDamage);
						kv.GetDifficultyValues("player_smite_damage_type", out effect.SmiteDamageType, effect.SmiteDamageType);
						effect.SmiteSound = kv.GetKeyValue("player_smite_sound", effect.SmiteSound);
						effect.SmiteColor[0] = kv.GetKeyValue("player_smite_color_r", effect.SmiteColor[0]);
						effect.SmiteColor[1] = kv.GetKeyValue("player_smite_color_g", effect.SmiteColor[1]);
						effect.SmiteColor[2] = kv.GetKeyValue("player_smite_color_b", effect.SmiteColor[2]);
						effect.SmiteColor[3] = kv.GetKeyValue("player_smite_transparency", effect.SmiteColor[3]);
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_stun_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Stun);
						effect.Particle = "xms_icicle_melt";
						effect.Sound = ")weapons/icicle_freeze_victim_01.wav";
						kv.GetDifficultyValues("player_stun_duration", out effect.Duration, effect.Duration);
						kv.GetDifficultyValues("player_stun_slowdown", out effect.StunSlowdown, effect.StunSlowdown);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_stun_attack_indexs", "1"));
						effect.Particle = kv.GetKeyValue("player_stun_particle", effect.Particle);
						effect.Beam = kv.GetKeyValue("player_stun_beam_particle", false);
						effect.Sound = kv.GetKeyValue("player_stun_sound", effect.Sound);
						effect.AttachParticle = attach;
						switch (stunType)
						{
							case 0:
								effect.StunFlag = "slow";
								break;
							case 1:
								effect.StunFlag = "slow";
								break;
							case 2:
								effect.StunFlag = "loser";
								break;
							case 3:
								effect.StunFlag = "stuck no_fx";
								break;
							case 4:
								effect.StunFlag = "boo";
								break;
						}
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_electric_slow_on_hit", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Stun);
						effect.Particle = "electrocuted_gibbed_red";
						effect.Sound = string.Empty;
						kv.GetDifficultyValues("player_electric_slow_duration", out effect.Duration, effect.Duration);
						kv.GetDifficultyValues("player_electric_slow_slowdown", out effect.StunSlowdown, effect.StunSlowdown);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_electrocute_attack_indexs", "1"));
						effect.Particle = kv.GetKeyValue("player_electric_red_particle", effect.Particle);
						effect.Beam = kv.GetKeyValue("player_electric_beam_particle", false);
						effect.AttachParticle = attach;
						switch (stunType)
						{
							case 0:
								effect.StunFlag = "slow";
								break;
							case 1:
								effect.StunFlag = "slow";
								break;
							case 2:
								effect.StunFlag = "loser";
								break;
							case 3:
								effect.StunFlag = "stuck no_fx";
								break;
							case 4:
								effect.StunFlag = "boo";
								break;
						}
						datas.Add(effect);
					}

					if (kv.GetKeyValue("player_damage_random_effects", false))
					{
						DamageEffectData effect = new DamageEffectData(DamageType.Random);
						kv.GetDifficultyValues("player_random_duration", out effect.Duration, effect.Duration);
						kv.GetDifficultyValues("player_random_slowdown", out effect.StunSlowdown, effect.StunSlowdown);
						InsertAttackIndexes(ref effect.AttackIndexex, kv.GetKeyValue("player_random_attack_indexes", "1"));
						switch (kv.GetKeyValue("player_random_stun_type", 0))
						{
							case 0:
								effect.StunFlag = "slow";
								break;
							case 1:
								effect.StunFlag = "slow";
								break;
							case 2:
								effect.StunFlag = "loser";
								break;
							case 3:
								effect.StunFlag = "stuck no_fx";
								break;
							case 4:
								effect.StunFlag = "boo";
								break;
						}
						datas.Add(effect);
					}

					if (kv.JumpToKey("attacks"))
					{
						int atkIndex = 0;
						if (kv.GotoFirstSubKey())
						{
							do
							{
								atkIndex++;
								bool skip = true;
								for (int i = 0; i < datas.Count; i++)
								{
									if (datas[i].AttackIndexex.Contains(atkIndex))
									{
										skip = false;
										break;
									}
								}
								if (skip)
								{
									continue;
								}
								string section = kv.GetSectionName();
								index = kv.GetSectionIndex(section);
								int bracket = 0, endIndex = index;
								while (!globalLine[endIndex].Contains('{'))
								{
									endIndex++;
								}
								endIndex++;
								while (bracket >= 0)
								{
									if (globalLine[endIndex].Contains('}'))
									{
										bracket--;
									}
									else if (globalLine[endIndex].Contains('{'))
									{
										bracket++;
									}
									endIndex++;
								}
								endIndex--;
								InsertKeyValue(ref globalLine, ref endIndex, "");
								InsertKeyValue(ref globalLine, ref endIndex, "\"apply_conditions\"");
								InsertKeyValue(ref globalLine, ref endIndex, "{");
								bool addSpace = false;

								for (int i = 0; i < datas.Count; i++)
								{
									if (!datas[i].AttackIndexex.Contains(atkIndex) || datas[i].Type == DamageType.Invalid)
									{
										continue;
									}
									if (addSpace)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "");
									}
									addSpace = true;
									switch (datas[i].Type)
									{
										case DamageType.Jarate:
											InsertKeyValue(ref globalLine, ref endIndex, "\"jarate\"");
											break;

										case DamageType.Milk:
											InsertKeyValue(ref globalLine, ref endIndex, "\"milk\"");
											break;

										case DamageType.Gas:
											InsertKeyValue(ref globalLine, ref endIndex, "\"gas\"");
											break;

										case DamageType.Ignite:
											InsertKeyValue(ref globalLine, ref endIndex, "\"ignite\"");
											break;

										case DamageType.Mark:
											InsertKeyValue(ref globalLine, ref endIndex, "\"mark\"");
											break;

										case DamageType.Bleed:
											InsertKeyValue(ref globalLine, ref endIndex, "\"bleed\"");
											break;

										case DamageType.Smite:
											InsertKeyValue(ref globalLine, ref endIndex, "\"smite\"");
											break;

										case DamageType.Stun:
											InsertKeyValue(ref globalLine, ref endIndex, "\"stun\"");
											break;

										case DamageType.Random:
											InsertKeyValue(ref globalLine, ref endIndex, "\"random\"");
											break;
									}
									InsertKeyValue(ref globalLine, ref endIndex, "{");

									float duration = 8.0f, slowdown = 0.5f, damage = 9001.0f;
									int damageType = 1048576;
									int[] color = { 255, 255, 255, 255 };
									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (datas[i].Duration[i2] != duration)
										{
											duration = datas[i].Duration[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("duration", (Difficulty)i2) + "\" \"" + kv.FormatFloat(duration) + "\"");
										}
									}

									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (datas[i].StunSlowdown[i2] != slowdown)
										{
											slowdown = datas[i].StunSlowdown[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("slow_multiplier", (Difficulty)i2) + "\" \"" + kv.FormatFloat(slowdown) + "\"");
										}
									}

									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (datas[i].SmiteDamage[i2] != damage)
										{
											damage = datas[i].SmiteDamage[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("damage", (Difficulty)i2) + "\" \"" + kv.FormatFloat(damage) + "\"");
										}
									}

									for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
									{
										if (datas[i].SmiteDamageType[i2] != damageType)
										{
											damageType = datas[i].SmiteDamageType[i2];
											InsertKeyValue(ref globalLine, ref endIndex, "\"" + kv.GetProfileKeyWithDifficultySuffix("damagetype", (Difficulty)i2) + "\" \"" + damageType + "\"");
										}
									}

									for (int i2 = 0; i2 < 4; i2++)
									{
										if (datas[i].SmiteColor[i2] != color[i2])
										{
											string insert = string.Empty;
											for (int i3 = 0; i3 < datas[i].SmiteColor.Length; i3++)
											{
												insert += datas[i].SmiteColor[i3].ToString();
												if (i3 != datas[i].SmiteColor.Length - 1)
												{
													insert += " ";
												}
											}
											InsertKeyValue(ref globalLine, ref endIndex, "\"color\" \"" + insert + "\"");
											break;
										}
									}


									if (datas[i].SmiteMessage)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "\"message\" \"1\"");
									}

									if (datas[i].SmiteSound != ")ambient/explosions/explode_9.wav" && datas[i].SmiteSound != "ambient/explosions/explode_9.wav")
									{
										InsertKeyValue(ref globalLine, ref endIndex, "\"hit_sound\" \"" + datas[i].SmiteSound + "\"");
									}

									if (datas[i].MarkSilent)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "\"silent\" \"1\"");
									}

									if (datas[i].StunFlag != "slow")
									{
										InsertKeyValue(ref globalLine, ref endIndex, "\"flags\" \"" + datas[i].StunFlag + "\"");
									}

									if (datas[i].Type == DamageType.Random)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "\"random_types\" \"ignite gas bleed mark jarate milk stun\"");
									}

									if (datas[i].Sound != string.Empty)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "");
										InsertKeyValue(ref globalLine, ref endIndex, "\"sounds\"");
										InsertKeyValue(ref globalLine, ref endIndex, "{");

										InsertKeyValue(ref globalLine, ref endIndex, "\"paths\"");
										InsertKeyValue(ref globalLine, ref endIndex, "{");
										InsertKeyValue(ref globalLine, ref endIndex, "\"1\" \"" + datas[i].Sound + "\"");
										InsertKeyValue(ref globalLine, ref endIndex, "}");

										InsertKeyValue(ref globalLine, ref endIndex, "}");
									}

									if (datas[i].Particle != string.Empty)
									{
										InsertKeyValue(ref globalLine, ref endIndex, "");
										InsertKeyValue(ref globalLine, ref endIndex, "\"particles\"");
										InsertKeyValue(ref globalLine, ref endIndex, "{");

										InsertKeyValue(ref globalLine, ref endIndex, "\"base\"");
										InsertKeyValue(ref globalLine, ref endIndex, "{");
										InsertKeyValue(ref globalLine, ref endIndex, "\"particle\" \"" + datas[i].Particle + "\"");
										if (!datas[i].AttachParticle)
										{
											InsertKeyValue(ref globalLine, ref endIndex, "\"beam\" \"0\"");
										}
										if (datas[i].Beam)
										{
											InsertKeyValue(ref globalLine, ref endIndex, "\"beam\" \"1\"");
										}
										InsertKeyValue(ref globalLine, ref endIndex, "}");

										InsertKeyValue(ref globalLine, ref endIndex, "}");
									}
									InsertKeyValue(ref globalLine, ref endIndex, "}");
								}

								InsertKeyValue(ref globalLine, ref endIndex, "}");

								kv.ReadFromFile(globalLine);
								kv.JumpToKey("attacks");
								kv.JumpToKey(section);
							}
							while (kv.GotoNextKey());

							kv.GoBack();
						}
						kv.GoBack();
					}

					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"player_damage_effects\"") || globalLine[i].Contains("\"player_jarate_") || globalLine[i].Contains("\"player_milk_") || globalLine[i].Contains("\"player_ignite_") || globalLine[i].Contains("\"player_stun_") || globalLine[i].Contains("\"player_smite_") || globalLine[i].Contains("\"player_electric_") || globalLine[i].Contains("\"player_electrocute_") || globalLine[i].Contains("\"player_bleed_") || globalLine[i].Contains("\"player_attach_particle\"") || globalLine[i].Contains("\"player_damage_random_effects\"") || globalLine[i].Contains("\"player_random_") || globalLine[i].Contains("\"player_gas_") || globalLine[i].Contains("\"player_mark_") || globalLine[i].Contains("\"player_silent_mark_"))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}
					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("cloak_enable", false))
				{
					CloakData data = new CloakData();
					data.Enabled = true;
					kv.GetDifficultyValues("cloak_range", out data.CloakRange, data.CloakRange);
					kv.GetDifficultyValues("cloak_decloak_range", out data.DecloakRange, data.DecloakRange);
					kv.GetDifficultyValues("cloak_duration", out data.CloakDuration, data.CloakDuration);
					kv.GetDifficultyValues("cloak_cooldown", out data.Cooldown, data.Cooldown);
					kv.GetDifficultyValues("cloak_speed_multiplier", out data.SpeedMultiplier, data.SpeedMultiplier);
					kv.GetKeyValue("cloak_rendercolor", out data.RenderColor, data.RenderColor);
					data.RenderMode = kv.GetKeyValue("cloak_rendermode", data.RenderMode);
					data.CloakParticle = kv.GetKeyValue("cloak_particle", data.CloakParticle);
					data.CloakOnSound = kv.GetKeyValue("cloak_on_sound", data.CloakOnSound);
					data.CloakOffSound = kv.GetKeyValue("cloak_off_sound", data.CloakOffSound);
					index = kv.GetKeyIndex("cloak_enable");
					InsertKeyValue(ref globalLine, ref index, "\"cloaking\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"enabled\" \"1\"");
					InsertKeyValue(ref globalLine, ref index, "");

					float range = 350.0f, decloakRange = 150.0f, duration = 10.0f, cooldown = 8.0f;
					int[] renderColor = { 0, 0, 0, 0 };
					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.CloakRange[i2] != range)
						{
							range = data.CloakRange[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("cloak_range", (Difficulty)i2) + "\" \"" + kv.FormatFloat(range) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.DecloakRange[i2] != decloakRange)
						{
							decloakRange = data.DecloakRange[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("decloak_range", (Difficulty)i2) + "\" \"" + kv.FormatFloat(decloakRange) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.CloakDuration[i2] != duration)
						{
							duration = data.CloakDuration[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("duration", (Difficulty)i2) + "\" \"" + kv.FormatFloat(duration) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Cooldown[i2] != cooldown)
						{
							cooldown = data.Cooldown[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("cooldown", (Difficulty)i2) + "\" \"" + kv.FormatFloat(cooldown) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");

					for (int i2 = 0; i2 < 4; i2++)
					{
						if (data.RenderColor[i2] != renderColor[i2])
						{
							string insert = string.Empty;
							for (int i3 = 0; i3 < data.RenderColor.Length; i3++)
							{
								insert += data.RenderColor[i3].ToString();
								if (i3 != data.RenderColor.Length - 1)
								{
									insert += " ";
								}
							}
							InsertKeyValue(ref globalLine, ref index, "\"color\" \"" + insert + "\"");
							break;
						}
					}

					if (data.RenderMode != 1)
					{
						InsertKeyValue(ref globalLine, ref index, "\"rendermode\" \"" + data.RenderMode + "\"");
					}

					InsertKeyValue(ref globalLine, ref index, "");

					InsertKeyValue(ref globalLine, ref index, "\"effects\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					InsertKeyValue(ref globalLine, ref index, "\"cloak\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					InsertKeyValue(ref globalLine, ref index, "\"sound\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"type\" \"sound\"");
					InsertKeyValue(ref globalLine, ref index, "");
					InsertKeyValue(ref globalLine, ref index, "\"paths\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"1\" \"" + data.CloakOnSound + "\"");
					InsertKeyValue(ref globalLine, ref index, "}");
					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "");

					InsertKeyValue(ref globalLine, ref index, "\"particle\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"type\" \"particle\"");
					InsertKeyValue(ref globalLine, ref index, "");
					InsertKeyValue(ref globalLine, ref index, "\"particlename\" \"" + data.CloakParticle + "\"");
					InsertKeyValue(ref globalLine, ref index, "\"origin\" \"0 0 35\"");
					InsertKeyValue(ref globalLine, ref index, "\"lifetime\" \"0.1\"");
					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "");

					InsertKeyValue(ref globalLine, ref index, "\"decloak\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					InsertKeyValue(ref globalLine, ref index, "\"sound\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"type\" \"sound\"");
					InsertKeyValue(ref globalLine, ref index, "");
					InsertKeyValue(ref globalLine, ref index, "\"paths\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"1\" \"" + data.CloakOffSound + "\"");
					InsertKeyValue(ref globalLine, ref index, "}");
					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "");

					InsertKeyValue(ref globalLine, ref index, "\"particle\"");
					InsertKeyValue(ref globalLine, ref index, "{");
					InsertKeyValue(ref globalLine, ref index, "\"type\" \"particle\"");
					InsertKeyValue(ref globalLine, ref index, "");
					InsertKeyValue(ref globalLine, ref index, "\"particlename\" \"" + data.CloakParticle + "\"");
					InsertKeyValue(ref globalLine, ref index, "\"origin\" \"0 0 35\"");
					InsertKeyValue(ref globalLine, ref index, "\"lifetime\" \"0.1\"");
					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "}");

					kv.ReadFromFile(globalLine);
					float[] speed = new float[(int)Difficulty.Max];
					kv.GetDifficultyValues("speed", out speed, speed);
					float defSpeed = speed[1];
					bool goSearch = false;
					for (int i = 0; i < (int)Difficulty.Max; i++)
					{
						if (data.SpeedMultiplier[i] != 1.0f)
						{
							goSearch = true;
							break;
						}
					}
					if (goSearch && kv.JumpToKey("postures"))
					{
						if (kv.GotoFirstSubKey())
						{
							while (kv.GotoNextKey()) ;

							string section = kv.GetSectionName();
							index = kv.GetSectionIndex(section);
							int bracket = 0;
							while (!globalLine[index].Contains('{'))
							{
								index++;
							}
							index++;
							while (bracket >= 0)
							{
								if (globalLine[index].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[index].Contains('{'))
								{
									bracket++;
								}
								index++;
							}

							InsertKeyValue(ref globalLine, ref index, "");
							InsertKeyValue(ref globalLine, ref index, "\"cloaked\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							defSpeed = speed[1] * data.SpeedMultiplier[1];

							for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
							{
								if (speed[i2] != defSpeed)
								{
									defSpeed = speed[i2] * data.SpeedMultiplier[i2];
									InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("speed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defSpeed) + "\"");
								}
							}
							InsertKeyValue(ref globalLine, ref index, "");

							InsertKeyValue(ref globalLine, ref index, "\"conditions\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "\"on_cloak\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "}");
							InsertKeyValue(ref globalLine, ref index, "}");

							InsertKeyValue(ref globalLine, ref index, "}");

						}
						kv.GoBack();
					}
					else
					{
						index = kv.GetSectionIndex("animations");
						InsertKeyValue(ref globalLine, ref index, "\"postures\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"cloaked\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						defSpeed = speed[1] * data.SpeedMultiplier[1];

						for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
						{
							if (speed[i2] != defSpeed)
							{
								defSpeed = speed[i2] * data.SpeedMultiplier[i2];
								InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("speed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defSpeed) + "\"");
							}
						}
						InsertKeyValue(ref globalLine, ref index, "");

						InsertKeyValue(ref globalLine, ref index, "\"conditions\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertKeyValue(ref globalLine, ref index, "\"on_cloak\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "");
					}

					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"cloak_") && !globalLine[i].Contains("\"cloak_to_heal\""))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}

					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("copy", false) || kv.GetKeyValue("copy_hard", false)
					|| kv.GetKeyValue("copy_insane", false) || kv.GetKeyValue("copy_nightmare", false)
					 || kv.GetKeyValue("copy_apollyon", false))
				{
					CopyData data = new CopyData();
					kv.GetDifficultyValues("copy", out data.Enabled, data.Enabled);
					kv.GetDifficultyValues("copy_max", out data.MaxCopies, data.MaxCopies);
					kv.GetDifficultyValues("copy_teleport_dist_from_others", out data.TeleportDistance, data.TeleportDistance);
					kv.GetDifficultyValues("fake_copies", out data.Fakes, data.Fakes);
					index = kv.GetKeyIndex("copy");
					if (index == 0)
					{
						index = kv.GetKeyIndex("copy_hard");
					}
					if (index == 0)
					{
						index = kv.GetKeyIndex("copy_insane");
					}
					if (index == 0)
					{
						index = kv.GetKeyIndex("copy_nightmare");
					}
					if (index == 0)
					{
						index = kv.GetKeyIndex("copy_apollyon");
					}
					InsertKeyValue(ref globalLine, ref index, "\"copies\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					bool enabled = false, fakes = false;
					int max = 1;
					float distance = 800.0f;
					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Enabled[i2] != enabled)
						{
							enabled = data.Enabled[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("enabled", (Difficulty)i2) + "\" \"" + kv.FormatBool(enabled) + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.MaxCopies[i2] != max)
						{
							max = data.MaxCopies[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("max", (Difficulty)i2) + "\" \"" + max + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.TeleportDistance[i2] != distance)
						{
							distance = data.TeleportDistance[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("teleport_spacing_between", (Difficulty)i2) + "\" \"" + kv.FormatFloat(distance) + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Fakes[i2] != fakes)
						{
							fakes = data.Fakes[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("fakes", (Difficulty)i2) + "\" \"" + kv.FormatBool(fakes) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "}");
					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"copy") || globalLine[i].Contains("\"fake_copies\""))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}

					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("auto_chase_enabled", false))
				{
					AutoChaseData data = new AutoChaseData();
					kv.GetDifficultyValues("auto_chase_enabled", out data.Enabled, data.Enabled);
					kv.GetDifficultyValues("auto_chase_sound_threshold", out data.Threshold, data.Threshold);
					kv.GetDifficultyValues("auto_chase_sprinters", out data.Sprinters, data.Sprinters);
					kv.GetDifficultyValues("auto_chase_sound_add", out data.AddOnStateChange, data.AddOnStateChange);
					kv.GetDifficultyValues("auto_chase_sound_add_footsteps", out data.AddFootsteps, data.AddFootsteps);
					kv.GetDifficultyValues("auto_chase_sound_add_footsteps_loud", out data.AddLoudFootsteps, data.AddLoudFootsteps);
					kv.GetDifficultyValues("auto_chase_sound_add_footsteps_quiet", out data.AddQuietFootsteps, data.AddQuietFootsteps);
					kv.GetDifficultyValues("auto_chase_sound_add_voice", out data.AddVoice, data.AddVoice);
					kv.GetDifficultyValues("auto_chase_sound_add_weapon", out data.AddWeapon, data.AddWeapon);
					index = kv.GetKeyIndex("auto_chase_enabled");

					InsertKeyValue(ref globalLine, ref index, "\"autochase\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					bool enabled = false, sprinters = false;
					int threshold = 100, addState = 0, addFootsteps = 2, addLoudFootsteps = 2, addQuietFootsteps = 0, addVoice = 8, addWeapon = 4;
					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Enabled[i2] != enabled)
						{
							enabled = data.Enabled[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("enabled", (Difficulty)i2) + "\" \"" + kv.FormatBool(enabled) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Threshold[i2] != threshold)
						{
							threshold = data.Threshold[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("threshold", (Difficulty)i2) + "\" \"" + threshold + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.Sprinters[i2] != sprinters)
						{
							sprinters = data.Sprinters[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("sprinters", (Difficulty)i2) + "\" \"" + kv.FormatBool(sprinters) + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "");
					InsertKeyValue(ref globalLine, ref index, "\"add\"");
					InsertKeyValue(ref globalLine, ref index, "{");

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddOnStateChange[i2] != addState)
						{
							addState = data.AddOnStateChange[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("on_state_change", (Difficulty)i2) + "\" \"" + addState + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddFootsteps[i2] != addFootsteps)
						{
							addFootsteps = data.AddFootsteps[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("footsteps", (Difficulty)i2) + "\" \"" + addFootsteps + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddLoudFootsteps[i2] != addLoudFootsteps)
						{
							addLoudFootsteps = data.AddLoudFootsteps[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("footsteps_loud", (Difficulty)i2) + "\" \"" + addLoudFootsteps + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddQuietFootsteps[i2] != addQuietFootsteps)
						{
							addQuietFootsteps = data.AddQuietFootsteps[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("footsteps_quiet", (Difficulty)i2) + "\" \"" + addQuietFootsteps + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddVoice[i2] != addVoice)
						{
							addVoice = data.AddVoice[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("voice", (Difficulty)i2) + "\" \"" + addVoice + "\"");
						}
					}

					for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
					{
						if (data.AddWeapon[i2] != addWeapon)
						{
							addWeapon = data.AddWeapon[i2];
							InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("weapon", (Difficulty)i2) + "\" \"" + addWeapon + "\"");
						}
					}

					InsertKeyValue(ref globalLine, ref index, "}");

					InsertKeyValue(ref globalLine, ref index, "}");

					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"auto_chase_"))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}

					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				if (kv.GetKeyValue("boxing_boss", false))
				{
					List<ProfileAnimation> rageAnimations = new List<ProfileAnimation>();
					List<ProfileAnimation> healAnimations = new List<ProfileAnimation>();
					List<ProfileAnimation> fleeAnimations = new List<ProfileAnimation>();
					if (kv.JumpToKey("animations"))
					{
						if (kv.JumpToKey("rage"))
						{
							StoreAnimationData(ref rageAnimations, kv);

							int bracket = 0, endIndex = 0;
							index = 0;
							index = kv.GetSectionIndex("rage");
							endIndex = index;
							while (!globalLine[endIndex].Contains('{'))
							{
								endIndex++;
							}
							endIndex++;
							while (bracket >= 0)
							{
								if (globalLine[endIndex].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[endIndex].Contains('{'))
								{
									bracket++;
								}
								endIndex++;
							}

							while (index != endIndex)
							{
								globalLine.RemoveAt(index);
								endIndex--;
							}
							kv.ReadFromFile(globalLine);
							kv.JumpToKey("animations");
						}

						if (kv.JumpToKey("heal"))
						{
							StoreAnimationData(ref healAnimations, kv);

							int bracket = 0, endIndex = 0;
							index = 0;
							index = kv.GetSectionIndex("heal");
							endIndex = index;
							while (!globalLine[endIndex].Contains('{'))
							{
								endIndex++;
							}
							endIndex++;
							while (bracket >= 0)
							{
								if (globalLine[endIndex].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[endIndex].Contains('{'))
								{
									bracket++;
								}
								endIndex++;
							}

							while (index != endIndex)
							{
								globalLine.RemoveAt(index);
								endIndex--;
							}
							kv.ReadFromFile(globalLine);
							kv.JumpToKey("animations");
						}

						if (kv.JumpToKey("fleestart"))
						{
							StoreAnimationData(ref fleeAnimations, kv);

							int bracket = 0, endIndex = 0;
							index = 0;
							index = kv.GetSectionIndex("fleestart");
							endIndex = index;
							while (!globalLine[endIndex].Contains('{'))
							{
								endIndex++;
							}
							endIndex++;
							while (bracket >= 0)
							{
								if (globalLine[endIndex].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[endIndex].Contains('{'))
								{
									bracket++;
								}
								endIndex++;
							}

							while (index != endIndex)
							{
								globalLine.RemoveAt(index);
								endIndex--;
							}
							kv.ReadFromFile(globalLine);
							kv.JumpToKey("animations");
						}
						kv.GoBack();
					}

					List<ProfileSound> rageSounds = new List<ProfileSound>();
					if (kv.JumpToKey("sound_rage"))
					{
						ProfileSound rageSound = new ProfileSound();
						rageSound.GetValues(kv);
						kv.GoBack();
						rageSounds.Add(rageSound);
					}

					if (kv.JumpToKey("sound_rage_2"))
					{
						ProfileSound rageSound = new ProfileSound();
						rageSound.GetValues(kv);
						kv.GoBack();
						rageSounds.Add(rageSound);
					}

					if (kv.JumpToKey("sound_rage_3"))
					{
						ProfileSound rageSound = new ProfileSound();
						rageSound.GetValues(kv);
						kv.GoBack();
						rageSounds.Add(rageSound);
					}
					ProfileSound healSound = new ProfileSound();
					if (kv.JumpToKey("sound_heal_self"))
					{
						healSound.GetValues(kv);
						kv.GoBack();
					}

					index = kv.GetKeyIndex("boxing_boss");
					bool goHeal = kv.GetKeyValue("self_heal_enabled", false);
					float healPercent = kv.GetKeyValue("health_percentage_to_heal", 0.35f);
					float healTimer = kv.GetKeyValue("heal_timer", 0.0f);
					float healDuration = kv.GetKeyValue("heal_timer_animation", 0.0f) - healTimer;
					float[] heals = new float[3];
					heals[0] = kv.GetKeyValue("heal_percentage_one", 0.75f);
					heals[1] = kv.GetKeyValue("heal_percentage_two", 0.5f);
					heals[2] = kv.GetKeyValue("heal_percentage_three", 0.25f);
					float[] ranges = new float[2];
					ranges[0] = kv.GetKeyValue("heal_range_min", 600.0f);
					ranges[1] = kv.GetKeyValue("heal_range_max", 1200.0f);
					bool cloak = kv.GetKeyValue("cloak_to_heal", false);

					if (!kv.JumpToKey("rages"))
					{
						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"rages\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"1\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (!goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"0.75\"");
							InsertKeyValue(ref globalLine, ref index, "\"invincible\" \"1\"");
						}
						else
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"" + kv.FormatFloat(healPercent) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"heal\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "\"delay\" \"" + kv.FormatFloat(healTimer) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"duration\" \"" + kv.FormatFloat(healDuration) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_min\" \"" + kv.FormatFloat(ranges[0]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_max\" \"" + kv.FormatFloat(ranges[1]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"amount\" \"" + kv.FormatFloat(heals[0]) + "\"");

							if (cloak)
							{
								InsertKeyValue(ref globalLine, ref index, "\"cloak\" \"1\"");
							}
							InsertKeyValue(ref globalLine, ref index, "}");
						}

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"animations\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						List<ProfileAnimation> animations = rageAnimations;
						if (goHeal)
						{
							animations = fleeAnimations;
						}
						InsertAnimationSection(ref globalLine, ref index, "start", animations, kv);

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							animations = healAnimations;
							InsertAnimationSection(ref globalLine, ref index, "healing", animations, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"sounds\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (rageSounds.Count > 0)
						{
							ProfileSound sound = rageSounds[0];
							sound.InsertSection("start", ref globalLine, ref index, kv);
						}

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							healSound.InsertSection("healing", ref globalLine, ref index, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"2\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (!goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"0.5\"");
							InsertKeyValue(ref globalLine, ref index, "\"invincible\" \"1\"");
						}
						else
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"" + kv.FormatFloat(healPercent) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"heal\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "\"delay\" \"" + kv.FormatFloat(healTimer) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"duration\" \"" + kv.FormatFloat(healDuration) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_min\" \"" + kv.FormatFloat(ranges[0]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_max\" \"" + kv.FormatFloat(ranges[1]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"amount\" \"" + kv.FormatFloat(heals[1]) + "\"");

							if (cloak)
							{
								InsertKeyValue(ref globalLine, ref index, "\"cloak\" \"1\"");
							}
							InsertKeyValue(ref globalLine, ref index, "}");
						}

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"animations\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						animations = rageAnimations;
						if (goHeal)
						{
							animations = fleeAnimations;
						}
						InsertAnimationSection(ref globalLine, ref index, "start", animations, kv);

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							animations = healAnimations;
							InsertAnimationSection(ref globalLine, ref index, "healing", animations, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"sounds\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (rageSounds.Count > 0)
						{
							ProfileSound sound = rageSounds[0];
							if (rageSounds.Count > 1)
							{
								sound = rageSounds[1];
							}
							sound.InsertSection("start", ref globalLine, ref index, kv);
						}

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							healSound.InsertSection("healing", ref globalLine, ref index, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"3\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (!goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"0.25\"");
							InsertKeyValue(ref globalLine, ref index, "\"invincible\" \"1\"");
						}
						else
						{
							InsertKeyValue(ref globalLine, ref index, "\"health_percent\" \"" + kv.FormatFloat(healPercent) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"heal\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "\"delay\" \"" + kv.FormatFloat(healTimer) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"duration\" \"" + kv.FormatFloat(healDuration) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_min\" \"" + kv.FormatFloat(ranges[0]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"flee_range_max\" \"" + kv.FormatFloat(ranges[1]) + "\"");
							InsertKeyValue(ref globalLine, ref index, "\"amount\" \"" + kv.FormatFloat(heals[2]) + "\"");

							if (cloak)
							{
								InsertKeyValue(ref globalLine, ref index, "\"cloak\" \"1\"");
							}
							InsertKeyValue(ref globalLine, ref index, "}");
						}

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"animations\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						animations = rageAnimations;
						if (goHeal)
						{
							animations = fleeAnimations;
						}
						InsertAnimationSection(ref globalLine, ref index, "start", animations, kv);

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							animations = healAnimations;
							InsertAnimationSection(ref globalLine, ref index, "healing", animations, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"sounds\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						if (rageSounds.Count > 0)
						{
							ProfileSound sound = rageSounds[0];
							if (rageSounds.Count > 2)
							{
								sound = rageSounds[2];
							}
							sound.InsertSection("start", ref globalLine, ref index, kv);
						}

						if (goHeal)
						{
							InsertKeyValue(ref globalLine, ref index, "");
							healSound.InsertSection("healing", ref globalLine, ref index, kv);
						}

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "");

						for (int i = 0; i < globalLine.Count; i++)
						{
							if (globalLine[i].Contains("\"heal_timer_animation\"") || globalLine[i].Contains("\"heal_timer\"")
								|| globalLine[i].Contains("\"heal_range_min\"") || globalLine[i].Contains("\"heal_range_max\"")
								|| globalLine[i].Contains("\"heal_time_min\"") || globalLine[i].Contains("\"heal_time_max\"")
								|| globalLine[i].Contains("\"self_heal_enabled\"") || globalLine[i].Contains("\"health_percentage_to_heal\"")
								|| globalLine[i].Contains("\"heal_percentage_one\"") || globalLine[i].Contains("\"heal_percentage_two\"")
								|| globalLine[i].Contains("\"heal_percentage_three\"") || globalLine[i].Contains("\"cloak_to_heal\""))
							{
								globalLine.RemoveAt(i);
								i--;
							}
						}

						File.WriteAllLines(fileName, globalLine);
						kv.ReadFromFile(fileName);
					}
					else
					{
						// Fuck go back
						kv.GoBack();
					}
				}

				if (kv.GetKeyValue("crawling_enabled", false))
				{
					List<ProfileAnimation> walkAnimations = new List<ProfileAnimation>();
					List<ProfileAnimation> runAnimations = new List<ProfileAnimation>();
					float[] speedMultiplier = new float[(int)Difficulty.Max];
					float[] speed = new float[(int)Difficulty.Max];
					float[] walkSpeed = new float[(int)Difficulty.Max];
					float[] mins = new float[3];
					float[] maxs = new float[3];
					for (int i = 0; i < (int)Difficulty.Max; i++)
					{
						speedMultiplier[i] = 0.5f;
					}
					kv.GetKeyValue("crawl_detect_mins", out mins, mins);
					kv.GetKeyValue("crawl_detect_maxs", out maxs, maxs);
					kv.GetDifficultyValues("crawl_multiplier", out speedMultiplier, speedMultiplier);
					kv.GetDifficultyValues("speed", out speed, speed);
					kv.GetDifficultyValues("walkspeed", out walkSpeed, walkSpeed);
					if (kv.JumpToKey("animations"))
					{
						if (kv.JumpToKey("crawlwalk"))
						{
							StoreAnimationData(ref walkAnimations, kv);

							int bracket = 0, endIndex = 0;
							index = 0;
							index = kv.GetSectionIndex("crawlwalk");
							endIndex = index;
							while (!globalLine[endIndex].Contains('{'))
							{
								endIndex++;
							}
							endIndex++;
							while (bracket >= 0)
							{
								if (globalLine[endIndex].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[endIndex].Contains('{'))
								{
									bracket++;
								}
								endIndex++;
							}

							while (index != endIndex)
							{
								globalLine.RemoveAt(index);
								endIndex--;
							}
							kv.ReadFromFile(globalLine);
							kv.JumpToKey("animations");
						}

						if (kv.JumpToKey("crawlrun"))
						{
							StoreAnimationData(ref runAnimations, kv);

							int bracket = 0, endIndex = 0;
							index = 0;
							index = kv.GetSectionIndex("crawlrun");
							endIndex = index;
							while (!globalLine[endIndex].Contains('{'))
							{
								endIndex++;
							}
							endIndex++;
							while (bracket >= 0)
							{
								if (globalLine[endIndex].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[endIndex].Contains('{'))
								{
									bracket++;
								}
								endIndex++;
							}

							while (index != endIndex)
							{
								globalLine.RemoveAt(index);
								endIndex--;
							}
							kv.ReadFromFile(globalLine);
							kv.JumpToKey("animations");
						}

						kv.GoBack();
					}

					if (kv.JumpToKey("postures"))
					{
						if (kv.GotoFirstSubKey())
						{
							while (kv.GotoNextKey()) ;

							string section = kv.GetSectionName();
							index = kv.GetSectionIndex(section);
							int bracket = 0;
							while (!globalLine[index].Contains('{'))
							{
								index++;
							}
							index++;
							while (bracket >= 0)
							{
								if (globalLine[index].Contains('}'))
								{
									bracket--;
								}
								else if (globalLine[index].Contains('{'))
								{
									bracket++;
								}
								index++;
							}

							InsertKeyValue(ref globalLine, ref index, "");
							InsertKeyValue(ref globalLine, ref index, "\"crawling\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							float defSpeed = speed[1] * speedMultiplier[1];
							float defWalkSpeed = walkSpeed[1] * speedMultiplier[1];

							for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
							{
								if (walkSpeed[i2] != defWalkSpeed)
								{
									defWalkSpeed = walkSpeed[i2] * speedMultiplier[i2];
									InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("walkspeed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defWalkSpeed) + "\"");
								}
							}

							for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
							{
								if (speed[i2] != defSpeed)
								{
									defSpeed = speed[i2] * speedMultiplier[i2];
									InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("speed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defSpeed) + "\"");
								}
							}
							InsertKeyValue(ref globalLine, ref index, "");

							InsertKeyValue(ref globalLine, ref index, "\"conditions\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertKeyValue(ref globalLine, ref index, "\"within_bounds\"");
							InsertKeyValue(ref globalLine, ref index, "{");

							string insert = string.Empty;
							for (int i3 = 0; i3 < mins.Length; i3++)
							{
								insert += mins.ToString();
								if (i3 != mins.Length - 1)
								{
									insert += " ";
								}
							}
							InsertKeyValue(ref globalLine, ref index, "\"mins\" \"" + insert + "\"");
							insert = string.Empty;
							for (int i3 = 0; i3 < maxs.Length; i3++)
							{
								insert += maxs.ToString();
								if (i3 != maxs.Length - 1)
								{
									insert += " ";
								}
							}
							InsertKeyValue(ref globalLine, ref index, "\"maxs\" \"" + insert + "\"");

							InsertKeyValue(ref globalLine, ref index, "}");

							InsertKeyValue(ref globalLine, ref index, "}");

							InsertKeyValue(ref globalLine, ref index, "");
							InsertKeyValue(ref globalLine, ref index, "\"animations\"");
							InsertKeyValue(ref globalLine, ref index, "{");
							InsertAnimationSection(ref globalLine, ref index, "walk", walkAnimations, kv);

							InsertKeyValue(ref globalLine, ref index, "");
							InsertAnimationSection(ref globalLine, ref index, "run", runAnimations, kv);
							InsertKeyValue(ref globalLine, ref index, "}");

							InsertKeyValue(ref globalLine, ref index, "}");

						}
						kv.GoBack();
					}
					else
					{
						index = kv.GetSectionIndex("animations");
						InsertKeyValue(ref globalLine, ref index, "\"postures\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						InsertKeyValue(ref globalLine, ref index, "\"crawling\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						float defSpeed = speed[1] * speedMultiplier[1];
						float defWalkSpeed = walkSpeed[1] * speedMultiplier[1];

						for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
						{
							if (walkSpeed[i2] != defWalkSpeed)
							{
								defWalkSpeed = walkSpeed[i2] * speedMultiplier[i2];
								InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("walkspeed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defWalkSpeed) + "\"");
							}
						}

						for (int i2 = 1; i2 < (int)Difficulty.Max; i2++)
						{
							if (speed[i2] != defSpeed)
							{
								defSpeed = speed[i2] * speedMultiplier[i2];
								InsertKeyValue(ref globalLine, ref index, "\"" + kv.GetProfileKeyWithDifficultySuffix("speed", (Difficulty)i2) + "\" \"" + kv.FormatFloat(defSpeed) + "\"");
							}
						}
						InsertKeyValue(ref globalLine, ref index, "");

						InsertKeyValue(ref globalLine, ref index, "\"conditions\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertKeyValue(ref globalLine, ref index, "\"within_bounds\"");
						InsertKeyValue(ref globalLine, ref index, "{");

						string insert = string.Empty;
						for (int i3 = 0; i3 < mins.Length; i3++)
						{
							insert += mins[i3].ToString();
							if (i3 != mins.Length - 1)
							{
								insert += " ";
							}
						}
						InsertKeyValue(ref globalLine, ref index, "\"mins\" \"" + insert + "\"");
						insert = string.Empty;
						for (int i3 = 0; i3 < maxs.Length; i3++)
						{
							insert += maxs[i3].ToString();
							if (i3 != maxs.Length - 1)
							{
								insert += " ";
							}
						}
						InsertKeyValue(ref globalLine, ref index, "\"maxs\" \"" + insert + "\"");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "");
						InsertKeyValue(ref globalLine, ref index, "\"animations\"");
						InsertKeyValue(ref globalLine, ref index, "{");
						InsertAnimationSection(ref globalLine, ref index, "walk", walkAnimations, kv);

						InsertKeyValue(ref globalLine, ref index, "");
						InsertAnimationSection(ref globalLine, ref index, "run", runAnimations, kv);
						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");

						InsertKeyValue(ref globalLine, ref index, "}");
						InsertKeyValue(ref globalLine, ref index, "");
					}

					for (int i = 0; i < globalLine.Count; i++)
					{
						if (globalLine[i].Contains("\"crawling_enabled\"") || globalLine[i].Contains("\"crawl_multiplier")
							|| globalLine[i].Contains("\"crawl_detect_mins\"") || globalLine[i].Contains("\"crawl_detect_maxs\""))
						{
							globalLine.RemoveAt(i);
							i--;
						}
					}

					File.WriteAllLines(fileName, globalLine);
					kv.ReadFromFile(fileName);
				}

				// Delete empty globalLines having increments greater than 1
				for (int i = 0; i < globalLine.Count - 1; i++)
				{
					if (!globalLine[i + 1].Contains('\"') && !globalLine[i + 1].Contains('{') && !globalLine[i + 1].Contains('}') &&
						!globalLine[i].Contains('\"') && !globalLine[i].Contains('{') && !globalLine[i].Contains('}'))
					{
						globalLine.RemoveAt(i + 1);
						i--;
					}
				}

				// Finally auto-indent
				int curlyIndex = 0;
				StringBuilder sb, charBuilder;
				for (int i = 0; i < globalLine.Count; i++)
				{
					bool commentFound = false;
					charBuilder = new StringBuilder();
					globalLine[i] = globalLine[i].Replace("\t", "");
					List<char> charList = new List<char>();
					charList.AddRange(globalLine[i]);
					for (int i2 = 0; i2 < charList.Count; i2++)
					{
						bool skipSpace = false;
						if (charList[i2] == ' ' && charList[i2] != '{' && charList[i2] != '}')
						{
							if ((i2 - 1 > 0 && (charList[i2 - 1] != ' ')) &&
								(i2 + 1 < charList.Count && (charList[i2 + 1] != ' ')))
							{
								skipSpace = true;
							}
							if (charList[i2] == '/' && (i2 + 1 < charList.Count && charList[i2 + 1] == '/'))
							{
								commentFound = true;
							}
							if (!commentFound && !skipSpace)
							{
								charList.RemoveAt(i2);
								i2--;
								continue;
							}
						}
						charBuilder.Append(charList[i2]);
					}
					globalLine[i] = charBuilder.ToString();
					charBuilder = new StringBuilder();
					charList = new List<char>();
					charList.AddRange(globalLine[i]);
					for (int i2 = 0; i2 < charList.Count; i2++)
					{
						if (charList[i2] == '\"' && (i2 + 1 < charList.Count && charList[i2 + 1] == '\"'))
						{
							charList.Insert(i2 + 1, ' ');
						}
						charBuilder.Append(charList[i2]);
					}
					globalLine[i] = charBuilder.ToString();
				}
				for (int i = 0; i < globalLine.Count; i++)
				{
					if (globalLine[i].Contains('}') && curlyIndex != 0 && !globalLine[i].Contains('\"'))
					{
						curlyIndex--;
					}
					sb = new StringBuilder(globalLine[i]);
					if (globalLine[i].Length > 0)
					{
						for (int i2 = 0; i2 < curlyIndex; i2++)
						{
							sb.Insert(0, "\t");
						}
						globalLine[i] = sb.ToString();
					}
					if (globalLine[i].Contains('{') && !globalLine[i].Contains('\"'))
					{
						curlyIndex++;
					}
				}

				for (int i = 0; i < globalLine.Count; i++)
				{
					char[] arr = globalLine[i].ToCharArray();
					if (arr.Length > 0 || globalLine[i].Contains('{') || globalLine[i].Contains('}'))
					{
						continue;
					}
					globalLine[i] = string.Empty;
				}

				// Check any missing curly brackets
				curlyIndex = 0;
				for (int i = 0; i < globalLine.Count; i++)
				{
					bool doContinue = false;
					if (globalLine[i].Contains('{') || globalLine[i].Contains('}'))
					{
						List<char> charList = new List<char>();
						charList.AddRange(globalLine[i]);
						for (int i2 = 0; i2 < charList.Count; i2++)
						{
							if (char.IsLetterOrDigit(charList[i2]))
							{
								doContinue = true;
								break;
							}
						}
					}
					if (doContinue)
					{
						continue;
					}

					if (globalLine[i].Contains('{'))
					{
						curlyIndex++;
					}
					if (globalLine[i].Contains('}'))
					{
						curlyIndex--;
					}
				}
				if (curlyIndex > 0)
				{
					for (int i = 0; i < curlyIndex; i++)
					{
						globalLine.Add("}");
					}
				}
				else if (curlyIndex < 0)
				{
					for (int i = globalLine.Count - 1; i >= 0; i--)
					{
						if (curlyIndex == 0)
						{
							break;
						}
						bool doContinue = false;
						if (globalLine[i].Contains('}'))
						{
							List<char> charList = new List<char>();
							charList.AddRange(globalLine[i]);
							for (int i2 = 0; i2 < charList.Count; i2++)
							{
								if (char.IsLetterOrDigit(charList[i2]))
								{
									doContinue = true;
									break;
								}
							}
						}
						if (doContinue)
						{
							continue;
						}

						if (globalLine[i].Contains('}'))
						{
							globalLine.RemoveAt(i);
							curlyIndex++;
						}
					}
				}
				File.WriteAllLines(fileName, globalLine);
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			if (success)
			{
				progressBox.Text = "Finished rewriting " + configsList.Items.Count + " config(s)!";
			}
		}

		private void clearButton_Click(object sender, EventArgs e)
		{
			configsList.Items.Clear();
		}
	}
}