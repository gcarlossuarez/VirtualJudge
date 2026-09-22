using System;

namespace CsJudgeApi.Models;

/// <summary>
/// Relación M:N entre Contest y Question.
/// Permite reutilizar preguntas en múltiples contests y semestres.
/// </summary>
public class ContestQuestion
{
    public int ContestId { get; set; }
    public int QuestionId { get; set; }
    
    /// <summary>
    /// Orden de aparición de la pregunta en el contest.
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Indica si está permitido copiar, cortar, pegar y arrastrar código en esta pregunta.
    /// Por defecto: true (permitido)
    /// </summary>
    public bool AllowCopyPaste { get; set; } = true;
    
    // Navegación
    public Contest? Contest { get; set; }
    public Question? Question { get; set; }
}
