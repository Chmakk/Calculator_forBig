using System;

namespace Chmak_BigCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.WriteLine("Введите первое число:");
                BigNumber numberA = new BigNumber(Console.ReadLine());

                Console.WriteLine("Введите знак желаемого действия (+, -, *, /)");
                string action = Console.ReadLine();

                Console.WriteLine("Введите второе число:");
                BigNumber numberB = new BigNumber(Console.ReadLine());

                string result = "";

                switch (action)
                {
                    case "+":
                        result = Convert.ToString(numberA + numberB);
                        break;
                    case "-":
                        result = Convert.ToString(numberA - numberB);
                        break;
                    case "*":
                        result = Convert.ToString(numberA * numberB);
                        break;
                    case "/":
                        result = Convert.ToString(numberA / numberB);
                        break;
                    default:
                        Console.WriteLine("Что-то пошло не так");
                        break;
                }
                Console.WriteLine("\nРезультат:");
                Console.WriteLine(result+"\n");
                Console.WriteLine("нажмите Enter чтобы выполнить новое действие");
                Console.ReadLine();
                Console.Clear();
            }
            
        }
    }
}
