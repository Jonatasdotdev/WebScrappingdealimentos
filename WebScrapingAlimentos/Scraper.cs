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
            var codigoNode = row.SelectSingleNode(".//td[1]");
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
            var componentes = await ExtrairComponentesNutricionaisAsync(alimento.Codigo);
            alimento.Componentes = componentes;

            // Armazenar no banco
            SalvarAlimentoNoBanco(alimento);
        }

        Console.WriteLine("Dados dos alimentos e componentes salvos no banco de dados com sucesso.");
    }

    // Função para extrair componentes nutricionais usando Selenium
    private async Task<List<ComponenteNutricional>> ExtrairComponentesNutricionaisAsync(string codigo)
    {
        var componentes = new List<ComponenteNutricional>();

        // Usar Selenium para buscar os componentes nutricionais
        using (var driver = new ChromeDriver())
        {
            driver.Navigate().GoToUrl("https://www.tbca.net.br/base-dados/busca_componente.php"); // URL do filtro

            // Espera que os elementos estejam visíveis
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Seleciona o componente
            var componenteSelect = wait.Until(d => d.FindElement(By.Id("cmb_componente")));
            var selectComponente = new SelectElement(componenteSelect);
            selectComponente.SelectByText("energia (kj)"); // Ajuste conforme necessário

            // Seleciona o grupo
            var grupoSelect = wait.Until(d => d.FindElement(By.Id("cmb_grupo")));
            var selectGrupo = new SelectElement(grupoSelect);
            selectGrupo.SelectByText("açúcares e doces"); // Ajuste conforme necessário

            // Seleciona o tipo de alimento
            var tipoAlimentoSelect = wait.Until(d => d.FindElement(By.Id("cmb_tipo_alimento")));
            var selectTipoAlimento = new SelectElement(tipoAlimentoSelect);
            selectTipoAlimento.SelectByText("alimento in natura"); // Ajuste conforme necessário

            // Clica no botão de busca
            var buscarButton = wait.Until(d => d.FindElement(By.Id("submit")));
            buscarButton.Click();

            // Espera a página de resultados carregar
            wait.Until(d => d.Title.Contains("Resultados")); // Ajuste conforme necessário

            // Coleta dados dos componentes nutricionais
            var componentesNodes = driver.FindElements(By.CssSelector("SELETOR_DOS_COMPONENTES")); // Ajuste o seletor conforme necessário

            foreach (var componenteRow in componentesNodes)
            {
                var nomeComponenteNode = componenteRow.SelectSingleNode(".//td[1]");
                var valorNode = componenteRow.SelectSingleNode(".//td[2]");
                var unidadeNode = componenteRow.SelectSingleNode(".//td[3]");

                if (nomeComponenteNode != null && valorNode != null && unidadeNode != null)
                {
                    var componente = new ComponenteNutricional
                    {
                        Nome = nomeComponenteNode.InnerText.Trim(),
                        Valor = decimal.Parse(valorNode.InnerText.Trim().Replace('.', ',')), // Ajustar conforme necessário
                        Unidade = unidadeNode.InnerText.Trim()
                    };

                    componentes.Add(componente);
                }
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
                cmdComponente.CommandText = "INSERT INTO componentes_nutricionais (alimento_codigo, nome, valor, unidade) VALUES (@codigo, @nome, @valor, @unidade)";
                cmdComponente.Parameters.AddWithValue("@codigo", alimento.Codigo);
                cmdComponente.Parameters.AddWithValue("@nome", componente.Nome);
                cmdComponente.Parameters.AddWithValue("@valor", componente.Valor);
                cmdComponente.Parameters.AddWithValue("@unidade", componente.Unidade);
                cmdComponente.ExecuteNonQuery();
            }
        }
    }
}
