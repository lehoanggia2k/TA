using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TA1
{
    class Program
    {
        // Phương thức kiểm tra nếu ký tự đầu vào là toán tử
        static private bool IsOperator(char c)
        {
            return "+-/*()#".IndexOf(c) != -1;
        }

        // Phương thức kiểm tra nếu ký tự đầu vào là dấu phân cách (khoảng trắng hoặc tab)
        static private bool IsDelimiter(char c)
        {
            return c == ' ' || c == '\t';
        }

        // Phương thức trả về độ ưu tiên của toán tử
        static private byte GetPriority(char s)
        {
            switch (s)
            {
                case '(': return 1;
                case ')': return 1;
                case '+': return 2;
                case '-': return 2;
                case '*': return 3;
                case '/': return 3;
                default: return 4;
            }
        }

        // Phương thức chuyển đổi biểu thức đầu vào từ dạng trung tố sang hậu tố
        static private string GetExpression(string input)
        {
            input = input.Replace(".", ",");
            while (input.IndexOf("pow") != -1)
                input = Regex.Replace(input, @"pow\((?<first>.*?),(?<second>.*?)\)", "((${first})#(${second}))");
            if (input.IndexOf(",,") != -1)
                return "Incorrect number format!";

            string output = string.Empty; // Chuỗi để lưu trữ biểu thức hậu tố
            Stack<char> operStack = new Stack<char>(); // Ngăn xếp để lưu trữ toán tử

            for (int i = 0; i < input.Length; i++) // Duyệt từng ký tự trong chuỗi đầu vào
            {
                // Bỏ qua các dấu phân cách
                if (IsDelimiter(input[i]))
                    continue;
                // Nếu ký tự là số, đọc toàn bộ số đó
                else if (char.IsDigit(input[i]) || input[i] == ',')
                {
                    string tempOut = string.Empty;
                    // Đọc cho đến khi gặp dấu phân cách hoặc toán tử để lấy toàn bộ số
                    while ((!IsDelimiter(input[i])) && !IsOperator(input[i]))
                    {
                        if (!char.IsDigit(input[i]) && input[i] != ',')
                            return "Error! The " + (i + 1) + "-th character is not an operator or digit";
                        tempOut += input[i];
                        i++;
                        if (i == input.Length) break;
                    }
                    output += tempOut + " "; // Thêm số vào chuỗi hậu tố, kèm theo dấu cách
                    i--;
                }
                // Nếu ký tự là toán tử
                else if (IsOperator(input[i]))
                {
                    if (i + 1 < input.Length)
                        if (input[i] == '(' && input[i + 1] == '-') // Nếu ký tự là '(' và ký tự tiếp theo là '-'
                            output += "0 "; // Thêm 0 vào chuỗi hậu tố

                    if (input[i] == '(') // Nếu ký tự là '('
                        operStack.Push(input[i]); // Đẩy vào ngăn xếp

                    else if (input[i] == ')') // Nếu ký tự là ')'
                    {
                        // Lấy tất cả toán tử ra khỏi ngăn xếp cho đến khi gặp '('
                        try
                        {
                            char s = operStack.Pop();
                            while (s != '(')
                            {
                                output += s.ToString() + ' ';
                                s = operStack.Pop();
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            return "Error! Mismatched parentheses!";
                        }
                    }
                    else // Nếu ký tự là toán tử khác
                    {
                        if (operStack.Count > 0) // Nếu ngăn xếp không rỗng
                            if (GetPriority(input[i]) <= GetPriority(operStack.Peek())) // Nếu độ ưu tiên của toán tử hiện tại nhỏ hơn hoặc bằng toán tử trên đỉnh ngăn xếp
                                output += operStack.Pop().ToString() + " "; // Thêm toán tử từ đỉnh ngăn xếp vào chuỗi hậu tố

                        operStack.Push(input[i]); // Đẩy toán tử hiện tại vào ngăn xếp
                    }
                }
                else return "Error! The " + (i + 1) + "-th character is not an operator or digit";
            }

            // Lấy tất cả toán tử còn lại trong ngăn xếp và thêm vào chuỗi hậu tố
            while (operStack.Count > 0)
                output += operStack.Pop() + " ";

            return output; // Trả về chuỗi hậu tố
        }

        // Phương thức tính toán giá trị của biểu thức hậu tố
        static private double Counting(string input)
        {
            double result = double.NaN; // Kết quả
            Stack<double> temp = new Stack<double>(); // Ngăn xếp tạm để tính toán

            for (int i = 0; i < input.Length; i++) // Duyệt từng ký tự trong chuỗi hậu tố
            {
                // Nếu ký tự là số, đọc toàn bộ số và đẩy vào ngăn xếp
                if (char.IsDigit(input[i]) || input[i] == ',')
                {
                    string a = string.Empty;
                    while (!IsDelimiter(input[i]) && !IsOperator(input[i]))
                    {
                        a += input[i];
                        i++;
                        if (i == input.Length) break;
                    }
                    a = a.Replace(",", "0,");
                    temp.Push(Convert.ToDouble(a)); // Chuyển đổi chuỗi thành số và đẩy vào ngăn xếp
                    i--;
                }
                else if (IsOperator(input[i])) // Nếu ký tự là toán tử
                {
                    // Lấy hai giá trị trên đỉnh ngăn xếp
                    try
                    {
                        double a = temp.Pop();
                        double b = (input[i] == '-' && temp.Count == 0) ? 0 : temp.Pop();

                        // Thực hiện phép toán
                        switch (input[i])
                        {
                            case '+':
                                result = b + a; break;
                            case '-':
                                result = b - a; break;
                            case '*':
                                result = b * a; break;
                            case '#':
                                if (b < 0)
                                {
                                    Console.WriteLine("Cannot raise a negative number to a power!");
                                    return double.NaN;
                                }
                                result = Math.Pow(b, a); break;
                            case '/':
                                if (a == 0)
                                {
                                    Console.WriteLine("Division by zero!");
                                    return double.NaN;
                                }
                                result = b / a; break;
                        }
                        temp.Push(result); // Đẩy kết quả của phép toán vào ngăn xếp
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("Incorrect number of operators!");
                        return double.NaN;
                    }
                }
            }
            try
            {
                return temp.Peek(); // Lấy kết quả cuối cùng từ ngăn xếp
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No arithmetic operations found");
                return double.NaN;
            }
        }

        // Phương thức Calculate nhận biểu thức dạng chuỗi và trả về kết quả tính toán
        static public double Calculate(string input)
        {
            string output = GetExpression(input); // Chuyển đổi biểu thức sang hậu tố
            Console.WriteLine("Converted expression: " + output.Replace("#", "pow"));
            double result = Counting(output); // Tính toán biểu thức hậu tố
            return result; // Trả về kết quả
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter an expression: "); // Yêu cầu nhập biểu thức
                string parsestr = Console.ReadLine();
                if (parsestr == "") break;

                Console.WriteLine("Result: " + Calculate(parsestr)); // Đọc và hiển thị kết quả
            }
        }
    }
}
