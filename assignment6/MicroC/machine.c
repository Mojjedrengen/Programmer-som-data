/* File MicroC/machine.c
   A unified-stack abstract machine for imperative programs.
   sestoft@itu.dk * 2009-10-18, 2026-06-17

   To compile:
     clang -Wall machine.c -o machine

   To execute a program file using this abstract machine, do:
      machine <programfile> <arg1> <arg2> ...
   To get also a trace of the program execution:
      machine -trace <programfile> <arg1> <arg2> ...
*/

#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <inttypes.h>  // E.g., defines PRId64 formatting

// Check Windows
#if _WIN64
  #define WIN
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

// These numeric instruction codes must agree with MicroC/Machine.fs:
// (Use #define because const int does not define a constant in C)

// Constants
#define CSTI 0   // Integer constant.

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
#define DUP 30    // duplicate stack top
#define SWAP 31   // swap s[sp-1] and s[sp]

// Address, load and store
#define LDI 40    // Stack, load indirect
#define STI 41    // Stack, store indirect

// Stack and stack frame
#define GETBP 60  // Get base pointer
#define GETSP 61  // Get stack pointer
#define INCSP 62  // Increase stack top by m

// Code flow
#define GOTO 70     // Go to label 
#define IFZERO 71   // Go to label if s[sp] == 0
#define IFNZRO 72   // Go to label if s[sp] != 0

// Call and return
#define CALL 80     // Move m args up 2, push pc, bp and jump
#define TCALL 81    // Move m args down to bp, jump  
#define RET 86      // Pop m and return to s[sp]

// Print on std. out
#define PRINTI 90    // Print s[sp] as integer
#define PRINTNL 94   // Print newline, leave stack untouched.

// Start and stop program
#define LDARGS 100  // Load command line arguments on stack
#define STOP 101    // Stop program execution

#define STACKSIZE 1000
  
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
  case CALL:   printf("CALL %" WORD_FMT " %" WORD_FMT, p[pc+1], p[pc+2]); break;
  case TCALL:  printf("TCALL %" WORD_FMT " %" WORD_FMT " %" WORD_FMT,
		      p[pc+1], p[pc+2], p[pc+3]); break;
  case RET:    printf("RET %" WORD_FMT, p[pc+1]); break;
  case PRINTI: printf("PRINTI"); break;
  case PRINTNL: printf("PRINTNL"); break;
  case LDARGS: printf("LDARGS %" WORD_FMT, p[pc+1]); break;
  case STOP:   printf("STOP"); break;
  default:     printf("<unknown>"); break; 
  }
}

// Print current stack and current instruction
void printStackAndPc(word s[], word bp, word sp, word p[], word pc) {
  word i;
  printf("[ ");
  for (i=0; i<=sp; i++)
    printf("% " WORD_FMT, s[i]);
  printf("]");
  printf("{%" WORD_FMT ":", pc); printInstruction(p, pc); printf("}\n"); 
}

// The machine: execute the code starting at p[pc] 
int execcode(word p[], word s[], word iargs[], word iargc, word /* boolean */ trace) {
  word bp = -999;	// Base pointer, for local variable access 
  word sp = -1;	        // Stack top pointer
  word pc = 0;		// Program counter: next instruction
  for (;;) {
    if (STACKSIZE-sp <= 0) {
      printf("Stack overflow");
      return 0;
    }
    if (trace) 
      printStackAndPc(s, bp, sp, p, pc);
    switch (p[pc++]) {
    case CSTI:
      s[sp+1] = p[pc++]; sp++; break;
    case ADD: 
      s[sp-1] = s[sp-1] + s[sp]; sp--; break;
    case SUB: 
      s[sp-1] = s[sp-1] - s[sp]; sp--; break;
    case MUL: 
      s[sp-1] = s[sp-1] * s[sp]; sp--; break;
    case DIV: 
      s[sp-1] = s[sp-1] / s[sp]; sp--; break;
    case MOD: 
      s[sp-1] = s[sp-1] % s[sp]; sp--; break;
    case EQ: 
      s[sp-1] = (s[sp-1] == s[sp] ? 1 : 0); sp--; break;
    case LT: 
      s[sp-1] = (s[sp-1] < s[sp] ? 1 : 0); sp--; break;
    case NOT: 
      s[sp] = (s[sp] == 0 ? 1 : 0); break;
    case DUP: 
      s[sp+1] = s[sp]; sp++; break;
    case SWAP: 
      { int tmp = s[sp];  s[sp] = s[sp-1];  s[sp-1] = tmp; } break; 
    case LDI:                 // load indirect
      s[sp] = s[s[sp]]; break;
    case STI:                 // store indirect, keep value on top
      s[s[sp-1]] = s[sp]; s[sp-1] = s[sp]; sp--; break;
    case GETBP:
      s[sp+1] = bp; sp++; break;
    case GETSP:
      s[sp+1] = sp; sp++; break;
    case INCSP:
      sp = sp+p[pc++]; break;
    case GOTO:
      pc = p[pc]; break;
    case IFZERO:
      pc = (s[sp--] == 0 ? p[pc] : pc+1); break;
    case IFNZRO:
      pc = (s[sp--] != 0 ? p[pc] : pc+1); break;
    case CALL: { 
      int argc = p[pc++];
      int i;
      for (i=0; i<argc; i++)		   // Make room for return address
	s[sp-i+2] = s[sp-i];		   // and old base pointer
      s[sp-argc+1] = pc+1; sp++; 
      s[sp-argc+1] = bp;   sp++; 
      bp = sp+1-argc;
      pc = p[pc]; 
    } break; 
    case TCALL: { 
      int argc = p[pc++];                  // Number of new arguments
      int pop  = p[pc++];		   // Number of variables to discard
      int i;
      for (i=argc-1; i>=0; i--)	   // Discard variables
	s[sp-i-pop] = s[sp-i];
      sp = sp - pop; pc = p[pc]; 
    } break; 
    case RET: { 
      int res = s[sp]; 
      sp = sp-p[pc]; bp = s[--sp]; pc = s[--sp]; 
      s[sp] = res; 
    } break; 
    case PRINTI:
      printf("%" WORD_FMT " ", s[sp]); break; 
    case PRINTNL:
      printf("\n"); break;
      // printf("%c", (char)(s[sp])); break; todo
    case LDARGS: {
      int i;
      word n = p[pc++]; // Number of expected arguments
      if (n != iargc) {
	printf("Program expects %" WORD_FMT " argument(s), but got %" WORD_FMT ".", n, iargc);
	return 0;
      }
      for (i=0; i<iargc; i++) // Push commandline arguments
	s[++sp] = iargs[i];
    } break;
    case STOP:
      return 0;
    default:
      printf("Illegal instruction %" WORD_FMT "  at address %" WORD_FMT "\n",
	     p[pc-1], pc-1);
      return -1;
    }
  }
}

// Read program from file, and execute it
int execute(int argc, char** argv, int trace) {
  int filenameidx = 1 + (trace?1:0); // Index to filename depends on interpreter options. 
  int argsidx = filenameidx+1; // Index to extra program arguments depends on interpreter options. 
  word *p = readfile(argv[trace ? 2 : 1]);         // program bytecodes: word[]
  word *s = (word*)malloc(sizeof(word)*STACKSIZE);   // stack: word[]
  int iargc = argc-argsidx;
  word *iargs = (word*)malloc(sizeof(word)*iargc);   // program inputs: word[]

  int i;
  int t1, t2;
  int res;
  int runtime;
  
  for (i=0; i<iargc; i++)                         // Convert commandline arguments
    iargs[i] = atoi(argv[trace ? i+3 : i+2]);
  // Measure cpu time for executing the program
  t1 = getUserTimeMs();
  res = execcode(p, s, iargs, iargc, trace);  // Execute program proper
  t2 = getUserTimeMs();  
  runtime = t2 - t1;
  printf("\nUsed %d cpu milli-seconds\n", runtime);  
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
    printf("Usage: machine [-trace] <programfile> <arg1> ...\n");
    return -1;
  } else {
    int trace = argc >= 3 && 0==strncmp(argv[1], "-trace", 7);
    return execute(argc, argv, trace);
  }
}
