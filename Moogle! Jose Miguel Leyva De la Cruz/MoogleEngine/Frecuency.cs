namespace MoogleEngine;
public class Frecuency
{
    public static double TF_IDF(double frecuency, double totalword, double fichers = 1, double Ifrecuency = 1)
    {
        return (frecuency / totalword) * Math.Log10(fichers / Ifrecuency);
    }
    //Crear un nuevo vector Dictionario con las frecuencias de las palabras
    public static Dictionary<string, double> VectorWords(Dictionary<string, double> vector, Dictionary<string, InfoDocument> documents, double totalfichers)
    {
        Dictionary<string, double> ToReturn = new Dictionary<string, double>();
        foreach (var item in vector)
        {
            string[] full = documents[item.Key].cuerpo.Split(DataServer.Delimitors, StringSplitOptions.RemoveEmptyEntries);
            ToReturn.Add(item.Key, TF_IDF(item.Value, full.Length, totalfichers, vector.Count));
        }
        return ToReturn;
    }
    //Calcular el TF-IDF de todas las palabras
    public static Dictionary<string, Dictionary<string, double>> GetTF_IDF(Dictionary<string, Dictionary<string, double>> laurensse, Dictionary<string, InfoDocument> moda)
    {
        Dictionary<string, Dictionary<string, double>> word_tfidf = new();
        Dictionary<string, double> result = new Dictionary<string, double>();
        foreach (var item in laurensse.Keys)
        {
            result = VectorWords(laurensse[item], moda, moda.Count);
            word_tfidf.Add(item, result);
        }
        return word_tfidf;
    }
}