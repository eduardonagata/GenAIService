

using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GenAIService.Plugins;

public class TimePlugin
{
    [KernelFunction("get_current_time")]
    [Description("Fornece a data e hora atuais no UTC no formato ISO 8601.")]
    public string GetCurrentTime()
    {
        return DateTimeOffset.UtcNow.ToString("o");
    }

    [KernelFunction("convert_utc_to_local_iso_8601")]
    [Description("Converte uma string de data e hora no UTC no formato ISO 8601 para uma string do horário local também no formato ISO 8601.")] 
    public static string ConvertUtcToLocalISO8601(string utcTime)
    {
        // Tenta fazer o parsing da string para um DateTimeOffset
        if (DateTimeOffset.TryParse(utcTime, out DateTimeOffset utcDateTimeOffset))
        {
            // Converte para o horário local do sistema
            DateTime localDateTime = utcDateTimeOffset.LocalDateTime;

            // Retorna no formato ISO 8601
            return localDateTime.ToString("o"); // "o" garante o formato ISO 8601
        }
        else
        {
            throw new ArgumentException("Formato de data inválido. Certifique-se de que está no padrão ISO 8601.");
        }
    }
}