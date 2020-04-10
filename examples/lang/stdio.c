#include <stdio.h>

struct Person {
	int x;
};

void print_int(int val) {
	printf("%d", val);
}

void print_float(float val) {
	printf("%f", val);
}

void print_str(const char* val) {
	printf("%s", val);
}

char getkey() {
	return getchar();
}


void Test() {
	struct Person p;
	p.x = 20;
}