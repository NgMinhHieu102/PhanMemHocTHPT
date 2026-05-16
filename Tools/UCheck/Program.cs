using System.Globalization;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var tran = "\u1EA7";
var vn = "\u0103";
var lap = "\u1EAD";
var ich = '\u00EC';

Console.WriteLine("Tr+u1EA7+n = Tr" + tran + "n  hex=" + ((int)tran[0]).ToString("X4", CultureInfo.InvariantCulture));
Console.WriteLine("V+u0103+n = V" + vn + "n");
Console.WriteLine("L+u1EAD+p = L" + lap + "p");
Console.WriteLine("tr+uEC0+nh = tr" + ich + "nh");
Console.WriteLine("du horn/tone samples: \u0111\u1edd \u0111\u1eef");
