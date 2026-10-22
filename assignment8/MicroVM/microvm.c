/* File microVM/microvm.c

   A unified-stack abstract machine and garbage collector for all
   micro languages, micro-C, list-C, micro-SML and micro-Java.

   nh@itu.dk * 2026-04-05
   sestoft@itu.dk * 2009-11-17, 2012-02-08

   See README.TXT on how to compile and test.
   
   The code assumes 64 bit machine using type int64_t for words and
   uint64_t unsigned words.

   Data representation in the stack s[...] and the heap:
    * All integers are tagged with a 1 bit in the least significant
      position, regardless of whether they represent program integers,
      return addresses, array base addresses or old base pointers
      (into the stack).  
    * All heap references are word-aligned, that is, the two least
      significant bits of a heap reference are 00.  
    * Integer constants and code addresses in the program array
      p[...] are not tagged.
   The distinction between integers and references is necessary for 
   the garbage collector to be precise (not conservative).

   The heap consists of 64-bit words, and the heap is divided into
   blocks.  A block has a one-word header block[0] followed by the
   block's contents: zero or more words block[i], i=1..n.

   A header has the below form, 64 bits
   ttttttttnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnngg
   where 
    - tttttttt is the block tag, all 0 for cons cells
    - nn....nn is the block length (excluding header). 22 bits for 32 bit
               and 54 bits for 64 bit.
    - gg       is the block's color

   The block color has this meaning:
   gg=00=White: block is dead (after mark, before sweep)
   gg=01=Grey:  block is live, children not marked (during mark)
   gg=11=Black: block is live (after mark, before sweep)
   gg=11=Blue:  block is on the freelist or orphaned

   A block of length zero is an orphan; it cannot be used 
   for data and cannot be on the freelist.  An orphan is 
   created when allocating all but the last word of a free block.
*/

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <inttypes.h>  // E.g., defines PRId64 formatting

// Check Windows
#if _WIN64
  #define WIN
#endif

// Convert integer macro to string macro, e.g., PPVER.
#define STR(x) #x
#define XSTR(x) STR(x)

// Check if compiled with gcc or clang
#if defined(__clang__)
  #define PPCOMP "clang"
  #define PPVER XSTR(__clang_major__)
#elif defined(__GNUC__)
  #define PPCOMP "gcc"
  #define PPVER XSTR(__GNUC__)
#else
  #define PPCOMP "Not compiled with clang or gcc."
  #define PPVER "unknown"
#endif

#if defined(_WIN64)
   #define OS "Windows"
#elif defined(__linux__)
   #define OS "Linux"
#elif defined(__APPLE__)
   #define OS "MacOS"
#else
   #define OS "Not recognized operating system"
#endif

#if _WIN64 || __x86_64__ || __ppc64__ || __aarch64__
  #define PPARCH "64 bit architecture"
#else
  #define PPARCH "not recognized architecture"
#endif

// Cross compilation types for guaranteed 64 bits across linux, macos and windows.
typedef int64_t word;
typedef uint64_t uword;

// Cross compilation formatting.
#define WORD_FMT PRId64
#define UWORD_FMT PRIu64

// Get the user time in milli-seconds
int getUserTime();

// Read instructions from a file, return array of instructions
word* readfile(char* filename);

#if defined(WIN)
  #include "utils_win.c"
#else
  #include "utils_unix.c"
#endif 

#define IsInt(v) (((v)&1)==1)
#define Tag(v) (((v)<<1)|1)
#define Untag(v) ((v)>>1)
#define TagPtr(v) ((v)|1)       // To make garbage collection for micro Java safe.
#define UntagPtr(v) ((v)&(~1))  // To make garbage collection for micro Java safe.

//#define TagPtr(v) (v)    
//#define UntagPtr(v) (v)


#define White 0
#define Grey  1
#define Black 2
#define Blue  3

#define BlockTag(hdr) (((hdr)>>56))
#define Length(hdr)   (((hdr)>>2)&0x003FFFFFFFFFFFFF)
#define Color(hdr)    ((hdr)&3)
#define Paint(hdr, color)  (((hdr)&(0xFFFFFFFFFFFFFFFC))|(color))

#define CONSTAG 0   // Used for cons cells in List-C
#define CLOSTAG 1   // Used for both closures and instance objects.
#define STRINGTAG 2 // Used for heap allocated strings, exercise.
#define ARRAYTAG 3  // Used for heap allocated arrays, exercise.
#define OBJECTTAG 4 // Used for heap allocated instance objects.

#define NILVALUE 0  // Used for NIL in micro-SML and null in micro-Java.

// Heap size in words
#define HEAPSIZE 20000

word* heap;
word* afterHeap;
word *freelist;

int silent=0; /* Glocal boolean value to run the interpreter in silent mode. Default false. */
int numGC=0;  // Glocal counter on number of GC.

// These numeric instruction codes must agree with MicroSML/Machine.fs:
// (Use #define because const int does not define a constant in C)

// Constants
#define CSTI 0  // Integer constant.
#define NIL 1   // NIL or null constant.

// Arithmetic
#define ADD 10   // addition
#define SUB 11   // subtraction
#define MUL 12   // multiplication
#define DIV 13   // division
#define MOD 14   // modulus

// Logical
#define EQ 20   // equality: s[sp-1] == s[sp]
#define LT 21   // less than: s[sp-1] < s[sp]
#define NOT 22  // logical negation: s[sp] != 0

// Stack
#define DUP 30   // duplicate stack top
#define SWAP 31  // swap s[sp-1] and s[sp]

// Address, load and store
#define LDI 40       // Stack, load indirect
#define STI 41       // Stack, store indirect
#define HEAPSTI 42   // Heap, store indirect
#define HEAPLDI 43   // Heap, load indirect
#define HEAPALLOC 44 // Allocate heap object with some tag.
#define HEAPCOPY 45  // Copy from stack to heap object.
#define LDD 46       // Load direct - both heap and stack.
#define STD 47       // Store direct - both heap and stack.
#define STACKADDR 48 // Calculate direct address on stack.
#define HEAPADDR 49  // Calculate direct address on heap.

// Stack and stack frame
#define GETBP 60     // Get base pointer
#define GETSP 61     // Get stack pointer
#define INCSP 62     // Increase stack top by m

// Code flow
#define GOTO 70      // Go to label 
#define IFZERO 71    // Go to label if s[sp] == 0
#define IFNZRO 72    // Go to label if s[sp] != 0
#define PUSHLAB 73   // Push label on stack

// Call and return
#define CALL 80      // Move m args up 2, push pc, bp and jump
#define TCALL 81     // Move m args down to bp, jump  
#define CLOSCALL 82  // Move m args up 2, push pc, bp and jump to addr in closure
#define TCLOSCALL 83 // Move m args down to bp, and jump to addr in closure
#define VCALL 84     // Move m args up 2, push pc, bp and jump to index into vTable
//#define TVCALL 85  // Exercise for compiling micro Java.
#define RET 86       // Pop m and return to s[sp]

// Print on std. out
#define PRINTI 90    // Print s[sp] as integer
#define PRINTN 91    // Print s[sp] as NIL or null
#define PRINTB 92    // Print s[sp] as true/false
#define PRINTO 93    // Print s[sp] as Object
#define PRINTNL 94   // Print new line
#define PRINTC 95    // Print s[sp] as character
#define PRINTL 96    // Print s[sp] as list
#define PRINTVAL 97  // Polymorphic print

// Start and stop program
#define LDARGS 100  // Load command line arguments on stack
#define STOP 101    // Stop program execution

// Cons / Pairs
#define CONS 110    // Allocate Cons cell.
#define CAR 111     // Load first component
#define CDR 112     // Load second component
#define SETCAR 113  // Set first component
#define SETCDR 114  // Set second component 

// Exceptions
#define THROW 120    // Search for exception handle and execute affiliated exception code
#define PUSHHDLR 121 // Push exception handler on stack 
#define POPHDLR 122  // Pop exception handler from stack


// We check for stack overflow in execcode inbetween execution of two byte code instructions.
// Such instructions can increate the stack arbitraily, e.g., INCSP. The STACKSAFETYSIZE
// is to have a buffer for this not to happen. 
#define STACKSAFETYSIZE 200
#define STACKSIZE 2000000
  
// Print the stack machine instruction at p[pc]
void printInstruction(word p[], word pc) {
  switch (p[pc]) {
  case CSTI:   printf("CSTI %" WORD_FMT, p[pc+1]); break;
  case ADD:    printf("ADD"); break;
  case SUB:    printf("SUB"); break;
  case MUL:    printf("MUL"); break;
  case DIV:    printf("DIV"); break;
  case MOD:    printf("MOD"); break;
  case EQ:     printf("EQ"); break;
  case LT:     printf("LT"); break;
  case NOT:    printf("NOT"); break;
  case DUP:    printf("DUP"); break;
  case SWAP:   printf("SWAP"); break;
  case LDI:    printf("LDI"); break;
  case STI:    printf("STI"); break;
  case GETBP:  printf("GETBP"); break;
  case GETSP:  printf("GETSP"); break;
  case INCSP:  printf("INCSP %" WORD_FMT, p[pc+1]); break;
  case GOTO:   printf("GOTO %" WORD_FMT, p[pc+1]); break;
  case IFZERO: printf("IFZERO %" WORD_FMT, p[pc+1]); break;
  case IFNZRO: printf("IFNZRO %" WORD_FMT, p[pc+1]); break;
  case CALL:   printf("CALL %" WORD_FMT " %" WORD_FMT, p[pc+1], p[pc+2]);
               break;
  case TCALL:  printf("TCALL %" WORD_FMT " %" WORD_FMT " %" WORD_FMT,
		      p[pc+1], p[pc+2], p[pc+3]);
               break;
  case RET:    printf("RET %" WORD_FMT, p[pc+1]); break;
  case PRINTI: printf("PRINTI"); break;
  case PRINTB: printf("PRINTB"); break;
  case PRINTN: printf("PRINTN"); break;
  case PRINTO: printf("PRINTO"); break;
  case LDARGS: printf("LDARGS"); break;
  case STOP:   printf("STOP"); break;
  case NIL:   printf("NIL"); break;
  case CONS:   printf("CONS"); break;
  case CAR:    printf("CAR"); break;
  case CDR:    printf("CDR"); break;
  case SETCAR: printf("SETCAR"); break;
  case SETCDR: printf("SETCDR"); break;
  case PUSHLAB: printf("PUSHLAB %" WORD_FMT, p[pc+1]); break;
  case HEAPSTI: printf("HEAPSTI "); break;
  case HEAPLDI: printf("HEAPLDI "); break;    
  case HEAPALLOC:   printf("HEAPALLOC %" WORD_FMT " %" WORD_FMT, p[pc+1], p[pc+2]); break;
  case CLOSCALL: printf("CLOSCALL %" WORD_FMT, p[pc+1]); break;
  case TCLOSCALL: printf("TCLOSCALL %" WORD_FMT, p[pc+1]); break;
  case THROW:     printf("THROW"); break;
  case PUSHHDLR:  printf("PUSHHDLR %" WORD_FMT, p[pc+1]); break;
  case POPHDLR:   printf("POPHDLR"); break;
  case PRINTNL: printf("PRINTNL"); break;    
  case VCALL:  printf("VCALL %" WORD_FMT " %" WORD_FMT, p[pc+1], p[pc+2]);
               break;
  case HEAPCOPY: printf("HEAPCOPY %" WORD_FMT, p[pc+1]); break;
  case LDD: printf("LDD"); break;
  case STD: printf("STD"); break;
  case STACKADDR: printf("STACKADDR"); break;
  case HEAPADDR: printf("HEAPADDR"); break;
  case PRINTC: printf("PRINTC"); break;
  case PRINTL: printf("PRINTL"); break;
  case PRINTVAL: printf("PRINTVAL"); break;
  default:     printf("<unknown> %" WORD_FMT, p[pc]); break; 
  }
}

// Print stack value as either scalar value or address.
void printStackVal(word w) {
  if (IsInt(w))
    printf("%" WORD_FMT " ", Untag(w));
  else
    printf("#%" WORD_FMT " ", w);
}

// Print current stack (marking heap references by #) and current instruction
void printStackAndPc(word s[], word bp, word sp, word p[], word pc) {
  word i;
  printf("[ ");
  for (i=0; i<=sp; i++)
    printStackVal(s[i]);
  printf("]");
  printf("{%" WORD_FMT ":", pc); printInstruction(p, pc); printf("}\n"); 
}

// Tags and values
word mkheader(uword tag, uword length, unsigned int color) {
  return (tag << 56) | (length << 2) | color;
}

void printI (word i) {
  printf("%" WORD_FMT " ", IsInt(i) ? Untag(i) : i);
}

void printB(word i) {
  if IsInt(i) {
    printf("%s ", Untag(i)?"true":"false");
  } else {
    printf("PRINTB applied on non scalar value.\n");
    exit(-1);
  }
  return;
}       

void printC(word i) {
  if IsInt(i) {
    printf("%c", (char)Untag(i));
  } else {
    printf("PRINTC applied on non scalar value.\n");
    exit(-1);
  }
  return;
}

void printList(word i) {
  //  printf("in printList " WORD_FMT "\n",i);
  if (i == NILVALUE) {
    printf("[] ");
  } else {
    word *consPtr = (word *)i;
    int done;    
    if (!(BlockTag(*consPtr) == CONSTAG)) {
      printf("PRINTL: Expected CONSTAG.\n");
      exit(-1);
    }
    printf("[");
    done=0;
    do {
      word hd = consPtr[1];
      word tl = consPtr[2];
      if (IsInt(hd)) printf("%" WORD_FMT, Untag(hd));
      else {
        //int *vPtr = (int *)hd;
	if (hd == NILVALUE || (BlockTag(*((word *)hd)) == CONSTAG)) printList(hd);
        else printf("Unexpected hd=%" WORD_FMT "\n", hd);
      }
      if (tl != NILVALUE) {
        consPtr = (word *)tl;
	printf(",");
      }
      else
	done = 1;
    } while (!done);
    printf("] ");
  } 
  return;
}

void printNil(word i) {
  if (i == NILVALUE) {
    printf("null ");
  } else {
    printf("PRINTN applied on non null value.\n");
    exit(-1);
  }
  return;
}

void printObject(word i) {
  if (i == NILVALUE) {
    printf("null ");
  } else {
    word *objPtr = (word *)i;
    if (BlockTag(*objPtr) == OBJECTTAG) {
      printf("@%p ", objPtr);
    } else {
      printf("microVM, printObject: Expected object tag.");
    }
  }
  return;
}

void printClosure(word *closPtr) {
  word numFreeVars = Length(closPtr[0]) - 1;
  word fnPtr = Untag(closPtr[1]);
  printf("Fn@%" WORD_FMT "(%" WORD_FMT ") ", fnPtr, numFreeVars);
}

void printVal(word i) {
  // print all values - scalar as integers, pointers using tag
  if IsInt(i) {
    printI(i);  // A scalar value printed as integer
    return;
  }
  if (i == NILVALUE) {
    printNil(i);  // A pointer null value.
    return;
  }
  // Reference, look at tag
  word *valPtr = (word *)i;
  if (BlockTag(*valPtr) == CONSTAG) {
    printList(i);  // We have a list type  
    return;
  }
  if (BlockTag(*valPtr) == CLOSTAG) {
    printClosure(valPtr);  // We have a closure value.
    return;
  } 
  if (BlockTag(*valPtr) == STRINGTAG) {
    printf("microVM, Print: string not implemented."); // We have a string value.
    return;
  }
  if (BlockTag(*valPtr) == ARRAYTAG) {
    printf("microVM, Print: arrays not implemented."); // We have an array value.
    return;
  } 
  if (BlockTag(*valPtr) == OBJECTTAG) {
    printObject(i);  // We have an object value
    return;
  } 
}

void printNL() {
  printf("\n");
}

// Garbage collection and heap allocation 
int inHeap(word* p) {
  return heap <= p && p < afterHeap;
}

// Call this after a GC to get heap statistics:
void heapStatistics() {
  word blocks = 0, free = 0, orphans = 0, 
    blocksSize = 0, freeSize = 0, largestFree = 0;
  word* heapPtr = heap;
  word* freePtr;  
  while (heapPtr < afterHeap) {
    word* nextBlock;    
    if (Length(heapPtr[0]) > 0) {
      blocks++;
      blocksSize += Length(heapPtr[0]);
    } else 
      orphans++;
    nextBlock = heapPtr + Length(heapPtr[0]) + 1;
    if (nextBlock > afterHeap) {
      printf("heapStatistics HEAP ERROR: block at heap[%" WORD_FMT "] (%" WORD_FMT
	     "), length %" WORD_FMT " extends beyond heap\n", 
	     (word)(heapPtr-heap),(word)&heapPtr[0], Length(heapPtr[0]));
      exit(-1);
    }
    heapPtr = nextBlock;
  }
  freePtr = freelist;
  while (freePtr != 0) {
    int length;
    free++; 
    length = Length(freePtr[0]);
    if (freePtr < heap || afterHeap < freePtr+length+1) {
      printf("HEAP ERROR: freelist item %" WORD_FMT " (at heap[%" WORD_FMT "], length %d) is outside heap\n", 
	     free, (word)(freePtr-heap), length);
      exit(-1);
    }
    freeSize += length;
    largestFree = length > largestFree ? length : largestFree;
    if (Color(freePtr[0])!=Blue)
      printf("Non-blue block at heap[%" UWORD_FMT "] on freelist\n", (uword)freePtr);
    freePtr = (word*)freePtr[1];
  }
  if (!silent)
    printf("Heap: %" WORD_FMT " blocks (%" WORD_FMT " words); of which %" WORD_FMT " free (%" WORD_FMT " words, largest %" WORD_FMT " words); %" WORD_FMT " orphans\n", 
	   blocks, blocksSize, free, freeSize, largestFree, orphans);
}

void initheap() {
  heap = (word*)malloc(sizeof(word)*HEAPSIZE);
  afterHeap = &heap[HEAPSIZE];
  // Initially, entire heap is one block on the freelist:
  heap[0] = mkheader(0, HEAPSIZE-1, Blue);
  heap[1] = (word)0;
  freelist = &heap[0];
}

void markPhase(word s[], word sp) {
  if (!silent) printf("GC[");
  if (!silent) printf("M");
}

void sweepPhase() {
  if (!silent) printf(",");
  if (!silent) printf("BS]");
}

void collect(word s[], word sp) {
  markPhase(s, sp);
  sweepPhase();
  heapStatistics();
  numGC++;
}

word* allocate(unsigned int tag, uword length, word s[], word sp) {
  int attempt = 1;
  do {
    word* free = freelist;
    word** prev = &freelist;
    while (free != 0) {
      word rest = Length(free[0]) - length;
      if (rest >= 0)  {
        if (rest == 0) // Exact fit with free block
	  *prev = (word*)free[1];
        else if (rest == 1) { // Create orphan (unusable) block
          *prev = (word*)free[1];
          free[length+1] = mkheader(0, rest-1, Blue);
	} else { // Make previous free block point to rest of this block
          *prev = &free[length+1];
          free[length+1] = mkheader(0, rest-1, Blue);
          free[length+2] = free[1];
        }
        free[0] = mkheader(tag, length, White);
        return free;
      }
      prev = (word**)&free[1];
      free = (word*)free[1];
    }
    // No free space, do a garbage collection and try again
    if (attempt==1)
      collect(s, sp);
  } while (attempt++ == 1);
  printf("Out of memory\n");
  exit(1);
}
  
// The machine: execute the code starting at p[pc] 
int execcode(word p[], word s[], word iargs[], int iargc, int /* boolean */ trace) {
  word bp = -999;        // Base pointer, for local variable access 
  word sp = -1;          // Stack top pointer
  word pc = 0;           // Program counter: next instruction
  word hr = -1;          // Handler Register
  for (;;) {
    if (STACKSIZE-sp <= 0) {
      printf("Stack overflow");
      return 0;
    }
    if (trace) 
      printStackAndPc(s, bp, sp, p, pc);
    switch (p[pc++]) {
    case CSTI:
      s[sp+1] = Tag(p[pc++]); sp++; break;
    case ADD: 
      s[sp-1] = Tag(Untag(s[sp-1]) + Untag(s[sp])); sp--; break;
    case SUB: 
      s[sp-1] = Tag(Untag(s[sp-1]) - Untag(s[sp])); sp--; break;
    case MUL: 
      s[sp-1] = Tag(Untag(s[sp-1]) * Untag(s[sp])); sp--; break;
    case DIV: 
      s[sp-1] = Tag(Untag(s[sp-1]) / Untag(s[sp])); sp--; break;
    case MOD: 
      s[sp-1] = Tag(Untag(s[sp-1]) % Untag(s[sp])); sp--; break;
    case EQ: 
      s[sp-1] = Tag(s[sp-1] == s[sp] ? 1 : 0); sp--; break;
    case LT: 
      s[sp-1] = Tag(s[sp-1] < s[sp] ? 1 : 0); sp--; break;
    case NOT: {
      word v = s[sp];
      s[sp] = Tag((IsInt(v) ? Untag(v) == 0 : v == 0) ? 1 : 0);
    } break;
    case DUP: 
      s[sp+1] = s[sp]; sp++; break;
    case SWAP: 
      { word tmp = s[sp];  s[sp] = s[sp-1];  s[sp-1] = tmp; } break; 
    case LDI:                 // load indirect
      s[sp] = s[Untag(s[sp])]; break;
    case STI: // s, i, v -> s, v; s[i] = v store indirect, keep value on top
      s[Untag(s[sp-1])] = s[sp]; s[sp-1] = s[sp]; sp--; break;
    case GETBP:
      s[sp+1] = Tag(bp); sp++; break;
    case GETSP:
      s[sp+1] = Tag(sp); sp++; break;
    case INCSP:
      sp = sp+p[pc++]; break;
    case GOTO:
      pc = p[pc]; break;
    case IFZERO: { 
      word v = s[sp--];
      pc = (IsInt(v) ? Untag(v) == 0 : v == 0) ? p[pc] : pc+1; 
    } break;
    case IFNZRO: { 
      word v = s[sp--];
      pc = (IsInt(v) ? Untag(v) != 0 : v != 0) ? p[pc] : pc+1; 
    } break;
    case CALL: { 
      word argc = p[pc++];
      int i;
      for (i=0; i<argc; i++)               // Make room for return address
        s[sp-i+2] = s[sp-i];               // and old base pointer
      s[sp-argc+1] = Tag(pc+1); sp++; 
      s[sp-argc+1] = Tag(bp);   sp++; 
      bp = sp+1-argc;
      pc = p[pc]; 
    } break; 
    case TCALL: { 
      word argc = p[pc++];                  // Number of new arguments
      word pop  = p[pc++];                  // Number of variables to discard
      word i;
      for (i=argc-1; i>=0; i--)    // Discard variables
        s[sp-i-pop] = s[sp-i];
      sp = sp - pop; pc = p[pc]; 
    } break; 
    case RET: { 
      word res = s[sp];
      sp = sp-p[pc]; bp = Untag(s[--sp]); pc = Untag(s[--sp]); 
      s[sp] = res; 
    } break; 
    case PRINTI: // s, i -> s, i ; outputs i, leave i on stack.
      printI(s[sp]);
      break;
    case PRINTB: // s, b -> s, b ; outputs b, leave b on stack.
      printB(s[sp]);
      break;
    case PRINTN: // s, nil -> s, nil ; outputs nil, leaves nil on stack.
      printNil(s[sp]); break; 
    case PRINTO: // s, p -> s, p ; outputs object location p, leave p on stack.
      printObject(s[sp]); break; 
    case LDARGS: {
      int i;
      word n = p[pc++]; // Number of expected arguments
      if (n != iargc) {
	printf("Program expects %" WORD_FMT " argument(s), but got %d.", n, iargc);
	return 0;
      }
      for (i=0; i<iargc; i++) // Push commandline arguments
        s[++sp] = Tag(iargs[i]);
    } break;
    case STOP:
      if (!silent) {
	printf("\nResult value: ");
	printStackVal(s[sp]);
      }
      return 0;
    case NIL:    
      s[sp+1] = NILVALUE; sp++; break;
    case CONS: {
      word* p = allocate(CONSTAG, 2, s, sp); 
      p[1] = (word)s[sp-1];
      p[2] = (word)s[sp];
      s[sp-1] = (word)p;
      sp--;
    } break;
    case CAR: {
      word* p = (word*)s[sp]; 
      if (p == 0) 
        { printf("Cannot take car of null\n"); return -1; }
      s[sp] = (word)(p[1]);
    } break;
    case CDR: {
      word* p = (word*)s[sp]; 
      if (p == 0) 
        { printf("Cannot take cdr of null\n"); return -1; }
      s[sp] = (word)(p[2]);
    } break;
    case SETCAR: {
      word v = (word)s[sp--];
      word* p = (word*)s[sp]; 
      p[1] = v;
    } break;
    case SETCDR: {
      word v = (word)s[sp--];
      word* p = (word*)s[sp]; 
      p[2] = v;
    } break;
    case PUSHLAB: {
      s[++sp] = (word)(Tag(p[pc++]));
    } break;
    case HEAPCOPY: { // s, v_1, ..., v_n, p -> s, p
      word n = p[pc++];
      word* ptr = (word *)s[sp];
      int i;
      for (i=0;i<n;i++)
        ptr[i+1] = (word)s[sp-n+i];  // p[0] is the heap allocated tag.
      s[sp-n] = s[sp];               // Move pointer to heap object.
      sp = sp-n;                     // Pointer to heap object now top of stack.
    } break;
    case HEAPLDI: {
      word offset = p[pc++];
      word* ptr = (word*)s[sp];
      s[sp] = (word)ptr[offset+1];      // +1 to accomodate for the closure tag.
    } break;
    case HEAPSTI: {   
      word index = Untag(s[sp-1]);      
      word* ptr = (word*)s[sp-2];
      ptr[index + 1] = (word)s[sp];  // p[0] is the heap allocated tag.
      s[sp-2] = s[sp];  // Keep value on stack.
      sp = sp-2;  
    } break;
    case HEAPALLOC: { // s -> s, p
      word tag = p[pc++];  // Tag of allocated object.
      word n = p[pc++];    // size of object, n>0 as first index is mandatory code pointer or class descriptor pointer
      word* ptr = allocate(tag, n, s, sp);
      int i;
      for (i=0;i<n;i++)
	// Init storage scalar values in case gc is invoked before data is filled in with HEAPSTI.
	// Could happen with mutually recursive functions.
	ptr[i+1] = Tag(0); 
      s[++sp] = (word)ptr;
    } break;
    case CLOSCALL: {
      word argc = p[pc++];
      int i;
      word* cp;
      argc++;                        // Closure is additional first argument.
      for (i=0; i<argc; i++)         // Make room for return address
        s[sp-i+2] = s[sp-i];         // and old base pointer
      s[sp-argc+1] = Tag(pc); sp++; 
      s[sp-argc+1] = Tag(bp); sp++; 
      bp = sp+1-argc;
      cp = (word*)s[bp];             // cp is pointer to closure.
      pc = Untag(cp[1]);             // Label is a tagged scalar at index 1, see PUSHLAB.
    } break;
    case TCLOSCALL: {
      word argc = p[pc++];
      word pop;
      word i;
      word* cp;
      argc++;                           // Closure is additional first argument.
      pop = sp-bp-argc+1;               // Number of variables to discard
      if (pop < 0) printf("PANIC\n");
      for (i=argc-1; i>=0; i--)         // Tail call, do not touch existing return address
        s[sp-i-pop] = s[sp-i];          // and old base pointer
      sp = sp - pop;
      cp = (word*)s[bp];                // cp is pointer to closure.
      pc = Untag(cp[1]);                // Label is a tagged scalar at index 1, see PUSHLAB.
    } break;
    case THROW: { // stack,exnVal1,exnlab,prevHr,...,exnVal2 -> stack if exnVal1 = exnVal2
      word exn = Untag(s[sp]);
      while (hr != -1 && Untag(s[hr]) != exn) {
	hr = Untag(s[hr+2]);           // Try next exception handler
      }
      if (hr != -1) {           // Found a handler for exn
        pc = Untag(s[hr+1]);    //   execute the handler code (exnlab)
	sp = hr-1; 	        //    after popping frames above hr
	hr = Untag(s[hr+2]);    //   with current handler being hr 
	while (bp > sp)  
	  bp = Untag(s[bp-1]);  // Restore bp to stack frame containing the exception handler description.
      } else {
	printf("\nResult value: Uncaught exception %" WORD_FMT " ", exn);
	return 0;
      }
    } break;
    case PUSHHDLR: { // stack,exn  -> stack,exn,lab,prevHr
      s[++sp] = (word)(Tag(p[pc++]));
      s[++sp] = Tag(hr);
      hr = sp-2;
    } break;
    case POPHDLR: { // stack,exn,lab,prevHr,v -> stack,v
      hr = Untag(s[sp-1]);
      s[sp-3] = s[sp];
      sp = sp - 3;
    } break;
    case PRINTNL: // s -> s ; output newline and leave stack untouched.
      printNL(); break;
    case VCALL: {
      word argc = p[pc++];
      word index = p[pc++];  // Index into vTable.
      int i;
      for (i=0; i<argc; i++) {              // Make room for return address
        s[sp-i+2] = s[sp-i];                // and old base pointer
      }
      s[sp-argc+1] = Tag(pc); sp++; 
      s[sp-argc+1] = Tag(bp);   sp++; 
      bp = sp-argc+1;
      word *objPtr = (word *)s[bp];   // Pointer to object in heap - first argument.
      word vtAddr = Untag(objPtr[1]); // First address is address of vTable in program code. Slot 0 is heap tag.
      word mthAddr = p[vtAddr+index];
      pc = mthAddr;  // Jump via vTable
    } break;

    case LDD: { // s, p -> s, *p - a is a physical address and untagged.
      word *objPtr = (word *)(UntagPtr(s[sp]));
      s[sp] = *objPtr;
    } break;

    case STD: { // s, p, v -> s, *p = v
      word v = s[sp--];  // Just copying word so no need to untag / tag.
      word *objPtr = (word *)(UntagPtr(s[sp]));
      *objPtr = v;
      s[sp] = v;
    } break;

    case STACKADDR: { // s, i -> s, &s[i]
      word i = Untag(s[sp]);
      s[sp] = TagPtr((word)&(s[i]));  // Convert pointer to word type.
    } break;

    case HEAPADDR: { // s, p, i -> s, &p[i+1]
      word i = Untag(s[sp--]);
      word *objPtr = (word *)s[sp];
      s[sp] = TagPtr((word)&(objPtr[i+1]));  // To accomodate for the closure tag
    } break;
    case PRINTC:
      printC(s[sp]); break;
    case PRINTL:
      printList(s[sp]); break;
    case PRINTVAL:
      printVal(s[sp]); break;
    default:
      printf("Illegal instruction %" WORD_FMT " at address %" WORD_FMT " (%" WORD_FMT ")\n", p[pc-1], pc-1, (word)&p[pc-1]);
      heapStatistics();
      return -1;
    }
  }
}

// Read program from file, and execute it
int execute(int argc, char** argv, int trace) {
  int filenameidx = 1 + (trace?1:0) + (silent?1:0); // Index to filename depends on interpreter options.
  int argsidx = filenameidx+1; // Index to extra program arguments depends on interpreter options. 
  word *p = readfile(argv[filenameidx]);         // program bytecodes: int[]
  word *s = (word*)malloc(sizeof(word)*(STACKSIZE+STACKSAFETYSIZE));   // stack: int[] 
  int iargc = argc-argsidx;
  word *iargs = (word*)malloc(sizeof(word)*iargc);   // program inputs: int[]
  
  int i;
  int t1, t2;
  int res;
  int runtime;

  for (i=0; i<iargc; i++) {                         // Convert commandline arguments
    if (strcmp(argv[i+argsidx],"true") == 0)
      iargs[i] = 1;
    else if (strcmp(argv[i+argsidx],"false") == 0)
      iargs[i] = 0;
    else iargs[i] = atoi(argv[i+argsidx]);
  }

  // Initialize global GC counter.
  numGC = 0;  

  // Measure cpu time for executing the program
  t1 = getUserTimeMs();
  res = execcode(p, s, iargs, iargc, trace);  // Execute program proper
  t2 = getUserTimeMs();
  runtime = t2 - t1;
  if (!silent) printf("\nUsed %d cpu milli-seconds\n", runtime);

  if (!silent) printf("Number of GC: %d\n", numGC);

  return res;
}

// Read code from file and execute it
int main(int argc, char** argv) {
  if (sizeof(word) != 8 ||
      sizeof(word*) != 8 ||
      sizeof(uword) != 8) {
     printf("Size of word, word* is not 64 bit, cannot run\n");
     return -1;
   }
  
  if (argc < 2) {
    printf("micro virtual machine for %s, running on %s\n", PPARCH, OS);
    printf("Compiled with %s version %s on %s at %s\n", PPCOMP, PPVER,  __DATE__, __TIME__);
    printf("Usage: microvm [-trace] [-silent] <programfile> <arg1> ...\n");
    return -1;
  } else {
    int trace = argc >= 3 && (0==strncmp(argv[1], "-trace", 7) || 0==strncmp(argv[2], "-trace", 7));
    silent = argc >= 3 && (0==strncmp(argv[1], "-silent", 7) || 0==strncmp(argv[2], "-silent", 7));
    initheap();
    return execute(argc, argv, trace);
  }
}
