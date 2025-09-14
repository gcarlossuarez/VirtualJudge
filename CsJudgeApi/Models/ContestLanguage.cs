namespace CsJudgeApi.Models;

public class ContestLanguage
{
    public int ContestId { get; set; }
    public string Language { get; set; } = "";

    public Contest Contest { get; set; } = null!;
}