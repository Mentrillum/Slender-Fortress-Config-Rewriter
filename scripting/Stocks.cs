using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SF2MConfigRewrite;

public static class Stocks
{ 
    public static string BreakMultiSoundDown(string line)
    {
        StringBuilder stringBuilder = new StringBuilder();
        byte quoteCheck = 0;
        char[] arr = line.ToCharArray();
        for (int i2 = 0; i2 < arr.Length; i2++)
        {
            if (arr[i2] == '\"')
            {
                quoteCheck++;
                if (quoteCheck > 2)
                {
                    break;
                }
            }
            if (char.IsLetterOrDigit(arr[i2]) || arr[i2] == '_')
            {
                stringBuilder.Append(arr[i2]);
            }
        }
        string returnValue = stringBuilder.ToString();
        return returnValue;
    }
    public static bool IsCharSymbol(char c)
    {
        if (c == '-' || c == '/' || c == '{' || c == '}' || c == '[' || c == ']' || c == '(' || c == ')' || c == '<' || c == '>' || c == '!' ||
            c == '?' || c == '=' || c == '+' || c == '|' || c == '\\' || c == '/' || c == '\'' || c == '@' || c == '#' || c == '$' || c == '%' || c == '^'
            || c == '^' || c == '&' || c == '*' || c == '_' || c == '.' || c == '`' || c == '~' || c == ';' || c == ':' || c == ',')
        {
            return true;
        }
        return false;
    }

    public static void WriteAnimationSection(List<string> keys, string baseKey, List<string> line, int firstIndex, out int storedIndex)
    {
        line.Insert(firstIndex, "\t\t\t\"" + baseKey + "\"");
        firstIndex++;
        line.Insert(firstIndex, "\t\t\t{");
        firstIndex++;
        line.Insert(firstIndex, "\t\t\t\t\"1\"");
        firstIndex++;
        line.Insert(firstIndex, "\t\t\t\t{");
        firstIndex++;

        string newKey = string.Empty;
        for (int i = 0; i < keys.Count; i++)
        {
            newKey = "\t\t\t\t\t" + keys[i];
            line.Insert(firstIndex, newKey);
            firstIndex++;
        }
        line.Insert(firstIndex, "\t\t\t\t}");
        firstIndex++;
        line.Insert(firstIndex, "\t\t\t}");
        firstIndex++;
        storedIndex = firstIndex;
    }

    public static void AddFootstepIntervals(List<string> line, string footstepInterval, string baseAnimationName)
    {
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseAnimationName + "\""))
            {
                int bracketIndex = 1;
                int bracketCheck = i;
                while (!line[bracketCheck].Contains("{"))
                {
                    bracketCheck++;
                }
                while (!line[bracketCheck].Contains("\""))
                {
                    bracketCheck++;
                }
                for (int j = bracketCheck; j < line.Count; j++)
                {
                    if (line[j].Contains("}"))
                    {
                        bracketIndex--;
                        if (bracketIndex <= 0)
                        {
                            return;
                        }
                    }
                    if (line[j].Contains("{"))
                    {
                        bracketIndex++;
                        while (!line[j].Contains("}"))
                        {
                            j++;
                        }
                        bracketIndex--;
                        if (footstepInterval != "0.0" && footstepInterval != "0")
                        {
                            line.Insert(j, "\"animation_" + baseAnimationName + "_footstepinterval\" \"" + footstepInterval + "\"");
                        }
                        j++;
                    }
                }
            }
        }
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

    public static void RewriteMultiSoundSections(string baseKeyName, string fileName, List<string> line, List<string> floatSoundParams, List<int> intSoundParams, KeyValues kv)
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
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "_volume\""))
            {
                floatSoundParams[0] = kv.GetFloat(baseKeyName + "_volume", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_cooldown_min\""))
            {
                floatSoundParams[1] = kv.GetFloat(baseKeyName + "_cooldown_min", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_cooldown_max\""))
            {
                floatSoundParams[2] = kv.GetFloat(baseKeyName + "_cooldown_max", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_channel\""))
            {
                intSoundParams[0] = kv.GetNum(baseKeyName + "_channel", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_flags\""))
            {
                intSoundParams[1] = kv.GetNum(baseKeyName + "_flags", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_level\""))
            {
                intSoundParams[2] = kv.GetNum(baseKeyName + "_level", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_pitch\""))
            {
                intSoundParams[3] = kv.GetNum(baseKeyName + "_pitch", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_pitch_random_min\""))
            {
                intSoundParams[4] = kv.GetNum(baseKeyName + "_pitch_random_min", i);
            }
            if (line[i].Contains("\"" + baseKeyName + "_pitch_random_max\""))
            {
                intSoundParams[5] = kv.GetNum(baseKeyName + "_pitch_random_max", i);
            }
        }
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "\""))
            {
                byte bracketIndex = 1;
                int bracketCheck = i;
                while (!line[bracketCheck].Contains('{'))
                {
                    bracketCheck++;
                }
                while (!line[bracketCheck].Contains('\"'))
                {
                    bracketCheck++;
                }
                for (int j = bracketCheck; j < line.Count; j++)
                {
                    if (line[j].Contains('}'))
                    {
                        bracketIndex--;
                        if (bracketIndex < 1)
                        {
                            break;
                        }
                    }
                    if (line[j].Contains('{'))
                    {
                        bracketIndex++;
                        j++;
                        bool doSpace = false;
                        if (floatSoundParams[0] != "1.0" && floatSoundParams[0] != "1")
                        {
                            line.Insert(j, "\"volume\" \"" + floatSoundParams[0] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (floatSoundParams[1] != "1.5")
                        {
                            line.Insert(j, "\"cooldown_min\" \"" + floatSoundParams[1] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (floatSoundParams[2] != "1.5")
                        {
                            line.Insert(j, "\"cooldown_max\" \"" + floatSoundParams[2] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[0] != 0)
                        {
                            line.Insert(j, "\"channel\" \"" + intSoundParams[0] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[1] != 0)
                        {
                            line.Insert(j, "\"flags\" \"" + intSoundParams[1] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[2] != 90)
                        {
                            line.Insert(j, "\"level\" \"" + intSoundParams[2] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[3] != 100)
                        {
                            line.Insert(j, "\"pitch\" \"" + intSoundParams[3] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[4] != 100)
                        {
                            line.Insert(j, "\"pitch_random_min\" \"" + intSoundParams[4] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (intSoundParams[5] != 100)
                        {
                            line.Insert(j, "\"pitch_random_max\" \"" + intSoundParams[5] + "\"");
                            j++;
                            doSpace = true;
                        }
                        if (doSpace)
                        {
                            line.Insert(j, "");
                            j++;
                        }
                        line.Insert(j, "\"paths\"");
                        j++;
                        line.Insert(j, "{");
                        j++;
                        while (!line[j].Contains('}'))
                        {
                            j++;
                        }
                        line.Insert(j, "}");
                        bracketCheck = j;
                    }
                }
            }
        }
        File.WriteAllLines(fileName, line);
    }

    public static void ChangeMultiSoundSections(string baseKeyName, string fileName, List<string> line, KeyValues kv, bool splitSections)
    {
        List<List<string>> listCeptionSounds = new List<List<string>>();
        List<List<string>> listCeptionFloats = new List<List<string>>();
        List<List<int>> listCeptionInts = new List<List<int>>();
        List<byte> tempIndexes = new List<byte>();
        List<byte> attackIndexes = new List<byte>();
        StringBuilder stringBuilder;
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName) && (line[i].Contains("_volume\"") || line[i].Contains("_pitch\"")
                || line[i].Contains("_flags\"") || line[i].Contains("_level\"") || line[i].Contains("_cooldown_min\"")
                || line[i].Contains("_cooldown_max\"") || line[i].Contains("_pitch_random_min\"") || line[i].Contains("_pitch_random_max\"") || line[i].Contains("_channel\"")))
            {
                List<string> tempFloatSoundParams;
                List<int> tempIntSoundParams;
                stringBuilder = new StringBuilder();
                byte byteIndex = 0, quoteCheck = 0;
                char[] arr = line[i].ToCharArray();
                for (int i2 = 0; i2 < arr.Length; i2++)
                {
                    if (arr[i2] == '\"')
                    {
                        quoteCheck++;
                        if (quoteCheck == 2)
                        {
                            break;
                        }
                    }
                    if (char.IsDigit(arr[i2]))
                    {
                        stringBuilder.Append(arr[i2]);
                    }
                }
                if (stringBuilder.Length <= 0)
                {
                    stringBuilder.Append('1');
                }
                byteIndex = byte.Parse(stringBuilder.ToString());
                if (tempIndexes.Count > 0)
                {
                    int index = tempIndexes.IndexOf(byteIndex);
                    if (index == -1)
                    {
                        tempIndexes.Add(byteIndex);
                        tempFloatSoundParams = new List<string>();
                        tempIntSoundParams = new List<int>();
                        tempFloatSoundParams.Add("1.0"); // Volume
                        tempFloatSoundParams.Add("1.5"); // Cooldown Min
                        tempFloatSoundParams.Add("1.5"); // Cooldown Max
                        tempIntSoundParams.Add(0); // Channel
                        tempIntSoundParams.Add(0); // Flags
                        tempIntSoundParams.Add(90); // Level
                        tempIntSoundParams.Add(100); // Pitch
                        tempIntSoundParams.Add(100); // Pitch Random Min
                        tempIntSoundParams.Add(100); // Pitch Random Max
                        switch (line[i])
                        {
                            case string a when line[i].Contains("_volume\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[0] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_cooldown_min\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[1] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_cooldown_max\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[2] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_channel\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[0] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_flags\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[1] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_level\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[2] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[3] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch_random_min\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[4] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch_random_max\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[5] = kv.GetNum(a, i);
                                break;
                        }
                        listCeptionFloats.Add(tempFloatSoundParams);
                        listCeptionInts.Add(tempIntSoundParams);
                    }
                    else
                    {
                        tempFloatSoundParams = listCeptionFloats[index];
                        tempIntSoundParams = listCeptionInts[index];
                        switch (line[i])
                        {
                            case string a when line[i].Contains("_volume\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[0] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_cooldown_min\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[1] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_cooldown_max\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempFloatSoundParams[2] = kv.GetFloat(a, i).ToString();
                                break;
                            case string a when line[i].Contains("_channel\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[0] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_flags\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[1] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_level\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[2] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[3] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch_random_min\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[4] = kv.GetNum(a, i);
                                break;
                            case string a when line[i].Contains("_pitch_random_max\""):
                                a = BreakMultiSoundDown(line[i]);
                                tempIntSoundParams[5] = kv.GetNum(a, i);
                                break;
                        }
                        listCeptionFloats[index] = tempFloatSoundParams;
                        listCeptionInts[index] = tempIntSoundParams;
                    }
                }
                else
                {
                    tempIndexes.Add(byteIndex);
                    tempFloatSoundParams = new List<string>();
                    tempIntSoundParams = new List<int>();
                    tempFloatSoundParams.Add("1.0"); // Volume
                    tempFloatSoundParams.Add("1.5"); // Cooldown Min
                    tempFloatSoundParams.Add("1.5"); // Cooldown Max
                    tempIntSoundParams.Add(0); // Channel
                    tempIntSoundParams.Add(0); // Flags
                    tempIntSoundParams.Add(90); // Level
                    tempIntSoundParams.Add(100); // Pitch
                    tempIntSoundParams.Add(100); // Pitch Random Min
                    tempIntSoundParams.Add(100); // Pitch Random Max
                    switch (line[i])
                    {
                        case string a when line[i].Contains("_volume\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempFloatSoundParams[0] = kv.GetFloat(a, i).ToString();
                            break;
                        case string a when line[i].Contains("_cooldown_min\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempFloatSoundParams[1] = kv.GetFloat(a, i).ToString();
                            break;
                        case string a when line[i].Contains("_cooldown_max\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempFloatSoundParams[2] = kv.GetFloat(a, i).ToString();
                            break;
                        case string a when line[i].Contains("_channel\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[0] = kv.GetNum(a, i);
                            break;
                        case string a when line[i].Contains("_flags\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[1] = kv.GetNum(a, i);
                            break;
                        case string a when line[i].Contains("_level\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[2] = kv.GetNum(a, i);
                            break;
                        case string a when line[i].Contains("_pitch\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[3] = kv.GetNum(a, i);
                            break;
                        case string a when line[i].Contains("_pitch_random_min\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[4] = kv.GetNum(a, i);
                            break;
                        case string a when line[i].Contains("_pitch_random_max\""):
                            a = BreakMultiSoundDown(line[i]);
                            tempIntSoundParams[5] = kv.GetNum(a, i);
                            break;
                    }
                    listCeptionFloats.Add(tempFloatSoundParams);
                    listCeptionInts.Add(tempIntSoundParams);
                }
                line.RemoveAt(i);
                i--;
                File.WriteAllLines(fileName, line);
            }
        }
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName) && !line[i].Contains("_volume\"") && !line[i].Contains("_pitch\"")
                && !line[i].Contains("_flags\"") && !line[i].Contains("_level\"") && !line[i].Contains("_channel\"") && !line[i].Contains("_cooldown_min\"")
                && !line[i].Contains("_cooldown_max\"") && !line[i].Contains("_pitch_random_min\"") && !line[i].Contains("_pitch_random_max\""))
            {
                int originalIndex = i;
                int bracketCheck = i + 2;
                if (line[bracketCheck + 1].Contains('{'))
                {
                    continue;
                }
                else
                {
                    List<string> attackSoundPaths = new List<string>();
                    int index = 1;
                    while (bracketCheck < line.Count - 1 && !line[bracketCheck].Contains('\"') && !line[bracketCheck].Contains('}'))
                    {
                        bracketCheck++;
                    }
                    if (line[i].Contains("sound_attackenemy"))
                    {

                    }
                    while (!line[bracketCheck].Contains('}'))
                    {
                        string s1 = kv.GetString(index.ToString(), bracketCheck);
                        if (s1 == string.Empty)
                        {
                            break;
                        }
                        attackSoundPaths.Add(s1);
                        index++;
                        bracketCheck++;
                    }
                    listCeptionSounds.Add(attackSoundPaths);
                    if (line[i].Contains("\"" + baseKeyName + "_"))
                    {
                        int bracketCheck2 = i;
                        while (!line[bracketCheck2].Contains('{') && bracketCheck2 < line.Count)
                        {
                            bracketCheck2++;
                        }
                        stringBuilder = new StringBuilder();
                        byte byteIndex = 0, quoteCheck = 0;
                        char[] arr = line[i].ToCharArray();
                        for (int i2 = 0; i2 < arr.Length; i2++)
                        {
                            if (arr[i2] == '\"')
                            {
                                quoteCheck++;
                                if (quoteCheck == 2)
                                {
                                    break;
                                }
                            }
                            if (char.IsDigit(arr[i2]))
                            {
                                stringBuilder.Append(arr[i2]);
                            }
                        }
                        if (stringBuilder.Length > 0)
                        {
                            byteIndex = byte.Parse(stringBuilder.ToString());
                            if (attackIndexes.IndexOf(byteIndex) == -1)
                            {
                                attackIndexes.Add(byteIndex);
                            }
                        }
                        line.RemoveAt(i);
                        bracketCheck2--;
                        while (!line[bracketCheck2].Contains('}'))
                        {
                            line.RemoveAt(bracketCheck2);
                        }
                        line.RemoveAt(bracketCheck2);
                        i = bracketCheck2 - 1;
                    }
                    else if (line[i].Contains("\"" + baseKeyName + "\"") && splitSections)
                    {
                        if (attackIndexes.IndexOf(1) == -1)
                        {
                            attackIndexes.Add(1);
                        }
                        int removeAt = i;
                        while (!line[removeAt].Contains('{'))
                        {
                            removeAt++;
                        }
                        removeAt++;
                        while (!line[removeAt].Contains('}'))
                        {
                            line.RemoveAt(removeAt);
                            i--;
                        }
                        i = originalIndex;
                    }
                    File.WriteAllLines(fileName, line);
                }
            }
        }
        bool foundSection = false;
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "\""))
            {
                foundSection = true;
                int bracketCheck = i;
                while (!line[bracketCheck].Contains('}'))
                {
                    bracketCheck++;
                }
                for (int j = 0; j < attackIndexes.Count; j++)
                {
                    line.Insert(bracketCheck, "\"" + attackIndexes[j] + "\"");
                    bracketCheck++;
                    line.Insert(bracketCheck, "{");
                    bracketCheck++;
                    List<string> attackSoundPaths = listCeptionSounds[j];
                    if (attackSoundPaths != null)
                    {
                        bool bracketIncrease = false;
                        if (listCeptionFloats.Count > 0 && listCeptionFloats.Count > j)
                        {
                            List<string> tempFloatSoundParams = listCeptionFloats[j];
                            for (int k = 0; k < tempFloatSoundParams.Count; k++)
                            {
                                if (k == 0)
                                {
                                    if (tempFloatSoundParams[k] == "1.0" || tempFloatSoundParams[k] == "1")
                                    {
                                        continue;
                                    }
                                    line.Insert(bracketCheck, "\"volume\" \"" + tempFloatSoundParams[k] + "\"");
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                                else
                                {
                                    if (tempFloatSoundParams[k] == "1.5")
                                    {
                                        continue;
                                    }
                                    if (k == 1)
                                    {
                                        line.Insert(bracketCheck, "\"cooldown_min\" \"" + tempFloatSoundParams[k] + "\"");
                                    }
                                    else
                                    {
                                        line.Insert(bracketCheck, "\"cooldown_max\" \"" + tempFloatSoundParams[k] + "\"");
                                    }
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                            }
                        }
                        if (listCeptionInts.Count > 0 && listCeptionInts.Count > j)
                        {
                            List<int> tempIntSoundParams = listCeptionInts[j];
                            for (int k = 0; k < tempIntSoundParams.Count; k++)
                            {
                                if (k == 0)
                                {
                                    if (tempIntSoundParams[k] == 0)
                                    {
                                        continue;
                                    }
                                    line.Insert(bracketCheck, "\"channel\" \"" + tempIntSoundParams[k] + "\"");
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                                else if (k == 1)
                                {
                                    if (tempIntSoundParams[k] == 0)
                                    {
                                        continue;
                                    }
                                    line.Insert(bracketCheck, "\"flags\" \"" + tempIntSoundParams[k] + "\"");
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                                else if (k == 2)
                                {
                                    if (tempIntSoundParams[k] == 90)
                                    {
                                        continue;
                                    }
                                    line.Insert(bracketCheck, "\"level\" \"" + tempIntSoundParams[k] + "\"");
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                                else
                                {
                                    if (tempIntSoundParams[k] == 100)
                                    {
                                        continue;
                                    }
                                    if (k == 3)
                                    {
                                        line.Insert(bracketCheck, "\"pitch\" \"" + tempIntSoundParams[k] + "\"");
                                    }
                                    else if (k == 4)
                                    {
                                        line.Insert(bracketCheck, "\"pitch_random_min\" \"" + tempIntSoundParams[k] + "\"");
                                    }
                                    else
                                    {
                                        line.Insert(bracketCheck, "\"pitch_random_max\" \"" + tempIntSoundParams[k] + "\"");
                                    }
                                    bracketCheck++;
                                    bracketIncrease = true;
                                }
                            }
                        }
                        if (bracketIncrease)
                        {
                            line.Insert(bracketCheck, string.Empty);
                            bracketCheck++;
                        }
                        line.Insert(bracketCheck, "\"paths\"");
                        bracketCheck++;
                        line.Insert(bracketCheck, "{");
                        bracketCheck++;
                        for (int k = 0; k < attackSoundPaths.Count; k++)
                        {
                            line.Insert(bracketCheck, "\"" + (k + 1) + "\" \"" + attackSoundPaths[k] + "\"");
                            bracketCheck++;
                        }
                        line.Insert(bracketCheck, "}");
                        bracketCheck++;
                    }
                    line.Insert(bracketCheck, "}");
                    bracketCheck++;
                }

            }
        }
        if (!foundSection)
        {
            int newLine = line.Count - 1;
            line.Insert(newLine, "\"" + baseKeyName + "\"");
            newLine++;
            line.Insert(newLine, "{");
            newLine++;
            for (int i = 0; i < attackIndexes.Count; i++)
            {
                line.Insert(newLine, "\"" + attackIndexes[i] + "\"");
                newLine++;
                line.Insert(newLine, "{");
                newLine++;
                List<string> attackSoundPaths = listCeptionSounds[i];
                if (attackSoundPaths != null)
                {
                    bool bracketIncrease = false;
                    if (listCeptionFloats.Count > 0 && listCeptionFloats.Count > i)
                    {
                        List<string> tempFloatSoundParams = listCeptionFloats[i];
                        for (int k = 0; k < tempFloatSoundParams.Count; k++)
                        {
                            if (k == 0)
                            {
                                if (tempFloatSoundParams[k] == "1.0" || tempFloatSoundParams[k] == "1")
                                {
                                    continue;
                                }
                                line.Insert(newLine, "\"volume\" \"" + tempFloatSoundParams[k] + "\"");
                                newLine++;
                                bracketIncrease = true;
                            }
                            else
                            {
                                if (tempFloatSoundParams[k] == "1.5")
                                {
                                    continue;
                                }
                                if (k == 1)
                                {
                                    line.Insert(newLine, "\"cooldown_min\" \"" + tempFloatSoundParams[k] + "\"");
                                }
                                else
                                {
                                    line.Insert(newLine, "\"cooldown_max\" \"" + tempFloatSoundParams[k] + "\"");
                                }
                                newLine++;
                                bracketIncrease = true;
                            }
                        }
                    }
                    if (listCeptionInts.Count > 0 && listCeptionInts.Count > i)
                    {
                        List<int> tempIntSoundParams = listCeptionInts[i];
                        for (int k = 0; k < tempIntSoundParams.Count; k++)
                        {
                            if (k == 0)
                            {
                                if (tempIntSoundParams[k] == 0)
                                {
                                    continue;
                                }
                                line.Insert(newLine, "\"channel\" \"" + tempIntSoundParams[k] + "\"");
                                newLine++;
                                bracketIncrease = true;
                            }
                            else if (k == 1)
                            {
                                if (tempIntSoundParams[k] == 0)
                                {
                                    continue;
                                }
                                line.Insert(newLine, "\"flags\" \"" + tempIntSoundParams[k] + "\"");
                                newLine++;
                                bracketIncrease = true;
                            }
                            else if (k == 2)
                            {
                                if (tempIntSoundParams[k] == 90)
                                {
                                    continue;
                                }
                                line.Insert(newLine, "\"level\" \"" + tempIntSoundParams[k] + "\"");
                                newLine++;
                                bracketIncrease = true;
                            }
                            else
                            {
                                if (tempIntSoundParams[k] == 100)
                                {
                                    continue;
                                }
                                if (k == 3)
                                {
                                    line.Insert(newLine, "\"pitch\" \"" + tempIntSoundParams[k] + "\"");
                                }
                                else if (k == 4)
                                {
                                    line.Insert(newLine, "\"pitch_random_min\" \"" + tempIntSoundParams[k] + "\"");
                                }
                                else
                                {
                                    line.Insert(newLine, "\"pitch_random_max\" \"" + tempIntSoundParams[k] + "\"");
                                }
                                newLine++;
                                bracketIncrease = true;
                            }
                        }
                    }
                    if (bracketIncrease)
                    {
                        line.Insert(newLine, string.Empty);
                        newLine++;
                    }
                    line.Insert(newLine, "\"paths\"");
                    newLine++;
                    line.Insert(newLine, "{");
                    newLine++;
                    for (int k = 0; k < attackSoundPaths.Count; k++)
                    {
                        line.Insert(newLine, "\"" + (k + 1) + "\" \"" + attackSoundPaths[k] + "\"");
                        newLine++;
                    }
                    line.Insert(newLine, "}");
                    newLine++;
                }
                line.Insert(newLine, "}");
                newLine++;
            }
            line.Insert(newLine, "}");
        }
        File.WriteAllLines(fileName, line);
    }

    public static void ChangeSoundSection(List<string> line, List<string> keyvalueName, List<string> floatSoundParams, List<int> intSoundParams, string baseKeyName, bool includeCooldowns = false, bool includePitchRandoms = false)
    {
        KeyValues kv = new KeyValues();
        bool foundSomething = false;
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "\"") && !line[i].Contains("//"))
            {
                bool noPaths = false;
                int findPaths = i + 1;
                while (!line[findPaths].Contains('}'))
                {
                    if (line[findPaths].Contains("\"paths\""))
                    {
                        noPaths = true;
                        break;
                    }
                    findPaths++;
                }
                if (noPaths)
                {
                    return;
                }
            }
        }
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "_volume\"") || line[i].Contains("\"" + baseKeyName + "_channel\"")
                || line[i].Contains("\"" + baseKeyName + "_flags\"") || line[i].Contains("\"" + baseKeyName + "_level\"")
                || line[i].Contains("\"" + baseKeyName + "_pitch\"") || line[i].Contains("\"" + baseKeyName + "_cooldown_min\"")
                || line[i].Contains("\"" + baseKeyName + "_cooldown_max\"") || line[i].Contains("\"" + baseKeyName + "_pitch_random_min\"")
                || line[i].Contains("\"" + baseKeyName + "_pitch_random_max\""))
            {
                string keyValue = line[i];
                if (line[i].Contains("_volume"))
                {
                    floatSoundParams[0] = kv.GetFloat(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_cooldown_min"))
                {
                    floatSoundParams[1] = kv.GetFloat(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_cooldown_max"))
                {
                    floatSoundParams[2] = kv.GetFloat(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_channel"))
                {
                    intSoundParams[0] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_flags"))
                {
                    intSoundParams[1] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_level"))
                {
                    intSoundParams[2] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_pitch\""))
                {
                    intSoundParams[3] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_pitch_random_min"))
                {
                    intSoundParams[4] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
                else if (line[i].Contains("_pitch_random_max"))
                {
                    intSoundParams[5] = kv.GetNum(keyValue);
                    line.RemoveAt(i);
                    i--;
                    foundSomething = true;
                }
            }
        }
        List<int> itemIndex = new List<int>();
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "\"") && !line[i].Contains("//") && !line[i + 2].Contains("\"volume\"") && !line[i + 2].Contains("\"channel\"")
                && !line[i + 2].Contains("\"flags\"") && !line[i + 2].Contains("\"level\"") && !line[i + 2].Contains("\"pitch\"")
                && !line[i + 2].Contains("\"cooldown_min\"") && !line[i + 2].Contains("\"cooldown_max\"") && !line[i + 2].Contains("\"pitch_random_min\"")
                && !line[i + 2].Contains("\"pitch_random_max\"") && !line[i].Contains("_loop"))
            {
                if (baseKeyName == "sound_hitenemy")
                {

                }
                int iterations = 1;
                int index = 1;
                int startDelete = 0;
                while (!line[i + iterations].Contains('}'))
                {
                    if (line[i + iterations].Contains('{'))
                    {
                        startDelete = i + iterations + 1;
                    }
                    if (itemIndex.Count == 0)
                    {
                        if (line[i + iterations].Contains("\"" + index.ToString() + "\""))
                        {
                            keyvalueName.Add(line[i + iterations]);
                            line.RemoveAt(i + iterations);
                            itemIndex.Add(index);
                            index++;
                            iterations--;
                        }
                    }
                    else
                    {
                        for (int listIndex = 0; listIndex < itemIndex.Count; listIndex++)
                        {
                            if (line[i + iterations].Contains("\"" + itemIndex[listIndex].ToString() + "\""))
                            {
                                line[i + iterations] = line[i + iterations].Replace("\"" + itemIndex[listIndex].ToString() + "\"",
                                    "\"" + index.ToString() + "\"");
                                break;
                            }
                        }
                        if (line[i + iterations].Contains("\"" + index.ToString() + "\""))
                        {
                            keyvalueName.Add(line[i + iterations]);
                            line.RemoveAt(i + iterations);
                            itemIndex.Add(index);
                            index++;
                            iterations--;
                        }
                        else
                        {
                            if (!line[i + iterations].Contains('{') && !line[i + iterations].Contains('}') && !line[i + iterations].Contains('\"'))
                            {
                                line.RemoveAt(i + iterations);
                                iterations--;
                            }
                        }
                    }
                    iterations++;
                }
                bool deleteCurly = false;
                bool finished = false;
                bool values = false;
                bool paths = false;
                while (!finished)
                {
                    if (!deleteCurly)
                    {
                        line[startDelete] = "";
                        startDelete--;
                        deleteCurly = true;
                    }
                    else
                    {
                        if (!values)
                        {
                            if (intSoundParams[0] != 0)
                            {
                                line.Insert(startDelete, "\t\t\t\"channel\" \"" + intSoundParams[0] + "\"");
                                startDelete++;
                            }
                            if (floatSoundParams[0] != "1.0" && floatSoundParams[0] != "1")
                            {
                                line.Insert(startDelete, "\t\t\t\"volume\" \"" + floatSoundParams[0] + "\"");
                                startDelete++;
                            }
                            if (intSoundParams[1] != 0)
                            {
                                line.Insert(startDelete, "\t\t\t\"flags\" \"" + intSoundParams[1] + "\"");
                                startDelete++;
                            }
                            if (intSoundParams[2] != 90)
                            {
                                line.Insert(startDelete, "\t\t\t\"level\" \"" + intSoundParams[2] + "\"");
                                startDelete++;
                            }
                            if (intSoundParams[3] != 100)
                            {
                                line.Insert(startDelete, "\t\t\t\"pitch\" \"" + intSoundParams[3] + "\"");
                                startDelete++;
                            }
                            if (floatSoundParams[1] != "1.5" && includeCooldowns)
                            {
                                line.Insert(startDelete, "\t\t\t\"cooldown_min\" \"" + floatSoundParams[1] + "\"");
                                startDelete++;
                            }
                            if (floatSoundParams[2] != "1.5" && includeCooldowns)
                            {
                                line.Insert(startDelete, "\t\t\t\"cooldown_max\" \"" + floatSoundParams[2] + "\"");
                                startDelete++;
                            }
                            if (intSoundParams[4] != 100 && includePitchRandoms)
                            {
                                line.Insert(startDelete, "\t\t\t\"pitch_random_min\" \"" + intSoundParams[4] + "\"");
                                startDelete++;
                            }
                            if (intSoundParams[5] != 100 && includePitchRandoms)
                            {
                                line.Insert(startDelete, "\t\t\t\"pitch_random_min\" \"" + intSoundParams[5] + "\"");
                                startDelete++;
                            }
                            startDelete--;
                            values = true;
                        }
                        else
                        {
                            if (!paths)
                            {
                                if (foundSomething)
                                {
                                    startDelete++;
                                }
                                line.Insert(startDelete, "\t\t\t\"paths\"");
                                startDelete++;
                                line.Insert(startDelete, "\t\t\t{");
                                paths = true;
                            }
                            else
                            {
                                for (int i2 = 0; i2 < keyvalueName.Count; i2++)
                                {
                                    keyvalueName[i2] = "\t" + keyvalueName[i2];
                                    line.Insert(startDelete, keyvalueName[i2]);
                                    startDelete++;
                                }
                                line.Insert(startDelete, "\t\t\t}");
                                startDelete++;
                                line.Insert(startDelete, "\t\t}");
                                finished = true;
                            }
                        }
                    }
                    startDelete++;
                }
                break;
            }
        }
    }

    public static void RemoveUnnecessaryKeys(List<string> line, string baseKeyName, List<string> associatedKeys, KeyValues kv)
    {
        bool replace = false;
        for (int i = 0; i < line.Count; i++)
        {
            if (line[i].Contains("\"" + baseKeyName + "\"") && !line[i].Contains("\"animation_"))
            {
                bool enabled = Convert.ToBoolean(kv.GetNum(line[i]));
                if (!enabled)
                {
                    replace = true;
                    break;
                }
            }
        }
        if (replace)
        {
            for (int i = 0; i < line.Count; i++)
            {
                for (int i2 = 0; i2 < associatedKeys.Count; i2++)
                {
                    if (line[i].Contains(associatedKeys[i2]))
                    {
                        line.RemoveAt(i);
                        i--;
                        if (!line[i].Contains('\"') && !line[i].Contains('{') && !line[i].Contains('}'))
                        {
                            line.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}