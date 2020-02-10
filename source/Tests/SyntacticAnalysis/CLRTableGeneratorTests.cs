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
            string solutionStates = @"	{[S' -> .S, $]}	0	{[S' -> .S, $]; [S -> .lvalue = expression, $]; [lvalue -> .id, =]}
goto(0, S)	{[S' -> S., $]}	1	{[S' -> S., $]}
goto(0, lvalue)	{[S -> lvalue.= expression, $]}	2	{[S -> lvalue.= expression, $]}
goto(0, id)	{[lvalue -> id., =]}	3	{[lvalue -> id., =]}
goto(2, =)	{[S -> lvalue =.expression, $]}	4	{[S -> lvalue =.expression, $]; [expression -> .expression p10_operator p9_expression, $/+/-]; [expression -> .p9_expression, $/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, $/+/-/*//]; [p9_expression -> .rvalue, $/+/-/*//]; [rvalue -> .( expression ), $/+/-/*//]; [rvalue -> .lvalue, $/+/-/*//]; [rvalue -> .integer, $/+/-/*//]; [rvalue -> .pointer_operator rvalue, $/+/-/*//]; [rvalue -> .p10_operator rvalue, $/+/-/*//]; [lvalue -> .id, $/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(4, expression)	{[S -> lvalue = expression., $]; [expression -> expression.p10_operator p9_expression, $/+/-]}	5	{[S -> lvalue = expression., $]; [expression -> expression.p10_operator p9_expression, $/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(4, p9_expression)	{[expression -> p9_expression., $/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, $/+/-/*//]}	6	{[expression -> p9_expression., $/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, $/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(4, rvalue)	{[p9_expression -> rvalue., $/+/-/*//]}	7	{[p9_expression -> rvalue., $/+/-/*//]}
goto(4, ()	{[rvalue -> (.expression ), $/+/-/*//]}	8	{[rvalue -> (.expression ), $/+/-/*//]; [expression -> .expression p10_operator p9_expression, )/+/-]; [expression -> .p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(4, lvalue)	{[rvalue -> lvalue., $/+/-/*//]}	9	{[rvalue -> lvalue., $/+/-/*//]}
goto(4, integer)	{[rvalue -> integer., $/+/-/*//]}	10	{[rvalue -> integer., $/+/-/*//]}
goto(4, pointer_operator)	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]}	11	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]; [rvalue -> .( expression ), $/+/-/*//]; [rvalue -> .lvalue, $/+/-/*//]; [rvalue -> .integer, $/+/-/*//]; [rvalue -> .pointer_operator rvalue, $/+/-/*//]; [rvalue -> .p10_operator rvalue, $/+/-/*//]; [lvalue -> .id, $/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(4, p10_operator)	{[rvalue -> p10_operator.rvalue, $/+/-/*//]}	12	{[rvalue -> p10_operator.rvalue, $/+/-/*//]; [rvalue -> .( expression ), $/+/-/*//]; [rvalue -> .lvalue, $/+/-/*//]; [rvalue -> .integer, $/+/-/*//]; [rvalue -> .pointer_operator rvalue, $/+/-/*//]; [rvalue -> .p10_operator rvalue, $/+/-/*//]; [lvalue -> .id, $/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(4, id)	{[lvalue -> id., $/+/-/*//]}	13	{[lvalue -> id., $/+/-/*//]}
goto(4, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	{[pointer_operator -> &., (/integer/id/&/*/+/-]}
goto(4, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	{[pointer_operator -> *., (/integer/id/&/*/+/-]}
goto(4, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	{[p10_operator -> +., (/integer/id/&/*/+/-]}
goto(4, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	{[p10_operator -> -., (/integer/id/&/*/+/-]}
goto(5, p10_operator)	{[expression -> expression p10_operator.p9_expression, $/+/-]}	18	{[expression -> expression p10_operator.p9_expression, $/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, $/+/-/*//]; [p9_expression -> .rvalue, $/+/-/*//]; [rvalue -> .( expression ), $/+/-/*//]; [rvalue -> .lvalue, $/+/-/*//]; [rvalue -> .integer, $/+/-/*//]; [rvalue -> .pointer_operator rvalue, $/+/-/*//]; [rvalue -> .p10_operator rvalue, $/+/-/*//]; [lvalue -> .id, $/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(5, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(5, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(6, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, $/+/-/*//]}	19	{[p9_expression -> p9_expression p9_operator.rvalue, $/+/-/*//]; [rvalue -> .( expression ), $/+/-/*//]; [rvalue -> .lvalue, $/+/-/*//]; [rvalue -> .integer, $/+/-/*//]; [rvalue -> .pointer_operator rvalue, $/+/-/*//]; [rvalue -> .p10_operator rvalue, $/+/-/*//]; [lvalue -> .id, $/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(6, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	20	{[p9_operator -> *., (/integer/id/&/*/+/-]}
goto(6, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	21	{[p9_operator -> /., (/integer/id/&/*/+/-]}
goto(8, expression)	{[rvalue -> ( expression.), $/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]}	22	{[rvalue -> ( expression.), $/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, p9_expression)	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	23	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(8, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	24	{[p9_expression -> rvalue., )/+/-/*//]}
goto(8, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	{[rvalue -> (.expression ), )/+/-/*//]; [expression -> .expression p10_operator p9_expression, )/+/-]; [expression -> .p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	{[rvalue -> lvalue., )/+/-/*//]}
goto(8, integer)	{[rvalue -> integer., )/+/-/*//]}	27	{[rvalue -> integer., )/+/-/*//]}
goto(8, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	{[rvalue -> p10_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, id)	{[lvalue -> id., )/+/-/*//]}	30	{[lvalue -> id., )/+/-/*//]}
goto(8, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(8, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(8, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(8, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(11, rvalue)	{[rvalue -> pointer_operator rvalue., $/+/-/*//]}	31	{[rvalue -> pointer_operator rvalue., $/+/-/*//]}
goto(11, ()	{[rvalue -> (.expression ), $/+/-/*//]}	8	 
goto(11, lvalue)	{[rvalue -> lvalue., $/+/-/*//]}	9	 
goto(11, integer)	{[rvalue -> integer., $/+/-/*//]}	10	 
goto(11, pointer_operator)	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]}	11	 
goto(11, p10_operator)	{[rvalue -> p10_operator.rvalue, $/+/-/*//]}	12	 
goto(11, id)	{[lvalue -> id., $/+/-/*//]}	13	 
goto(11, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(11, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(11, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(11, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(12, rvalue)	{[rvalue -> p10_operator rvalue., $/+/-/*//]}	32	{[rvalue -> p10_operator rvalue., $/+/-/*//]}
goto(12, ()	{[rvalue -> (.expression ), $/+/-/*//]}	8	 
goto(12, lvalue)	{[rvalue -> lvalue., $/+/-/*//]}	9	 
goto(12, integer)	{[rvalue -> integer., $/+/-/*//]}	10	 
goto(12, pointer_operator)	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]}	11	 
goto(12, p10_operator)	{[rvalue -> p10_operator.rvalue, $/+/-/*//]}	12	 
goto(12, id)	{[lvalue -> id., $/+/-/*//]}	13	 
goto(12, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(12, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(12, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(12, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(18, p9_expression)	{[expression -> expression p10_operator p9_expression., $/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, $/+/-/*//]}	33	{[expression -> expression p10_operator p9_expression., $/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, $/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(18, rvalue)	{[p9_expression -> rvalue., $/+/-/*//]}	7	 
goto(18, ()	{[rvalue -> (.expression ), $/+/-/*//]}	8	 
goto(18, lvalue)	{[rvalue -> lvalue., $/+/-/*//]}	9	 
goto(18, integer)	{[rvalue -> integer., $/+/-/*//]}	10	 
goto(18, pointer_operator)	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]}	11	 
goto(18, p10_operator)	{[rvalue -> p10_operator.rvalue, $/+/-/*//]}	12	 
goto(18, id)	{[lvalue -> id., $/+/-/*//]}	13	 
goto(18, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(18, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(18, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(18, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(19, rvalue)	{[p9_expression -> p9_expression p9_operator rvalue., $/+/-/*//]}	34	{[p9_expression -> p9_expression p9_operator rvalue., $/+/-/*//]}
goto(19, ()	{[rvalue -> (.expression ), $/+/-/*//]}	8	 
goto(19, lvalue)	{[rvalue -> lvalue., $/+/-/*//]}	9	 
goto(19, integer)	{[rvalue -> integer., $/+/-/*//]}	10	 
goto(19, pointer_operator)	{[rvalue -> pointer_operator.rvalue, $/+/-/*//]}	11	 
goto(19, p10_operator)	{[rvalue -> p10_operator.rvalue, $/+/-/*//]}	12	 
goto(19, id)	{[lvalue -> id., $/+/-/*//]}	13	 
goto(19, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(19, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(19, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(19, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(22, ))	{[rvalue -> ( expression )., $/+/-/*//]}	35	{[rvalue -> ( expression )., $/+/-/*//]}
goto(22, p10_operator)	{[expression -> expression p10_operator.p9_expression, )/+/-]}	36	{[expression -> expression p10_operator.p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(22, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(22, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(23, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]}	37	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(23, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(23, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	21	 
goto(25, expression)	{[rvalue -> ( expression.), )/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]}	38	{[rvalue -> ( expression.), )/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(25, p9_expression)	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	23	 
goto(25, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	24	 
goto(25, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	 
goto(25, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	 
goto(25, integer)	{[rvalue -> integer., )/+/-/*//]}	27	 
goto(25, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	 
goto(25, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	 
goto(25, id)	{[lvalue -> id., )/+/-/*//]}	30	 
goto(25, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(25, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(25, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(25, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(28, rvalue)	{[rvalue -> pointer_operator rvalue., )/+/-/*//]}	39	{[rvalue -> pointer_operator rvalue., )/+/-/*//]}
goto(28, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	 
goto(28, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	 
goto(28, integer)	{[rvalue -> integer., )/+/-/*//]}	27	 
goto(28, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	 
goto(28, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	 
goto(28, id)	{[lvalue -> id., )/+/-/*//]}	30	 
goto(28, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(28, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(28, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(28, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(29, rvalue)	{[rvalue -> p10_operator rvalue., )/+/-/*//]}	40	{[rvalue -> p10_operator rvalue., )/+/-/*//]}
goto(29, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	 
goto(29, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	 
goto(29, integer)	{[rvalue -> integer., )/+/-/*//]}	27	 
goto(29, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	 
goto(29, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	 
goto(29, id)	{[lvalue -> id., )/+/-/*//]}	30	 
goto(29, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(29, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(29, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(29, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(33, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, $/+/-/*//]}	19	 
goto(33, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(33, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	21	 
goto(36, p9_expression)	{[expression -> expression p10_operator p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	41	{[expression -> expression p10_operator p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(36, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	24	 
goto(36, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	 
goto(36, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	 
goto(36, integer)	{[rvalue -> integer., )/+/-/*//]}	27	 
goto(36, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	 
goto(36, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	 
goto(36, id)	{[lvalue -> id., )/+/-/*//]}	30	 
goto(36, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(36, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(36, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(36, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(37, rvalue)	{[p9_expression -> p9_expression p9_operator rvalue., )/+/-/*//]}	42	{[p9_expression -> p9_expression p9_operator rvalue., )/+/-/*//]}
goto(37, ()	{[rvalue -> (.expression ), )/+/-/*//]}	25	 
goto(37, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	26	 
goto(37, integer)	{[rvalue -> integer., )/+/-/*//]}	27	 
goto(37, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	28	 
goto(37, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	29	 
goto(37, id)	{[lvalue -> id., )/+/-/*//]}	30	 
goto(37, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	14	 
goto(37, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	15	 
goto(37, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(37, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(38, ))	{[rvalue -> ( expression )., )/+/-/*//]}	43	{[rvalue -> ( expression )., )/+/-/*//]}
goto(38, p10_operator)	{[expression -> expression p10_operator.p9_expression, )/+/-]}	36	 
goto(38, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	16	 
goto(38, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	17	 
goto(41, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]}	37	 
goto(41, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(41, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	21	 ";
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
            string solutionStates = @"	{[S' -> .S, $]}	0	{[S' -> .S, $]; [S -> .statement statements, $]; [S -> .statement, $]; [statement -> .lvalue ;, id/$]; [statement -> .lvalue = expression ;, id/$]; [lvalue -> .id, ;/=]}
goto(0, S)	{[S' -> S., $]}	1	{[S' -> S., $]}
goto(0, statement)	{[S -> statement.statements, $]; [S -> statement., $]}	2	{[S -> statement.statements, $]; [S -> statement., $]; [statements -> .statement statements, $]; [statements -> .statement, $]; [statement -> .lvalue ;, id/$]; [statement -> .lvalue = expression ;, id/$]; [lvalue -> .id, ;/=]}
goto(0, lvalue)	{[statement -> lvalue.;, id/$]; [statement -> lvalue.= expression ;, id/$]}	3	{[statement -> lvalue.;, id/$]; [statement -> lvalue.= expression ;, id/$]}
goto(0, id)	{[lvalue -> id., ;/=]}	4	{[lvalue -> id., ;/=]}
goto(2, statements)	{[S -> statement statements., $]}	5	{[S -> statement statements., $]}
goto(2, statement)	{[statements -> statement.statements, $]; [statements -> statement., $]}	6	{[statements -> statement.statements, $]; [statements -> statement., $]; [statements -> .statement statements, $]; [statements -> .statement, $]; [statement -> .lvalue ;, id/$]; [statement -> .lvalue = expression ;, id/$]; [lvalue -> .id, ;/=]}
goto(2, lvalue)	{[statement -> lvalue.;, id/$]; [statement -> lvalue.= expression ;, id/$]}	3	 
goto(2, id)	{[lvalue -> id., ;/=]}	4	 
goto(3, ;)	{[statement -> lvalue ;., id/$]}	7	{[statement -> lvalue ;., id/$]}
goto(3, =)	{[statement -> lvalue =.expression ;, id/$]}	8	{[statement -> lvalue =.expression ;, id/$]; [expression -> .expression p10_operator p9_expression, ;/+/-]; [expression -> .p9_expression, ;/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, ;/+/-/*//]; [p9_expression -> .rvalue, ;/+/-/*//]; [rvalue -> .( expression ), ;/+/-/*//]; [rvalue -> .lvalue, ;/+/-/*//]; [rvalue -> .integer, ;/+/-/*//]; [rvalue -> .pointer_operator rvalue, ;/+/-/*//]; [rvalue -> .p10_operator rvalue, ;/+/-/*//]; [lvalue -> .id, ;/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(6, statements)	{[statements -> statement statements., $]}	9	{[statements -> statement statements., $]}
goto(6, statement)	{[statements -> statement.statements, $]; [statements -> statement., $]}	6	 
goto(6, lvalue)	{[statement -> lvalue.;, id/$]; [statement -> lvalue.= expression ;, id/$]}	3	 
goto(6, id)	{[lvalue -> id., ;/=]}	4	 
goto(8, expression)	{[statement -> lvalue = expression.;, id/$]; [expression -> expression.p10_operator p9_expression, ;/+/-]}	10	{[statement -> lvalue = expression.;, id/$]; [expression -> expression.p10_operator p9_expression, ;/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, p9_expression)	{[expression -> p9_expression., ;/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, ;/+/-/*//]}	11	{[expression -> p9_expression., ;/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, ;/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(8, rvalue)	{[p9_expression -> rvalue., ;/+/-/*//]}	12	{[p9_expression -> rvalue., ;/+/-/*//]}
goto(8, ()	{[rvalue -> (.expression ), ;/+/-/*//]}	13	{[rvalue -> (.expression ), ;/+/-/*//]; [expression -> .expression p10_operator p9_expression, )/+/-]; [expression -> .p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, lvalue)	{[rvalue -> lvalue., ;/+/-/*//]}	14	{[rvalue -> lvalue., ;/+/-/*//]}
goto(8, integer)	{[rvalue -> integer., ;/+/-/*//]}	15	{[rvalue -> integer., ;/+/-/*//]}
goto(8, pointer_operator)	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]}	16	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]; [rvalue -> .( expression ), ;/+/-/*//]; [rvalue -> .lvalue, ;/+/-/*//]; [rvalue -> .integer, ;/+/-/*//]; [rvalue -> .pointer_operator rvalue, ;/+/-/*//]; [rvalue -> .p10_operator rvalue, ;/+/-/*//]; [lvalue -> .id, ;/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, p10_operator)	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]}	17	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]; [rvalue -> .( expression ), ;/+/-/*//]; [rvalue -> .lvalue, ;/+/-/*//]; [rvalue -> .integer, ;/+/-/*//]; [rvalue -> .pointer_operator rvalue, ;/+/-/*//]; [rvalue -> .p10_operator rvalue, ;/+/-/*//]; [lvalue -> .id, ;/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(8, id)	{[lvalue -> id., ;/+/-/*//]}	18	{[lvalue -> id., ;/+/-/*//]}
goto(8, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	{[pointer_operator -> &., (/integer/id/&/*/+/-]}
goto(8, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	{[pointer_operator -> *., (/integer/id/&/*/+/-]}
goto(8, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	{[p10_operator -> +., (/integer/id/&/*/+/-]}
goto(8, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	{[p10_operator -> -., (/integer/id/&/*/+/-]}
goto(10, ;)	{[statement -> lvalue = expression ;., id/$]}	23	{[statement -> lvalue = expression ;., id/$]}
goto(10, p10_operator)	{[expression -> expression p10_operator.p9_expression, ;/+/-]}	24	{[expression -> expression p10_operator.p9_expression, ;/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, ;/+/-/*//]; [p9_expression -> .rvalue, ;/+/-/*//]; [rvalue -> .( expression ), ;/+/-/*//]; [rvalue -> .lvalue, ;/+/-/*//]; [rvalue -> .integer, ;/+/-/*//]; [rvalue -> .pointer_operator rvalue, ;/+/-/*//]; [rvalue -> .p10_operator rvalue, ;/+/-/*//]; [lvalue -> .id, ;/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(10, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(10, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(11, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, ;/+/-/*//]}	25	{[p9_expression -> p9_expression p9_operator.rvalue, ;/+/-/*//]; [rvalue -> .( expression ), ;/+/-/*//]; [rvalue -> .lvalue, ;/+/-/*//]; [rvalue -> .integer, ;/+/-/*//]; [rvalue -> .pointer_operator rvalue, ;/+/-/*//]; [rvalue -> .p10_operator rvalue, ;/+/-/*//]; [lvalue -> .id, ;/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(11, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	26	{[p9_operator -> *., (/integer/id/&/*/+/-]}
goto(11, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	27	{[p9_operator -> /., (/integer/id/&/*/+/-]}
goto(13, expression)	{[rvalue -> ( expression.), ;/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]}	28	{[rvalue -> ( expression.), ;/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(13, p9_expression)	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	29	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(13, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	30	{[p9_expression -> rvalue., )/+/-/*//]}
goto(13, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	{[rvalue -> (.expression ), )/+/-/*//]; [expression -> .expression p10_operator p9_expression, )/+/-]; [expression -> .p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(13, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	{[rvalue -> lvalue., )/+/-/*//]}
goto(13, integer)	{[rvalue -> integer., )/+/-/*//]}	33	{[rvalue -> integer., )/+/-/*//]}
goto(13, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(13, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	{[rvalue -> p10_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(13, id)	{[lvalue -> id., )/+/-/*//]}	36	{[lvalue -> id., )/+/-/*//]}
goto(13, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(13, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(13, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(13, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(16, rvalue)	{[rvalue -> pointer_operator rvalue., ;/+/-/*//]}	37	{[rvalue -> pointer_operator rvalue., ;/+/-/*//]}
goto(16, ()	{[rvalue -> (.expression ), ;/+/-/*//]}	13	 
goto(16, lvalue)	{[rvalue -> lvalue., ;/+/-/*//]}	14	 
goto(16, integer)	{[rvalue -> integer., ;/+/-/*//]}	15	 
goto(16, pointer_operator)	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]}	16	 
goto(16, p10_operator)	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]}	17	 
goto(16, id)	{[lvalue -> id., ;/+/-/*//]}	18	 
goto(16, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(16, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(16, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(16, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(17, rvalue)	{[rvalue -> p10_operator rvalue., ;/+/-/*//]}	38	{[rvalue -> p10_operator rvalue., ;/+/-/*//]}
goto(17, ()	{[rvalue -> (.expression ), ;/+/-/*//]}	13	 
goto(17, lvalue)	{[rvalue -> lvalue., ;/+/-/*//]}	14	 
goto(17, integer)	{[rvalue -> integer., ;/+/-/*//]}	15	 
goto(17, pointer_operator)	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]}	16	 
goto(17, p10_operator)	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]}	17	 
goto(17, id)	{[lvalue -> id., ;/+/-/*//]}	18	 
goto(17, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(17, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(17, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(17, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(24, p9_expression)	{[expression -> expression p10_operator p9_expression., ;/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, ;/+/-/*//]}	39	{[expression -> expression p10_operator p9_expression., ;/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, ;/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(24, rvalue)	{[p9_expression -> rvalue., ;/+/-/*//]}	12	 
goto(24, ()	{[rvalue -> (.expression ), ;/+/-/*//]}	13	 
goto(24, lvalue)	{[rvalue -> lvalue., ;/+/-/*//]}	14	 
goto(24, integer)	{[rvalue -> integer., ;/+/-/*//]}	15	 
goto(24, pointer_operator)	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]}	16	 
goto(24, p10_operator)	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]}	17	 
goto(24, id)	{[lvalue -> id., ;/+/-/*//]}	18	 
goto(24, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(24, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(24, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(24, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(25, rvalue)	{[p9_expression -> p9_expression p9_operator rvalue., ;/+/-/*//]}	40	{[p9_expression -> p9_expression p9_operator rvalue., ;/+/-/*//]}
goto(25, ()	{[rvalue -> (.expression ), ;/+/-/*//]}	13	 
goto(25, lvalue)	{[rvalue -> lvalue., ;/+/-/*//]}	14	 
goto(25, integer)	{[rvalue -> integer., ;/+/-/*//]}	15	 
goto(25, pointer_operator)	{[rvalue -> pointer_operator.rvalue, ;/+/-/*//]}	16	 
goto(25, p10_operator)	{[rvalue -> p10_operator.rvalue, ;/+/-/*//]}	17	 
goto(25, id)	{[lvalue -> id., ;/+/-/*//]}	18	 
goto(25, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(25, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(25, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(25, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(28, ))	{[rvalue -> ( expression )., ;/+/-/*//]}	41	{[rvalue -> ( expression )., ;/+/-/*//]}
goto(28, p10_operator)	{[expression -> expression p10_operator.p9_expression, )/+/-]}	42	{[expression -> expression p10_operator.p9_expression, )/+/-]; [p9_expression -> .p9_expression p9_operator rvalue, )/+/-/*//]; [p9_expression -> .rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(28, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(28, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(29, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]}	43	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]; [rvalue -> .( expression ), )/+/-/*//]; [rvalue -> .lvalue, )/+/-/*//]; [rvalue -> .integer, )/+/-/*//]; [rvalue -> .pointer_operator rvalue, )/+/-/*//]; [rvalue -> .p10_operator rvalue, )/+/-/*//]; [lvalue -> .id, )/+/-/*//]; [pointer_operator -> .&, (/integer/id/&/*/+/-]; [pointer_operator -> .*, (/integer/id/&/*/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(29, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	26	 
goto(29, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	27	 
goto(31, expression)	{[rvalue -> ( expression.), )/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]}	44	{[rvalue -> ( expression.), )/+/-/*//]; [expression -> expression.p10_operator p9_expression, )/+/-]; [p10_operator -> .+, (/integer/id/&/*/+/-]; [p10_operator -> .-, (/integer/id/&/*/+/-]}
goto(31, p9_expression)	{[expression -> p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	29	 
goto(31, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	30	 
goto(31, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	 
goto(31, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	 
goto(31, integer)	{[rvalue -> integer., )/+/-/*//]}	33	 
goto(31, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	 
goto(31, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	 
goto(31, id)	{[lvalue -> id., )/+/-/*//]}	36	 
goto(31, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(31, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(31, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(31, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(34, rvalue)	{[rvalue -> pointer_operator rvalue., )/+/-/*//]}	45	{[rvalue -> pointer_operator rvalue., )/+/-/*//]}
goto(34, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	 
goto(34, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	 
goto(34, integer)	{[rvalue -> integer., )/+/-/*//]}	33	 
goto(34, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	 
goto(34, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	 
goto(34, id)	{[lvalue -> id., )/+/-/*//]}	36	 
goto(34, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(34, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(34, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(34, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(35, rvalue)	{[rvalue -> p10_operator rvalue., )/+/-/*//]}	46	{[rvalue -> p10_operator rvalue., )/+/-/*//]}
goto(35, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	 
goto(35, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	 
goto(35, integer)	{[rvalue -> integer., )/+/-/*//]}	33	 
goto(35, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	 
goto(35, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	 
goto(35, id)	{[lvalue -> id., )/+/-/*//]}	36	 
goto(35, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(35, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(35, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(35, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(39, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, ;/+/-/*//]}	25	 
goto(39, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	26	 
goto(39, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	27	 
goto(42, p9_expression)	{[expression -> expression p10_operator p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]}	47	{[expression -> expression p10_operator p9_expression., )/+/-]; [p9_expression -> p9_expression.p9_operator rvalue, )/+/-/*//]; [p9_operator -> .*, (/integer/id/&/*/+/-]; [p9_operator -> ./, (/integer/id/&/*/+/-]}
goto(42, rvalue)	{[p9_expression -> rvalue., )/+/-/*//]}	30	 
goto(42, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	 
goto(42, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	 
goto(42, integer)	{[rvalue -> integer., )/+/-/*//]}	33	 
goto(42, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	 
goto(42, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	 
goto(42, id)	{[lvalue -> id., )/+/-/*//]}	36	 
goto(42, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(42, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(42, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(42, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(43, rvalue)	{[p9_expression -> p9_expression p9_operator rvalue., )/+/-/*//]}	48	{[p9_expression -> p9_expression p9_operator rvalue., )/+/-/*//]}
goto(43, ()	{[rvalue -> (.expression ), )/+/-/*//]}	31	 
goto(43, lvalue)	{[rvalue -> lvalue., )/+/-/*//]}	32	 
goto(43, integer)	{[rvalue -> integer., )/+/-/*//]}	33	 
goto(43, pointer_operator)	{[rvalue -> pointer_operator.rvalue, )/+/-/*//]}	34	 
goto(43, p10_operator)	{[rvalue -> p10_operator.rvalue, )/+/-/*//]}	35	 
goto(43, id)	{[lvalue -> id., )/+/-/*//]}	36	 
goto(43, &)	{[pointer_operator -> &., (/integer/id/&/*/+/-]}	19	 
goto(43, *)	{[pointer_operator -> *., (/integer/id/&/*/+/-]}	20	 
goto(43, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(43, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(44, ))	{[rvalue -> ( expression )., )/+/-/*//]}	49	{[rvalue -> ( expression )., )/+/-/*//]}
goto(44, p10_operator)	{[expression -> expression p10_operator.p9_expression, )/+/-]}	42	 
goto(44, +)	{[p10_operator -> +., (/integer/id/&/*/+/-]}	21	 
goto(44, -)	{[p10_operator -> -., (/integer/id/&/*/+/-]}	22	 
goto(47, p9_operator)	{[p9_expression -> p9_expression p9_operator.rvalue, )/+/-/*//]}	43	 
goto(47, *)	{[p9_operator -> *., (/integer/id/&/*/+/-]}	26	 
goto(47, /)	{[p9_operator -> /., (/integer/id/&/*/+/-]}	27	 ";
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
