using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using static Stocks;

namespace SF2MConfigRewrite
{
    class Program
    {
        // Variables
        static int filesFound = 0;
        public static List<string>? globalLine = null;
        /*static string[] validConfigChars = { "a", "A", "b", "B", "c", "C", "d", "D", "e", "E", "f", "F", "g", "G",
        "h", "H", "i", "I", "j", "J", "k", "K", "l", "L", "m", "M", "n", "N", "o", "O", "p", "P", "q", "Q", "r", "R",
        "s", "S", "t", "T", "u", "U", "v", "V", "w", "W", "x", "X", "y", "Y", "z", "Z" };
        static string[] validConfigNumbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };*/

        static void Main(string[] args)
        {
            // Actual program functions
            Console.Title = "Slender Fortress Modified Config Rewriter";
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine("Input a directory where all config files are located.");
            var directory = Console.ReadLine();
            while (!Directory.Exists(directory))
            {
                Console.WriteLine("Invalid directory " + directory + ", input a new directory.");
                directory = Console.ReadLine();
            }
            Console.WriteLine("Directory is valid, back up your config files just in case, press any key to start the rewriting process");
            Console.ReadKey(true);
            Console.WriteLine("Rewriting configs...");
            Stopwatch sw = Stopwatch.StartNew();
            ProcessDirectory(directory);
            TimeSpan ts = sw.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("Rewrote " + filesFound + " config files in " + elapsedTime);
            Console.ReadKey(true);
        }

        // Functions
        static void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                string extension = Path.GetExtension(fileName);
                if (extension == ".cfg")
                {
                    if (fileName.Contains("profiles.cfg")) // Split this guy up
                    {
                        //FixConfig(fileName);
                        SplitConfig(fileName, targetDirectory);
                    }
                    else if (fileName.Contains("profiles_packs.cfg")) // Search in this
                    {
                        SearchPacksConfig(fileName, targetDirectory);
                    }
                    else
                    {
                        if (!fileName.Contains("class_stats.cfg") && !fileName.Contains("restrictedweapons.cfg") && !fileName.Contains("specialrounds.cfg"))
                        {
                            RewriteConfig(fileName);
                        }
                    }
                }
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory);
            }
        }

        static void SearchPacksConfig(string fileName, string targetDirectory)
        {
            KeyValues kv = new KeyValues();
            globalLine = File.ReadAllLines(fileName).ToList<string>();
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"file\"") && !globalLine[i].Contains("//"))
                {
                    string bossPack = kv.GetString("file", i, fileName);
                    if (bossPack.Contains(".cfg")) // We got our boss pack
                    {
                        string bossDirectory = bossPack.Replace(".cfg", "/");
                        string newFile = fileName.Replace("profiles_packs.cfg", "/profiles/packs/" + bossDirectory);
                        if (!Directory.Exists(newFile))
                        {
                            Directory.CreateDirectory(newFile);
                        }
                        string tempFile = fileName.Replace("profiles_packs.cfg", "/profiles/packs/");
                        string bossFile = tempFile + bossPack;
                        SplitConfig(bossFile, targetDirectory, "/profiles/packs/" + bossDirectory + "/", true);
                        globalLine[i] = globalLine[i].Replace(".cfg\"", "\"");
                        File.WriteAllLines(fileName, globalLine);
                    }
                }
            }
        }

        // UNFINISHED
        static void FixConfig(string fileName)
        {
            List<string> globalLine = File.ReadAllLines(fileName).ToList<string>();
            for (int i = 0; i < globalLine.Count; i++) // First find missing quotation marks
            {
                char quotes = '\"';

                int expectedCount = globalLine[i].Split(quotes).Length - 1;
                if (expectedCount != 0 && expectedCount != 2 && expectedCount != 4)
                {
                    char[] arr = globalLine[i].ToCharArray();
                    StringBuilder builder = new StringBuilder();
                    for (int character = 1; character < arr.Length; character++)
                    {
                        if (char.IsLetterOrDigit(arr[character]) && char.IsLetterOrDigit(arr[character-1]))
                        {
                            builder.Append(arr[character]);
                        }
                    }
                    globalLine[i] = builder.ToString();
                }
            }
        }

        static void SplitConfig(string fileName, string targetDirectory, string newDirectory = "/profiles/", bool deleteFile = false)
        {
            if (!File.Exists(fileName))
            {
                return;
            }
            List<string> globalLine = File.ReadAllLines(fileName).ToList<string>();
            List<string> brackets = new List<string>();
            for (int i = 2; i < globalLine.Count; i++)
            {
                // First look for the profile name
                if (globalLine[i].Contains('\"') && (globalLine[i + 1].Contains('{') || globalLine[i + 2].Contains('{')))
                {
                    int startPos = 0;
                    if (globalLine[i + 1].Contains('{'))
                    {
                        startPos = i + 2;
                    }
                    else if (globalLine[i + 2].Contains('{'))
                    {
                        startPos = i + 3;
                    }
                    string profile = globalLine[i];
                    int profileEndPos = 0, continueTo = 0;
                    for (int i2 = startPos; i2 < globalLine.Count; i2++)
                    {
                        if (globalLine[i2].Contains('{')) // Found a section that is NOT the end of the profile
                        {
                            brackets.Add(globalLine[i2]);
                        }
                        if (globalLine[i2].Contains('}') && brackets.Count > 0)
                        {
                            brackets.RemoveAt(0);
                        }
                        else if (globalLine[i2].Contains('}') && brackets.Count <= 0) // We found our profile
                        {
                            profileEndPos = i2 - i;
                            continueTo = i2;
                            break;
                        }
                    }
                    char[] arr = profile.ToCharArray();
                    string profileFile;
                    StringBuilder builder = new StringBuilder();
                    for (int character = 0; character < arr.Length; character++)
                    {
                        if (char.IsLetterOrDigit(arr[character]))
                        {
                            builder.Append(arr[character]);
                        }
                    }
                    profileFile = builder.ToString();
                    string newFile = targetDirectory + newDirectory + profileFile + ".cfg";
                    List<string> newLines = new List<string>();
                    for (int i2 = i; i2 <= continueTo; i2++)
                    {
                        newLines.Add(globalLine[i2]);
                    }
                    File.WriteAllLines(newFile, newLines);
                    i = continueTo;
                    brackets.Clear();
                }
            }
            List<string> overwrittenFile = new List<string>();
            overwrittenFile.Add("\"Profiles\"");
            overwrittenFile.Add("{");
            overwrittenFile.Add("}");
            if (!deleteFile)
            {
                File.WriteAllLines(fileName, overwrittenFile);
            }
            else
            {
                File.Delete(fileName);
            }
        }

        static void RewriteConfig(string fileName)
        {
            Console.WriteLine("Rewriting " + fileName);
            globalLine = File.ReadAllLines(fileName).ToList<string>();
            KeyValues kv = new KeyValues();
            kv.fileName = fileName;
            string chaseInitialDuration = "0.0";
            string spawnAnimationTimer = "0.0";
            string stunDuration = "3.5";
            string rageTimer = "0.0";
            string fleeDelayTimer = "0.0";

            // Delete any unused key values
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"jump_speed\"") || globalLine[i].Contains("\"airspeed\"") || globalLine[i].Contains("\"jump_cooldown\"") ||
                    globalLine[i].Contains("\"random_attacks\"") || globalLine[i].Contains("\"enable_boss_tilting\"") || globalLine[i].Contains("\"think_time_min\"")
                    || globalLine[i].Contains("\"think_time_max\"") || globalLine[i].Contains("\"anger_start\"") || globalLine[i].Contains("\"anger_page_time_diff\"") || globalLine[i].Contains("\"anger_page_add\"") || globalLine[i].Contains("\"appear_chance_threshold\"") || globalLine[i].Contains("\"appear_chance_min\"")
                     || globalLine[i].Contains("\"appear_chance_max\"") || globalLine[i].Contains("\"proxies_teleport_enabled\"")
                     || globalLine[i].Contains("\"attack_props\"") || globalLine[i].Contains("\"attack_damageforce\"") || globalLine[i].Contains("\"attack_damage_vs_props\"") || globalLine[i].Contains("\"use_engine_sounds\"") || globalLine[i].Contains("\"difficulty_affects_animations\"")
                     || globalLine[i].Contains("\"multi_miss_sounds\"") || globalLine[i].Contains("\"multi_hit_sounds\"") || globalLine[i].Contains("\"multi_attack_sounds\""))
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
            File.WriteAllLines(fileName, globalLine);

            globalLine = File.ReadAllLines(fileName).ToList<string>();
            // Add [PLAYER] in chat_message_upon_death
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"chat_message_upon_death\""))
                {
                    int bracketCheck = i + 1;
                    StringBuilder builder = new StringBuilder();
                    char[] arr;
                    while (!globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    bracketCheck++;
                    while (globalLine[bracketCheck].Contains("\""))
                    {
                        arr = globalLine[bracketCheck].ToCharArray();
                        builder.Clear();
                        byte quoteCheck = 0;
                        for (int i2 = 0; i2 < arr.Length; i2++)
                        {
                            if (arr[i2] == '\"')
                            {
                                quoteCheck++;
                            }
                            if (quoteCheck < 3)
                            {
                                continue;
                            }
                            if (char.IsLetterOrDigit(arr[i2]) || IsCharSymbol(arr[i2]) || arr[i2] == ' ')
                            {
                                builder.Append(arr[i2]);
                            }
                        }
                        if (builder.Length > 0)
                        {
                            string originalString = builder.ToString();
                            string result = builder.ToString();
                            if (!result.Contains("[PLAYER]"))
                            {
                                arr = result.ToCharArray();
                                if (arr[0] != ' ')
                                {
                                    result = result.Insert(0, "[PLAYER] ");
                                }
                                else
                                {
                                    result = result.Insert(0, "[PLAYER]");
                                }
                            }
                            else
                            {
                                bracketCheck++;
                                continue;
                            }
                            globalLine[bracketCheck] = globalLine[bracketCheck].Replace(originalString, result);
                        }
                        bracketCheck++;
                    }
                    break;
                }
            }

            // Add "attacks" section if needed
            bool foundAttacks = false;
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"attacks\""))
                {
                    // Don't do anything
                    foundAttacks = true;
                }
            }
            if (!foundAttacks)
            {
                List<string> attackKeys = new List<string>();
                int firstIndex = 0;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"attack_") && !globalLine[i].Contains("//") && !globalLine[i].Contains("\"attack_props\"") && !globalLine[i].Contains("\"animation_attack") && !globalLine[i].Contains("\"name\""))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        attackKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                }
                if (firstIndex != 0 && attackKeys.Count > 0)
                {
                    while (!globalLine[firstIndex].Contains('\"') && !globalLine[firstIndex].Contains('{') && !globalLine[firstIndex].Contains('}'))
                    {
                        firstIndex--;
                    }
                    globalLine.Insert(firstIndex, "\t\t\"attacks\"");
                    firstIndex++;
                    globalLine.Insert(firstIndex, "\t\t{");
                    firstIndex++;
                    globalLine.Insert(firstIndex, "\t\t\t\"1\"");
                    firstIndex++;
                    globalLine.Insert(firstIndex, "\t\t\t{");
                    firstIndex++;
                    string newKey = string.Empty;
                    for (int i = 0; i < attackKeys.Count; i++)
                    {
                        newKey = "\t\t" + attackKeys[i];
                        globalLine.Insert(firstIndex, newKey);
                        firstIndex++;
                    }
                    globalLine.Insert(firstIndex, "\t\t\t}");
                    firstIndex++;
                    if (!globalLine[firstIndex].Contains('}'))
                    {
                        globalLine.Insert(firstIndex, "\t\t}");
                    }
                    
                    firstIndex++;
                    if (globalLine[firstIndex].Contains(string.Empty))
                    {
                        globalLine.Insert(firstIndex, string.Empty);
                    }
                }
            }
            File.WriteAllLines(fileName, globalLine);

            // Replace attack_ with nothing
            string text = File.ReadAllText(fileName);
            text = text.Replace("\"attack_while_running\"", "\"run_enabled\"");
            File.WriteAllText(fileName, text);

            globalLine = File.ReadAllLines(fileName).ToList<string>();
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"attack_") && !globalLine[i].Contains("\"animation_attack") && 
                    !globalLine[i].Contains("\"attack_weaponsenable\"") && !globalLine[i].Contains("\"attack_custom_deathflag") && !globalLine[i].Contains("\"name\"") &&
                    !globalLine[i].Contains("\"animation_idle") && !globalLine[i].Contains("\"animation_walk") && !globalLine[i].Contains("\"animation_walkalert") &&
                    !globalLine[i].Contains("\"animation_run") && !globalLine[i].Contains("\"animation_stun") && !globalLine[i].Contains("\"animation_shoot") &&
                    !globalLine[i].Contains("\"animation_chaseinitial") && !globalLine[i].Contains("\"animation_heal") && !globalLine[i].Contains("\"animation_crawlwalk") &&
                    !globalLine[i].Contains("\"animation_crawlrun") && !globalLine[i].Contains("\"animation_spawn") && !globalLine[i].Contains("\"animation_jump") &&
                    !globalLine[i].Contains("\"animation_duck") && !globalLine[i].Contains("\"animation_rage") && !globalLine[i].Contains("\"animation_fleestart") &&
                    !globalLine[i].Contains("\"animation_death") && !globalLine[i].Contains("\"name\""))
                {
                    globalLine[i] = globalLine[i].Replace("\"attack_", "\"");
                }
            }
            File.WriteAllLines(fileName, globalLine);

            globalLine = File.ReadAllLines(fileName).ToList<string>();
            bool foundAnimations = false;
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"animations\""))
                {
                    // Don't do anything
                    foundAnimations = true;
                }
            }
            if (!foundAnimations)
            {
                List<string> idleKeys = new List<string>();
                List<string> walkKeys = new List<string>();
                List<string> walkAlertKeys = new List<string>();
                List<string> runKeys = new List<string>();
                List<string> attackKeys = new List<string>();
                List<string> stunKeys = new List<string>();
                List<string> shootKeys = new List<string>();
                List<string> chaseInitialKeys = new List<string>();
                List<string> rageKeys = new List<string>();
                List<string> fleeKeys = new List<string>();
                List<string> healKeys = new List<string>();
                List<string> deathCamKeys = new List<string>();
                List<string> spawnKeys = new List<string>();
                List<string> crawlWalkKeys = new List<string>();
                List<string> crawlRunKeys = new List<string>();
                List<string> jumpKeys = new List<string>();
                List<string> deathKeys = new List<string>();
                List<string> duckKeys = new List<string>();

                int firstIndex = 0;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"animation_idle") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        idleKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_walk") && !globalLine[i].Contains("//") && !globalLine[i].Contains("\"animation_walkalert"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        walkKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_walkalert") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        walkAlertKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_run") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        runKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_attack") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        attackKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_shoot") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        shootKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_stun") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        stunKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_chaseinitial") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        chaseInitialKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_rage") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        rageKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_fleestart") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        fleeKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_heal") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        healKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_deathcam") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        deathCamKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_spawn") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        spawnKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_crawlwalk") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        crawlWalkKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_crawlrun") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        crawlRunKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_death") && !globalLine[i].Contains("//") && !globalLine[i].Contains("\"animation_deathcam"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        deathKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_jump") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        jumpKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_duck") && !globalLine[i].Contains("//"))
                    {
                        if (firstIndex == 0)
                        {
                            firstIndex = i;
                        }
                        duckKeys.Add(globalLine[i]);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                }
                if (firstIndex != 0)
                {
                    int bracketCheck = 0;
                    while (globalLine[firstIndex].Contains('\"') || globalLine[firstIndex].Contains('{') || globalLine[firstIndex].Contains('}') || bracketCheck > 0)
                    {
                        if (globalLine[firstIndex].Contains('{'))
                        {
                            bracketCheck--;
                        }
                        if (globalLine[firstIndex].Contains('}'))
                        {
                            bracketCheck++;
                        }
                        firstIndex--;
                    }
                    globalLine.Insert(firstIndex, "\t\t\"animations\"");
                    firstIndex++;
                    globalLine.Insert(firstIndex, "\t\t{");
                    firstIndex++;
                    if (idleKeys.Count > 0)
                    {
                        WriteAnimationSection(idleKeys, "idle", globalLine, firstIndex, out firstIndex);
                    }
                    if (walkKeys.Count > 0)
                    {
                        WriteAnimationSection(walkKeys, "walk", globalLine, firstIndex, out firstIndex);
                    }
                    if (walkAlertKeys.Count > 0)
                    {
                        WriteAnimationSection(walkAlertKeys, "walkalert", globalLine, firstIndex, out firstIndex);
                    }
                    if (runKeys.Count > 0)
                    {
                        WriteAnimationSection(runKeys, "run", globalLine, firstIndex, out firstIndex);
                    }
                    if (attackKeys.Count > 0)
                    {
                        WriteAnimationSection(attackKeys, "attack", globalLine, firstIndex, out firstIndex);
                    }
                    if (shootKeys.Count > 0)
                    {
                        WriteAnimationSection(shootKeys, "shoot", globalLine, firstIndex, out firstIndex);
                    }
                    if (stunKeys.Count > 0)
                    {
                        WriteAnimationSection(stunKeys, "stun", globalLine, firstIndex, out firstIndex);
                    }
                    if (chaseInitialKeys.Count > 0)
                    {
                        WriteAnimationSection(chaseInitialKeys, "chaseinitial", globalLine, firstIndex, out firstIndex);
                    }
                    if (rageKeys.Count > 0)
                    {
                        WriteAnimationSection(rageKeys, "rage", globalLine, firstIndex, out firstIndex);
                    }
                    if (fleeKeys.Count > 0)
                    {
                        WriteAnimationSection(fleeKeys, "fleestart", globalLine, firstIndex, out firstIndex);
                    }
                    if (healKeys.Count > 0)
                    {
                        WriteAnimationSection(healKeys, "heal", globalLine, firstIndex, out firstIndex);
                    }
                    if (deathCamKeys.Count > 0)
                    {
                        WriteAnimationSection(deathCamKeys, "deathcam", globalLine, firstIndex, out firstIndex);
                    }
                    if (spawnKeys.Count > 0)
                    {
                        WriteAnimationSection(spawnKeys, "spawn", globalLine, firstIndex, out firstIndex);
                    }
                    if (crawlWalkKeys.Count > 0)
                    {
                        WriteAnimationSection(crawlWalkKeys, "crawlwalk", globalLine, firstIndex, out firstIndex);
                    }
                    if (crawlRunKeys.Count > 0)
                    {
                        WriteAnimationSection(crawlRunKeys, "crawlrun", globalLine, firstIndex, out firstIndex);
                    }
                    if (deathKeys.Count > 0)
                    {
                        WriteAnimationSection(deathKeys, "death", globalLine, firstIndex, out firstIndex);
                    }
                    if (jumpKeys.Count > 0)
                    {
                        WriteAnimationSection(jumpKeys, "jump", globalLine, firstIndex, out firstIndex);
                    }
                    if (duckKeys.Count > 0)
                    {
                        WriteAnimationSection(duckKeys, "duck", globalLine, firstIndex, out firstIndex);
                    }

                    if (!globalLine[firstIndex].Contains('}'))
                    {
                        globalLine.Insert(firstIndex, "\t\t}");
                    }

                    firstIndex++;
                    if (globalLine[firstIndex].Contains(string.Empty))
                    {
                        globalLine.Insert(firstIndex, string.Empty);
                    }
                }
            }
            if (foundAnimations)
            {
                string idleFootstepInterval = "0.0";
                string walkFootstepInterval = "0.0";
                string runFootstepInterval = "0.0";
                string attackFootstepInterval = "0.0";
                string stunFootstepInterval = "0.0";

                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"animation_idle_footstepinterval\""))
                    {
                        idleFootstepInterval = kv.GetFloat("animation_idle_footstepinterval", i);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_walk_footstepinterval\""))
                    {
                        walkFootstepInterval = kv.GetFloat("animation_walk_footstepinterval", i);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_run_footstepinterval\""))
                    {
                        runFootstepInterval = kv.GetFloat("animation_run_footstepinterval", i);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_attack_footstepinterval\""))
                    {
                        attackFootstepInterval = kv.GetFloat("animation_attack_footstepinterval", i);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                    if (globalLine[i].Contains("\"animation_stun_footstepinterval\""))
                    {
                        stunFootstepInterval = kv.GetFloat("animation_stun_footstepinterval", i);
                        globalLine.RemoveAt(i);
                        i--;
                    }
                }
                AddFootstepIntervals(globalLine, idleFootstepInterval, "idle");

                AddFootstepIntervals(globalLine, walkFootstepInterval, "walk");

                AddFootstepIntervals(globalLine, walkFootstepInterval, "walkalert");

                AddFootstepIntervals(globalLine, walkFootstepInterval, "crawlwalk");

                AddFootstepIntervals(globalLine, runFootstepInterval, "run");

                AddFootstepIntervals(globalLine, runFootstepInterval, "crawlrun");

                AddFootstepIntervals(globalLine, attackFootstepInterval, "attack");

                AddFootstepIntervals(globalLine, stunFootstepInterval, "stun");
            }

            // Delete any unneeded key values
            List<string> associatedKeys = new List<string>();
            associatedKeys.Add("\"proxies");
            associatedKeys.Add("\"proxy_difficulty_");
            RemoveUnnecessaryKeys(globalLine, "proxies", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"stun_");
            associatedKeys.Add("\"disappear_on_stun\"");
            associatedKeys.Add("\"animation_stun\"");
            associatedKeys.Add("\"animation_stun_playbackrate\"");
            associatedKeys.Add("\"animation_stun_footstepinterval\"");
            associatedKeys.Add("\"animation_stun_cycle\"");
            RemoveUnnecessaryKeys(globalLine, "stun_enabled", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_random");
            associatedKeys.Add("\"player_jarate");
            associatedKeys.Add("\"player_milk");
            associatedKeys.Add("\"player_gas");
            associatedKeys.Add("\"player_mark");
            associatedKeys.Add("\"player_silent_mark");
            associatedKeys.Add("\"player_ignite");
            associatedKeys.Add("\"player_stun");
            associatedKeys.Add("\"player_bleed");
            associatedKeys.Add("\"player_electric");
            associatedKeys.Add("\"player_smite");
            RemoveUnnecessaryKeys(globalLine, "player_damage_effects", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_damage_random_effects");
            RemoveUnnecessaryKeys(globalLine, "player_damage_random_effects", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_jarate");
            RemoveUnnecessaryKeys(globalLine, "player_jarate_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_milk");
            RemoveUnnecessaryKeys(globalLine, "player_milk_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_gas");
            RemoveUnnecessaryKeys(globalLine, "player_gas_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_mark");
            RemoveUnnecessaryKeys(globalLine, "player_mark_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_silent_mark");
            RemoveUnnecessaryKeys(globalLine, "player_silent_mark_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_ignite");
            RemoveUnnecessaryKeys(globalLine, "player_ignite_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_stun");
            RemoveUnnecessaryKeys(globalLine, "player_stun_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_bleed");
            RemoveUnnecessaryKeys(globalLine, "player_bleed_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_electric");
            RemoveUnnecessaryKeys(globalLine, "player_electric_slow_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"player_smite");
            RemoveUnnecessaryKeys(globalLine, "player_smite_on_hit", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"shockwave_");
            RemoveUnnecessaryKeys(globalLine, "shockwave", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"traps_enabled\"");
            associatedKeys.Add("\"trap_");
            RemoveUnnecessaryKeys(globalLine, "traps_enabled", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"copy\"");
            associatedKeys.Add("\"copy_");
            associatedKeys.Add("\"teleport_distance_between_copies\"");
            RemoveUnnecessaryKeys(globalLine, "copy", associatedKeys, kv);

            associatedKeys.Clear();
            associatedKeys.Add("\"auto_chase_enabled\"");
            associatedKeys.Add("\"sound_alert_");
            RemoveUnnecessaryKeys(globalLine, "auto_chase_enabled", associatedKeys, kv);

            // Look for any timers
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"chase_initial_timer\""))
                {
                    chaseInitialDuration = kv.GetFloat("chase_initial_timer", i);
                    globalLine.RemoveAt(i);
                    i--;
                }
                if (globalLine[i].Contains("\"spawn_timer\""))
                {
                    spawnAnimationTimer = kv.GetFloat("\"spawn_timer\"", i);
                    globalLine.RemoveAt(i);
                    i--;
                }
                if (globalLine[i].Contains("\"stun_duration\""))
                {
                    stunDuration = kv.GetFloat("\"stun_duration\"", i);
                    globalLine.RemoveAt(i);
                    i--;
                }
                if (globalLine[i].Contains("\"rage_timer\""))
                {
                    rageTimer = kv.GetFloat("\"rage_timer\"", i);
                    globalLine.RemoveAt(i);
                    i--;
                }
                if (globalLine[i].Contains("\"flee_delay_time\""))
                {
                    fleeDelayTimer = kv.GetFloat("\"flee_delay_time\"", i);
                    globalLine.RemoveAt(i);
                    i--;
                }
            }

            File.WriteAllLines(fileName, globalLine);

            // Start replacing key values
            text = File.ReadAllText(fileName);
            text = text.Replace("\"turnrate\"", "\"maxyawrate\"");

            text = text.Replace("\"ash_ragdoll_on_kill\"", "\"disintegrate_ragdoll_on_kill\"");

            text = text.Replace("\"chance\"", "\"weight\"");

            text = text.Replace("\"auto_chase_count\"", "\"auto_chase_sound_threshold\"");

            text = text.Replace("\"sound_alert_add\"", "\"auto_chase_sound_add\"");

            text = text.Replace("\"sound_alert_add_footsteps\"", "\"auto_chase_sound_add_footsteps\"");

            text = text.Replace("\"sound_alert_add_voice\"", "\"auto_chase_sound_add_voice\"");

            text = text.Replace("\"sound_alert_add_weapon\"", "\"auto_chase_sound_add_weapon\"");

            text = text.Replace("\"chase_upon_look\"", "\"auto_chase_upon_look\"");

            text = text.Replace("\"sound_player_death\"", "\"sound_player_deathcam\"");

            text = text.Replace("\"sound_player_death_all\"", "\"sound_player_deathcam_all\"");
            File.WriteAllText(fileName, text);

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

            text = File.ReadAllText(fileName);
            text = text.Replace("\"gesture_attack\"", "\"gesture_name\"");

            text = text.Replace("\"gesture_attack_playbackrate\"", "\"gesture_playbackrate\"");

            text = text.Replace("\"gesture_attack_cycle\"", "\"gesture_cycle\"");
            File.WriteAllText(fileName, text);

            // "companions" to the new companions system
            globalLine = File.ReadAllLines(fileName).ToList<string>();
            for (int i = 0; i < globalLine.Count; i++)
            {
                if (globalLine[i].Contains("\"chaseinitial\""))
                {
                    int bracketCheck = i + 1;
                    byte bracketIndex = 0;
                    while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    if (bracketCheck >= globalLine.Count)
                    {
                        continue;
                    }
                    bracketIndex++;
                    bracketCheck++;
                    for (int j = bracketCheck; j < globalLine.Count; j++)
                    {
                        if (chaseInitialDuration == "0.0" || chaseInitialDuration == "0")
                        {
                            break;
                        }
                        if (globalLine[j].Contains('}') && bracketIndex == 1)
                        {
                            i = j;
                            break;
                        }
                        if (globalLine[j].Contains("\"1") || globalLine[j].Contains("\"2") || globalLine[j].Contains("\"3") || globalLine[j].Contains("\"4")
                            || globalLine[j].Contains("\"5") || globalLine[j].Contains("\"6") || globalLine[j].Contains("\"7") || globalLine[j].Contains("\"8")
                            || globalLine[j].Contains("\"9"))
                        {
                            j++;
                            while (!globalLine[j].Contains('{'))
                            {
                                j++;
                            }
                            bracketIndex++;
                            j++;
                            while (!globalLine[j].Contains('}'))
                            {
                                j++;
                            }
                            bracketIndex--;
                            if (chaseInitialDuration != "0.0" && chaseInitialDuration != "0")
                            {
                                globalLine.Insert(j, "\"duration\" \"" + chaseInitialDuration + "\"");
                                j++;
                            }
                        }
                    }
                }
                if (globalLine[i].Contains("\"spawn\""))
                {
                    int bracketCheck = i + 1;
                    byte bracketIndex = 0;
                    while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    if (bracketCheck >= globalLine.Count)
                    {
                        continue;
                    }
                    bracketIndex++;
                    bracketCheck++;
                    for (int j = bracketCheck; j < globalLine.Count; j++)
                    {
                        if (spawnAnimationTimer == "0.0" || spawnAnimationTimer == "0")
                        {
                            break;
                        }
                        if (globalLine[j].Contains('}') && bracketIndex == 1)
                        {
                            i = j;
                            break;
                        }
                        if (globalLine[j].Contains("\"1") || globalLine[j].Contains("\"2") || globalLine[j].Contains("\"3") || globalLine[j].Contains("\"4")
                            || globalLine[j].Contains("\"5") || globalLine[j].Contains("\"6") || globalLine[j].Contains("\"7") || globalLine[j].Contains("\"8")
                            || globalLine[j].Contains("\"9"))
                        {
                            j++;
                            while (!globalLine[j].Contains('{'))
                            {
                                j++;
                            }
                            bracketIndex++;
                            j++;
                            while (!globalLine[j].Contains('}'))
                            {
                                j++;
                            }
                            bracketIndex--;
                            if (spawnAnimationTimer != "0.0" && spawnAnimationTimer != "0")
                            {
                                globalLine.Insert(j, "\"duration\" \"" + spawnAnimationTimer + "\"");
                                j++;
                            }
                        }
                    }
                }
                if (globalLine[i].Contains("\"stun\""))
                {
                    int bracketCheck = i + 1;
                    byte bracketIndex = 0;
                    while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    if (bracketCheck >= globalLine.Count)
                    {
                        continue;
                    }
                    bracketIndex++;
                    bracketCheck++;
                    for (int j = bracketCheck; j < globalLine.Count; j++)
                    {
                        if (stunDuration == "0.0" || stunDuration == "0" || stunDuration == "3.5")
                        {
                            break;
                        }
                        if (globalLine[j].Contains('}') && bracketIndex == 1)
                        {
                            i = j;
                            break;
                        }
                        if (globalLine[j].Contains("\"1") || globalLine[j].Contains("\"2") || globalLine[j].Contains("\"3") || globalLine[j].Contains("\"4")
                            || globalLine[j].Contains("\"5") || globalLine[j].Contains("\"6") || globalLine[j].Contains("\"7") || globalLine[j].Contains("\"8")
                            || globalLine[j].Contains("\"9"))
                        {
                            j++;
                            while (!globalLine[j].Contains('{'))
                            {
                                j++;
                            }
                            bracketIndex++;
                            j++;
                            while (!globalLine[j].Contains('}'))
                            {
                                j++;
                            }
                            bracketIndex--;
                            if (stunDuration != "0.0" && stunDuration != "0" && stunDuration != "3.5")
                            {
                                globalLine.Insert(j, "\"duration\" \"" + stunDuration + "\"");
                                j++;
                            }
                        }
                    }
                }
                if (globalLine[i].Contains("\"rage\""))
                {
                    int bracketCheck = i + 1;
                    byte bracketIndex = 0;
                    while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    if (bracketCheck >= globalLine.Count)
                    {
                        continue;
                    }
                    bracketIndex++;
                    bracketCheck++;
                    for (int j = bracketCheck; j < globalLine.Count; j++)
                    {
                        if (rageTimer == "0.0" || rageTimer == "0")
                        {
                            break;
                        }
                        if (globalLine[j].Contains('}') && bracketIndex == 1)
                        {
                            i = j;
                            break;
                        }
                        if (globalLine[j].Contains("\"1") || globalLine[j].Contains("\"2") || globalLine[j].Contains("\"3") || globalLine[j].Contains("\"4")
                            || globalLine[j].Contains("\"5") || globalLine[j].Contains("\"6") || globalLine[j].Contains("\"7") || globalLine[j].Contains("\"8")
                            || globalLine[j].Contains("\"9"))
                        {
                            j++;
                            while (!globalLine[j].Contains('{'))
                            {
                                j++;
                            }
                            bracketIndex++;
                            j++;
                            while (!globalLine[j].Contains('}'))
                            {
                                j++;
                            }
                            bracketIndex--;
                            if (rageTimer != "0.0" && rageTimer != "0")
                            {
                                globalLine.Insert(j, "\"duration\" \"" + rageTimer + "\"");
                                j++;
                            }
                        }
                    }
                }
                if (globalLine[i].Contains("\"fleestart\""))
                {
                    int bracketCheck = i + 1;
                    byte bracketIndex = 0;
                    while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{'))
                    {
                        bracketCheck++;
                    }
                    if (bracketCheck >= globalLine.Count)
                    {
                        continue;
                    }
                    bracketIndex++;
                    bracketCheck++;
                    for (int j = bracketCheck; j < globalLine.Count; j++)
                    {
                        if (fleeDelayTimer == "0.0" || fleeDelayTimer == "0")
                        {
                            break;
                        }
                        if (globalLine[j].Contains('}') && bracketIndex == 1)
                        {
                            i = j;
                            break;
                        }
                        if (globalLine[j].Contains("\"1") || globalLine[j].Contains("\"2") || globalLine[j].Contains("\"3") || globalLine[j].Contains("\"4")
                            || globalLine[j].Contains("\"5") || globalLine[j].Contains("\"6") || globalLine[j].Contains("\"7") || globalLine[j].Contains("\"8")
                            || globalLine[j].Contains("\"9"))
                        {
                            j++;
                            while (!globalLine[j].Contains('{'))
                            {
                                j++;
                            }
                            bracketIndex++;
                            j++;
                            while (!globalLine[j].Contains('}'))
                            {
                                j++;
                            }
                            bracketIndex--;
                            if (fleeDelayTimer != "0.0" && fleeDelayTimer != "0")
                            {
                                globalLine.Insert(j, "\"duration\" \"" + fleeDelayTimer + "\"");
                                j++;
                            }
                        }
                    }
                }
            }
            List<string> keyvalueName = new List<string>();
            for (int i = 0; i < globalLine.Count; i++)
            {
                int iterations = 1;
                int index = 1;
                int startDelete = 0;
                if (globalLine[i].Contains("\"companions\"") && !globalLine[i + 2].Contains("\"type\"") && !globalLine[i].Contains("//"))
                {
                    while (!globalLine[i + iterations].Contains('}'))
                    {
                        if (globalLine[i + iterations].Contains('{'))
                        {
                            startDelete = i + iterations + 1;
                        }
                        if (globalLine[i + iterations].Contains("\"" + index.ToString() + "\""))
                        {
                            keyvalueName.Add(globalLine[i + iterations]);
                            globalLine.RemoveAt(i + iterations);
                            index++;
                            iterations--;
                        }
                        iterations++;
                    }
                    bool spawnType = false;
                    bool groupName = false;
                    bool bossGroupName = false;
                    bool finished = false;
                    while (!finished)
                    {
                        if (!spawnType)
                        {
                            globalLine.Insert(startDelete, "\t\t\t\"type\" \"on_spawn on_difficulty_change\"");
                            startDelete--;
                            spawnType = true;
                        }
                        else
                        {
                            if (!groupName)
                            {
                                startDelete++;
                                globalLine.Insert(startDelete, "\t\t\t\"boss_group\"");
                                startDelete++;
                                globalLine.Insert(startDelete, "\t\t\t{");
                                groupName = true;
                            }
                            else
                            {
                                if (!bossGroupName)
                                {
                                    globalLine.Insert(startDelete, "\t\t\t\t\"bosses\"");
                                    startDelete++;
                                    globalLine.Insert(startDelete, "\t\t\t\t{");
                                    bossGroupName = true;
                                }
                                else
                                {
                                    for (int i2 = 0; i2 < keyvalueName.Count; i2++)
                                    {
                                        keyvalueName[i2] = "\t\t" + keyvalueName[i2];
                                        globalLine.Insert(startDelete, keyvalueName[i2]);
                                        startDelete++;
                                    }
                                    globalLine.Insert(startDelete, "\t\t\t\t}");
                                    startDelete++;
                                    globalLine.Insert(startDelete, "\t\t\t}");
                                    startDelete++;
                                    if (!globalLine[startDelete].Contains("}"))
                                    {
                                        globalLine.Insert(startDelete, "\t\t}");
                                    }
                                    startDelete++;
                                    globalLine.Insert(startDelete, "");
                                    finished = true;
                                }
                            }
                        }

                        startDelete++;
                    }
                    break;
                }
            }

            // Sound sections
            {
                File.WriteAllLines(fileName, globalLine);
                // Sound sections starting with sound_idle
                List<string> floatSoundParams = new List<string>();
                /*
                 * int Channel;
                   float Volume;
                   int Flags;
                   int Level;
                   int Pitch;
                   float CooldownMin;
                   float CooldownMax;
                   int PitchRandomMin;
                   int PitchRandomMax;
                 */
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                List<int> intSoundParams = new List<int>();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_idle", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_alertofenemy", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_chasingenemy", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_chaseenemyinitial", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_attack_killed", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_attack_killed_all");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_attack_killed_client");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_rage", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_rage_2", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_rage_3", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_heal_self", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_stun", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_footsteps", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music_hard");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music_insane");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music_nightmare");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music_apollyon");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_chase_music");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_chase_visible");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_alert_music");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_idle_music");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_20dollars");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_20dollars_music");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_sight", true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_scare_player");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_deathcam_local");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_deathcam");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_death");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_deathcam_all");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_death_all");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_spawn_all");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_spawn_local", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_player_deathcam_overlay");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_proxy_spawn", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_proxy_idle", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_proxy_hurt", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_proxy_death", true, true);

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_music_outro");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_move");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_move_single");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_jumpscare");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_static");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_static_loop_local");

                floatSoundParams.Clear();
                floatSoundParams.Add("1.0"); // Volume
                floatSoundParams.Add("1.5"); // Cooldown Min
                floatSoundParams.Add("1.5"); // Cooldown Max
                intSoundParams.Clear();
                intSoundParams.Add(0); // Channel
                intSoundParams.Add(0); // Flags
                intSoundParams.Add(90); // Level
                intSoundParams.Add(100); // Pitch
                intSoundParams.Add(100); // Pitch Random Min
                intSoundParams.Add(100); // Pitch Random Max
                keyvalueName.Clear();
                ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_static_shake_local");
                File.WriteAllLines(fileName, globalLine);

                globalLine = File.ReadAllLines(fileName).ToList<string>();
                List<int> eventSoundIndexes = new List<int>();
                StringBuilder stringBuilder;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_event_") && !globalLine[i].Contains("volume\"") && !globalLine[i].Contains("pitch\"")
                        && !globalLine[i].Contains("flags\"") && !globalLine[i].Contains("level\""))
                    {
                        stringBuilder = new StringBuilder();
                        char[] arr = globalLine[i].ToCharArray();
                        bool stopLoop = false;
                        for (int i2 = 0; i2 < arr.Length; i2++)
                        {
                            if (char.IsDigit(arr[i2]))
                            {
                                stopLoop = true;
                                stringBuilder.Append(arr[i2]);
                            }
                            if (arr[i2] == '\"' && stopLoop)
                            {
                                break;
                            }
                        }
                        string result = stringBuilder.ToString();
                        eventSoundIndexes.Add(Int32.Parse(result));
                    }
                }
                string keyValue = string.Empty;
                for (int i = 0; i < eventSoundIndexes.Count; i++)
                {
                    keyValue = "sound_event_" + eventSoundIndexes[i].ToString();
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, keyValue);
                    File.WriteAllLines(fileName, globalLine);
                }

                globalLine = File.ReadAllLines(fileName).ToList<string>();
                eventSoundIndexes.Clear();
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_footsteps_event_") && !globalLine[i].Contains("volume\"") && !globalLine[i].Contains("pitch\"")
                        && !globalLine[i].Contains("flags\"") && !globalLine[i].Contains("level\""))
                    {
                        stringBuilder = new StringBuilder();
                        char[] arr = globalLine[i].ToCharArray();
                        bool stopLoop = false;
                        for (int i2 = 0; i2 < arr.Length; i2++)
                        {
                            if (char.IsDigit(arr[i2]))
                            {
                                stopLoop = true;
                                stringBuilder.Append(arr[i2]);
                            }
                            if (arr[i2] == '\"' && stopLoop)
                            {
                                break;
                            }
                        }
                        string result = stringBuilder.ToString();
                        eventSoundIndexes.Add(Int32.Parse(result));
                    }
                }
                for (int i = 0; i < eventSoundIndexes.Count; i++)
                {
                    keyValue = "sound_footsteps_event_" + eventSoundIndexes[i].ToString();
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, keyValue);
                    File.WriteAllLines(fileName, globalLine);
                }

                globalLine = File.ReadAllLines(fileName).ToList<string>();
                bool splitSections = false, rewriteSections = false;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_attackenemy_") && !globalLine[i].Contains("_volume\"") && !globalLine[i].Contains("_pitch\"")
                        && !globalLine[i].Contains("_flags\"") && !globalLine[i].Contains("_level\"") && !globalLine[i].Contains("_cooldown_min\"")
                        && !globalLine[i].Contains("_cooldown_max\"") && !globalLine[i].Contains("_pitch_random_min\"") && !globalLine[i].Contains("_pitch_random_max\"") 
                        && !globalLine[i].Contains("_channel\""))
                    {
                        splitSections = true;
                        break;
                    }
                }
                if (!splitSections)
                {
                    for (int i = 0; i < globalLine.Count; i++)
                    {
                        if (globalLine[i].Contains("\"sound_attackenemy\""))
                        {
                            int bracketCheck = i + 1;
                            bool breakOut = false;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('\"'))
                                {
                                    if (globalLine[bracketCheck].Contains("\"paths\""))
                                    {
                                        breakOut = true;
                                        break;
                                    }
                                }
                            }
                            if (breakOut)
                            {
                                break;
                            }
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{') && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('}'))
                                {
                                    break;
                                }
                            }
                            int originalPosition = bracketCheck;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}') && !globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                bracketCheck++;
                            }
                            if (globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                break;
                            }
                            else
                            {
                                bracketCheck = originalPosition;
                            }
                            if (bracketCheck < globalLine.Count && globalLine[bracketCheck].Contains('{'))
                            {
                                rewriteSections = true;
                            }
                        }
                    }
                }
                if (splitSections && !rewriteSections)
                {
                    ChangeMultiSoundSections("sound_attackenemy", fileName, globalLine, kv, splitSections);
                }
                else if (rewriteSections && !splitSections)
                {
                    RewriteMultiSoundSections("sound_attackenemy", fileName, globalLine, floatSoundParams, intSoundParams, kv);
                }
                else
                {
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_attackenemy");
                    File.WriteAllLines(fileName, globalLine);
                }
                globalLine = File.ReadAllLines(fileName).ToList<string>();
                splitSections = false;
                rewriteSections = false;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_hitenemy_") && !globalLine[i].Contains("_volume\"") && !globalLine[i].Contains("_pitch\"")
                        && !globalLine[i].Contains("_flags\"") && !globalLine[i].Contains("_level\"") && !globalLine[i].Contains("_cooldown_min\"")
                        && !globalLine[i].Contains("_cooldown_max\"") && !globalLine[i].Contains("_pitch_random_min\"") && !globalLine[i].Contains("_pitch_random_max\"")
                        && !globalLine[i].Contains("_channel\""))
                    {
                        splitSections = true;
                        break;
                    }
                }
                if (!splitSections)
                {
                    for (int i = 0; i < globalLine.Count; i++)
                    {
                        if (globalLine[i].Contains("\"sound_hitenemy\""))
                        {
                            int bracketCheck = i + 1;
                            bool breakOut = false;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('\"'))
                                {
                                    if (globalLine[bracketCheck].Contains("\"paths\""))
                                    {
                                        breakOut = true;
                                        break;
                                    }
                                }
                            }
                            if (breakOut)
                            {
                                break;
                            }
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{') && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('}'))
                                {
                                    break;
                                }
                            }
                            int originalPosition = bracketCheck;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}') && !globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                bracketCheck++;
                            }
                            if (globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                break;
                            }
                            else
                            {
                                bracketCheck = originalPosition;
                            }
                            if (bracketCheck < globalLine.Count && globalLine[bracketCheck].Contains('{'))
                            {
                                rewriteSections = true;
                            }
                        }
                    }
                }
                if (splitSections && !rewriteSections)
                {
                    ChangeMultiSoundSections("sound_hitenemy", fileName, globalLine, kv, splitSections);
                }
                else if (rewriteSections && !splitSections)
                {
                    RewriteMultiSoundSections("sound_hitenemy", fileName, globalLine, floatSoundParams, intSoundParams, kv);
                }
                else
                {
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_hitenemy");
                    File.WriteAllLines(fileName, globalLine);
                }
                globalLine = File.ReadAllLines(fileName).ToList<string>();
                splitSections = false;
                rewriteSections = false;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_missenemy_") && !globalLine[i].Contains("_volume\"") && !globalLine[i].Contains("_pitch\"")
                        && !globalLine[i].Contains("_flags\"") && !globalLine[i].Contains("_level\"") && !globalLine[i].Contains("_cooldown_min\"")
                        && !globalLine[i].Contains("_cooldown_max\"") && !globalLine[i].Contains("_pitch_random_min\"") && !globalLine[i].Contains("_pitch_random_max\"")
                        && !globalLine[i].Contains("_channel\""))
                    {
                        splitSections = true;
                        break;
                    }
                }
                if (!splitSections)
                {
                    for (int i = 0; i < globalLine.Count; i++)
                    {
                        if (globalLine[i].Contains("\"sound_missenemy\""))
                        {
                            int bracketCheck = i + 1;
                            bool breakOut = false;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('\"'))
                                {
                                    if (globalLine[bracketCheck].Contains("\"paths\""))
                                    {
                                        breakOut = true;
                                        break;
                                    }
                                }
                            }
                            if (breakOut)
                            {
                                break;
                            }
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{') && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('}'))
                                {
                                    break;
                                }
                            }
                            int originalPosition = bracketCheck;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}') && !globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                bracketCheck++;
                            }
                            if (globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                break;
                            }
                            else
                            {
                                bracketCheck = originalPosition;
                            }
                            if (bracketCheck < globalLine.Count && globalLine[bracketCheck].Contains('{'))
                            {
                                rewriteSections = true;
                            }
                        }
                    }
                }
                if (splitSections && !rewriteSections)
                {
                    ChangeMultiSoundSections("sound_missenemy", fileName, globalLine, kv, splitSections);
                }
                else if (rewriteSections && !splitSections)
                {
                    RewriteMultiSoundSections("sound_missenemy", fileName, globalLine, floatSoundParams, intSoundParams, kv);
                }
                else
                {
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_missenemy");
                    File.WriteAllLines(fileName, globalLine);
                }
                globalLine = File.ReadAllLines(fileName).ToList<string>();
                splitSections = false;
                rewriteSections = false;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_bulletshoot_") && !globalLine[i].Contains("_volume\"") && !globalLine[i].Contains("_pitch\"")
                        && !globalLine[i].Contains("_flags\"") && !globalLine[i].Contains("_level\"") && !globalLine[i].Contains("_cooldown_min\"")
                        && !globalLine[i].Contains("_cooldown_max\"") && !globalLine[i].Contains("_pitch_random_min\"") && !globalLine[i].Contains("_pitch_random_max\"")
                        && !globalLine[i].Contains("_channel\""))
                    {
                        splitSections = true;
                        break;
                    }
                }
                if (!splitSections)
                {
                    for (int i = 0; i < globalLine.Count; i++)
                    {
                        if (globalLine[i].Contains("\"sound_bulletshoot\""))
                        {
                            int bracketCheck = i + 1;
                            bool breakOut = false;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('\"'))
                                {
                                    if (globalLine[bracketCheck].Contains("\"paths\""))
                                    {
                                        breakOut = true;
                                        break;
                                    }
                                }
                            }
                            if (breakOut)
                            {
                                break;
                            }
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{') && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('}'))
                                {
                                    break;
                                }
                            }
                            int originalPosition = bracketCheck;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}') && !globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                bracketCheck++;
                            }
                            if (globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                break;
                            }
                            else
                            {
                                bracketCheck = originalPosition;
                            }
                            if (bracketCheck < globalLine.Count && globalLine[bracketCheck].Contains('{'))
                            {
                                rewriteSections = true;
                            }
                        }
                    }
                }
                if (splitSections && !rewriteSections)
                {
                    ChangeMultiSoundSections("sound_bulletshoot", fileName, globalLine, kv, splitSections);
                }
                else if (rewriteSections && !splitSections)
                {
                    RewriteMultiSoundSections("sound_bulletshoot", fileName, globalLine, floatSoundParams, intSoundParams, kv);
                }
                else
                {
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_bulletshoot");
                    File.WriteAllLines(fileName, globalLine);
                }
                globalLine = File.ReadAllLines(fileName).ToList<string>();
                splitSections = false;
                rewriteSections = false;
                for (int i = 0; i < globalLine.Count; i++)
                {
                    if (globalLine[i].Contains("\"sound_attackshootprojectile_") && !globalLine[i].Contains("_volume\"") && !globalLine[i].Contains("_pitch\"")
                        && !globalLine[i].Contains("_flags\"") && !globalLine[i].Contains("_level\"") && !globalLine[i].Contains("_cooldown_min\"")
                        && !globalLine[i].Contains("_cooldown_max\"") && !globalLine[i].Contains("_pitch_random_min\"") && !globalLine[i].Contains("_pitch_random_max\"")
                        && !globalLine[i].Contains("_channel\""))
                    {
                        splitSections = true;
                        break;
                    }
                }
                if (!splitSections)
                {
                    for (int i = 0; i < globalLine.Count; i++)
                    {
                        if (globalLine[i].Contains("\"sound_attackshootprojectile\""))
                        {
                            int bracketCheck = i + 1;
                            bool breakOut = false;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('\"'))
                                {
                                    if (globalLine[bracketCheck].Contains("\"paths\""))
                                    {
                                        breakOut = true;
                                        break;
                                    }
                                }
                            }
                            if (breakOut)
                            {
                                break;
                            }
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('{') && !globalLine[bracketCheck].Contains('}'))
                            {
                                bracketCheck++;
                                if (globalLine[bracketCheck].Contains('}'))
                                {
                                    break;
                                }
                            }
                            int originalPosition = bracketCheck;
                            while (bracketCheck < globalLine.Count && !globalLine[bracketCheck].Contains('}') && !globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                bracketCheck++;
                            }
                            if (globalLine[bracketCheck].Contains("\"paths\""))
                            {
                                break;
                            }
                            else
                            {
                                bracketCheck = originalPosition;
                            }
                            if (bracketCheck < globalLine.Count && globalLine[bracketCheck].Contains('{'))
                            {
                                rewriteSections = true;
                            }
                        }
                    }
                }
                if (splitSections && !rewriteSections)
                {
                    ChangeMultiSoundSections("sound_attackshootprojectile", fileName, globalLine, kv, splitSections);
                }
                else if (rewriteSections && !splitSections)
                {
                    RewriteMultiSoundSections("sound_attackshootprojectile", fileName, globalLine, floatSoundParams, intSoundParams, kv);
                }
                else
                {
                    floatSoundParams.Clear();
                    floatSoundParams.Add("1.0"); // Volume
                    floatSoundParams.Add("1.5"); // Cooldown Min
                    floatSoundParams.Add("1.5"); // Cooldown Max
                    intSoundParams.Clear();
                    intSoundParams.Add(0); // Channel
                    intSoundParams.Add(0); // Flags
                    intSoundParams.Add(90); // Level
                    intSoundParams.Add(100); // Pitch
                    intSoundParams.Add(100); // Pitch Random Min
                    intSoundParams.Add(100); // Pitch Random Max
                    keyvalueName.Clear();
                    ChangeSoundSection(globalLine, keyvalueName, floatSoundParams, intSoundParams, "sound_attackshootprojectile");
                    File.WriteAllLines(fileName, globalLine);
                }
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
                        if ((i2 - 1 > 0 && (char.IsLetterOrDigit(charList[i2 - 1]) || IsCharSymbol(charList[i2 - 1]))) && 
                            (i2 + 1 < charList.Count && (char.IsLetterOrDigit(charList[i2 + 1]) || IsCharSymbol(charList[i2 + 1]))))
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
                for (int i2 = 0; i2 < curlyIndex; i2++)
                {
                    sb.Insert(0, "\t");
                }
                globalLine[i] = sb.ToString();
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
            filesFound++;
        }
    }
}