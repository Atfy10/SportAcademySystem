using CsvHelper;
using CsvHelper.Configuration;
using SportAcademy.Application.DTOs.TraineeDtos;
using System.Globalization;
using System.Text;

namespace SportAcademy.Web.Features.Trainees;

// Reading and writing the trainee CSV. Deliberately forgiving about the file itself (encoding,
// delimiter, Excel's ="..." text wrapping) so the only errors a user ever sees are about their
// data, reported per row by TraineeImportValidator.
public static class TraineeCsvFile
{
    static TraineeCsvFile()
    {
        // Windows-1256 (Arabic) - what Excel writes for plain "CSV (Comma delimited)" on an
        // Arabic Windows install.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public sealed record ReadResult(IReadOnlyList<string> Headers, IReadOnlyList<TraineeImportRawRow> Rows);

    public static async Task<ReadResult> ReadAsync(IFormFile file, CancellationToken ct)
    {
        byte[] bytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, ct);
            bytes = ms.ToArray();
        }

        var text = Decode(bytes);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            DetectDelimiter = true,
            DetectDelimiterValues = [",", ";", "\t"],
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
        };

        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, config);

        if (!await csv.ReadAsync() || !csv.ReadHeader() || csv.HeaderRecord is null)
            return new ReadResult([], []);

        var headers = csv.HeaderRecord.Select(h => h.Trim().Trim('﻿')).ToArray();
        var rows = new List<TraineeImportRawRow>();

        while (await csv.ReadAsync())
        {
            var cells = new Dictionary<string, string?>();
            var anyValue = false;
            for (var i = 0; i < headers.Length; i++)
            {
                var value = Unwrap(csv.TryGetField<string>(i, out var v) ? v : null);
                if (!string.IsNullOrWhiteSpace(value)) anyValue = true;
                cells.TryAdd(headers[i], value);
            }

            // A row of empty cells (",,,,") is Excel padding, not a trainee.
            if (anyValue)
                rows.Add(new TraineeImportRawRow(csv.Parser.Row, cells));
        }

        return new ReadResult(headers, rows);
    }

    // UTF-8 (with or without BOM) when the bytes are valid UTF-8; otherwise the Arabic Windows
    // code page, so an "ordinary" Excel CSV still reads correctly instead of turning to "????".
    private static string Decode(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

        try
        {
            return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1256).GetString(bytes);
        }
    }

    // ="0501234567" is the trick for keeping leading zeros/"+" in Excel - accept it, and strip
    // the leading apostrophe some users type for the same reason.
    private static string? Unwrap(string? value)
    {
        if (value is null) return null;
        var v = value.Trim();
        if (v.Length >= 3 && v.StartsWith("=\"") && v.EndsWith('"')) v = v[2..^1];
        if (v.StartsWith('\'')) v = v[1..];
        return v;
    }

    // RFC 4180: every field quoted, quotes doubled. Values that a spreadsheet would run as a
    // formula (starting with = + - @) get a leading apostrophe so opening the file is safe.
    public static byte[] Write(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Quote)));
        foreach (var row in rows)
            sb.AppendLine(string.Join(",", row.Select(Quote)));

        // UTF-8 with BOM so Excel shows Arabic correctly on open.
        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return [.. preamble, .. body];
    }

    private static string Quote(string? value)
    {
        var v = value ?? string.Empty;
        if (v.Length > 0 && v[0] is '=' or '+' or '-' or '@' && !IsPlainNumberOrDate(v)) v = "'" + v;
        return "\"" + v.Replace("\"", "\"\"") + "\"";
    }

    private static bool IsPlainNumberOrDate(string v)
        => v.Skip(1).All(c => char.IsDigit(c) || c is ' ' or '-' or '.');
}
