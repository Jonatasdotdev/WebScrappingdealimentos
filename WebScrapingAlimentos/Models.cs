public class Alimento
{
    public string? Codigo { get; set; }
    public string? Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public string? Grupo { get; set; }
    public string? Marca { get; set; } 
    public List<ComponenteNutricional> Componentes { get; set; } = new List<ComponenteNutricional>();
}

public class ComponenteNutricional
{
    public string? Nome { get; set; }
    public decimal Valor { get; set; }
    public string? Unidade { get; set; }
}