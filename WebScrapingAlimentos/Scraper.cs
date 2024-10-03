using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Scraper
{
    private string connectionString = "server=localhost;database=db_composicao_alimentos;user=Jonatas;password=Jonatasariel";

    public async Task ExtrairDadosAsync()
    {
        var urlAlimentos = "https://www.tbca.net.br/base-dados/composicao_estatistica.php?pagina=1&atuald=1#";
        var web = new HtmlWeb();

        // Extrair dados dos alimentos
        var docAlimentos = await web.LoadFromWebAsync(urlAlimentos);
        var alimentos = docAlimentos.DocumentNode.SelectNodes("//table[@class='table table-striped']//tbody//tr");

        if (alimentos == null || alimentos.Count == 0)
        {
            Console.WriteLine("Nenhum dado encontrado na tabela de alimentos.");
            return;
        }

        Console.WriteLine($"Foram encontrados {alimentos.Count} alimentos.");

        foreach (var row in alimentos)
        {
            var codigoNode = row.SelectSingleNode(".//td[1]//a");
            var nomeNode = row.SelectSingleNode(".//td[2]");
            var nomeCientificoNode = row.SelectSingleNode(".//td[3]");
            var grupoNode = row.SelectSingleNode(".//td[4]");
            var marcaNode = row.SelectSingleNode(".//td[5]");

            if (codigoNode == null || nomeNode == null || nomeCientificoNode == null || grupoNode == null || marcaNode == null)
            {
                Console.WriteLine("Um ou mais nós estão ausentes. Verifique a estrutura da página de alimentos.");
                continue;
            }

            var alimento = new Alimento
            {
                Codigo = codigoNode.InnerText.Trim(),
                Nome = nomeNode.InnerText.Trim(),
                NomeCientifico = nomeCientificoNode.InnerText.Trim(),
                Grupo = grupoNode.InnerText.Trim(),
                Marca = marcaNode.InnerText.Trim()
            };

            // Extraindo componentes nutricionais para o alimento usando Selenium
            var linkComposicao = "https://www.tbca.net.br/base-dados/" + codigoNode.GetAttributeValue("href", string.Empty);
            var componentes = await ExtrairComponentesNutricionaisAsync(linkComposicao);
            alimento.Componentes = componentes;

            // Armazenar no banco
            SalvarAlimentoNoBanco(alimento);
        }

        Console.WriteLine("Dados dos alimentos e componentes salvos no banco de dados com sucesso.");
    }

    // Função para extrair componentes nutricionais usando Selenium
private async Task<List<ComponenteNutricional>> ExtrairComponentesNutricionaisAsync(string urlComposicao)
{
    var componentes = new List<ComponenteNutricional>();

    using (var driver = new ChromeDriver())
    {
        driver.Navigate().GoToUrl(urlComposicao);

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));

        try
        {
            // Espera até que o contêiner da tabela esteja visível
            wait.Until(d => d.FindElement(By.Id("tabela1_wrapper")).Displayed);

            // Agora espera até que as linhas da tabela estejam disponíveis
            await wait.Until(d => Task.Run(() => d.FindElements(By.CssSelector("#tabela1 tbody tr")).Count > 0));

            // Coleta dados dos componentes nutricionais
            var componentesNodes = driver.FindElements(By.CssSelector("#tabela1 tbody tr"));

            foreach (var componenteRow in componentesNodes)
            {
                var nomeNode = componenteRow.FindElement(By.CssSelector("td:nth-child(1)"));
                var unidadeNode = componenteRow.FindElement(By.CssSelector("td:nth-child(2)"));
                var valorNode = componenteRow.FindElement(By.CssSelector("td:nth-child(3)"));
                var desvioPadraoNode = componenteRow.FindElement(By.CssSelector("td:nth-child(4)"));
                var valorMinimoNode = componenteRow.FindElement(By.CssSelector("td:nth-child(5)"));
                var valorMaximoNode = componenteRow.FindElement(By.CssSelector("td:nth-child(6)"));
                var numeroDadosNode = componenteRow.FindElement(By.CssSelector("td:nth-child(7)"));
                var referenciasNode = componenteRow.FindElement(By.CssSelector("td:nth-child(8)"));
                var tipoDadosNode = componenteRow.FindElement(By.CssSelector("td:nth-child(9)"));

                decimal valorPor100g = 0; // Valor padrão em caso de erro
                string valorText = valorNode.Text.Trim();

                // Verifica se o valor não é vazio antes de tentar converter
                if (!string.IsNullOrWhiteSpace(valorText) && !valorText.Equals("-"))
                {
                    decimal.TryParse(valorText.Replace('.', ','), out valorPor100g); // Ajustar conforme necessário
                }

                // Verifica se o numero de dados utilizados é "-" e define como null se for
                string numeroDadosUtilizados = numeroDadosNode.Text.Trim();
                if (numeroDadosUtilizados.Equals("-"))
                {
                    numeroDadosUtilizados = null; // ou use string.Empty, dependendo do que você prefere
                }

                var componente = new ComponenteNutricional
                {
                    Nome = nomeNode.Text.Trim(),
                    Unidade = unidadeNode.Text.Trim(),
                    ValorPor100g = valorPor100g, // Usar o valor convertido
                    DesvioPadrao = desvioPadraoNode.Text.Trim(),
                    ValorMinimo = valorMinimoNode.Text.Trim(),
                    ValorMaximo = valorMaximoNode.Text.Trim(),
                    NumeroDadosUtilizados = numeroDadosUtilizados, // Aqui passamos o valor ajustado
                    Referencias = referenciasNode.Text.Trim(),
                    TipoDeDados = tipoDadosNode.Text.Trim()
                };

                componentes.Add(componente);
            }
        }
        catch (WebDriverTimeoutException)
        {
            Console.WriteLine("Tabela não encontrada ou não preenchida. Verifique se a página carregou corretamente.");
        }

        driver.Quit();
    }

    return componentes;
}

private void SalvarAlimentoNoBanco(Alimento alimento)
{
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        var cmdAlimento = connection.CreateCommand();
        cmdAlimento.CommandText = "INSERT INTO alimentos (codigo, nome, nome_cientifico, grupo, marca) VALUES (@codigo, @nome, @nome_cientifico, @grupo, @marca)";
        cmdAlimento.Parameters.AddWithValue("@codigo", alimento.Codigo);
        cmdAlimento.Parameters.AddWithValue("@nome", alimento.Nome);
        cmdAlimento.Parameters.AddWithValue("@nome_cientifico", alimento.NomeCientifico);
        cmdAlimento.Parameters.AddWithValue("@grupo", alimento.Grupo);
        cmdAlimento.Parameters.AddWithValue("@marca", alimento.Marca);
        cmdAlimento.ExecuteNonQuery();

        foreach (var componente in alimento.Componentes)
        {
            var cmdComponente = connection.CreateCommand();
            cmdComponente.CommandText = @"INSERT INTO componentes_nutricionais 
                (alimento_codigo, nome, unidade, valor_por_100g, desvio_padrao, valor_minimo, valor_maximo, numero_dados_utilizados, referencias, tipo_de_dados) 
                VALUES (@codigo, @nome, @unidade, @valor_por_100g, @desvio_padrao, @valor_minimo, @valor_maximo, @numero_dados_utilizados, @referencias, @tipo_de_dados)";

            cmdComponente.Parameters.AddWithValue("@codigo", alimento.Codigo);
            cmdComponente.Parameters.AddWithValue("@nome", componente.Nome);
            cmdComponente.Parameters.AddWithValue("@unidade", componente.Unidade);
            cmdComponente.Parameters.AddWithValue("@valor_por_100g", componente.ValorPor100g);
            cmdComponente.Parameters.AddWithValue("@desvio_padrao", componente.DesvioPadrao);
            cmdComponente.Parameters.AddWithValue("@valor_minimo", componente.ValorMinimo);
            cmdComponente.Parameters.AddWithValue("@valor_maximo", componente.ValorMaximo);
            cmdComponente.Parameters.AddWithValue("@numero_dados_utilizados", (object)componente.NumeroDadosUtilizados ?? DBNull.Value); // Passa null para o banco se necessário
            cmdComponente.Parameters.AddWithValue("@referencias", componente.Referencias);
            cmdComponente.Parameters.AddWithValue("@tipo_de_dados", componente.TipoDeDados);
            cmdComponente.ExecuteNonQuery();
        }
    }
}}
