using Nitro;

const string path = @"A:\ModelExporter\Platin\output_nds\c1_s01.nsbtp";

using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
var cursor = new FileCursor(stream);

var header = CommonHeader.ReadFromCursor(cursor);