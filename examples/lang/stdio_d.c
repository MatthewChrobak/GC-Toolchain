#include <stdio.h>

void print_int(int val) {
	printf("%d", val);
	FILE* fs = fopen("./debug_output.out", "a");
	fprintf(fs, "%d", val);
	fclose(fs);
}

void print_float(float val) {
	printf("%f", val);
	FILE* fs = fopen("./debug_output.out", "a");
	fprintf(fs, "%f", val);
	fclose(fs);
}

void print_str(const char* val) {
	printf("%s", val);
	FILE* fs = fopen("./debug_output.out", "a");
	fprintf(fs, "%s", val);
	fclose(fs);
}

char getkey() {
	return getchar();
}