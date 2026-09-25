void main() {
  int arr[7];
  arr[0] = 1;
  arr[1] = 2;
  arr[2] = 1;
  arr[3] = 1;
  arr[4] = 2;
  arr[5] = 1;
  arr[6] = 0;
  int freq[4];
  int i;
  i = 0;
  while (i < 4) {
    freq[i] = 0;
    i = i + 1;
  }
  histogram(7, arr, 3, freq);
  int i;
  i = 0;
  while (i < 4) {
    print i;
    print freq[i];
    println;
    i = i + 1;
  }
}

void histogram(int n, int ns[], int max, int freq[]) {
  int i;
  i = 0;
  while (i < n) {
    int j;
    j = ns[i];
    if (j <= max) {
      freq[j] = freq[j] + 1;
    }
    i = i + 1;
  }
}
