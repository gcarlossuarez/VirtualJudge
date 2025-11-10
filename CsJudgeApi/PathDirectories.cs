namespace CsJudgeApi;

public class PathDirectories
{
    public static string DB_PATH = Environment.GetEnvironmentVariable("DB_PATH")
                       ?? "/home/virtualbox/VirtualJudge/CsJudgeApi/submissions.db";
    public static string PROBLEMS_PATH = Environment.GetEnvironmentVariable("PROBLEMS_PATH")
                       ?? "/home/virtualbox/VirtualJudge/problems";
    public static string UTILS_PATH = Environment.GetEnvironmentVariable("UTILS_PATH")
                         ?? "/home/virtualbox/VirtualJudge/Utilitarios";
}


