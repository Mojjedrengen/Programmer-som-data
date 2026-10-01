// micro-C example on tail calls.
// Example refered to in slide deck chap11Continuations.pdf

int id(int b) {
  return b;
}

int f(int a) {
  int b;
  b = a + 1;
  return id(b);
}

void main() {
  print f(2);
}

