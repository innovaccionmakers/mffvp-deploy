namespace Common.SharedKernel.Application.Reports.Strategies;

public enum ColumnAlignment
{
    Left,
    Right,
    Center
}

public enum PaddingSide
{
    Right,
    Left
}

public class ColumnConfiguration
{
    public int Width { get; set; }
    public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;
    public char PaddingChar { get; set; } = ' ';
    public PaddingSide PaddingSide { get; set; } = PaddingSide.Right;

    public ColumnConfiguration(int width, ColumnAlignment alignment = ColumnAlignment.Left, char paddingChar = ' ', PaddingSide paddingSide = PaddingSide.Right)
    {
        Width = width;
        Alignment = alignment;
        PaddingChar = paddingChar;
        PaddingSide = paddingSide;
    }
}

