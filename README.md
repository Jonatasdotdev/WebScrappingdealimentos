**Web Scraping de Composição de Alimentos**

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

**Tecnologias Utilizadas**

    C#
    .NET Core, .NET 5 ou .NET 6
    MySQL
    Docker
    Html Agility Pack para web scraping

**Estrutura do Projeto**

    Dockerfile: Configuração para containerização da aplicação.
    src/: Código-fonte da aplicação.
    tests/: Testes unitários (a serem implementados).

Requisitos

    Docker
    MySQL Server

Como Executar

    Clone o repositório:

    bash

git clone <URL_DO_REPOSITORIO>
cd <NOME_DO_DIRETORIO>

Inicie o contêiner do banco de dados MySQL:

bash

docker run --name mysql -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=nutrientes -p 3306:3306 -d mysql

Compile e execute a aplicação:

bash

    docker build -t web-scraping-alimentos .
    docker run --rm web-scraping-alimentos

    Verifique a conexão com o banco de dados: Assegure-se de que a aplicação está configurada para se conectar ao banco de dados MySQL.

**Contribuição**

Contribuições são bem-vindas! Sinta-se à vontade para abrir um pull request ou relatar um problema.
Licença

Este projeto está licenciado sob a MIT License.
