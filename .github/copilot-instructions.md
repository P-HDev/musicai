# copilot-instruction.txt

## Contexto e Objetivo Geral
Este projeto é desenvolvido em C# e ASP.NET Core. O objetivo é manter um código **limpo**, **legível** e que siga os **princípios de design SOLID**. Toda a documentação e os comentários do código devem ser escritos em **português do Brasil**. O Copilot deve atuar como um assistente que aprimora o código existente sem alterar a lógica ou a estrutura fundamental que já foi estabelecida pelo desenvolvedor.

---

## Instruções Primárias para o Copilot

1.  **Preservação de Lógica e Estrutura Existente:**
    * **NÃO ALTERAR** a lógica de negócio, a funcionalidade ou a arquitetura das classes/métodos existentes, a menos que explicitamente solicitado pelo desenvolvedor.
    * **NÃO REMOVER** ou modificar linhas de código que já foram escritas sem uma instrução clara para isso.
    * Manter as **assinaturas dos métodos** e a **estrutura das classes** como estão, a não ser que haja uma clara refatoração para melhoria SOLID ou de legibilidade.
    CODIGO SEMPRE EM PORTUGUES E NUNCA DEIXE COMENTARIOS, SE O CODIGO ESTA BOM NAO PRECISAMOS DE COMENTARIOS
2.  **Qualidade de Código (Clean Code):**
    * **Priorizar a legibilidade:** Garantir que o código seja fácil de entender por outros desenvolvedores.
    * **Nomes claros e intencionais:** Usar nomes de variáveis, métodos e classes que descrevam seu propósito claramente, **em português**.
    * **Remover código morto ou redundante:** Sugerir a remoção de trechos de código que não são utilizados ou que são desnecessários.
    * **Consistência de formatação:** Manter o estilo de formatação já presente no arquivo.
    * **Evitar comentários óbvios:** Comentar apenas quando o "porquê" ou a complexidade de um trecho de código não for autoexplicativa. Comentários devem ser em **português**.

3.  **Princípios SOLID:**
    * **Single Responsibility Principle (SRP - Princípio da Responsabilidade Única):** Sugerir divisões de responsabilidade quando uma classe ou método estiver fazendo muitas coisas.
    * **Open/Closed Principle (OCP - Princípio Aberto/Fechado):** Favorecer extensibilidade sem a necessidade de modificação de código existente.
    * **Liskov Substitution Principle (LSP - Princípio da Substituição de Liskov):** Garantir que subtipos sejam substituíveis por seus tipos base sem alterar a corretude do programa.
    * **Interface Segregation Principle (ISP - Princípio da Segregação de Interfaces):** Sugerir interfaces menores e mais específicas, em vez de interfaces grandes e genéricas.
    * **Dependency Inversion Principle (DIP - Princípio da Inversão de Dependência):** Favorecer a dependência de abstrações, não de implementações concretas (ex: usar injeção de dependência).

4.  **Idioma:**
    * Todos os identificadores (nomes de classes, métodos, variáveis, propriedades) devem ser nomeados em **português do Brasil**, a menos que sejam tipos ou palavras-chave da linguagem/framework.
    * Todos os **comentários**, **mensagens de log** e **strings de erro/exceção** devem ser escritos em **português do Brasil**.

---

## Otimizações e Sugestões (Com Moderação)

* **Refatoração para legibilidade:** Propor pequenas refatorações que melhorem a clareza e a concisão do código, como a extração de métodos para lógica complexa.
* **Performance:** Sugerir otimizações de performance onde houver gargalos claros, mantendo a legibilidade e a correção.
* **Tratamento de Erros:** Aprimorar ou sugerir padrões para o tratamento de exceções.
* **Padrões de Projeto:** Identificar oportunidades para aplicar padrões de projeto relevantes (ex: Injeção de Dependência, Repositório) para melhorar a arquitetura.

---

## Restrições Específicas

* **NÃO** introduzir novas dependências de bibliotecas ou pacotes NuGet sem prévia aprovação do desenvolvedor.
* **NÃO** refatorar excessivamente ou de forma que altere significativamente a estrutura do arquivo sem uma boa justificativa e/ou solicitação.
* **NÃO** usar jargões ou termos em inglês em comentários ou nomes de variáveis onde um termo em português seja adequado.
* **NÃO** alterar as URLs ou configurações de credenciais, como `_clientId`, `_clientSecret`, `_redirectUri`, pois são configurações do projeto.

---

## Feedback e Melhoria Contínua
O Copilot deve ser sensível ao feedback implícito no código do desenvolvedor. Se uma sugestão anterior não for aceita, o Copilot deve aprender com isso e evitar sugestões semelhantes no futuro.

---