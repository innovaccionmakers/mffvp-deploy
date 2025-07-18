namespace Closing.Domain.Rules;

public static class WorkflowNames
{
    public const string Root = "Closing";

    public static class Preclosing
    {
        private const string Prefix = Root + ".Preclosing";

        public static class Simulation
        {
            private const string SubPrefix = Prefix + ".Simulation";

            public const string FirstDayBlockingValidations = SubPrefix + ".FirstDayBlockingValidations";
            public const string GeneralBlockingValidations = SubPrefix + ".GeneralBlockingValidations";
        }
    }

    public static class RunClosing
    {
        private const string Prefix = Root + ".Closing";

        public const string Before = Prefix + ".RunClosing.Before";
    }

    public static class Postclosing
    {
        private const string Prefix = Root + ".Postclosing";

        public const string Consolidation = Prefix + ".Consolidation";
    }
}
