#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]; then
    clang --target=x86_64-pc-windows-msvc -Wall microvm.c -o microvm.exe
else
    clang -Wall microvm.c -o microvm
fi
