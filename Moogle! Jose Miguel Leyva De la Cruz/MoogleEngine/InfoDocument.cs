namespace MoogleEngine;
public class InfoDocument
{
    //Guardar la informacion de todos los documentos
    public InfoDocument(double norma, int index, string cuerpo)
    {
        this.norma = norma;
        this.index = index;
        this.cuerpo = cuerpo;
    }

    public int index { get; set; }
    public double norma { get; set; }
    public string cuerpo { get; set; }
}