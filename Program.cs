using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Assistants;
using static System.Environment;


string endpoint = GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", EnvironmentVariableTarget.User) ?? "";
string key = GetEnvironmentVariable("AZURE_OPENAI_API_KEY", EnvironmentVariableTarget.User) ?? "";
const string deploymentName = "EstudosIAGPT4";

AzureOpenAIClient azureClient = new(
           new Uri(endpoint),
           new AzureKeyCredential(key));
AssistantClient assistantClient = azureClient.GetAssistantClient();
Assistant assistant = await assistantClient.CreateAssistantAsync(
            model: deploymentName,
            new AssistantCreationOptions()
            {
                Name = "Programador do Setembro Amarelo",
                Instructions = "Você será o apoio emocional de um programador. Quando o programador reclamar, você irá confortá-lo e dizer que tudo vai ficar bem no final.",
                Tools = { ToolDefinition.CreateCodeInterpreter() },
            });

ThreadInitializationMessage initialMessage = new(
    MessageRole.User,
    [
        "Olá, assistente! Meu código nunca funciona, estou muito triste."
    ]);
AssistantThread thread = await assistantClient.CreateThreadAsync(new ThreadCreationOptions()
{
    InitialMessages = { initialMessage },
});
await foreach (StreamingUpdate streamingUpdate
    in assistantClient.CreateRunStreamingAsync(thread, assistant))
{
    if (streamingUpdate.UpdateKind == StreamingUpdateReason.RunCreated)
    {
        Console.WriteLine($"--- Run started! ---");
    }
}