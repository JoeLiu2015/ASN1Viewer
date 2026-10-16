# Render this small SVG set with Windows GDI+; supports its M/L/C/Z paths and ellipses.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$culture = [Globalization.CultureInfo]::InvariantCulture
foreach ($file in Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.svg') {
  [xml]$svg = [IO.File]::ReadAllText($file.FullName)
  $strokeWidth = [single]::Parse($svg.svg.g.GetAttribute('stroke-width'),$culture)
  $small = New-Object Drawing.Bitmap(16,16)
  $g = [Drawing.Graphics]::FromImage($small)
  $g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
  # SVG half-pixel stroke centers map to GDI+ integer pixel centers.
  $g.TranslateTransform(-0.5,-0.5)
  foreach ($shape in $svg.svg.g.ChildNodes) {
    $path = New-Object Drawing.Drawing2D.GraphicsPath
    if ($shape.LocalName -eq 'path') {
      $tokens = $shape.d.Split(' ')
      $i=0; $x=0.0; $y=0.0
      while ($i -lt $tokens.Length) {
        $command=$tokens[$i++]; $count=switch($command){'M'{2};'L'{2};'C'{6};'Z'{0};default{throw "Unsupported command: $command"}}
        $v=@(); for($j=0;$j -lt $count;$j++){ $v += [single]::Parse($tokens[$i++],$culture) }
        switch($command) {
          'M' { $path.StartFigure(); $x=$v[0]; $y=$v[1] }
          'L' { $path.AddLine([single]$x,[single]$y,$v[0],$v[1]); $x=$v[0]; $y=$v[1] }
          'C' { $path.AddBezier([single]$x,[single]$y,$v[0],$v[1],$v[2],$v[3],$v[4],$v[5]); $x=$v[4]; $y=$v[5] }
          'Z' { $path.CloseFigure() }
        }
      }
    } else {
      $cx=[single]::Parse($shape.cx,$culture); $cy=[single]::Parse($shape.cy,$culture)
      if($shape.LocalName -eq 'circle'){ $rx=[single]::Parse($shape.r,$culture); $ry=$rx }
      else { $rx=[single]::Parse($shape.rx,$culture); $ry=[single]::Parse($shape.ry,$culture) }
      $path.AddEllipse(($cx-$rx),($cy-$ry),(2*$rx),(2*$ry))
    }
    if($shape.fill -ne 'none') {
      $brush=New-Object Drawing.SolidBrush([Drawing.ColorTranslator]::FromHtml($shape.fill))
      $g.FillPath($brush,$path); $brush.Dispose()
    }
    $pen=New-Object Drawing.Pen([Drawing.ColorTranslator]::FromHtml($shape.stroke),$strokeWidth)
    $pen.StartCap=$pen.EndCap=[Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin=[Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($pen,$path); $pen.Dispose(); $path.Dispose()
  }
  $g.Dispose()
  # Native 16x16, 32-bit DIB icon with alpha and a legacy transparency mask.
  $output=Join-Path $PSScriptRoot ($file.BaseName+'.ico')
  $stream=[IO.File]::Create($output)
  $writer=New-Object IO.BinaryWriter($stream)
  try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
    $writer.Write([byte]16); $writer.Write([byte]16)
    $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([int]1128); $writer.Write([int]22)
    $writer.Write([int]40); $writer.Write([int]16); $writer.Write([int]32)
    $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([int]0); $writer.Write([int]1088)
    1..4 | ForEach-Object { $writer.Write([int]0) }
    for($y=15;$y -ge 0;$y--) {
      for($x=0;$x -lt 16;$x++) {
        $c=$small.GetPixel($x,$y)
        $writer.Write([byte]$c.B); $writer.Write([byte]$c.G)
        $writer.Write([byte]$c.R); $writer.Write([byte]$c.A)
      }
    }
    for($y=15;$y -ge 0;$y--) {
      $mask=New-Object byte[] 4
      for($x=0;$x -lt 16;$x++) {
        if($small.GetPixel($x,$y).A -eq 0) {
          $index=[int][Math]::Floor($x/8)
          $mask[$index]=$mask[$index] -bor (128 -shr ($x%8))
        }
      }
      $writer.Write($mask)
    }
  } finally { $writer.Dispose(); $stream.Dispose() }
  $small.Dispose()
}
