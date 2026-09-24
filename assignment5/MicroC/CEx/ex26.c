// micro-C example 26
// Counting prime numbers 

void main(int range) {
  int count;
  int i;
  count = 0;
  i = 2;
  while (i < range) {
    if (isPrime(i))
      count = count + 1;
    i = i + 1;
  }
  print range; print count; println;
}
  
int isPrime(int n) {
  int k;
  k = 2;
  while (k * k <= n && n % k != 0)
    k = k + 1;
  return n >= 2 && k * k > n;
}
