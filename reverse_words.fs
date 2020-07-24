#! /usr/bin/gforth

: not-empty?  dup 0 > ;
: (reverse)  parse-name not-empty? IF recurse THEN type space ;
: reverse  (reverse) cr ;

reverse the string to be reversed
