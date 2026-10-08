public class Ex26 {
  public static void main(String[] args) {
    long range = Long.parseLong(args[0]);
    long count = 0;
    long i = 2;
    while (i < range) {
      if (isPrime(i))
	count = count + 1;
      i = i + 1;
    }
    System.out.println("range = " + range + ", primes = " + count);
  }
  
  private static boolean isPrime(long n) {
    long k = 2;
    while (k * k <= n && n % k != 0)
      k++;
    return n >= 2 && k * k > n;
  }
}
