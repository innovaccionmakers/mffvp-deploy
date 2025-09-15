namespace Reports.Domain.TransmissionFormat.Layout;

public static class TransmissionFormatLayout
{
    public static class Rt4
    {
        public const string R4312 = "4312"; // Unit Value 312
        public const string R4313 = "4313"; // Movements 313
        public const string R4314 = "4314"; // Profitabilities 314

        // 4313 subcodes
        public const string PreviousUnits = "0101005";
        public const string PreviousAmount = "0201005";
        public const string YieldAmount = "0202005";
        public const string PreClosingAmount = "0202010";

        public const string ContributionUnits = "0103005";
        public const string ContributionAmount = "0203005";

        public const string TransferUnits = "0103010";
        public const string TransferAmount = "0203010";

        public const string PensionUnits = "0103015";
        public const string PensionAmount = "0203015";

        public const string WithdrawalUnits = "0103020";
        public const string WithdrawalAmount = "0203020";

        public const string OtherCommissionUnits = "0103025";
        public const string OtherCommissionAmount = "0203025";

        public const string VitalityTransferUnits = "0103030";
        public const string VitalityTransferAmount = "0203030";

        public const string OtherWithdrawalUnits = "0103035";
        public const string OtherWithdrawalAmount = "0203035";

        public const string CancellationUnits = "0103040";
        public const string CancellationAmount = "0203040";

        public const string CurrentUnits = "0103045";
        public const string CurrentAmount = "0203045";

        // 4312 subcodes
        public const string UnitValue = "0101011";

        // 4314 subcodes
        public const string Return30Days = "0101005";
        public const string Return180Days = "0101010";
        public const string Return365Days = "0101015";
    }
}

