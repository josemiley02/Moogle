namespace MoogleEngine;
public static class Moogle
{
    public static SearchResult Query(string query)
    {
        // Metodo Modificado para responder a la búsqueda 
        List<WordInfo> wordInfos = Operators.NewQuery(query).ToList();
        double[] scores = Scores(wordInfos);

        Dictionary<int, string> suggestion = new();
        int best = int.MaxValue;
        suggestion.Add(best, query);

        SearchItem[] items = Evaluate(wordInfos, scores).Where(x => x.Score != 0).OrderBy(x => x.Score).Reverse().Take(10).ToArray();

        for (int i = 0; i < wordInfos.Count; i++)
        {
            if (!DataServer.BaseDeDatos!.Larousse.ContainsKey(wordInfos[i].word))
            {
                foreach (var item in DataServer.BaseDeDatos!.Larousse.Keys)
                {
                    int Levi = Levenshtein.DistanciaLev(wordInfos[i].word, item);
                    if (!suggestion.ContainsKey(Levi)) { suggestion.Add(Levi, item); }
                    best = Math.Min(Levi, best);
                }
            }
        }
        return new SearchResult(items, suggestion[best]);
    }
    //Usando la formula de Similitud para calcular los Scores
    public static double[] Scores(List<WordInfo> Result)
    {
        double normaQuery = 0;
        int indexresul = 0;
        double[] score = new double[DataServer.BaseDeDatos!.Infos.Count];
        double[] nearscore = new double[DataServer.BaseDeDatos!.Infos.Count];
        foreach (var element in Result)
        {
            if (element.near != null)
            {
                nearscore = MinDistance(element.word, element.near.word);
            }
            if (!DataServer.BaseDeDatos!.TF_IDF.ContainsKey(element.word))
            {
                continue;
            }
            foreach (var item in DataServer.BaseDeDatos.TF_IDF[element.word])
            {
                var info = DataServer.BaseDeDatos.Infos[item.Key];
                score[info.index] += element.count * item.Value;
            }
        }
        foreach (var item in Result)
        {
            normaQuery += Math.Pow(item.count, 2);
        }
        normaQuery = Math.Sqrt(normaQuery);
        indexresul = 0;
        foreach (var item in Result)
        {
            foreach (var element in DataServer.BaseDeDatos.Infos.Values)
            {
                //Operador *
                score[element.index] *= Result[indexresul].asther;
                score[element.index] /= (normaQuery * element.norma);
                score[element.index] += nearscore[element.index];
            }
            indexresul++;
        }
        return score;
    }
    //Rellenar el array de SearchItem para devolver los mejores resultados de la busqueda
    public static SearchItem[] Evaluate(List<WordInfo> query, double[] scores)
    {
        SearchItem[] finalresult = new SearchItem[DataServer.BaseDeDatos!.Infos.Count];
        //Recorro todos los documentos de la BBDD
        foreach (var document in DataServer.BaseDeDatos!.Infos)
        {
            //Obtengo el tittle y el snippet de cada uno
            string tittle = string.Concat(document.Key.Reverse().TakeWhile(x => x != '\\').Reverse());
            string snippet = Snippetplus(query, document.Key);
            foreach (var item in query)
            {
                if (!item.ishere) //La palabra no puede aparecer
                {
                    if (document.Value.cuerpo.Contains(item.word))
                    {
                        //Si el documento que estoy analizando actualmente contiene la palabra, su escore se convierte en 0
                        //Asi aseguro no devolverlo en los resultados de busqueda.
                        finalresult[document.Value.index] = new SearchItem(tittle, snippet, 0);
                        scores[document.Value.index] = 0;
                        continue;
                    }
                    else
                    {
                        //Aqui si el documento no contiene la palabra, aumento su score en 1
                        //Esto lo hice para el caso en que la query estuviera compuesta por una sola palabra y esta a su vez tuviera el operador !
                        finalresult[document.Value.index] = new SearchItem(tittle, snippet, (float)scores[document.Value.index] + 1);
                        continue;
                    }
                }
                if (item.isimportant)
                {
                    if (!document.Value.cuerpo.Contains(item.word))
                    {
                        //Reduzco el Score si el documento no contiene la palabra
                        finalresult[document.Value.index] = new SearchItem(tittle, snippet, 0);
                        continue;
                    }
                    else
                    {
                        //Aumento en 1 el score si el documento la contiene
                        finalresult[document.Value.index] = new SearchItem(tittle, snippet, (float)scores[document.Value.index] + 1);
                        continue;
                    }
                }
                else
                {
                    //Si es una palabra sin Operador nada se modifica
                    finalresult[document.Value.index] = new SearchItem(tittle, snippet, (float)scores[document.Value.index]);
                    continue;
                }
            }
        }
        return finalresult;
    }
    //Calculo de la distancia entre dos palabras para el operador ~
    public static double[] MinDistance(string word1, string word2)
    {
        double best = double.MaxValue;
        double[] minimos = new double[DataServer.BaseDeDatos!.Infos.Count];
        foreach (var item in DataServer.BaseDeDatos!.Infos)
        {
            int pivote = -1;
            string[] superbody = item.Value.cuerpo.Split(DataServer.Delimitors.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            //Pregunto si el documento actual esta relacionado con las dos palabras a las cuales les calculare la cercania
            if (DataServer.BaseDeDatos.Larousse[word1].ContainsKey(item.Key) &&
            DataServer.BaseDeDatos.Larousse[word2].ContainsKey(item.Key))
            {
                for (int i = 0; i < superbody.Length; i++)
                {
                    if ((superbody[i] == word1 || superbody[i] == word2) && pivote == -1)
                    {
                        pivote = i;
                        continue;
                    }
                    if ((superbody[i] == word1 || superbody[i] == word2))
                    {
                        if (superbody[i] != superbody[pivote])
                        {
                            best = Math.Min(best, i - pivote);
                            pivote = i;
                            continue;
                        }
                        else
                        {
                            pivote = i;
                            continue;
                        }
                    }
                }
                minimos[item.Value.index] = 1 / best;
            }
        }
        return minimos;
    }
    public static string Snippetplus(List<WordInfo> query, string rutedocument)
    {
        string document = File.ReadAllText(rutedocument).ToLower();
        List<string> documentplus = document.Split(DataServer.Delimitors.ToArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
        if (documentplus.Count <= 30) { return document; }
        int cant = 0; //Cantidad de veces que una palabra de la query se repite en un snippet
        int temp = int.MinValue;
        int start = 0;
        int end = 30;
        int noesta = 0;

        (int, int) indexers = (start, end); //Tupla de 3 enteros para relacionar la importancia del snippet con los indices

        for (int i = start; i <= end; i++)
        {
            foreach (var word in query)
            {
                if (documentplus[i] == word.word) { cant++; }
            }
        }
        start++; end++;

        while (documentplus.Count - start >= 30 && noesta != query.Count)
        {
            foreach (var item in query)
            {
                if (!item.ishere) continue;

                //Pregunto si esta palabra de la query esta en la BBDD, si no, continuo
                if (!DataServer.BaseDeDatos!.Larousse.ContainsKey(item.word)) { continue; }

                //Pregunto si la palabra actual tiene asociado el documento que estoy analizando
                //De esa manera solo recorro los documentos que dicha palabra tiene asociado
                if (!DataServer.BaseDeDatos!.TF_IDF[item.word].ContainsKey(rutedocument))
                {
                    noesta++; //Indica la cantidad de palabras de la query que no tienen relacion con el documento actual
                    if (noesta == query.Count) { break; }
                    continue;
                }

                if (documentplus[start - 1] == item.word)
                {
                    cant--;
                }
                if (documentplus[end - 1] == item.word)
                {
                    cant++;
                }
            }
            if (cant > temp)
            {
                //Una vez terminada de analizar la query, verificamos si la cantidad de palabras de la query en este fragmento de Snippet
                //Es mayor a la que teniamos almacenda antes y actualizamos el valor de los indices
                indexers = (start, end);
                temp = cant;
            }
            start++;
            end++;
        }
        //Una vez obtenidos los mejores indices, concatenamos el sub-string que se encuentra entre estos
        string snippet = "";
        for (int i = indexers.Item1; i < Math.Min(indexers.Item2, documentplus.Count); i++)
        {
            snippet += documentplus[i] + " ";
        }
        return snippet;
    }
}