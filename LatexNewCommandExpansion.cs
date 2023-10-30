using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace LatexNewCommandExpansion
{
    class LatexNewCommandExpansion
    {
        public List<string> newcommand;
        public List<int> commandPara;
        public List<string> originalcommand;
        public List<string> filelines;
        public List<bool> validlines;
        public string vs =".*[^ a - zA - Z0 - 9].*";
        public LatexNewCommandExpansion()
        {
            filelines = new List<string>();
            newcommand = new List<string>();
            commandPara = new List<int>();
            originalcommand = new List<string>();
            validlines = new List<bool>();
        }
        public LatexNewCommandExpansion(string path)
        {
            ReadFile(path);
            NewCommand(filelines, out newcommand, out commandPara, out originalcommand);
            NewCommandIterate(ref newcommand,commandPara,ref originalcommand);
            validlines = MarkLines(filelines);
        }
        public void Process()
        {
            for (int i = 0; i < filelines.Count; i++)
            {
                if (!validlines[i])
                    continue;
                filelines[i] = ProcessLine(filelines[i]);
            }
        }
        public string ProcessLine(string line)
        {
            for (int i = 0; i < newcommand.Count; i++)
            {
                var nci = newcommand[i];
                var cci = commandPara[i];
                var oci = originalcommand[i];
                if (cci == 0)
                {
                    int idx = 0,idx1=0;
                    List<int> ss = new List<int>();
                    while (MyStringIndex(line, nci, ref idx,ref idx1))
                    {           
                        ss.Add(idx);
                        idx = idx1;
                    }  
                    if (ss.Count != 0)
                        line = ReplaceCommonSegment(line, ss, nci, oci);
                    continue;
                }
                nci = string.Copy(nci) + "{";
                //to distinguish \ubarb and \ubar, when add "{", \ubarb{ is different from \ubar{. Otherwise, IndexOf("\ubar"} is the same as IndexOf{"\ubarb"};
                int k = line.IndexOf(nci);
                if (k < 0)
                    continue;
                List<string> oldstring = new List<string>();
                List<string> newstring = new List<string>();
                while (k >= 0)
                {
                    int k1 = k;
                    int k2 = k;
                    string[] varis = new string[commandPara[i]];
                    for (int j = 0; j < cci; j++)
                    {
                        k2 = BracketPair(line, ref k1, '{', '}');
                        varis[j] = line.Substring(k1 + 1, k2 - k1 - 1);
                        k1 = k2;
                    }
                    string ocivari = string.Copy(oci);
                    for (int j = 0; j < cci; j++)
                    {
                        ocivari=ocivari.Replace("#" + (j+1).ToString(), varis[j]);
                    }
                    // line = line.Substring(0, k) + ocivari + line.Substring(k2, line.Length - k2);
                    oldstring.Add(line.Substring(k , k2 - k+1 ));
                    newstring.Add(ocivari);
                    k = line.IndexOf(nci, k2);
                }
                for (int j = 0; j < oldstring.Count; j++)
                    line = line.Replace(oldstring[j], newstring[j]);
            }
            return line;
            
        }
        public void ReadFile(string path)
        {
            filelines = ReadFile2(path);
        }
        public static List<string> ReadFile2(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Error, the file doesn't exist!");
                Console.WriteLine("Please close the program!");
                Console.ReadKey();
            }
            string line = "";
            List<string> lineAll = new List<string>();
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                //line = line.Replace(" {", "{");
                lineAll.Add(line);
                //Console.WriteLine(line);
                //counter++;
            }
            file.Close();
            return lineAll;
        }
        public static void WriteFile(string path, List<string> lines)
        {
            System.IO.StreamWriter file = new StreamWriter(path);
            for (int i = 0; i < lines.Count; i++)
                file.WriteLine(lines[i]);
            file.Close();
        }
        public void OutputLatex(string path)
        {
            WriteFile(path, filelines);
        }
        public static List<bool> MarkLines(List<string> lines)
        {
            List<bool> mark = new List<bool>();
            for (int i = 0; i < lines.Count; i++)
            {
                var li = lines[i];
                if (li.Length == 0)
                    mark.Add(false);
                else if (li[0] == '%' || li.Contains("\\newcommand"))
                    mark.Add(false);
                else
                    mark.Add(true);
            }
            return mark;
        }
        public static void NewCommand(List<string> lines, out List<string> nc, out List<int> np, out List<string> oc)
        {
            var s = new List<string>();
            nc = new List<string>();
            np = new List<int>();
            oc = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                var li = lines[i];
                if (li.Length==0 ||li[0] != '\\')
                    continue;
                if (li.Contains("\\newcommand"))
                {
                    //li = li.Replace(" ", string.Empty);
                    int k = li.IndexOf("\\newcommand");
                    int k2 = BracketPair(li, ref k, '{', '}');
                    string nc1 = "";
                    int np1 = 0;
                    string oc1 = "";
                    if (k2 < 0)
                    {
                        Console.WriteLine("Error, not find pair {}");
                        return;
                    }
                    else
                        nc1=(li.Substring(k+1, k2 - k-1));
                    k = k2;
                    if (li.Contains('['))
                    {
                        k2 = BracketPair(li, ref k, '[', ']');
                        np1=(int.Parse(li.Substring(k + 1, k2 - k - 1)));
                        k = k2;
                    }
                    k2 = BracketPair(li, ref k, '{', '}');
                    if (k2 < 0)
                    {
                        Console.WriteLine("Error, not find pair {}");
                    }
                    else
                        oc1=(li.Substring(k+1, k2 - k-1));
                    //if(np1==0)
                    //{//newcommand without parameter should be stored as "\bx " or "\bx\"
                    //    string[] fs = new string[] {" ","\\",",","'","^","(",")","$","}","{","_","+","-","*","/",":","="};
                    //    for(int m=0;m<fs.Length;m++)
                    //    {
                    //        oc.Add(oc1 + fs[m]);
                    //        nc.Add(nc1 + fs[m]);
                    //        np.Add(0);
                    //    }
                    //}
                    //else
                    {
                        oc.Add(oc1);
                        nc.Add(nc1);
                        np.Add(np1);
                    }
                }
            }
            var nc2 = new string[nc.Count];
            var np2 = new int[nc.Count];
            for (int i = 0; i < nc.Count; i++)
            {
                np2[i] = i;
                nc2[i] = String.Copy(nc[i]);
            }
            Array.Sort(nc2, np2);
            var np0 = np2.ToList();
            var nc3 = new List<string>(nc.Count);
            var np3 = new List<int>(nc.Count);
            var op3 = new List<string>(nc.Count);
            for(int i=0;i<np0.Count;i++)
            {
                int k = np0.IndexOf(np0.Count - i - 1);
                nc3.Add(nc[k]);
                np3.Add(np[k]);
                op3.Add(oc[k]);
            }
            nc = nc3;
            np = np3;
            oc = op3;
        }
        public void NewCommandIterate(ref List<string> nc,List<int> pc,ref List<string> oc)
        {
            for(int iter=0;iter<4;iter++)//iterate 4 time to elemelate the composite newcommand;
            for(int i=0;i<pc.Count;i++)
            {
                    //var line = oc[i];
                    for(int j=0;j<pc.Count;j++)
                    {
                        oc[i]=ProcessLine(oc[i]);
                    }
            }
        }
        /// <summary>
        /// return the index of c2
        /// </summary>
        /// <param name="line"></param>
        /// <param name="c1First">the index of c1</param>
        /// <param name="c1">e.g. "{"</param>
        /// <param name="c2">e.g. "}"</param>
        /// <returns></returns>
        public static int BracketPair(string line, ref int c1First, char c1, char c2)
        {
            int pairCount = 0; bool notFindc1 = true;
            for (int i = c1First + 1; i < line.Length; i++)
            {
                if (line[i] == c1)
                {
                    if (notFindc1)
                    {
                        c1First = i;
                        notFindc1 = false;
                    }
                    pairCount++;
                }
                if (line[i] == c2)
                {
                    pairCount--;
                    if (pairCount == 0)
                        return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="substr"></param>
        /// <param name="k0"></param>
        /// <param name="k1"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        static public bool MyStringIndex(string line, string substr, ref int k0, ref int k1, string w = @"^[a-zA-Z0-9]+$")
        {
            while (true)
            {
                k0 = line.IndexOf(substr, k1);
                if (k0 < 0)
                    return false;
                k1 = k0 + substr.Length;
                if (line.Length == k1 || !Regex.IsMatch(line[k1].ToString(), w))
                    return true;
                //char.IsLetterOrDigit(line[k1])
            }
        }
        /// <summary>
        /// Replace the common segments with new segment in a string.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="location">the start index of the common segment</param>
        /// <param name="oldsubs">common segment</param>
        /// <param name="newsubs"></param>
        /// <returns></returns>
        static public string ReplaceCommonSegment(string line, List<int> location, string oldsubs,string newsubs)
        {
            var s2 = "";
            s2 = line.Substring(0, location[0]);//head;
            int i = 0;int li = 0;int len = 0;
            for(i=0;i<location.Count-1;i++)
            {
                li = location[i]+oldsubs.Length;
                len = location[i + 1] - li;
                s2 = s2 + newsubs + line.Substring(li, len);//middle
            }
            li = location[i] + oldsubs.Length;
            len = line.Length - li;
            s2 = s2 + newsubs + line.Substring(li, len);//tail;
            return s2;
        }
    }
}
