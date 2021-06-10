#!/bin/sh
for f in lang/*.lua; do luajit -bg $f ${f%.*}.bc; done
luajit -bg core.lua core.bc;
