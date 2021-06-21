using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chmak_BigCalculator
{


    // https://github.com/Chmakk/Calculator_forBig.git

    // перечисление ЗНАКОВ которыми будем пользоваться при выполнении действий
    public enum Sign
    {
        Minus = -1,
        Plus = 1
    }
    public class BigNumber
    {
        private readonly List<byte> digits = new List<byte>();

        public BigNumber(List<byte> bytes)
        {
            digits = bytes.ToList();
            RemoveNulls();
        }

        public BigNumber(Sign sign, List<byte> bytes)
        {
            Sign = sign;
            digits = bytes;
            RemoveNulls();
        }

        public BigNumber(string s)
        {
            if (s.StartsWith("-"))
            {
                Sign = Sign.Minus;
                s = s.Substring(1);
            }

            foreach (var c in s.Reverse())
            {
                digits.Add(Convert.ToByte(c.ToString()));
            }

            RemoveNulls();
        }

        public BigNumber(uint x) => digits.AddRange(GetBytes(x));

        public BigNumber(int x)
        {
            if (x < 0)
            {
                Sign = Sign.Minus;
            }

            digits.AddRange(GetBytes((uint)Math.Abs(x)));
        }


        // метод для получения списка цифр из целого беззнакового числа
        private List<byte> GetBytes(uint num)
        {
            var bytes = new List<byte>();
            do
            {
                bytes.Add((byte)(num % 10));
                num /= 10;
            } while (num > 0);

            return bytes;
        }


        // метод для удаления лидирующих нулей длинного числа
        private void RemoveNulls()
        {
            for (var i = digits.Count - 1; i > 0; i--)
            {
                if (digits[i] == 0)
                {
                    digits.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }


        // метод для получения больших чисел формата valEexp(пример 1E3 = 1000)
        public static BigNumber Exp(byte val, int exp)
        {
            var bigInt = Zero;
            bigInt.SetByte(exp, val);
            bigInt.RemoveNulls();
            return bigInt;
        }

        public static BigNumber Zero => new BigNumber(0);
        public static BigNumber One => new BigNumber(1);

        //длина числа
        public int Size => digits.Count;

        //знак числа
        public Sign Sign { get; private set; } = Sign.Plus;

        //получение цифры по индексу
        public byte GetByte(int i) => i < Size ? digits[i] : (byte)0;

        //установка цифры по индексу
        public void SetByte(int i, byte b)
        {
            while (digits.Count <= i)
            {
                digits.Add(0);
            }

            digits[i] = b;
        }

        //преобразование длинного числа в строку
        public override string ToString()
        {
            if (this == Zero) return "0";
            var s = new StringBuilder(Sign == Sign.Plus ? "" : "-");

            for (int i = digits.Count - 1; i >= 0; i--)
            {
                s.Append(Convert.ToString(digits[i]));
            }

            return s.ToString();
        }

        
        // метод сложения типа СТОЛБИКОМ
        private static BigNumber Add(BigNumber numberA, BigNumber numberB)
        {
            var digits = new List<byte>();
            var maxLength = Math.Max(numberA.Size, numberB.Size);
            byte t = 0;
            for (int i = 0; i < maxLength; i++)
            {
                byte sum = (byte)(numberA.GetByte(i) + numberB.GetByte(i) + t);
                if (sum > 10)
                {
                    sum -= 10;
                    t = 1;
                }
                else
                {
                    t = 0;
                }

                digits.Add(sum);
            }

            if (t > 0)
            {
                digits.Add(t);
            }

            return new BigNumber(numberA.Sign, digits);
        }

        // метод вычитания 
        private static BigNumber Substract(BigNumber numberA, BigNumber numberB)
        {
            var digits = new List<byte>();

            BigNumber max = Zero;
            BigNumber min = Zero;

            //сравниваем числа игнорируя знак
            var compare = Comparison(numberA, numberB, ignoreSign: true);

            switch (compare)
            {
                case -1:
                    min = numberA;
                    max = numberB;
                    break;
                case 0:
                    return Zero;
                case 1:
                    min = numberB;
                    max = numberA;
                    break;
            }

            //из большего вычитаем меньшее
            var maxLength = Math.Max(numberA.Size, numberB.Size);

            var t = 0;
            for (var i = 0; i < maxLength; i++)
            {
                var s = max.GetByte(i) - min.GetByte(i) - t;
                if (s < 0)
                {
                    s += 10;
                    t = 1;
                }
                else
                {
                    t = 0;
                }

                digits.Add((byte)s);
            }

            return new BigNumber(max.Sign, digits);
        }

        // метод умножения
        private static BigNumber Multiply(BigNumber numberA, BigNumber numberB)
        {
            var retValue = Zero;

            for (var i = 0; i < numberA.Size; i++)
            {
                for (int j = 0, carry = 0; (j < numberB.Size) || (carry > 0); j++)
                {
                    var cur = retValue.GetByte(i + j) + numberA.GetByte(i) * numberB.GetByte(j) + carry;
                    retValue.SetByte(i + j, (byte)(cur % 10));
                    carry = cur / 10;
                }
            }

            retValue.Sign = numberA.Sign == numberB.Sign ? Sign.Plus : Sign.Minus;
            return retValue;
        }

        // метод деления (БЕЗ ОСТАТКА)
        private static BigNumber Div(BigNumber numberA, BigNumber numberB)
        {
            var retValue = Zero;
            var curValue = Zero;

            for (var i = numberA.Size - 1; i >= 0; i--)
            {
                curValue += Exp(numberA.GetByte(i), i);

                var x = 0;
                var l = 0;
                var r = 10;
                while (l <= r)
                {
                    var m = (l + r) / 2;
                    var cur = numberB * Exp((byte)m, i);
                    if (cur <= curValue)
                    {
                        x = m;
                        l = m + 1;
                    }
                    else
                    {
                        r = m - 1;
                    }
                }

                retValue.SetByte(i, (byte)(x % 10));
                var t = numberB * Exp((byte)x, i);
                curValue = curValue - t;
            }

            retValue.RemoveNulls();

            retValue.Sign = numberA.Sign == numberB.Sign ? Sign.Plus : Sign.Minus;
            return retValue;
        }

        // поменять знак на противоположный
        public static BigNumber operator -(BigNumber numberA)
        {
            numberA.Sign = numberA.Sign == Sign.Plus ? Sign.Minus : Sign.Plus;
            return numberA;
        }

        //сложение
        public static BigNumber operator +(BigNumber numberA, BigNumber numberB) => numberA.Sign == numberB.Sign
                ? Add(numberA, numberB)
                : Substract(numberA, numberB);

        //вычитание
        public static BigNumber operator -(BigNumber numberA, BigNumber numberB) => numberA + -numberB;

        //умножение
        public static BigNumber operator *(BigNumber numberA, BigNumber numberB) => Multiply(numberA, numberB);

        //целочисленное деление(без остатка)
        public static BigNumber operator /(BigNumber numberA, BigNumber numberB) => Div(numberA, numberB);


        //------------------------------------------------------------------
        // переопределение знаков
        public static bool operator <(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) < 0;

        public static bool operator >(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) > 0;

        public static bool operator <=(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) <= 0;

        public static bool operator >=(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) >= 0;

        public static bool operator ==(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) == 0;

        public static bool operator !=(BigNumber numberA, BigNumber numberB) => Comparison(numberA, numberB) != 0;


        public override bool Equals(object obj) => !(obj is BigNumber) ? false : this == (BigNumber)obj;
        private static int Comparison(BigNumber numberA, BigNumber numberB, bool ignoreSign = false)
        {
            return CompareSign(numberA, numberB, ignoreSign);
        }

        // Метод сравнения ЗНАКОВ для далбнейшего выполнения сложения и вычитания
        private static int CompareSign(BigNumber numberA, BigNumber numberB, bool ignoreSign = false)
        {
            if (!ignoreSign)
            {
                if (numberA.Sign < numberB.Sign)
                {
                    return -1;
                }
                else if (numberA.Sign > numberB.Sign)
                {
                    return 1;
                }
            }

            return CompareSize(numberA, numberB);
        }

        // Метод сравнения РАЗМЕРА для далбнейшего выполнения сложения и вычитания
        private static int CompareSize(BigNumber numberA, BigNumber numberB)
        {
            if (numberA.Size < numberB.Size)
            {
                return -1;
            }
            else if (numberA.Size > numberB.Size)
            {
                return 1;
            }

            return CompareDigits(numberA, numberB);
        }

        private static int CompareDigits(BigNumber numberA, BigNumber numberB)
        {
            var maxLength = Math.Max(numberA.Size, numberB.Size);
            for (var i = maxLength; i >= 0; i--)
            {
                if (numberA.GetByte(i) < numberB.GetByte(i))
                {
                    return -1;
                }
                else if (numberA.GetByte(i) > numberB.GetByte(i))
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}