namespace Common.SharedKernel.Application.Reports.Strategies;

public enum ColumnAlignment
{
    Left,
    Right,
    Center
}

public class ColumnConfiguration
{
    public int Width { get; set; }
    public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;
    public char PaddingChar { get; set; } = ' ';

    public ColumnConfiguration(int width, ColumnAlignment alignment = ColumnAlignment.Left, char paddingChar = ' ')
    {
        Width = width;
        Alignment = alignment;
        PaddingChar = paddingChar;
    }
}

