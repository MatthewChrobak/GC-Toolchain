#include <stdio.h>

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