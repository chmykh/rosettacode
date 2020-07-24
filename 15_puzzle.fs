#! /usr/bin/gforth

\ The implemented algorithm is https://en.wikipedia.org/wiki/Iterative_deepening_depth-first_search.
\ The search depth is limited NOT by recursion depth but by accumulated heuristic weight of a path traversed so far.
\ I don't know if there is any proof this approach gives an optimal solution; the idea is taken from
\ http://rosettacode.org/wiki/15_puzzle_solver.

cell 8 <> [if] s" 64-bit system required" exception throw [then]

\ In the stack comments below,
\ "h" stands for the hole position (0..15),
\ "s" for a 64-bit integer representing a board state,
\ "t" a tile value (0..15, 0 is the hole),
\ "b" for a bit offset of a position within a state,
\ "m" for a masked value (4 bits selected out of a 64-bit state),
\ "w" for a weight of a current path,
\ "d" for a direction constant (0..3)

: 3dup   2 pick 2 pick 2 pick ;
: 4dup   2over 2over ;
: shift   dup 0 > if lshift else negate rshift then ;

hex 123456789abcdef0 decimal constant solution
: row   2 rshift ;   : col   3 and ;

: up-valid?    ( h -- f ) row 0 > ;
: down-valid?  ( h -- f ) row 3 < ;
: left-valid?  ( h -- f ) col 0 > ;
: right-valid? ( h -- f ) col 3 < ;

: up-cost    ( h t -- 0|1 ) 1 - row swap row < 1 and ;
: down-cost  ( h t -- 0|1 ) 1 - row swap row > 1 and ;
: left-cost  ( h t -- 0|1 ) 1 - col swap col < 1 and ;
: right-cost ( h t -- 0|1 ) 1 - col swap col > 1 and ;

: ith ( u addr -- w ) swap cells + @ ;
create valid? ' up-valid? , ' left-valid? , ' right-valid? , ' down-valid? , does> ith execute ;
create cost ' up-cost , ' left-cost , ' right-cost , ' down-cost , does> ith execute ;
create step -4 , -1 , 1 , 4 , does> ith ;

: bits ( h -- b ) 15 swap - 4 * ;
: tile ( s b -- t ) rshift 15 and ;
: new-state ( s h d -- s' ) step dup >r + bits 2dup tile ( s b t ) swap lshift tuck - swap r> 4 * shift + ;
: new-weight ( w s h d -- w' ) >r tuck r@ step + bits tile r> cost + ;
: advance ( w s h d -- w s h w' s' h' ) 4dup new-weight >r  3dup new-state >r  step over + 2r> rot ;

\ : advance { w s h d -- w s h w' s' h' }
\	w s h
\	h d step + { h' } h' bits { b } s b tile { t } t b lshift { m }
\	w h t d cost +  s m - m d step 4 * shift +  h' ;

: rollback   2drop drop ;
: .dir ( u -- ) s" d..r.l..u" drop 4 + swap + c@ emit ;
: .dirs ( .. -- ) 0 begin >r 3 pick -1 <> while 3 pick over - .dir rollback r> 1+ repeat r> ;
: win   cr ." solved (read right-to-left!): " .dirs ."  - " . ." moves" bye ;

create limit 1 ,   : deeper  1 limit +! ;   create iter 0 ,

: .solve   cr ." search in progress, any key interrupts " cr ." dot=1M states checked, number=current weight limit" cr ;
: .progress   1 iter +!  iter @ 1000000 mod 0= if [char] . emit then ;
: u-turn ( .. h2 w1 s1 h1 ) 4 pick 2 pick - ;
: search ( .. h2 w1 s1 h1 )
	.progress
	key? if exit then
	over solution = if win then
	2 pick limit @ > if exit then
	4 0 do dup i valid? if i step u-turn <> if i advance recurse rollback then then loop ;

: hole? ( s u -- f ) bits tile 0= ;
: hole ( s -- h ) 16 0 do dup i hole? if drop i unloop exit then loop drop ;

: setup ( s -- ) -1 0 rot dup hole ;
: drop-key   key? if key drop then ;
: teardown ( h2 w1 s h1 -- s ) drop nip nip ;
: solve   .solve  0 iter !  1 limit !  begin limit ? search deeper key? until  drop-key teardown ;

\ hex 0c9dfbae37254861 decimal setup solve
\ hex fe169b4c0a73d852 decimal setup solve
\ hex 123456789afbde0c decimal setup solve
\ bye

0 constant up 1 constant left 2 constant right 3 constant down

: .hole   space space space ;
: .tile ( u -- ) ?dup-0=-if .hole else dup 10 < if space then . then ;
: .board ( s -- ) 4 0 do cr 4 0 do dup j 4 * i + bits tile .tile loop loop drop ;
: .help   cr ." ijkl move, q quit, s solve" ;

create (rnd)   utime drop ,
: rnd   (rnd) @ dup 13 lshift xor dup 17 rshift xor dup dup 5 lshift xor (rnd) ! ;

: move ( s u -- s' ) >r dup hole r> new-state ;
: ?move ( s u -- s' ) >r dup hole r@ valid? if r> move else rdrop then ;
: shuffle ( s u -- s' ) 0 do rnd 3 and ?move loop ;

: turn ( s -- )
	page dup .board .help
	key case
		[char] q of bye endof
		[char] i of down ?move endof
		[char] j of right ?move endof
		[char] k of up ?move endof
		[char] l of left ?move endof
		[char] s of .solve setup solve endof
	endcase ;

: play  begin dup solution <> while turn repeat ;

solution 1000 shuffle play

