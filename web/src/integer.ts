export function canonicalInteger(input: string): string {
  if (input.length === 0 || input.trim() !== input) {
    throw new Error("enter a canonical integer without surrounding whitespace.");
  }

  if (!/^(0|-?[1-9][0-9]*)$/.test(input) || input === "-0") {
    throw new Error("use canonical signed decimal form: no plus sign, leading zeroes, exponent, or decimal point.");
  }

  return BigInt(input).toString();
}

export function compareIntegers(left: string, right: string): number {
  const a = BigInt(left);
  const b = BigInt(right);
  return a < b ? -1 : a > b ? 1 : 0;
}
