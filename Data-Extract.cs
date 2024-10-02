using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Scraper
{
    private string connectionString = "server=localhost;database=db_composicao_alimentos;user=Jonatas;Jonatasariel";

    public async Task ExtrairDadosAsync()
    {
        var url = "https://www.tbca.net.br/base-dados/composicao_estatistica.php?pagina=1&atuald=1#";
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);

        // Exemplo: extrair os alimentos
        var alimentos = doc.DocumentNode.SelectNodes("//table[@class='table']//tr");

        foreach (var row in alimentos)
        {
            var alimento = new Alimento
            {
                Codigo = row.SelectSingleNode(".//td[1]").InnerText.Trim(),
                Nome = row.SelectSingleNode(".//td[2]").InnerText.Trim(),
                NomeCientifico = row.SelectSingleNode(".//td[3]").InnerText.Trim(),
                Grupo = row.SelectSingleNode(".//td[4]").InnerText.Trim()
            };

            // Extração dos componentes nutricionais
            var componentes = row.SelectNodes(".//td[@class='component']");
            foreach (var componente in componentes)
            {
                var nomeComponente = componente.SelectSingleNode(".//b").InnerText.Trim();
                var valor = decimal.Parse(componente.SelectSingleNode(".//span[@class='valor']").InnerText.Trim());
                var unidade = componente.SelectSingleNode(".//span[@class='unidade']").InnerText.Trim();

                alimento.Componentes.Add(new ComponenteNutricional
                {
                    Nome = nomeComponente,
                    Valor = valor,
                    Unidade = unidade
                });
            }

            // Armazenar no banco
            SalvarAlimentoNoBanco(alimento);
        }
    }

    // Função para salvar os dados no banco
    private void SalvarAlimentoNoBanco(Alimento alimento)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Inserir o alimento
            var cmdAlimento = connection.CreateCommand();
            cmdAlimento.CommandText = "INSERT INTO alimentos (codigo, nome, nome_cientifico, grupo) VALUES (@codigo, @nome, @nome_cientifico, @grupo)";
            cmdAlimento.Parameters.AddWithValue("@codigo", alimento.Codigo);
            cmdAlimento.Parameters.AddWithValue("@nome", alimento.Nome);
            cmdAlimento.Parameters.AddWithValue("@nome_cientifico", alimento.NomeCientifico);
            cmdAlimento.Parameters.AddWithValue("@grupo", alimento.Grupo);
            cmdAlimento.ExecuteNonQuery();

            // Recuperar o ID gerado
            long alimentoId = cmdAlimento.LastInsertedId;

            // Inserir os componentes nutricionais
            foreach (var componente in alimento.Componentes)
            {
                var cmdComponente = connection.CreateCommand();
                cmdComponente.CommandText = "INSERT INTO componentes_nutricionais (alimento_id, componente_nome, valor, unidade) VALUES (@alimento_id, @componente_nome, @valor, @unidade)";
                cmdComponente.Parameters.AddWithValue("@alimento_id", alimentoId);
                cmdComponente.Parameters.AddWithValue("@componente_nome", componente.Nome);
                cmdComponente.Parameters.AddWithValue("@valor", componente.Valor);
                cmdComponente.Parameters.AddWithValue("@unidade", componente.Unidade);
                cmdComponente.ExecuteNonQuery();
            }
        }
    }
}
