using System.Globalization;
using System.Numerics;

namespace Polyvalent.Domain;

public sealed record FriedmanValidation(
    IntegerValue Integer,
    string Expression,
    bool IsOrderly,
    EvidenceWitness Witness);

public static class FriedmanExpression
{
    public static FriedmanValidation Validate(
        IntegerValue integer,
        string expression,
        bool claimedOrderly)
    {
        if (integer.Value <= 0)
        {
            throw new ArgumentException("friedman evidence requires a positive integer.", nameof(integer));
        }

        if (string.IsNullOrWhiteSpace(expression) || expression.Length > 128)
        {
            throw new FormatException("the expression must contain between 1 and 128 characters.");
        }

        var parser = new Parser(expression);
        var result = parser.Parse();
        if (parser.OperationCount == 0)
        {
            throw new FormatException("a friedman expression must be nontrivial.");
        }

        if (!result.IsInteger || result.Numerator != integer.Value)
        {
            throw new FormatException(
                $"the expression evaluates to {result}, not {integer}.");
        }

        var targetDigits = integer.ToString();
        var usedDigits = parser.UsedDigits;
        if (!SortDigits(targetDigits).SequenceEqual(SortDigits(usedDigits)))
        {
            throw new FormatException(
                $"the expression uses digits {usedDigits}, not exactly {targetDigits}.");
        }

        var orderly = string.Equals(targetDigits, usedDigits, StringComparison.Ordinal);
        if (claimedOrderly && !orderly)
        {
            throw new FormatException("the expression does not preserve the integer's digit order.");
        }

        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            expression,
            result = integer.ToString(),
            usedDigits,
            orderly,
        });

        return new FriedmanValidation(
            integer,
            expression,
            orderly,
            new EvidenceWitness("digit_expression", 1, payload, Pretty(expression)));
    }

    private static IEnumerable<char> SortDigits(string digits) => digits.OrderBy(character => character);

    private static string Pretty(string expression) => expression
        .Replace("*", " × ", StringComparison.Ordinal)
        .Replace("/", " ÷ ", StringComparison.Ordinal)
        .Replace("+", " + ", StringComparison.Ordinal)
        .Replace("-", " − ", StringComparison.Ordinal)
        .Replace("^", " ^ ", StringComparison.Ordinal);

    private readonly record struct Rational
    {
        public Rational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("division by zero is not valid friedman evidence.");
            }

            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            var divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            Numerator = numerator / divisor;
            Denominator = denominator / divisor;
            GuardMagnitude(Numerator);
            GuardMagnitude(Denominator);
        }

        public BigInteger Numerator { get; }

        public BigInteger Denominator { get; }

        public bool IsInteger => Denominator == BigInteger.One;

        public static Rational From(BigInteger value) => new(value, BigInteger.One);

        public static Rational operator +(Rational left, Rational right) =>
            new(
                left.Numerator * right.Denominator + right.Numerator * left.Denominator,
                left.Denominator * right.Denominator);

        public static Rational operator -(Rational left, Rational right) =>
            new(
                left.Numerator * right.Denominator - right.Numerator * left.Denominator,
                left.Denominator * right.Denominator);

        public static Rational operator *(Rational left, Rational right) =>
            new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

        public static Rational operator /(Rational left, Rational right) =>
            new(left.Numerator * right.Denominator, left.Denominator * right.Numerator);

        public Rational Negate() => new(-Numerator, Denominator);

        public Rational Pow(int exponent)
        {
            if (exponent < 0)
            {
                var positive = -exponent;
                return new Rational(
                    BigInteger.Pow(Denominator, positive),
                    BigInteger.Pow(Numerator, positive));
            }

            return new Rational(
                BigInteger.Pow(Numerator, exponent),
                BigInteger.Pow(Denominator, exponent));
        }

        public override string ToString() => IsInteger
            ? Numerator.ToString(CultureInfo.InvariantCulture)
            : $"{Numerator.ToString(CultureInfo.InvariantCulture)}/{Denominator.ToString(CultureInfo.InvariantCulture)}";

        private static void GuardMagnitude(BigInteger value)
        {
            if (value.GetBitLength() > 4096)
            {
                throw new FormatException("the expression exceeds the bounded arithmetic budget.");
            }
        }
    }

    private sealed class Parser
    {
        private readonly string _text;
        private readonly List<char> _usedDigits = [];
        private int _position;
        private int _tokenCount;

        public Parser(string text)
        {
            _text = text;
        }

        public string UsedDigits => new(_usedDigits.ToArray());

        public int OperationCount { get; private set; }

        public Rational Parse()
        {
            var value = ParseAdditive();
            SkipWhitespace();
            if (_position != _text.Length)
            {
                throw Error($"unexpected character '{_text[_position]}'.");
            }

            if (_usedDigits.Count == 0)
            {
                throw Error("the expression contains no digits.");
            }

            return value;
        }

        private Rational ParseAdditive()
        {
            var value = ParseMultiplicative();
            while (true)
            {
                if (Take('+'))
                {
                    OperationCount++;
                    value += ParseMultiplicative();
                }
                else if (Take('-'))
                {
                    OperationCount++;
                    value -= ParseMultiplicative();
                }
                else
                {
                    return value;
                }
            }
        }

        private Rational ParseMultiplicative()
        {
            var value = ParsePower();
            while (true)
            {
                if (Take('*'))
                {
                    OperationCount++;
                    value *= ParsePower();
                }
                else if (Take('/'))
                {
                    OperationCount++;
                    value /= ParsePower();
                }
                else
                {
                    return value;
                }
            }
        }

        private Rational ParsePower()
        {
            var value = ParseUnary();
            if (!Take('^'))
            {
                return value;
            }

            OperationCount++;
            var exponent = ParsePower();
            if (!exponent.IsInteger || exponent.Numerator < -32 || exponent.Numerator > 32)
            {
                throw Error("exponents must be integers between -32 and 32.");
            }

            return value.Pow((int)exponent.Numerator);
        }

        private Rational ParseUnary()
        {
            if (Take('+'))
            {
                return ParseUnary();
            }

            if (Take('-'))
            {
                return ParseUnary().Negate();
            }

            return ParsePrimary();
        }

        private Rational ParsePrimary()
        {
            if (Take('('))
            {
                var value = ParseAdditive();
                if (!Take(')'))
                {
                    throw Error("a closing parenthesis is required.");
                }

                return value;
            }

            SkipWhitespace();
            var start = _position;
            while (_position < _text.Length && _text[_position] is >= '0' and <= '9')
            {
                _usedDigits.Add(_text[_position]);
                _position++;
            }

            if (start == _position)
            {
                throw Error("a number or parenthesized expression is required.");
            }

            CountToken();
            var token = _text[start.._position];
            if (token.Length > 1 && token[0] == '0')
            {
                throw Error("concatenated numbers may not have leading zeroes.");
            }

            return Rational.From(BigInteger.Parse(token, CultureInfo.InvariantCulture));
        }

        private bool Take(char expected)
        {
            SkipWhitespace();
            if (_position >= _text.Length || _text[_position] != expected)
            {
                return false;
            }

            _position++;
            CountToken();
            return true;
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]))
            {
                _position++;
            }
        }

        private void CountToken()
        {
            _tokenCount++;
            if (_tokenCount > 64)
            {
                throw Error("the expression exceeds the token budget.");
            }
        }

        private FormatException Error(string message) =>
            new($"invalid friedman expression at position {_position}: {message}");
    }
}
