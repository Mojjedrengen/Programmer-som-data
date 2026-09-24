// micro-C example 25 -- Takeuchi function, in McCarthy's version, see
// https://en.wikipedia.org/wiki/Tak_(function)

int count; // Number of calls made to the tak function 

void main() {
  count = 0;
  print tak(10, 5, 0);  // Should print 5 and 10345
  print count;
}

int tak(int x, int y, int z) {
  count = count + 1;
  if (y < x)
    return tak(tak(x - 1, y, z), tak(y - 1, z, x), tak(z - 1, x, y));
  else
    return z;
}
