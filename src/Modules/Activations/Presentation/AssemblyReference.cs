using System.Reflection;

namespace Activations.Presentation;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}