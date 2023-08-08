using CanvasRendering.Contracts.Controls;

namespace CanvasRendering.Controls;

public class UniformGrid
{
    public int Width { get; set; }

    public int Height { get; set; }

    public uint Columns { get; set; }

    public uint Rows { get; set; }

    public List<BaseControl> Child { get; } = new List<BaseControl>();

    public void Render()
    {
        int w = Width / (int)Columns;
        int h = Height / (int)Rows;

        int c = 0, r = 0;
        foreach (BaseControl control in Child)
        {
            control.Left = w * c + 2;
            control.Top = h * r + 2;
            control.Width = (uint)w - 4;
            control.Height = (uint)h - 4;

            c++;
            if (c == Columns)
            {
                c = 0;
                r++;
            }
        }
    }
}
