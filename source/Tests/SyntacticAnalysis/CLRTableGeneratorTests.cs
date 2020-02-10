using NUnit.Framework;
using SyntacticAnalysis;
using SyntacticAnalysis.CLR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.SyntacticAnalysis
{
    // Uses output from http://jsmachines.sourceforge.net/machines/lr1.html
    public class CLRTableGeneratorTests
    {
        private CLRStateGenerator GetCLRStateGenerator(ProductionTable productionTable, SyntacticConfigurationFile config) {
            return new CLRStateGenerator(productionTable, config);
        }

        private LRParsingTable GetLRParsingTable(ProductionTable productionTable, CLRStateGenerator clrStateGenerator) {
            return LRParsingTable.From(clrStateGenerator, productionTable);
        }

        [Test]
        public void SimpleTable() {
            string config = @"
#rule start
S

#production S
$C $C

#production C
c $C
d
";
            string solutionTable = @"State	c	d	$	S'	S	C
0	s3	s4	 	 	1	2
1	 	 	accept	 	 	 
2	s6	s7	 	 	 	5
3	s3	s4	 	 	 	8
4	r3	r3	 	 	 	 
5	 	 	r1	 	 	 
6	s6	s7	 	 	 	9
7	 	 	r3	 	 	 
8	r2	r2	 	 	 	 
9	 	 	r2	 	 	 ";
            string solutionStates = @"	{[S' -> .S, $]}	0	{[S' -> .S, $]; [S -> .C C, $]; [C -> .c C, c/d]; [C -> .d, c/d]}
goto(0, S)	{[S' -> S., $]}	1	{[S' -> S., $]}
goto(0, C)	{[S -> C.C, $]}	2	{[S -> C.C, $]; [C -> .c C, $]; [C -> .d, $]}
goto(0, c)	{[C -> c.C, c/d]}	3	{[C -> c.C, c/d]; [C -> .c C, c/d]; [C -> .d, c/d]}
goto(0, d)	{[C -> d., c/d]}	4	{[C -> d., c/d]}
goto(2, C)	{[S -> C C., $]}	5	{[S -> C C., $]}
goto(2, c)	{[C -> c.C, $]}	6	{[C -> c.C, $]; [C -> .c C, $]; [C -> .d, $]}
goto(2, d)	{[C -> d., $]}	7	{[C -> d., $]}
goto(3, C)	{[C -> c C., c/d]}	8	{[C -> c C., c/d]}
goto(3, c)	{[C -> c.C, c/d]}	3	 
goto(3, d)	{[C -> d., c/d]}	4	 
goto(6, C)	{[C -> c C., $]}	9	{[C -> c C., $]}
goto(6, c)	{[C -> c.C, $]}	6	 
goto(6, d)	{[C -> d., $]}	7	 ";
            BuildAndCompare(config, solutionTable, solutionStates);
        }

        [Test]
        public void MinComplexTable() {
            string config = @"#rule start
S

#blacklist
whitespace

#production S
$lvalue:lvalue = $expression:^

#production expression
$expression:lhs $p10_operator:^ $p9_expression:rhs
$p9_expression:^

#production p9_expression
$p9_expression:lhs $p9_operator:^ $rvalue:rhs
$rvalue:rvalue

#production rvalue
( $expression:^ )
$lvalue:lvalue
integer:integer
$pointer_operator:^ $rvalue:^
$p10_operator:^ $rvalue:^

#production lvalue
id:id

#production p10_operator
+:+
-:-

#production p9_operator
*:*
/:/

#production pointer_operator
&:address_of
*:value_of";
            string solutionTable = @"State	=	(	)	integer	id	+	-	*	/	&	$	S'	S	expression	p9_expression	rvalue	lvalue	p10_operator	p9_operator	pointer_operator
0	 	 	 	 	s3	 	 	 	 	 	 	 	1	 	 	 	2	 	 	 
1	 	 	 	 	 	 	 	 	 	 	accept	 	 	 	 	 	 	 	 	 
2	s4	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 
3	r11	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 
4	 	s8	 	s10	s13	s16	s17	s15	 	s14	 	 	 	5	6	7	9	12	 	11
5	 	 	 	 	 	s16	s17	 	 	 	r1	 	 	 	 	 	 	18	 	 
6	 	 	 	 	 	r3	r3	s20	s21	 	r3	 	 	 	 	 	 	 	19	 
7	 	 	 	 	 	r5	r5	r5	r5	 	r5	 	 	 	 	 	 	 	 	 
8	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	22	23	24	26	29	 	28
9	 	 	 	 	 	r7	r7	r7	r7	 	r7	 	 	 	 	 	 	 	 	 
10	 	 	 	 	 	r8	r8	r8	r8	 	r8	 	 	 	 	 	 	 	 	 
11	 	s8	 	s10	s13	s16	s17	s15	 	s14	 	 	 	 	 	31	9	12	 	11
12	 	s8	 	s10	s13	s16	s17	s15	 	s14	 	 	 	 	 	32	9	12	 	11
13	 	 	 	 	 	r11	r11	r11	r11	 	r11	 	 	 	 	 	 	 	 	 
14	 	r16	 	r16	r16	r16	r16	r16	 	r16	 	 	 	 	 	 	 	 	 	 
15	 	r17	 	r17	r17	r17	r17	r17	 	r17	 	 	 	 	 	 	 	 	 	 
16	 	r12	 	r12	r12	r12	r12	r12	 	r12	 	 	 	 	 	 	 	 	 	 
17	 	r13	 	r13	r13	r13	r13	r13	 	r13	 	 	 	 	 	 	 	 	 	 
18	 	s8	 	s10	s13	s16	s17	s15	 	s14	 	 	 	 	33	7	9	12	 	11
19	 	s8	 	s10	s13	s16	s17	s15	 	s14	 	 	 	 	 	34	9	12	 	11
20	 	r14	 	r14	r14	r14	r14	r14	 	r14	 	 	 	 	 	 	 	 	 	 
21	 	r15	 	r15	r15	r15	r15	r15	 	r15	 	 	 	 	 	 	 	 	 	 
22	 	 	s35	 	 	s16	s17	 	 	 	 	 	 	 	 	 	 	36	 	 
23	 	 	r3	 	 	r3	r3	s20	s21	 	 	 	 	 	 	 	 	 	37	 
24	 	 	r5	 	 	r5	r5	r5	r5	 	 	 	 	 	 	 	 	 	 	 
25	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	38	23	24	26	29	 	28
26	 	 	r7	 	 	r7	r7	r7	r7	 	 	 	 	 	 	 	 	 	 	 
27	 	 	r8	 	 	r8	r8	r8	r8	 	 	 	 	 	 	 	 	 	 	 
28	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	 	 	39	26	29	 	28
29	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	 	 	40	26	29	 	28
30	 	 	r11	 	 	r11	r11	r11	r11	 	 	 	 	 	 	 	 	 	 	 
31	 	 	 	 	 	r9	r9	r9	r9	 	r9	 	 	 	 	 	 	 	 	 
32	 	 	 	 	 	r10	r10	r10	r10	 	r10	 	 	 	 	 	 	 	 	 
33	 	 	 	 	 	r2	r2	s20	s21	 	r2	 	 	 	 	 	 	 	19	 
34	 	 	 	 	 	r4	r4	r4	r4	 	r4	 	 	 	 	 	 	 	 	 
35	 	 	 	 	 	r6	r6	r6	r6	 	r6	 	 	 	 	 	 	 	 	 
36	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	 	41	24	26	29	 	28
37	 	s25	 	s27	s30	s16	s17	s15	 	s14	 	 	 	 	 	42	26	29	 	28
38	 	 	s43	 	 	s16	s17	 	 	 	 	 	 	 	 	 	 	36	 	 
39	 	 	r9	 	 	r9	r9	r9	r9	 	 	 	 	 	 	 	 	 	 	 
40	 	 	r10	 	 	r10	r10	r10	r10	 	 	 	 	 	 	 	 	 	 	 
41	 	 	r2	 	 	r2	r2	s20	s21	 	 	 	 	 	 	 	 	 	37	 
42	 	 	r4	 	 	r4	r4	r4	r4	 	 	 	 	 	 	 	 	 	 	 
43	 	 	r6	 	 	r6	r6	r6	r6	 	 	 	 	 	 	 	 	 	 	 ";
            string solutionStates = @"";
            BuildAndCompare(config, solutionTable, solutionStates);
        }

        [Test]
        public void ComplexTable() {
            string config = @"#rule start
S

#blacklist
whitespace

#production S
$statement:statement $statements

#production statements epsilon:true
$statement:statement $statements:^

#production statement
$lvalue:lvalue ;
$lvalue:lvalue = $expression:^ ;

#production expression
$expression:lhs $p10_operator:^ $p9_expression:rhs
$p9_expression:^

#production p9_expression
$p9_expression:lhs $p9_operator:^ $rvalue:rhs
$rvalue:rvalue

#production rvalue
( $expression:^ )
$lvalue:lvalue
integer:integer
$pointer_operator:^ $rvalue:^
$p10_operator:^ $rvalue:^

#production lvalue
id:id

#production p10_operator
+:+
-:-

#production p9_operator
*:*
/:/

#production pointer_operator
&:address_of
*:value_of";
            string solutionTable = @"State	;	=	(	)	integer	id	+	-	*	/	&	$	S'	S	statements	statement	expression	p9_expression	rvalue	lvalue	p10_operator	p9_operator	pointer_operator
0	 	 	 	 	 	s4	 	 	 	 	 	 	 	1	 	2	 	 	 	3	 	 	 
1	 	 	 	 	 	 	 	 	 	 	 	accept	 	 	 	 	 	 	 	 	 	 	 
2	 	 	 	 	 	s4	 	 	 	 	 	r2	 	 	5	6	 	 	 	3	 	 	 
3	s7	s8	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 
4	r16	r16	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 	 
5	 	 	 	 	 	 	 	 	 	 	 	r1	 	 	 	 	 	 	 	 	 	 	 
6	 	 	 	 	 	s4	 	 	 	 	 	r4	 	 	9	6	 	 	 	3	 	 	 
7	 	 	 	 	 	r5	 	 	 	 	 	r5	 	 	 	 	 	 	 	 	 	 	 
8	 	 	s13	 	s15	s18	s21	s22	s20	 	s19	 	 	 	 	 	10	11	12	14	17	 	16
9	 	 	 	 	 	 	 	 	 	 	 	r3	 	 	 	 	 	 	 	 	 	 	 
10	s23	 	 	 	 	 	s21	s22	 	 	 	 	 	 	 	 	 	 	 	 	24	 	 
11	r8	 	 	 	 	 	r8	r8	s26	s27	 	 	 	 	 	 	 	 	 	 	 	25	 
12	r10	 	 	 	 	 	r10	r10	r10	r10	 	 	 	 	 	 	 	 	 	 	 	 	 
13	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	28	29	30	32	35	 	34
14	r12	 	 	 	 	 	r12	r12	r12	r12	 	 	 	 	 	 	 	 	 	 	 	 	 
15	r13	 	 	 	 	 	r13	r13	r13	r13	 	 	 	 	 	 	 	 	 	 	 	 	 
16	 	 	s13	 	s15	s18	s21	s22	s20	 	s19	 	 	 	 	 	 	 	37	14	17	 	16
17	 	 	s13	 	s15	s18	s21	s22	s20	 	s19	 	 	 	 	 	 	 	38	14	17	 	16
18	r16	 	 	 	 	 	r16	r16	r16	r16	 	 	 	 	 	 	 	 	 	 	 	 	 
19	 	 	r21	 	r21	r21	r21	r21	r21	 	r21	 	 	 	 	 	 	 	 	 	 	 	 
20	 	 	r22	 	r22	r22	r22	r22	r22	 	r22	 	 	 	 	 	 	 	 	 	 	 	 
21	 	 	r17	 	r17	r17	r17	r17	r17	 	r17	 	 	 	 	 	 	 	 	 	 	 	 
22	 	 	r18	 	r18	r18	r18	r18	r18	 	r18	 	 	 	 	 	 	 	 	 	 	 	 
23	 	 	 	 	 	r6	 	 	 	 	 	r6	 	 	 	 	 	 	 	 	 	 	 
24	 	 	s13	 	s15	s18	s21	s22	s20	 	s19	 	 	 	 	 	 	39	12	14	17	 	16
25	 	 	s13	 	s15	s18	s21	s22	s20	 	s19	 	 	 	 	 	 	 	40	14	17	 	16
26	 	 	r19	 	r19	r19	r19	r19	r19	 	r19	 	 	 	 	 	 	 	 	 	 	 	 
27	 	 	r20	 	r20	r20	r20	r20	r20	 	r20	 	 	 	 	 	 	 	 	 	 	 	 
28	 	 	 	s41	 	 	s21	s22	 	 	 	 	 	 	 	 	 	 	 	 	42	 	 
29	 	 	 	r8	 	 	r8	r8	s26	s27	 	 	 	 	 	 	 	 	 	 	 	43	 
30	 	 	 	r10	 	 	r10	r10	r10	r10	 	 	 	 	 	 	 	 	 	 	 	 	 
31	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	44	29	30	32	35	 	34
32	 	 	 	r12	 	 	r12	r12	r12	r12	 	 	 	 	 	 	 	 	 	 	 	 	 
33	 	 	 	r13	 	 	r13	r13	r13	r13	 	 	 	 	 	 	 	 	 	 	 	 	 
34	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	 	 	45	32	35	 	34
35	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	 	 	46	32	35	 	34
36	 	 	 	r16	 	 	r16	r16	r16	r16	 	 	 	 	 	 	 	 	 	 	 	 	 
37	r14	 	 	 	 	 	r14	r14	r14	r14	 	 	 	 	 	 	 	 	 	 	 	 	 
38	r15	 	 	 	 	 	r15	r15	r15	r15	 	 	 	 	 	 	 	 	 	 	 	 	 
39	r7	 	 	 	 	 	r7	r7	s26	s27	 	 	 	 	 	 	 	 	 	 	 	25	 
40	r9	 	 	 	 	 	r9	r9	r9	r9	 	 	 	 	 	 	 	 	 	 	 	 	 
41	r11	 	 	 	 	 	r11	r11	r11	r11	 	 	 	 	 	 	 	 	 	 	 	 	 
42	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	 	47	30	32	35	 	34
43	 	 	s31	 	s33	s36	s21	s22	s20	 	s19	 	 	 	 	 	 	 	48	32	35	 	34
44	 	 	 	s49	 	 	s21	s22	 	 	 	 	 	 	 	 	 	 	 	 	42	 	 
45	 	 	 	r14	 	 	r14	r14	r14	r14	 	 	 	 	 	 	 	 	 	 	 	 	 
46	 	 	 	r15	 	 	r15	r15	r15	r15	 	 	 	 	 	 	 	 	 	 	 	 	 
47	 	 	 	r7	 	 	r7	r7	s26	s27	 	 	 	 	 	 	 	 	 	 	 	43	 
48	 	 	 	r9	 	 	r9	r9	r9	r9	 	 	 	 	 	 	 	 	 	 	 	 	 
49	 	 	 	r11	 	 	r11	r11	r11	r11	 	 	 	 	 	 	 	 	 	 	 	 	 ";
            string solutionStates = @"";
            BuildAndCompare(config, solutionTable, solutionStates);
        }

        [Test]
        public void OtherSimpleTable() {
            string config = @"
#rule start
S

#production S
$A

#production A
a $A
a
";
            string solutionTable = @"State	a	$	S'	S	A
0	s3	 	 	1	2
1	 	accept	 	 	 
2	 	r1	 	 	 
3	s3	r3	 	 	4
4	 	r2	 	 	 ";
            string solutionStates = @"	{[S' -> .S, $]}	0	{[S' -> .S, $]; [S -> .A, $]; [A -> .a A, $]; [A -> .a, $]}
goto(0, S)	{[S' -> S., $]}	1	{[S' -> S., $]}
goto(0, A)	{[S -> A., $]}	2	{[S -> A., $]}
goto(0, a)	{[A -> a.A, $]; [A -> a., $]}	3	{[A -> a.A, $]; [A -> a., $]; [A -> .a A, $]; [A -> .a, $]}
goto(3, A)	{[A -> a A., $]}	4	{[A -> a A., $]}
goto(3, a)	{[A -> a.A, $]; [A -> a., $]}	3	 ";
            BuildAndCompare(config, solutionTable, solutionStates);
        }

        private void BuildAndCompare(string configContents, string solutionTable, string solutionStates) {
            var config = new SyntacticConfigurationFile(configContents.Split("\r\n"), "fakefile");
            var productionTable = new ProductionTable(config);
            var clrStates = GetCLRStateGenerator(productionTable, config);
            var submissionTable = GetLRParsingTable(productionTable, clrStates).ToTestString();
            var submissionStates = clrStates.ToTestString();
            Console.WriteLine("Submission:");
            Console.WriteLine(submissionTable);
            Console.WriteLine("Solution:");
            Console.WriteLine(solutionTable);

            Console.WriteLine("Submission:");
            Console.WriteLine(submissionStates);
            Console.WriteLine("Solution:");
            Console.WriteLine(string.Join("\r\n", GetSolutionRows(solutionStates.Split("\r\n")).Select(val => $"{val.action}\t{val.closure}")));

            CompareStates(solutionStates, submissionStates);
            CompareTables(solutionTable, submissionTable);
        }

        private void CompareTables(string solution, string submission) {
            var t1 = solution.Split("\r\n");
            var t2 = submission.Split("\r\n");

            Assert.AreEqual(t1.Length, t2.Length);

            var int_to_header = GetIntToHeader(t1[0]);
            var header_to_int = GetHeaderToInt(t2[0]);

            // Row 1 is the start because row 0 is the header
            for (int rowIndex = 1; rowIndex < t1.Length; rowIndex++) {
                var t1_row = t1[rowIndex];
                var t2_row = t2[rowIndex];

                var t1_row_data = t1_row.Split('\t').Select(val => val.ToLowerInvariant().Trim()).ToArray();
                var t2_row_data = t2_row.Split('\t').Select(val => val.ToLowerInvariant().Trim()).ToArray();

                for (int column = 0; column < t1_row_data.Length; column++) {
                    string columnName = int_to_header[column];
                    int t2_column_index = header_to_int[columnName];
                    Assert.AreEqual(t1_row_data[column].Trim(), t2_row_data[t2_column_index], $"Row: {rowIndex - 1}");
                }
            }
        }

        private void CompareStates(string solution, string submission) {
            var solutionRows = GetSolutionRows(solution.Split("\r\n"));
            var submissionRows = submission.Split("\r\n");

            for (int i = 0; i < solutionRows.Length; i++) {
                Assert.AreEqual(solutionRows[i].closure, submissionRows[i], $"GOTO: {solutionRows[i].action}");
            }

        }

        private (string action, string closure)[] GetSolutionRows(string[] rows) {
            var result = new (string, string)[rows.Length];

            for (int i= 0; i < rows.Length; i++) {
                var rowData = rows[i].Split('\t');
                result[i] = (rowData[0], rowData[3].Trim());
            }

            return result;
        }

        private Dictionary<string, int> GetHeaderToInt(string header) {
            var headerData = header.Split('\t');
            var map = new Dictionary<string, int>();
            foreach (var entry in headerData) {
                map[entry] = map.Count;
            }
            return map;
        }

        private Dictionary<int, string> GetIntToHeader(string header) {
            var headerData = header.Split('\t');
            var map = new Dictionary<int, string>();
            foreach (var entry in headerData) {
                map[map.Count] = entry;
            }
            return map;
        }
    }
}
