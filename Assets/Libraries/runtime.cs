﻿//#define DLL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Libraries.system
{
    public class Runtime : BaseLibrary
    {
        public void Wait(int time)
        {
            // Task.Delay(time).Wait();
            Thread.Sleep(System.Math.Max(time, system.WaitRefreshRate));

            // test.instance.count1++;
        }

        public void WaitUnitl(Func<bool> action)
        {
            while (action.Invoke())
            {
                Wait();
            }
        }

        public void Wait()
        {
            Thread.Sleep(system.WaitRefreshRate);
        }

        public static readonly char[] asciiMap =
        {
            ' ', '☺', '☻', '♥', '♦', '♣', '♠', '•', '◘', '○', '◙', '♂', '♀', '♪', '♫', '☼', '►', '◄', '↕', '‼', '¶',
            '§', '▬', '↨', '↑', '↓', '→', '←', '∟', '↔', '▲', '▼', ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')',
            '*', '+', ',', '-', '.', '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>',
            '?', '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S',
            'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
            'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}',
            '~', '⌂', 'Ç', 'ü', 'é', 'â', 'ä', 'à', 'å', 'ç', 'ê', 'ë', 'è', 'ï', 'î', 'ì', 'Ä', 'Å', 'É', 'æ', 'Æ',
            'ô', 'ö', 'ò', 'û', 'ù', 'ÿ', 'Ö', 'Ü', '¢', '£', '¥', '₧', 'ƒ', 'á', 'í', 'ó', 'ú', 'ñ', 'Ñ', 'ª', 'º',
            '¿', '⌐', '¬', '½', '¼', '¡', '«', '»', '░', '▒', '▓', '│', '┤', '╡', '╢', '╖', '╕', '╣', '║', '╗', '╝',
            '╜', '╛', '┐', '└', '┴', '┬', '├', '─', '┼', '╞', '╟', '╚', '╔', '╩', '╦', '╠', '═', '╬', '╧', '╨', '╤',
            '╥', '╙', '╘', '╒', '╓', '╫', '╪', '┘', '┌', '█', '▄', '▌', '▐', '▀', 'ɑ', 'ϐ', 'ᴦ', 'ᴨ', '∑', 'ơ', 'µ',
            'ᴛ', 'ɸ', 'ϴ', 'Ω', 'ẟ', '∞', '∅', '∈', '∩', '≡', '±', '≥', '≤', '⌠', '⌡', '÷', '≈', '°', '∙', '·', '√',
            'ⁿ', '²', '■', ' '
        };

        public unsafe static byte CharToByte(char character)
        {
            byte b = 0;
            Cronos.System.mainEncoding.GetBytes(&character, 1, &b, 1);
            return b;
        }

        public unsafe static char ByteToChar(byte character)
        {
            char b = 'l';
            Cronos.System.mainEncoding.GetChars(&character, 1, &b, 1);
            return b;
        }

        public static byte HexToByte(string value)
        {
            byte result = 0;
            byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out result);
            return result;
        }

        public static string ByteToHex(byte value, bool makeIt2)
        {
            return value.ToString("X" + (makeIt2 ? 2 : 1));
        }
    }
}