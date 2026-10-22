// micro-C example 27
// Testing assign as an expression

void main() {
  int i;
  int j;
  print(i=4);  // 4
  println;
  
  j = i = 42;
  print (j);   // 42
  print (i);   // 42
  println;

  print ((i=2)+j);  // 44
  print (i);        // 2
  print (i=5+j);    // 47
  print (i);        // 47
  println;
}
