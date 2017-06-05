using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{
    public static class  CodeFormat
    {
        
        public static string Format(string input)
        {

            var file = new List<string>(input.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));

            int insideBracket = 0;
            bool between = false;
            bool isComm = false;
            bool isLit = false;
            bool isSobLit = false;
            bool isAtt = false;
            bool isElseDir = false;
            Regex dirReg = new Regex(@"^\s*#(\w+)");

            List<string> result = new List<string>();
            string res = "";
            for (int i = 0; i < file.Count; i++)
            {
                bool hastz = false;
                bool oneMoreTime = false;

                for (int j = 0; j < file[i].Length; j++)
                {

                    if (isSobLit)
                    {
                        if (file[i][j] == '\"')
                        {
                            if (j + 1 < file[i].Length && file[i][j + 1] == '\"')
                            {
                                int k = j;
                                while (k < file[i].Length && file[i][k] == '\"')
                                    k++;

                                if (((k - j) % 2) != 0)
                                {
                                    isLit = false;
                                    isSobLit = false;
                                }
                                j = k - 1;
                            }
                            else
                            {
                                isLit = false;
                                isSobLit = false;
                            }
                        }
                    }
                    else if (isLit)
                    {
                        if (file[i][j] == '\"')
                        {
                            if (file[i][j - 1] == '\\')
                            {
                                int k = j - 1;
                                while (file[i][k] == '\\')
                                    k--;

                                if ((j - k) % 2 != 0)
                                {
                                    isLit = false;
                                    isSobLit = false;
                                }
                            }
                            else
                            {
                                isLit = false;
                                isSobLit = false;
                            }
                        }
                    }
                    else
                    if (isComm)
                    {
                        if (j != file[i].Length - 1 && file[i][j] == '*' && file[i][j + 1] == '/')
                        {
                            isComm = false;
                            if ((j + 2) < file[i].Length)
                            {
                                file[i] = file[i].Substring(j + 2);
                                i--;
                                oneMoreTime = true;
                                break;
                            }
                            else
                            {
                                oneMoreTime = true;
                                break;
                            }
                        }
                    }
                    else if (isElseDir)
                    {
                        if (dirReg.IsMatch(file[i]) && dirReg.Match(file[i]).Groups[1].Value == "endif")
                        {
                            isElseDir = false;
                            oneMoreTime = true;
                            break;
                        }
                    }
                    else if (isAtt)
                    {
                        if (file[i][j] == ']')
                        {
                            isAtt = false;
                            result.Add(res + file[i].Substring(0, j) + "]");
                            res = "";
                            if ((j + 1) < file[i].Length)
                            {
                                file[i] = file[i].Substring(j + 1);
                                i--;
                            }
                            oneMoreTime = true;
                            break;
                        }
                    }
                    else
                    if (file[i][j] == '\"')
                    {
                        isLit = true;
                        if (j != 0 && file[i][j - 1] == '@')
                        {
                            isSobLit = true;
                        }
                    }
                    else if (file[i][j] == '\'')
                    {
                        if (file[i][j + 1] == '\\')
                            j += 3;
                        else
                            j += 2;
                    }
                    
                    else if (file[i].Replace(" ", "").Replace("\t", "").StartsWith("[") && res.Replace(" ", "").Replace("\t", "") == string.Empty)
                    {
                        isAtt = true;
                    }
                    else if (new Regex(@"^\s*#\w+").IsMatch(file[i]))
                    {
                        oneMoreTime = true;
                        if (new Regex(@"^\s*#(\w+)").Match(file[i]).Groups[1].Value == "else")
                            isElseDir = true;
                        break;
                    }

                    else
                    if (j != file[i].Length - 1 && file[i][j] == '/' && file[i][j + 1] == '*')
                    {
                        var temp = file[i].Substring(0, j);
                        if (temp.Trim(new char[] { ' ', '\t' }) != string.Empty)
                            res = res + " " + temp;
                        isComm = true;
                        j++;
                    }
                    else if (j != file[i].Length - 1 && file[i][j] == '/' && file[i][j + 1] == '/')
                    {
                        var temp = file[i].Substring(0, j);
                        if (temp.Trim(new char[] { ' ', '\t' }) != string.Empty)
                            res = res + " " + temp;
                        oneMoreTime = true;
                        break;
                    }
                    else
                    if (file[i][j] == '(')
                        insideBracket++;
                    else if (file[i][j] == ')')
                        insideBracket--;
                    else if (file[i][j] == '=' && insideBracket == 0)
                    {
                        if (j != file[i].Length - 1 && file[i][j + 1] == '>' && between)
                        {
                            between = false;
                        }
                        else if (!(result.Count > 1 && (" " + result[result.Count - 2] + " ").Contains(" enum ") && result[result.Count - 1].Trim(' ', '\t') == "{"))
                            if (!(" " + res + " " + file[i].Substring(0, j) + " ").Contains(" operator "))
                                between = true;
                    }
                    else if (!between && insideBracket == 0 && (file[i][j] == '{' || file[i][j] == '}'))
                    {
                        var tempString = file[i].Trim(new char[] { ' ', '\t' });
                        if (tempString != "{" && tempString != "}" && tempString.Replace(" ", "").Replace("\t", "") != "};")
                        {
                            var t = file[i].Substring(0, j);
                            if (t.Trim(new char[] { ' ', '\t' }) != "")
                                res = res + t;
                            if (res != string.Empty)
                                result.Add(res);
                            res = "";
                            result.Add(new string(file[i][j], 1));
                            if ((j + 1) < file[i].Length)
                            {
                                file[i] = file[i].Substring(j + 1);
                                i--;
                                oneMoreTime = true;
                                break;
                            }
                            oneMoreTime = true;
                            break;



                        }
                        else
                        {
                            if (res != string.Empty)
                                result.Add(res);
                            result.Add(file[i]);
                            res = "";
                            oneMoreTime = true;
                            break;
                        }


                    }
                    else if (insideBracket == 0 && file[i][j] == ';')
                    {
                        res = res + file[i].Substring(0, j + 1);
                        if ((j + 1) < file[i].Length)
                        {
                            file[i] = file[i].Substring(j + 1);
                            i--;
                        }
                        hastz = true;
                        if (insideBracket == 0)
                            between = false;
                        break;
                    }

                }
                if (!oneMoreTime)
                {
                    if (hastz)
                    {
                        if (res != string.Empty)
                            result.Add(res);
                        res = "";
                    }
                    else if (!isComm && ! isElseDir)
                    {
                        if (res.Trim(' ', '\t') != string.Empty)
                            res = res + " " + file[i].TrimStart(' ', '\t');
                        else
                            res = file[i].TrimStart(' ', '\t');
                    }
                }
            }


            FixSquaredBrackets(result);
            
            return string.Join("\r\n", result);
        }

        private static void FixSquaredBrackets(List<string> file)
        {
            var reg1 = new Regex(@"\[\s+\]");
            var reg2 = new Regex(@"(\w)\s+\[");
            var reg3 = new Regex(@"(\w)\s+<");

            for (int i = 0; i < file.Count; i++)
            {
                if (reg1.IsMatch(file[i]))
                {
                    file[i] = Regex.Replace(file[i], @"\[\s+\]", "[]");
                }
                if (reg2.IsMatch(file[i]))
                {
                    file[i] = Regex.Replace(file[i], @"(\w)\s+\[", "$1[");
                }
                if (reg3.IsMatch(file[i]))
                {
                    file[i] = Regex.Replace(file[i], @"(\w)\s+<", "$1<");
                }
            }
        }
    }
}
