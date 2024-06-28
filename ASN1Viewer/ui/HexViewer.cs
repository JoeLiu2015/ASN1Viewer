using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ASN1Viewer.ui {
  internal class HexViewer : Control {
    private const int BYTES_COUNT_PER_LINE = 16;
    private readonly static Color GRAY_COLOR = Color.Gray; 

    #region Fields

    private Rectangle _recContent;
    private RectangleF _recLineInfo;
    private RectangleF _recHex;
    private RectangleF _recStringView;
    private SizeF _charSize;
    private StringFormat _stringFormat;
    private int _linesCountInScreen;
    private int _bytesCountInScreen;
    private int _scrollVmax;
    private int _scrollVpos;
    private VScrollBar _vScrollBar;

    
    private int _startByte;
    private int _endByte;

    private int _tagLen = -1;
    private int _start = -1;
    private int _end = -1;
    private int _contentStart = -1;
    private int _contentEnd = -1;


    private BorderStyle _borderStyle = BorderStyle.None;
    private Padding _borderPadding = Padding.Empty;

    private byte[] _data;

    #endregion

    #region Ctors
    public HexViewer() {
      _vScrollBar = new VScrollBar();
      _vScrollBar.Scroll += new ScrollEventHandler(_vScrollBar_Scroll);
      Controls.Add(_vScrollBar);

      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.ResizeRedraw, true);

      BackColor = SystemColors.Window;
      Font = new Font("YouYuan", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
      if (Font.Name != "YouYuan") {
        Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
      }

      _stringFormat = new StringFormat(StringFormat.GenericTypographic);
      _stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
    }

    #endregion

    #region Scroll methods

    void _vScrollBar_Scroll(object sender, ScrollEventArgs e) {
      switch (e.Type) {
        case ScrollEventType.First:
          break;
        case ScrollEventType.Last:
          break;
        case ScrollEventType.EndScroll:
          break;
        case ScrollEventType.SmallIncrement:
          PerformScrollToLine(1 + _scrollVpos);
          break;
        case ScrollEventType.SmallDecrement:
          PerformScrollToLine(-1 + _scrollVpos);
          break;
        case ScrollEventType.LargeIncrement:
          PerformScrollToLine(_linesCountInScreen+ _scrollVpos);
          break;
        case ScrollEventType.LargeDecrement:
          PerformScrollToLine(-_linesCountInScreen + _scrollVpos);
          break;
        case ScrollEventType.ThumbPosition:
          break;
        case ScrollEventType.ThumbTrack:
          if (e.NewValue + _vScrollBar.LargeChange > _scrollVmax) e.NewValue = _scrollVmax;
          PerformScrollToLine(e.NewValue);
          break;
        
        default:
          break;
      }
    }


    void UpdateScrollSize() {
      // calc scroll bar info
      if (_data != null && _data.Length > 0 ) {
        int totalLines = (_data.Length + BYTES_COUNT_PER_LINE - 1) / BYTES_COUNT_PER_LINE;
        int screenLines = (int)Math.Floor(_recHex.Height / _charSize.Height);
        int scrollmax = totalLines - screenLines;
        scrollmax = Math.Max(0, scrollmax);

        int scrollpos = _startByte / BYTES_COUNT_PER_LINE;


        if (scrollmax == _scrollVmax && scrollpos == _scrollVpos)
          return;

        _scrollVmax = scrollmax;
        _scrollVpos = Math.Min(scrollpos, scrollmax);
        UpdateVScroll();
      } else  {
        // disable scroll bar
        _scrollVmax = 0;
        _scrollVpos = 0;
        UpdateVScroll();
      }
    }

    void UpdateVScroll() {
      _vScrollBar.Minimum = 0;
      _vScrollBar.Maximum = _scrollVmax;
      _vScrollBar.Value = _scrollVpos;
    }




    void PerformScrollToLine(int pos) {
      pos = Math.Max(0, pos);
      pos = Math.Min(_scrollVmax, pos);
      if (pos == _scrollVpos)
        return;

      _scrollVpos = pos;
      UpdateVScroll();
      UpdateVisibilityBytes();
      Invalidate();
    }
    #endregion

    #region Paint methods

    protected override void OnPaintBackground(PaintEventArgs e) {
      switch (_borderStyle) {
        case BorderStyle.Fixed3D: {
          if (TextBoxRenderer.IsSupported) {
            VisualStyleElement state = VisualStyleElement.TextBox.TextEdit.Normal;
            Color backColor = this.BackColor;


            //state = VisualStyleElement.TextBox.TextEdit.ReadOnly;


            VisualStyleRenderer vsr = new VisualStyleRenderer(state);
            vsr.DrawBackground(e.Graphics, this.ClientRectangle);

            Rectangle rectContent = vsr.GetBackgroundContentRectangle(e.Graphics, this.ClientRectangle);
            e.Graphics.FillRectangle(new SolidBrush(backColor), rectContent);
          } else {
            // draw background
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            // draw default border
            ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
          }

          break;
        }
        case BorderStyle.FixedSingle: {
          // draw background
          e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

          // draw fixed single border
          ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
          break;
        }
        default: {
          // draw background
          e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
          break;
        }
      }
    }



    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      if (_data == null)
        return;

      // draw only in the content rectangle, so exclude the border and the scrollbar.
      Region r = new Region(ClientRectangle);
      r.Exclude(_recContent);
      e.Graphics.ExcludeClip(r);
      UpdateVisibilityBytes();

      PaintSelectionBackground(e.Graphics);
      PaintLineInfo(e.Graphics, _startByte, _endByte);
      PaintHexAndStringView(e.Graphics, _startByte, _endByte);
    }

    void PaintSelectionBackground(Graphics g) {
      if (_start < 0) return;
      using (SolidBrush brush = new SolidBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xB3))) {
        int start = Math.Max(_start, _startByte);
        int end = Math.Min(_end, _endByte);

        // First line
        int firstLineStart = start;
        int firstLineEnd = (start / BYTES_COUNT_PER_LINE) * BYTES_COUNT_PER_LINE + BYTES_COUNT_PER_LINE - 1;
        firstLineEnd = Math.Min(firstLineEnd, end);
        Point s = GetGridBytePoint(firstLineStart - _startByte);
        Point e = GetGridBytePoint(firstLineEnd - _startByte);
        PointF sf = GetBytePointF(s), se = GetBytePointF(e);
        PointF sf1 = GetByteStringPointF(s), se1 = GetByteStringPointF(e);
        g.FillRectangle(brush,
          new RectangleF(sf.X, sf.Y, se.X - sf.X + 2 * _charSize.Width, _charSize.Height));
        g.FillRectangle(brush,
          new RectangleF(sf1.X, sf1.Y, se1.X - sf1.X + _charSize.Width, _charSize.Height));
        if (firstLineEnd == end) return;

        // Last line
        int lastLineStart = (end / BYTES_COUNT_PER_LINE) * BYTES_COUNT_PER_LINE;
        int lastLineEnd = end;
        s = GetGridBytePoint(lastLineStart - _startByte);
        e = GetGridBytePoint(lastLineEnd - _startByte);
        sf = GetBytePointF(s);
        se = GetBytePointF(e);
        sf1 = GetByteStringPointF(s);
        se1 = GetByteStringPointF(e);
        g.FillRectangle(brush,
          new RectangleF(sf.X, sf.Y, se.X - sf.X + 2 * _charSize.Width, _charSize.Height));
        g.FillRectangle(brush,
          new RectangleF(sf1.X, sf1.Y, se1.X - sf1.X + _charSize.Width, _charSize.Height));
        if (lastLineStart == firstLineEnd + 1) return;

        // Body
        int bodyStart = firstLineEnd + 1;
        int bodyEnd = lastLineStart - 1;
        s = GetGridBytePoint(bodyStart - _startByte);
        e = GetGridBytePoint(bodyEnd - _startByte);
        sf = GetBytePointF(s);
        se = GetBytePointF(e);
        sf1 = GetByteStringPointF(s);
        se1 = GetByteStringPointF(e);
        g.FillRectangle(brush,
          new RectangleF(sf.X, sf.Y, se.X - sf.X + 2 * _charSize.Width, se.Y - sf.Y + _charSize.Height));
        g.FillRectangle(brush,
          new RectangleF(sf1.X, sf1.Y, se1.X - sf1.X + _charSize.Width, se.Y - sf.Y + _charSize.Height));
      }
    }


    void PaintLineInfo(Graphics g, int startByte, int endByte) {
      // Ensure endByte isn't > length of array.
      endByte = Math.Min(_data.Length - 1, endByte);

      Brush brush = new SolidBrush(GRAY_COLOR);

      int maxLine = GetGridBytePoint((int)(endByte - startByte)).Y + 1;

      for (int i = 0; i < maxLine; i++) {
        int firstLineByte = (startByte + (BYTES_COUNT_PER_LINE) * i) ;

        PointF bytePointF = GetBytePointF(new Point(0, 0 + i));
        string info = firstLineByte.ToString("X", System.Threading.Thread.CurrentThread.CurrentCulture);
        int nulls = 8 - info.Length;
        string formattedInfo;
        if (nulls > -1) {
          formattedInfo = new string('0', 8 - info.Length) + info;
        } else {
          formattedInfo = new string('~', 8);
        }

        formattedInfo += "|";

        g.DrawString(formattedInfo, Font, brush, new PointF(_recLineInfo.X, bytePointF.Y), _stringFormat);
        g.DrawString("|", Font, brush, new PointF(_recStringView.Left + _charSize.Width, bytePointF.Y), _stringFormat);
      }
    }





    void PaintHexString(Graphics g, byte b, Brush brush, Point gridPoint) {
      PointF bytePointF = GetBytePointF(gridPoint);
      string sB = ConvertByteToHex(b);
      g.DrawString(sB, Font, brush, bytePointF, _stringFormat);
    }



    void PaintHexAndStringView(Graphics g, int startByte, int endByte) {
      int intern_endByte = Math.Min(_data.Length - 1, endByte + BYTES_COUNT_PER_LINE);


      for (int i = startByte; i < intern_endByte + 1; i++) {
        Point gridPoint = GetGridBytePoint(i - startByte);
        byte b = _data[i];
        Brush brush = GetBrushForByte(i);

        PaintHexString(g, b, brush, gridPoint);

        string s  = (b < 0x20 || b > 0x7E) ? ".": "" + (char)b;
        PointF byteStringPointF = GetByteStringPointF(gridPoint);
        g.DrawString(s, Font, brush, byteStringPointF, _stringFormat);
      }
    }

    Brush GetBrushForByte(int idx) {
      if (idx >= _start && idx < _start + _tagLen) return Brushes.Red;
      if (idx >= _start + _tagLen && idx < _contentStart) return Brushes.Blue;
      if (idx >= _contentStart && idx <= _contentEnd) return Brushes.Green;
      if (idx > _contentEnd && idx <= _end) return Brushes.Blue;
      return Brushes.Gray;
    }

    void UpdateVisibilityBytes() {
      if (_data == null || _data.Length == 0)
        return;

      _startByte = _scrollVpos * BYTES_COUNT_PER_LINE;
      _endByte = Math.Min(_data.Length - 1, _startByte + _bytesCountInScreen - 1);
    }

    #endregion

    #region Positioning methods

    void UpdateRectanglePositioning() {
      // calc char size
      SizeF charSize;
      using (var graphics = this.CreateGraphics()) {
        charSize = graphics.MeasureString("A", Font, 100, _stringFormat);
      }

      _charSize = charSize;

      // calc content bounds
      _recContent = ClientRectangle;
      _recContent = SubtractPadding(_recContent, _borderPadding);
      _recContent.Width -= _vScrollBar.Width;
      
      _vScrollBar.Left = _recContent.Right;
      _vScrollBar.Top = _recContent.Top;
      _vScrollBar.Height = _recContent.Height;


      // calc line info bounds
      _recLineInfo = new RectangleF(
        _recContent.X,
        _recContent.Y,
        _charSize.Width * 9 + _charSize.Width * 2,
        _recContent.Height);

      // calc hex bounds and grid
      _recHex = new RectangleF(
        _recLineInfo.Right,
        _recLineInfo.Y,
        BYTES_COUNT_PER_LINE * _charSize.Width * 3 + _charSize.Width,
        _recContent.Height);


      _recStringView = new RectangleF(
        _recHex.Right,
        _recHex.Y,
        2 * _charSize.Width + _charSize.Width * BYTES_COUNT_PER_LINE,
        _recHex.Height);

      _linesCountInScreen = (int)Math.Ceiling((double)_recHex.Height / (double)_charSize.Height);

      _bytesCountInScreen = BYTES_COUNT_PER_LINE * _linesCountInScreen;

      UpdateScrollSize();
    }



    PointF GetBytePointF(Point gp) {
      float x = _recHex.X + (3 * _charSize.Width) * gp.X;
      if (gp.X >= 8) x += _charSize.Width;
      float y = _recHex.Y + gp.Y * _charSize.Height;

      return new PointF(x, y);
    }

    

    PointF GetByteStringPointF(Point gp) {
      float x = _recStringView.X + 2 * _charSize.Width + _charSize.Width * gp.X;
      if (gp.X >= 8) x += _charSize.Width;
      float y = _recStringView.Y + gp.Y* _charSize.Height;

      return new PointF(x, y);
    }

    Point GetGridBytePoint(int byteIndex) {
      return new Point( byteIndex%BYTES_COUNT_PER_LINE, byteIndex/BYTES_COUNT_PER_LINE);
    }

    #endregion

    #region Overridden properties
    public override Font Font {
      get { return base.Font; }
      set {
        if (value == null)
          return;

        base.Font = value;
        this.UpdateRectanglePositioning();
        this.Invalidate();
      }
    }


    #endregion

    #region Properties



    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public byte[] Data {
      get { return _data; }
      set {
        if (_data == value)
          return;

        _data = value;
        _tagLen = _contentStart = _contentEnd = _start = _end = -1;

        if (value == null) // do not raise events if value is null
        {

        } else {
          _scrollVpos = 0;

          UpdateVisibilityBytes();
          UpdateRectanglePositioning();
          Invalidate();
        }
      }
    }
    public BorderStyle BorderStyle {
      get { return _borderStyle; }
      set {
        if (_borderStyle == value)
          return;

        _borderStyle = value;
        switch (_borderStyle) {
          case BorderStyle.None:
            _borderPadding = Padding.Empty;
            break;
          case BorderStyle.Fixed3D:
            _borderPadding = new Padding(SystemInformation.Border3DSize.Width, SystemInformation.Border3DSize.Width,
                                         SystemInformation.Border3DSize.Height, SystemInformation.Border3DSize.Height);
            break;
          case BorderStyle.FixedSingle:
            _borderPadding = new Padding(1);
            break;
        }
        UpdateRectanglePositioning();
      }
    }
    #endregion

    #region Misc

    public void SelectNode(int tagLen, int start, int end, int contentStart, int contentEnd, byte[] data) {
      _tagLen = tagLen;
      _start = start;
      _end = end - 1;
      _contentStart = contentStart;
      _contentEnd = contentEnd - 1;
      PerformScrollToLine(start/BYTES_COUNT_PER_LINE);
      Invalidate();
    }
    string ConvertByteToHex(byte b) {
      string sB = b.ToString("X", System.Threading.Thread.CurrentThread.CurrentCulture);
      if (sB.Length == 1)
        sB = "0" + sB;
      return sB;
    }
    Rectangle SubtractPadding(Rectangle rect, Padding margins) {
      return new Rectangle(
        rect.X + margins.Left,
        rect.Y + margins.Top,
        rect.Width - margins.Left - margins.Right,
        rect.Height - margins.Top - margins.Bottom
      );
    }
    protected override void OnMouseWheel(MouseEventArgs e) {
      int linesToScroll = -(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
      this.PerformScrollToLine(linesToScroll + _scrollVpos);
      base.OnMouseWheel(e);
    }
    protected override void OnResize(EventArgs e) {
      base.OnResize(e);
      UpdateRectanglePositioning();
    }


    #endregion

    #region Scaling Support for High DPI resolution screens

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified) {
      base.ScaleControl(factor, specified);

      this.BeginInvoke(new MethodInvoker(() => {
        this.UpdateRectanglePositioning();
        this.Invalidate();
      }));
    }

    #endregion
  }
}
