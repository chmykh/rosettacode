snake.o : snake.c
	gcc -c snake.c

snake : snake.o
	gcc -lncurses -o snake snake.o

clean :
	rm snake.o snake
