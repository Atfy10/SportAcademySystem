using System.Globalization;
using System.Text;

namespace SportAcademy.Application.Common.CsvImport
{
    // Text folding shared by every CSV import (trainees, employees): header matching, name
    // lookups and enum labels all compare through Normalize.
    public static class ImportText
    {
        // Comparison key: case-, space-, punctuation- and diacritic-insensitive, Arabic-Indic
        // digits folded to ASCII, tatweel and Arabic letter variants (أ/إ/آ -> ا, ة -> ه, ى -> ي)
        // unified, so "Nationality Category", "nationality_category" and "فئة  الجنسية" all
        // compare equal to their canonical forms.
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var s = DigitNormalizer.ToAscii(value.Trim().Trim('﻿')).Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat == UnicodeCategory.NonSpacingMark) continue;   // accents, harakat
                if (ch is ' ' or '_' or '-' or '.' or '\t' or 'ـ') continue;   // incl. tatweel
                sb.Append(ch switch
                {
                    'أ' or 'إ' or 'آ' => 'ا',
                    'ة' => 'ه',
                    'ى' => 'ي',
                    _ => char.ToLowerInvariant(ch),
                });
            }
            return sb.ToString();
        }
    }

    public static class DigitNormalizer
    {
        // Arabic-Indic (٠-٩) and Eastern Arabic-Indic (۰-۹) digits to ASCII, and the Arabic
        // decimal/thousands separators to '.'/'' - Excel with an Arabic keyboard produces these.
        public static string ToAscii(string value)
        {
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (ch >= '٠' && ch <= '٩') sb.Append((char)('0' + (ch - '٠')));
                else if (ch >= '۰' && ch <= '۹') sb.Append((char)('0' + (ch - '۰')));
                else if (ch == '٫') sb.Append('.');
                else if (ch == '٬') continue;
                else sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}
