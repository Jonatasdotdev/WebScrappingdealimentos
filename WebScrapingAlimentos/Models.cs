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
    public string? Unidade { get; set; }
    public decimal ValorPor100g { get; set; }
    public string? DesvioPadrao { get; set; }
    public string? ValorMinimo { get; set; }
    public string? ValorMaximo { get; set; }
    public string? NumeroDadosUtilizados { get; set; }
    public string? Referencias { get; set; }
    public string? TipoDeDados { get; set; }
}
