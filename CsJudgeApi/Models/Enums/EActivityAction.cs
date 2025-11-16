namespace CsJudgeApi.Models.Enums;

public enum EActivityAction
{
    ContestLoaded = 1,
    ProblemViewed = 2,
    SubmissionSent = 3,
    LocalValidation = 4,
    DatasetSynced = 5,
    SdkDownloaded = 6,
    ContestCreated = 7,
    ContestEdited = 8,
    StudentAdded = 9,
    StudentRemoved = 10,
    StudentLogin = 11,  // Cada vez que un estudiante entra al juez
}
