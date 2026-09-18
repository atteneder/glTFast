// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Unity.Cloud.Gltfast.Objects
{
    static class FloatParser
    {
        static readonly double[] k_PosPowersOf10 = {
            1e0, 1e1, 1e2, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10,
            1e11, 1e12, 1e13, 1e14, 1e15, 1e16, 1e17, 1e18, 1e19, 1e20, 1e21, 1e22
        };

        // Largest mantissa that can still absorb one more decimal digit without
        // overflowing ulong (mantissa * 10 + 9 stays <= ulong.MaxValue).
        const ulong k_MantissaCap = (ulong.MaxValue - 9) / 10;

        public static double GetDouble(ReadOnlySpan<byte> json)
        {
            if (json.Length == 0)
                throw new InvalidDataException("Empty input");

            var pos = 0;

            var negative = false;
            var currentByte = json[pos];
            if (currentByte == '-')
            {
                negative = true;
                pos++;
            }

            // All significant digits (integer and fractional) are folded into a single
            // integer mantissa; the decimal point and exponent are tracked separately and
            // applied once at the end.
            ulong mantissa = 0;
            var hasDigit = false;

            // Number of significant integer digits dropped after the mantissa saturated;
            // each one still contributes a factor of ten to the value.
            var integerExponent = 0;

            // Number of fractional digits folded into the mantissa.
            var fractionalDigits = 0;

            while (pos < json.Length)
            {
                currentByte = json[pos];
                if (currentByte >= '0' && currentByte <= '9')
                {
                    hasDigit = true;
                    if (mantissa <= k_MantissaCap)
                        mantissa = mantissa * 10 + (ulong)(currentByte - '0');
                    else
                        integerExponent++;
                    pos++;
                }
                else if (currentByte == '.')
                {
                    pos++;
                    goto Radix;
                }
                else if ((currentByte & 0b11011111) == 'E')
                {
                    if (!hasDigit)
                        throw new InvalidDataException($"Expected digit before exponent at {pos}");
                    pos++;
                    goto Exponent;
                }
                else
                {
                    throw new InvalidDataException($"Unexpected char at {pos}");
                }
            }

            if (!hasDigit)
                throw new InvalidDataException("Missing integer digits");

            return Compose(mantissa, negative, integerExponent);

        Radix:
            var hasRadixDigit = false;
            while (pos < json.Length)
            {
                currentByte = json[pos];
                if (currentByte >= '0' && currentByte <= '9')
                {
                    hasRadixDigit = true;
                    if (mantissa <= k_MantissaCap)
                    {
                        mantissa = mantissa * 10 + (ulong)(currentByte - '0');
                        fractionalDigits++;
                    }
                    pos++;
                }
                else if ((currentByte & 0b11011111) == 'E')
                {
                    if (!hasRadixDigit)
                        throw new InvalidDataException($"Expected digit after '.' at {pos}");
                    pos++;
                    goto Exponent;
                }
                else if (currentByte == '.')
                {
                    throw new InvalidDataException($"Multiple radix points in number at {pos}");
                }
                else
                {
                    throw new InvalidDataException($"Unexpected char at {pos}");
                }
            }

            if (!hasRadixDigit)
                throw new InvalidDataException($"Expected digit after '.' at {pos}");

            return Compose(mantissa, negative, integerExponent - fractionalDigits);

        Exponent:
            short exponent = 0;
            var negateExponent = false;
            if (pos >= json.Length)
                throw new InvalidDataException("Unexpected end of input in exponent");
            currentByte = json[pos];
            if (currentByte == '+')
            {
                pos++;
            }
            else if (currentByte == '-')
            {
                pos++;
                negateExponent = true;
            }

            if (pos >= json.Length)
                throw new InvalidDataException("Missing exponent digits");

            while (pos < json.Length)
            {
                currentByte = json[pos];
                if (currentByte >= '0' && currentByte <= '9')
                {
                    exponent = (short)(exponent * 10 + (currentByte - 48));
                    pos++;
                }
                else
                {
                    throw new InvalidDataException($"Unexpected char at {pos}");
                }
            }

            var decimalExponent = (negateExponent ? -exponent : exponent) + integerExponent - fractionalDigits;
            return Compose(mantissa, negative, decimalExponent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Compose(ulong mantissa, bool negative, int decimalExponent)
        {
            double value = mantissa;
            if (decimalExponent > 0)
            {
                value *= decimalExponent < k_PosPowersOf10.Length
                    ? k_PosPowersOf10[decimalExponent]
                    : Math.Pow(10, decimalExponent);
            }
            else if (decimalExponent < 0)
            {
                var e = -decimalExponent;
                value = e < k_PosPowersOf10.Length
                    ? value / k_PosPowersOf10[e]
                    : value * Math.Pow(10, decimalExponent);
            }
            return negative ? -value : value;
        }
    }
}
