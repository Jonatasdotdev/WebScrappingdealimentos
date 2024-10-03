# Web Scraping de Composição de Alimentos

Este projeto realiza web scraping de informações sobre a composição de alimentos da base de dados do TBCA (Tabela Brasileira de Composição de Alimentos). As informações coletadas são armazenadas em um banco de dados MySQL.
Funcionalidades

    Extração de dados de alimentos, incluindo:
        Código
        Nome
        Nome Científico
        Grupo
        Componentes Nutricionais
    Armazenamento dos dados extraídos em um banco de dados MySQL.
    Containerização da aplicação utilizando Docker.
    Implementação de boas práticas de programação e documentação.

## Tecnologias Utilizadas

    C#
    .NET Core, .NET 5 ou .NET 6
    MySQL
    Docker
    Html Agility Pack para web scraping

## Estrutura do Projeto

    Dockerfile: Configuração para containerização da aplicação.
    WebScrapingAlimentos: Código-fonte da aplicação.
    AlimentosScraper.Tests: Testes unitários 

Requisitos

    Docker
    MySQL Server

Como Executar

Clone o repositório

Inicie o contêiner do banco de dados MySQL:

bash

docker run --name mysql -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=nutrientes -p 3306:3306 -d mysql

Compile e execute a aplicação:

bash

    docker build -t web-scraping-alimentos .
    docker run --rm web-scraping-alimentos

    Verifique a conexão com o banco de dados: Assegure-se de que a aplicação está configurada para se conectar ao banco de dados MySQL.


# Imagens
![Screenshot (616)](https://github.com/user-attachments/assets/0721aa56-d86c-424d-bdfd-db6dd2def417)
![Screenshot (619)](https://github.com/user-attachments/assets/3ea21365-1a99-476c-8a16-b3774cb880aa)

![Screenshot (623)](https://github.com/user-attachments/assets/7e344931-1427-442a-bece-2e29b033883d)


## Contribuição**

Contribuições são bem-vindas! Sinta-se à vontade para abrir um pull request ou relatar um problema.
Licença

Este projeto está licenciado sob a MIT License.
