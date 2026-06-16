using System.Text.Json.Serialization;

namespace FinanceTracker.Infrastructure.Fx;

internal sealed class NbrbRateDto
{
    [JsonPropertyName("Cur_Abbreviation")]
    public string CurAbbreviation { get; set; } = string.Empty;

    [JsonPropertyName("Cur_Scale")]
    public int CurScale { get; set; }

    [JsonPropertyName("Cur_OfficialRate")]
    public decimal CurOfficialRate { get; set; }

    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }
}
