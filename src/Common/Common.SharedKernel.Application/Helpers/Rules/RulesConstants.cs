namespace Common.SharedKernel.Application.Helpers.Rules;

/// <summary>
/// Determina cuantas <see cref="RuleValidationError"/> instances se deben mostrar cuando un flujo de trabajo falla.
/// </summary>
public enum ErrorSelection
{
    First,
    All
}

/// <summary>
/// Determina si la evaluación debe detenerse en el primer flujo de trabajo que falla (circuito corto)
/// o evaluar todos los flujos de trabajo suministrados y agregar los errores.
/// </summary>
public enum WorkflowEvaluationMode
{
    ShortCircuitOnFailure,
    EvaluateAll
}
