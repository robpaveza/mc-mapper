using System;
using System.Diagnostics;
using System.Text;

namespace MCBEWorld.Utility
{
    internal static class BinaryFormatter
    {
        /// <summary>
        /// Formats a data into 16-byte rows followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to format.</param>
        /// <returns>A string representing the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <b>data</b> is <b>null</b>
        /// (<b>Nothing</b> in Visual Basic).</exception>
        public static string FormatHexAndAscii(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            StringBuilder sb = new StringBuilder();
            sb.Append("0000   ");
            if (data.Length == 0)
            {
                sb.Append("(empty)");
                return sb.ToString();
            }

            StringBuilder lineAscii = new StringBuilder(16, 16);

            for (int i = 0; i < data.Length; i++)
            {
                #region build the end-of-line ascii

                char curData = (char)data[i];
                if (char.IsLetterOrDigit(curData) || char.IsPunctuation(curData) ||
                    char.IsSymbol(curData) || curData == ' ')
                {
                    lineAscii.Append(curData);
                }
                else
                {
                    lineAscii.Append('.');
                }
                #endregion

                sb.AppendFormat("{0:x2} ", data[i]);
                if ((i + 1) % 8 == 0)
                {
                    sb.Append(" ");
                }
                if (((i + 1) % 16 == 0) || ((i + 1) == data.Length))
                {
                    if ((i + 1) == data.Length && ((i + 1) % 16) != 0)
                    {
                        int lenOfCurStr = ((i % 16) * 3);
                        if ((i % 16) > 8) lenOfCurStr++;

                        for (int j = 0; j < (47 - lenOfCurStr); j++)
                            sb.Append(' ');
                    }

                    sb.AppendFormat("  {0}", lineAscii.ToString());
                    lineAscii = new StringBuilder(16, 16);
                    sb.Append(Environment.NewLine);

                    if (data.Length > (i + 1))
                    {
                        sb.AppendFormat("{0:x4}   ", i + 1);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats a data into 16-byte rows followed by an ASCII representation.
        /// </summary>
        /// <param name="data">The data to format.</param>
        /// <param name="startIndex">The starting position of the data to format.</param>
        /// <param name="length">The amount of data to format.</param>
        /// <returns>A string representing the data.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <b>data</b> is <b>null</b>
        /// (<b>Nothing</b> in Visual Basic).</exception>
        public static string FormatHexAndAscii(byte[] data, int startIndex, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            StringBuilder sb = new StringBuilder();
            sb.Append("0000   ");
            if (data.Length == 0)
            {
                sb.Append("(empty)");
                return sb.ToString();
            }

            StringBuilder lineAscii = new StringBuilder(16, 16);

            for (int i = startIndex; i < data.Length && i < (startIndex + length); i++)
            {
                #region build the end-of-line ascii

                char curData = (char)data[i];
                if (char.IsLetterOrDigit(curData) || char.IsPunctuation(curData) ||
                    char.IsSymbol(curData) || curData == ' ')
                {
                    lineAscii.Append(curData);
                }
                else
                {
                    lineAscii.Append('.');
                }
                #endregion

                sb.AppendFormat("{0:x2} ", data[i]);
                if ((i + 1) % 8 == 0)
                {
                    sb.Append(" ");
                }
                if (((i + 1) % 16 == 0) || ((i + 1) == data.Length))
                {
                    if ((i + 1) == data.Length && ((i + 1) % 16) != 0)
                    {
                        int lenOfCurStr = ((i % 16) * 3);
                        if ((i % 16) > 8) lenOfCurStr++;

                        for (int j = 0; j < (47 - lenOfCurStr); j++)
                            sb.Append(' ');
                    }

                    sb.AppendFormat("  {0}", lineAscii.ToString());
                    lineAscii = new StringBuilder(16, 16);
                    sb.Append(Environment.NewLine);

                    if (data.Length > (i + 1))
                    {
                        sb.AppendFormat("{0:x4}   ", i + 1);
                    }
                }
            }

            return sb.ToString();
        }

        public static string FormatHex(byte[] data)
        {
            StringBuilder result = new StringBuilder(data.Length * 2);
            for (int i = 0; i < data.Length; i++)
            {
                result.AppendFormat("{0:x2}", data[i]);
            }

            return result.ToString();
        }

        public static string FormatBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static string Format(BinarySerializationFormat format, byte[] data)
        {
            switch (format)
            {
                case BinarySerializationFormat.Base64:
                    return FormatBase64(data);

                case BinarySerializationFormat.Hex:
                    return FormatHex(data);

                case BinarySerializationFormat.HexAndAscii:
                default:
                    return FormatHexAndAscii(data);
            }
        }
    }
}
