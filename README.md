# Generic Compiler Toolchain

GC-Toolchain is a compiler development toolchain that is responsible for front-end validation and analysis, and offers a framework for back-end code-generation. A comprehensive compilation report can be generated on each run to give detailed information of the mechanisms involved in the compilation process.

![img](https://i.imgur.com/lwjLdHn.png)

## Usage

`GCT -t tokens.config -p program.c -v -r out`

`GCT -tokens tokens.config -program program.c -verbose -report out`

## Lexical Analysis


### Configuration Files
Token specification is given through a configuration file. The configuration file is made up of sections, which begin with the section declaration line. The section declaration line begins with the `#` symbol, followed by an identifier right next to the section symbol `#type`. Some section types require additional information which are usually contained in the remaining trailing whitespace-separated identifiers in the declaration line, or in the section body which are the lines trailing the section declaration line.

```
#type identifiers
body-line-1
body-line-2
body-line-3
...
body-line-N
```

For lexical analysis, the configuration file defines the token specification in the form of a grammar. There are three section types used:


### Rule
Specifies unique symbols used in the grammar.
The following rules are the default values used.
```
#rule zero_or_more
*

#rule literal_prefix
\

#rule sub_token_prefix
$

#rule hex_prefix
%

#rule range_inclusive
-
```

### Subtoken
Specifies a non-terminal which should **not** be considered as a token.

```
#subtoken nonzero
1-9

#subtoken digit
$nonzero
0
```

### Token
Specifies a non-terminal which should be considered as a token.
```
#subtoken nonzero
1-9

#subtoken digit
$nonzero
0

#token integer
$nonzero $digit*
```

## Syntactic Analysis

### Configuration Files

### Rule
Specifies unique symbols used in the grammar.
The following rules are the default values used.

```
#rule start
S

#rule production_prefix
$
```
### Production
Specifies a non-terminal in the grammar and its rules.

```
#producion statement
id ( ) ;
id id = id ;
```

Productions can have epsilon transitions which are denoted by annotation.

```
#production function epsilon:true
id id ( ) { }
```
is equivalent to 
```
function -> id id ( ) { }
          | Ɛ
```

### Blacklist
Specifies tokens to ignore in the token stream.
```
#blacklist
whitespace
```
