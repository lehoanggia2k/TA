using System;
using static Interpreter.Tokenizer;

namespace Interpreter
{
    public class ExpressionsHellper
    {
        public static void CheckStack(Interpreter.Context context)
        {
            if (context.Tokens.Count <= 0)
                throw new Exception("Unexpected simbol.");
        }

        public static Exception ThrowUnexpectedToken(Token token)
        {
            return new Exception($"Unexpected simbol {token.TokenString}.");
        }
    }
}
