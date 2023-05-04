namespace MoogleEngine;
public class BBDD
{
    public Dictionary<string, Dictionary<string, double>> Larousse; //Almacenar la relacion palabra documento
    public Dictionary<string, Dictionary<string, double>> TF_IDF; //Almacenar todas las fecuencias
    public Dictionary<string, InfoDocument> Infos;
    private char[] delimitors;

    public BBDD(string path, char[] delimitors)
    {
        Larousse = new();
        Infos = new();
        //Busca los archivos en la ruta, y las subrutas.
        string[] files = Directory.GetFiles(path, "", SearchOption.AllDirectories);
        this.delimitors = delimitors;
        foreach (var item in files)
        {
            AddFile(item);
        }
        TF_IDF = Frecuency.GetTF_IDF(Larousse, Infos);
    }
    //Añadir un nuevo archivo a la base de datos BBDD
    public void AddFile(string ficher)
    {
        string body = File.ReadAllText(ficher).ToLower();
        string[] splitbody = body.Split(delimitors, StringSplitOptions.RemoveEmptyEntries);
        double norma = 0;
        foreach (var item in splitbody)
        {
            //Si la palabra no estaba en el laurensse
            if (!Larousse.ContainsKey(item))
            {
                var docs = new Dictionary<string, double>();
                Larousse.Add(item, docs);
                docs.Add(ficher, 1);
            }
            //Si el archivo es totalmente nuevo entonces agregalo
            else if (!Larousse[item].ContainsKey(ficher))
            {
                var docs = Larousse[item];
                docs.Add(ficher, 1);
            }
            //Evidentemente esto es si la palabra aparece
            else
            {
                Larousse[item][ficher]++;
            }
        }
        List<string> temp = new List<string>();
        foreach (var item in splitbody)
        {
            if(!temp.Contains(item))
            {
                norma += Math.Pow(Larousse[item][ficher],2);
                temp.Add(item);
            }
        }
        norma = Math.Sqrt(norma);
        InfoDocument info = new InfoDocument(norma, Infos.Count, body);
        Infos.Add(ficher,info);
        return;
    }
}