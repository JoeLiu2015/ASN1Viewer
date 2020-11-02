using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ASN1Viewer
{
  public class HexViewer : Control {
    private const int DEF_BYTES = 1024;
    private const int DEF_X_COUNT = 16;

    private byte[] m_Data = new byte[DEF_BYTES];
    // Drawing Data
    private bool m_RefreshDrawingData = true;
    private int[] m_XPos = new int[DEF_X_COUNT];
    private int[] m_YPos = new int[DEF_BYTES/DEF_X_COUNT];
    private int m_XText = 0;
    private int m_TextMargin = 20;
    private int m_TextWidth = -1;
    private Rectangle m_HexRect = Rectangle.Empty;
    private Rectangle m_TextRect = Rectangle.Empty;
    private Size m_ByteSize = System.Drawing.Size.Empty;

    public HexViewer() {
      this.BackColor = SystemColors.Window;
    }
    public byte[] GetData() {
      return m_Data; 
    }

    public void SetData(byte[] val) {
      m_Data = val;
      m_RefreshDrawingData = true;
      this.Invalidate();
    }

    protected override void OnFontChanged(EventArgs e) {
      m_RefreshDrawingData = true;
      base.OnFontChanged(e); 
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      if (m_RefreshDrawingData) {
        m_RefreshDrawingData = false;
        RefreshDrawingData(e.Graphics);
      }
      Draw(e.Graphics, e.ClipRectangle);
    }
    private void RefreshDrawingData(Graphics g) {
      int xCount = m_XPos.Length;
      int yCount = (int)Math.Ceiling((double) m_Data.Length/xCount);
      if (m_ByteSize.IsEmpty) m_ByteSize = Size.Ceiling( g.MeasureString("00", this.Font));
      int w = m_ByteSize.Width + 4;
      int h = m_ByteSize.Height + 4;
      for (int i = 0; i < xCount; i++) m_XPos[i] = this.Margin.Left + i * w;
      for (int i = 0; i < yCount; i++) m_YPos[i] = this.Margin.Top + i * h;
      m_XText = m_XPos[xCount - 1] + w + m_TextMargin;
      if (m_TextWidth < 0) m_TextWidth = (int)Math.Ceiling(g.MeasureString("0123456789ABCDEF", this.Font).Width);
      m_HexRect = new Rectangle(this.Margin.Left, this.Margin.Top, w * xCount, h * yCount);
      m_TextRect = new Rectangle(this.m_XText, this.Margin.Top, m_TextWidth, h * yCount);
      this.Size = new Size(Margin.Left + Margin.Right + w * xCount + m_TextMargin + m_TextWidth,
        Margin.Top + Margin.Bottom + h * yCount
        );
    }

    private void Draw(Graphics g, Rectangle rect) {
      NativeTextRenderer ng = new NativeTextRenderer(g);
      Rectangle hexRect = Rectangle.Intersect(m_HexRect, rect);
      Rectangle txtRect = Rectangle.Intersect(m_TextRect, rect);
      int top = -1, left = -1, right = -1, bottom = -1;
      if (!hexRect.IsEmpty) {
        for (int i = 0; i < m_XPos.Length; i++) {
          if (m_XPos[i] <= hexRect.Left && (i == m_XPos.Length - 1 || m_XPos[i + 1] > hexRect.Left)) {
            left = i;
            break;
          }
        }
        for (int i = left; i < m_XPos.Length; i++) {
          if (m_XPos[i] <= hexRect.Right && (i == m_XPos.Length - 1 || m_XPos[i + 1] > hexRect.Right)) {
            right = i;
            break;
          }
        }
        for (int i = 0; i < m_YPos.Length; i++) {
          if (m_YPos[i] <= hexRect.Top && (i == m_YPos.Length - 1 || m_YPos[i + 1] > hexRect.Top)) {
            top = i;
            break;
          }
        }
        for (int i = top; i < m_YPos.Length; i++) {
          if (m_YPos[i] <= hexRect.Bottom && (i == m_YPos.Length - 1 || m_YPos[i + 1] > hexRect.Bottom)) {
            bottom = i;
            break;
          }
        }

        using (Brush b = new SolidBrush(this.ForeColor)) {
          for (int i = left; i <= right; i++) {
            for (int j = top; j <= bottom; j++) {
              int x = m_XPos[i] + 2, y = m_YPos[j] + 2, idx = j == 0 ? i : (j - 1)*m_XPos.Length + i;
              if (idx < m_Data.Length) {
                String s = m_Data[idx].ToString("X2");
                //g.DrawString(s, this.Font, b, x, y);
                ng.DrawString(s, this.Font, this.ForeColor, new Point(x, y));
              }
            }
          }
        }

      }
      if (!txtRect.IsEmpty) {
        top = left = right = bottom = -1;
        for (int i = 0; i < m_YPos.Length; i++)
        {
          if (m_YPos[i] <= txtRect.Top && (i == m_YPos.Length - 1 || m_YPos[i + 1] > txtRect.Top))
          {
            top = i;
            break;
          }
        }
        for (int i = top; i < m_YPos.Length; i++)
        {
          if (m_YPos[i] <= txtRect.Bottom && (i == m_YPos.Length - 1 || m_YPos[i + 1] > txtRect.Bottom))
          {
            bottom = i;
            break;
          }
        }

        using (Brush b1 = new SolidBrush(this.ForeColor))
        {
            for (int j = top; j <= bottom; j++)
            {
              int x = m_XText, y = m_YPos[j] + 2, idx = j == 0 ? 0 : (j - 1) * m_XPos.Length;
              String s = "";
              for (int i = 0; i < m_XPos.Length && idx + i < m_Data.Length; i++) {
                byte b = m_Data[idx + i];
                s += (b < 0x20 || b > 0x7E) ? '.' : (char)b;
              }
              //g.DrawString(s, this.Font, b1, x, y);
              ng.DrawString(s, this.Font, this.ForeColor, new Point(x, y));
            }
        }
      }
      ng.Dispose();

    }

    
 
  }


     
  public sealed class NativeTextRenderer : IDisposable {
    #region Fields and Consts      

    private static readonly int[] _charFit = new int[1];
    private static readonly int[] _charFitWidth = new int[1000];
    private static readonly Dictionary<string, Dictionary<float, Dictionary<FontStyle, IntPtr>>> _fontsCache = new Dictionary<string, Dictionary<float, Dictionary<FontStyle, IntPtr>>>(StringComparer.InvariantCultureIgnoreCase);

   
    private readonly Graphics _g;
    private IntPtr _hdc;

    #endregion


     
    public NativeTextRenderer(Graphics g) {
      _g = g;

      var clip = _g.Clip.GetHrgn(_g);

      _hdc = _g.GetHdc();
      SetBkMode(_hdc, 1);

      SelectClipRgn(_hdc, clip);

      DeleteObject(clip);
    }
    public Size MeasureString(string str, Font font) {
      SetFont(font);

      var size = new Size();
      GetTextExtentPoint32(_hdc, str, str.Length, ref size);
      return size;
    }
    public Size MeasureString(string str, Font font, float maxWidth, out int charFit, out int charFitWidth) {
      SetFont(font);

      var size = new Size();
      GetTextExtentExPoint(_hdc, str, str.Length, (int)Math.Round(maxWidth), _charFit, _charFitWidth, ref size);
      charFit = _charFit[0];
      charFitWidth = charFit > 0 ? _charFitWidth[charFit - 1] : 0;
      return size;
    }
    public void DrawString(String str, Font font, Color color, Point point) {
      SetFont(font);
      SetTextColor(color);

      TextOut(_hdc, point.X, point.Y, str, str.Length);
    }
    public void DrawString(String str, Font font, Color color, Rectangle rect, TextFormatFlags flags) {
      SetFont(font);
      SetTextColor(color);

      var rect2 = new Rect(rect);
      DrawText(_hdc, str, str.Length, ref rect2, (uint)flags);
    }
    public void Dispose() {
      if (_hdc != IntPtr.Zero) {
        SelectClipRgn(_hdc, IntPtr.Zero);
        _g.ReleaseHdc(_hdc);
        _hdc = IntPtr.Zero;
      }
    }


    #region Private methods      
     
    private void SetFont(Font font) {
      SelectObject(_hdc, GetCachedHFont(font));
    }
    private static IntPtr GetCachedHFont(Font font) {
      IntPtr hfont = IntPtr.Zero;
      Dictionary<float, Dictionary<FontStyle, IntPtr>> dic1;
      if (_fontsCache.TryGetValue(font.Name, out dic1)) {
        Dictionary<FontStyle, IntPtr> dic2;
        if (dic1.TryGetValue(font.Size, out dic2)) {
          dic2.TryGetValue(font.Style, out hfont);
        } else {
          dic1[font.Size] = new Dictionary<FontStyle, IntPtr>();
        }
      } else {
        _fontsCache[font.Name] = new Dictionary<float, Dictionary<FontStyle, IntPtr>>();
        _fontsCache[font.Name][font.Size] = new Dictionary<FontStyle, IntPtr>();
      }

      if (hfont == IntPtr.Zero) {
        _fontsCache[font.Name][font.Size][font.Style] = hfont = font.ToHfont();
      }

      return hfont;
    }
    private void SetTextColor(Color color) {
      int rgb = (color.B & 0xFF) << 16 | (color.G & 0xFF) << 8 | color.R;
      SetTextColor(_hdc, rgb);
    }

    [DllImport("gdi32.dll")]
    private static extern int SetBkMode(IntPtr hdc, int mode);

    [DllImport("gdi32.dll")]
    private static extern int SelectObject(IntPtr hdc, IntPtr hgdiObj);

    [DllImport("gdi32.dll")]
    private static extern int SetTextColor(IntPtr hdc, int color);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
    private static extern int GetTextExtentPoint32(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Size size);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW")]
    private static extern bool GetTextExtentExPoint(IntPtr hDc, [MarshalAs(UnmanagedType.LPWStr)]string str, int nLength, int nMaxExtent, int[] lpnFit, int[] alpDx, ref Size size);

    [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
    private static extern bool TextOut(IntPtr hdc, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string str, int len);

    [DllImport("user32.dll", EntryPoint = "DrawTextW")]
    private static extern int DrawText(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Rect rect, uint uFormat);

    [DllImport("gdi32.dll")]
    private static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    // ReSharper disable NotAccessedField.Local      
    private struct Rect {
      private int _left;
      private int _top;
      private int _right;
      private int _bottom;

      public Rect(Rectangle r) {
        _left = r.Left;
        _top = r.Top;
        _bottom = r.Bottom;
        _right = r.Right;
      }
    }
    // ReSharper restore NotAccessedField.Local      

    #endregion
  }

  [Flags]
  public enum TextFormatFlags : uint {
    Default = 0x00000000,
    Center = 0x00000001,
    Right = 0x00000002,
    VCenter = 0x00000004,
    Bottom = 0x00000008,
    WordBreak = 0x00000010,
    SingleLine = 0x00000020,
    ExpandTabs = 0x00000040,
    TabStop = 0x00000080,
    NoClip = 0x00000100,
    ExternalLeading = 0x00000200,
    CalcRect = 0x00000400,
    NoPrefix = 0x00000800,
    Internal = 0x00001000,
    EditControl = 0x00002000,
    PathEllipsis = 0x00004000,
    EndEllipsis = 0x00008000,
    ModifyString = 0x00010000,
    RtlReading = 0x00020000,
    WordEllipsis = 0x00040000,
    NoFullWidthCharBreak = 0x00080000,
    HidePrefix = 0x00100000,
    ProfixOnly = 0x00200000,
  }

}
