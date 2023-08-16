grammar RePraxis;

prog: expr (NL+ expr)* NL* EOF;

expr: where_expr | set_expr;

where_expr: Where Colon (sentence | relational_expr);

set_expr: Set Colon Identifier To (sentence | constant_expr);

sentence:
	(Identifier | variable_expr)
	| (Identifier | variable_expr) Dot sentence
	| (Identifier | variable_expr) Exclusion sentence;

relational_expr:
	less_than_expr
	| greater_than_expr
	| less_than_equals_expr
	| greater_than_equals_expr
	| equals_expr
	| not_equals_expr;

less_than_expr: value_expr LessThan value_expr;

greater_than_expr: value_expr GreaterThan value_expr;

less_than_equals_expr: value_expr LessThanEquals value_expr;

greater_than_equals_expr:
	value_expr GreaterThanEquals value_expr;

equals_expr: value_expr GreaterThanEquals value_expr;

not_equals_expr: value_expr GreaterThanEquals value_expr;

value_expr: constant_expr | sentence;

constant_expr:
	null_value_expr
	| integer_value_expr
	| boolean_value_expr
	| float_value_expr
	| string_value_expr;

null_value_expr: NullLiteral;

integer_value_expr: IntegerLiteral;

float_value_expr: FloatLiteral;

string_value_expr: StringLiteral;

boolean_value_expr: BooleanLiteral;

variable_expr: Variable;

QuestionMark: '?';
Exclusion: '!';
Dot: '.';
LessThan: '<';
GreaterThan: '>';
LessThanEquals: '<=';
GreaterThanEquals: '>=';
Equals: '==';
NotEquals: '!=';
Where: 'WHERE' | 'where' | 'Where';
Set: 'SET' | 'set' | 'Set';
To: 'TO' | 'to';
Colon: ':';

/// Null Literals

NullLiteral: 'null';

/// Boolean Literals

BooleanLiteral: 'true' | 'false' | 'True' | 'False';

/// Numeric Literals

IntegerLiteral: DecimalIntegerLiteral;

FloatLiteral: DecimalIntegerLiteral '.' [0-9] [0-9_]*;

/// Identifier Names and Identifiers

Variable: QuestionMark Identifier;

Identifier: IdentifierStart IdentifierPart*;

/// String Literals

StringLiteral: (
		'"' DoubleStringCharacter* '"'
		| '\'' SingleStringCharacter* '\''
	) {this.ProcessStringLiteral();};

// Catch new lines

NL: ('\r'? '\n')+;

// Fragment rules

fragment DoubleStringCharacter:
	~["\\\r\n]
	| '\\' EscapeSequence
	| LineContinuation;

fragment SingleStringCharacter:
	~['\\\r\n]
	| '\\' EscapeSequence
	| LineContinuation;

fragment EscapeSequence:
	CharacterEscapeSequence
	| '0' // no digit ahead! TODO
	| HexEscapeSequence
	| UnicodeEscapeSequence
	| ExtendedUnicodeEscapeSequence;

fragment CharacterEscapeSequence:
	SingleEscapeCharacter
	| NonEscapeCharacter;

fragment HexEscapeSequence: 'x' HexDigit HexDigit;

fragment UnicodeEscapeSequence:
	'u' HexDigit HexDigit HexDigit HexDigit
	| 'u' '{' HexDigit HexDigit+ '}';

fragment ExtendedUnicodeEscapeSequence: 'u' '{' HexDigit+ '}';

fragment SingleEscapeCharacter: ['"\\bfnrtv];

fragment NonEscapeCharacter: ~['"\\bfnrtv0-9xu\r\n];

fragment EscapeCharacter: SingleEscapeCharacter | [0-9] | [xu];

fragment LineContinuation: '\\' [\r\n\u2028\u2029];

fragment HexDigit: [_0-9a-fA-F];

fragment DecimalIntegerLiteral: '0' | [+-]? [1-9] [0-9_]*;

fragment ExponentPart: [eE] [+-]? [0-9_]+;

fragment IdentifierPart:
	IdentifierStart
	| [\p{Mn}]
	| [\p{Nd}]
	| [\p{Pc}]
	| '\u200C'
	| '\u200D';

fragment IdentifierStart:
	[\p{L}]
	| [$_]
	| '\\' UnicodeEscapeSequence;

/// Ignore whitespace 

WS: [ \t]+ -> skip;