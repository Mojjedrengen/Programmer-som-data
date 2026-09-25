void main(int n) {
  if (n > 20) {
    n = 20;
  }
  int arr[20];
  squares(n, arr);

  int i;
  i = 0;
  while (i < n) {
    print arr[i];
    println;
    i = i + 1;
  }
}

void squares(int n, int arr[]) {
  int i;
  i = 0;
  while (i < n) {
    arr[i] = i * i;
    i = i + 1;
  }
}
