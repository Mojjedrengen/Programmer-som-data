public class Ex26 {
  public static void Main(String[] args) {
    long range = long.Parse(args[0]);
    long count = 0;
    long i = 2;
    while (i < range) {
      if (isPrime(i))
	count = count + 1;
      i = i + 1;
    }
    Console.WriteLine("range = " + range + ", primes = " + count);
  }
  
  private static bool isPrime(long n) {
    long k = 2;
    while (k * k <= n && n % k != 0)
      k++;
    return n >= 2 && k * k > n;
  }
}
