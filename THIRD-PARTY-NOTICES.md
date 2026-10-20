# Third-party software

Sweetch uses MIT-licensed code (copyright notices are preserved in [LICENSE](LICENSE)) and the following software. Each component keeps its own license.

| Component | Used for | License |
|---|---|---|
| [Magick.NET](https://github.com/dlemstra/Magick.NET) / ImageMagick | Images | Apache-2.0 / ImageMagick License |
| [PDFtoImage](https://github.com/sungaila/PDFtoImage) / PDFium / SkiaSharp | PDF → images | MIT / BSD-3-Clause / MIT |
| [PdfPig](https://github.com/UglyToad/PdfPig) | PDF text | Apache-2.0 |
| [PDFsharp](https://github.com/empira/PDFsharp) 6.2 | Merge, split, rotate PDF | MIT |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | XLSX | MIT |
| [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader) | XLS | MIT |
| [SharpCompress](https://github.com/adamhathcock/sharpcompress) | RAR, 7z, TAR… | MIT |
| [FFmpeg](https://ffmpeg.org) (Windows: downloaded on demand, “essentials” build by [gyan.dev](https://www.gyan.dev/ffmpeg/builds/)) | Audio and video | GPL-3.0 – source: https://github.com/FFmpeg/FFmpeg |
| [Pandoc 3.12.1](https://pandoc.org) (included as a separate executable in `tools/`) | Documents | GPL-2.0-or-later – source: https://github.com/jgm/pandoc/tree/3.12.1 |
| [LibreOffice](https://www.libreoffice.org) (optional, installed separately) | Office files → PDF | MPL-2.0 |
| [Onest](https://github.com/simpals/onest) (bundled in `Assets/Fonts`) | Interface font | SIL Open Font License 1.1 |
| [Material Symbols Rounded](https://github.com/google/material-design-icons) (a subset, bundled as `SweetchIcons.ttf`) | Icons | Apache-2.0 |

The fonts are built by `scripts/build-fonts.py` from the files served by Google Fonts. The icon font is a subset renamed to “Sweetch Icons”; it is a modified version under Apache-2.0. Onest is unmodified; under the OFL it may be bundled with software but not sold on its own.

Pandoc is bundled from the unmodified official Windows distribution. Its `COPYING.rtf`, `COPYRIGHT.txt` and `MANUAL.html` are included in `tools/`. Source release: https://hackage.haskell.org/package/pandoc-3.12.1. The Windows ZIP is verified against SHA-256 `6ef431cb20b2c24a0371fc19200dc49d452be54b1b5c9239f3462841ef53e587` before packaging.

FFmpeg is downloaded on demand into `%LOCALAPPDATA%\Sweetch\tools`, checked against its published SHA-256. LibreOffice is installed separately. These components run as separate programs; Sweetch invokes them through their command-line interfaces.
