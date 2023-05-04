using System.Collections.Generic;

namespace MoogleEngine;
public class Operators
{
    public static string operators = "!*^~";
    //Crear una nueva query, teniendo en cuenta la existencia de operadores dentro de la misma
    public static WordInfo[] NewQuery(string query)
    {
        string[] spliter = query.ToLower().Split(DataServer.Delimitors.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        WordInfo[] result = new WordInfo[spliter.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new WordInfo(" ");
        }

        for (int i = 0; i < spliter.Length; i++)
        {
            if (!result.Any(x => x.word == spliter[i]))
            {
                WordInfo NEW = new WordInfo(spliter[i]);
                if (spliter[i][0] == '!')
                {
                    NEW.ishere = false;
                    string[] newfake = spliter[i].Split(operators.ToArray());
                    NEW.word = newfake[newfake.Length - 1];
                    result[i] = NEW;
                    continue;
                }
                int index = 0;
                if (spliter[i][index] == '~')
                {
                    NEW.near = result[i - 1];
                    index++;
                
                }
                if (spliter[i][index] == '^')
                {
                    NEW.isimportant = true;
                    index++;
                }
                NEW.asther = spliter[i].Where(actual => actual > index).TakeWhile(x => x == '*').Count() + 1;
                string[] clearing = spliter[i].Split(operators.ToArray());
                NEW.word = clearing[clearing.Length - 1];
                result[i] = NEW;
                continue;
            }
            for (int j = 0; j < result.Length; j++)
            {
                if (result[j].word == spliter[i])
                {
                    result[j].count++;
                    break;
                }
            }
        }
        return result;
    }
}



//Esta clase guarda la informacion de una palabra segun los operadores que posea
public class WordInfo
{
    public string word;
    public bool ishere;
    public bool isimportant;
    public int asther;
    public WordInfo? near;
    public int count;
    public WordInfo(string word, bool ishere = true, bool isimportant = false, int asther = 0, WordInfo? near = null, int count = 1)
    {
        this.word = word;
        this.ishere = ishere;
        this.isimportant = isimportant;
        this.asther = asther + 1;
        this.near = near;
        this.count = count;
    }
}