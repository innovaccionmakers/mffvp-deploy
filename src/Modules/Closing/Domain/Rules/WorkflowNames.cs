namespace Closing.Domain.Rules;

public static class WorkflowNames
{
    public const string Root = "Closing";

    public static class Preclosing
    {
        private const string Prefix = Root + ".PreClosing";

        public static class Simulation
        {
            private const string SubPrefix = Prefix + ".Simulation";

            public const string FirstDayBlockingValidations = SubPrefix + ".FirstDayBlockingValidations";
            public const string GeneralBlockingValidations = SubPrefix + ".GeneralBlockingValidations";
        }
    }

    public static class RunClosing
    {
        private const string Prefix = Root + ".RunClosing";

        private const string Prepare = Prefix + ".Prepare";

        public const string SecondDayBlockingValidations = Prepare + ".SecondDayBlockingValidations";
    }

    public static class Postclosing
    {
        private const string Prefix = Root + ".Postclosing";

        public const string Consolidation = Prefix + ".Consolidation";
    }
}
