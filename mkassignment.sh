#!/bin/bash

echo "assignment$1"

if [ ! -z $1 ]
then 
    echo $1
    mkdir "assignment$1"
    mkdir "assignment$1/assignment"
    touch "assignment$1/TouchedFiles.md"
    touch "assignment$1/assignment/README.md"
else 
    echo "null"
fi
