void main(int n) {
  int a[4];
  a[0] = 7;
  a[1] = 13;
  a[2] = 9;
  a[3] = 8;
  int sump;
  sump = 0;
  arrsum(n, a, &sump);
  print sump;
  println;
}

void arrsum(int n, int arr[], int *sump) {
  while (n > 0) {
    *sump = *sump + arr[n - 1];
    n = n - 1;
  }
}
