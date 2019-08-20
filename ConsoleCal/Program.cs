using System;
using System.Collections.Generic;
using System.Linq;
using IonLib;

namespace ConsoleCal
{
    class Program
    {
        //Predefinition
        static Dictionary<string, double> customVars = new Dictionary<string, double>();
        static double ans = 0;
        static readonly char[] operators = "+-*/^".ToCharArray();
        static string query;
        static readonly char[] allNum = "0123456789".ToCharArray();
        static void Main(string[] args)
        {
            while (true)
            {
                query = Console.ReadLine();
                if (!Command(query))
                {
                    ans = Calculator(query);
                    Console.WriteLine("Result: "+ans);
                }
            }
        }
        static bool Command(string cmd)
        {
            bool isCommand = true;

            if (cmd == "clear")
            {
                Console.Clear();
            }
            else if (cmd.StartsWith("echo"))
            {
                try
                {
                    string toEcho = cmd.Substring(5).Replace("\\n", "\n").Trim(' ');
                    toEcho = toEcho.Replace("{ans}", ans.ToString());
                    foreach (var var in customVars)
                    {
                        toEcho = toEcho.Replace("{" + var.Key + "}", var.Value.ToString());
                    }
                    Console.WriteLine(toEcho);
                }
                catch (Exception)
                {
                    Base.Error("\"echo\" requires a string to print out.");
                    Base.WriteLineInColor("echo STRING...", ConsoleColor.Yellow);
                    Console.WriteLine("To use variables, write them inside {} like so:");
                    Base.WriteLineInColor("echo {ans} is the newest result from a calculation.", ConsoleColor.Yellow);
                }
            }
            else if (cmd.StartsWith("varlist"))
            {
                Console.WriteLine("ans = " + ans + " (This variable is unchangable via commands)");
                foreach (var var in customVars)
                {
                    Console.WriteLine(var.Key + " = "+ var.Value);
                }
            }
            else if (cmd.StartsWith("var"))
            {
                try
                {
                    while (cmd.Contains("  "))
                    {
                        cmd = cmd.Replace("  ", " ");
                    }
                    while (cmd.Contains(" =") || cmd.Contains("= "))
                    {
                        cmd = cmd.Replace(" =","=");
                        cmd = cmd.Replace("= ", "=");
                    }
                    string[] _args = cmd.Split(' ', 2);
                    string[] _args2 = _args[1].Split('=', 2);

                    if (_args2[0] == "ans")
                    {
                        Base.Error("\"ans\" already exists as a constant variable");
                        return true;
                    }
                    if (double.TryParse(_args2[0], out _))
                    {
                        Base.Error("Cannot set \"" + _args2[0] + "\" as a variable, it must not only be a number.");
                        return true;
                    }
                    if (customVars.ContainsKey(_args2[0]))
                    {
                        if (_args2[1] == "_")
                        {
                            Console.WriteLine("Removed variable \""+ _args2[0]+"\"");
                            customVars.Remove(_args2[0]);
                            return true;
                        }
                        customVars[_args2[0]] = Calculator(_args2[1]);
                        Console.WriteLine("Set \"" + _args2[0] + "\" to \"" + customVars[_args2[0]] + "\"");
                        /*if (double.TryParse(_args2[1], out double _n))
                        {
                            customVars[_args2[0]] = _n;
                            Console.WriteLine("Set \"" + _args2[0] + "\" to \"" + _n + "\"");
                        }
                        else
                        {
                            Base.Error("\""+_args2[0]+"\" exists, but the value can't be set. Invalid number.");
                        }*/
                    }
                    else
                    {
                        if (_args2[1] == "_")
                        {
                            Base.Error("\"_\" is only used to delete a variable!");
                            return true;
                        }
                        customVars.Add(_args2[0], Calculator(_args2[1]));
                        Console.WriteLine("Set \"" + _args2[0] + "\" to \"" + customVars[_args2[0]] + "\"");
                        /*if (double.TryParse(_args2[1], out double _n))
                         {
                             customVars.Add(_args2[0], _n);
                             Console.WriteLine("Added and set \"" + _args2[0] + "\" to \"" + _n + "\"");
                         }
                         else
                         {
                             Base.Error("\"" + _args2[0] + "\" couldn't be set to \""+_n+"\". Invalid number.");
                         }*/
                    }
                }
                catch (Exception)
                {
                    Base.Error("\"var\" requires a name and a value.");
                    Base.WriteLineInColor("var NAME = VALUE", ConsoleColor.Yellow);
                }
            }
            else
            {
                isCommand = false;
            }
            return isCommand;
        }
        static double Calculator(string calQuery)
        {
            calQuery = calQuery.Trim(' ');
            if (calQuery == "")
            {
                return ans;
            }
            double total = 0;
            //variables
            calQuery = calQuery.Replace("ans", ans.ToString());
            foreach (var var in customVars.OrderBy(key => -key.Key.Length))
            {
                calQuery = calQuery.Replace(var.Key, var.Value.ToString());
            }
            string[] _args = calQuery.Split(operators);
            List<double> numArgs = new List<double>();
            for (int i = 0; i < _args.Length; i++)
            {
                if (double.TryParse(_args[i], out double _n))
                {
                    numArgs.Add(_n);
                }
                else
                {
                    Base.Error("Something went wrong trying to parse \""+_args[i]+"\".\n" +
                        "Invalid number, incorrect command or unset variable.");
                    Base.Error("Unrecognized query arguments: "+_args[i].Trim(allNum));
                }
            }
            List<char> operatorArgs = new List<char>();
            for (int i = 0; i < query.Length; i++)
            {
                char _char = query.Substring(i, 1).ToCharArray()[0];
                for (int i2 = 0; i2 < operators.Length; i2++)
                {
                    if (_char == operators[i2])
                    {
                        operatorArgs.Add(operators[i2]);
                        break;
                    }
                }
            }

            double UseOperator(double num1, char _operator, double num2)
            {
                double _result = num1;
                switch (_operator)
                {
                    case '+':
                        _result += num2;
                        break;
                    case '-':
                        _result -= num2;
                        break;
                    case '/':
                        _result /= num2;
                        break;
                    case '*':
                        _result *= num2;
                        break;
                    case '^':
                        _result = Math.Pow(num1, num2);
                        break;
                }
                return _result;
            }

            try
            {
                total = numArgs[0];
            }
            catch (Exception)
            {
                Base.Error("No valid starting number. Operation stopped.");
            }
            for (int i = 1; i < numArgs.Count; i++)
            {
                if (i-1 < operatorArgs.Count)
                {
                    total = UseOperator(total, operatorArgs[i-1], numArgs[i]);
                    //Console.WriteLine(total);
                }
            }

            return total;
        }
    }
}
