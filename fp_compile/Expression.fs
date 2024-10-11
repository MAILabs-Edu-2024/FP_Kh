module Expression

type Expr =
    | OPERATOR of string * Expr list
    | NUMBER of double
    | STRING of string
    | ID of string
    | BOOL of bool
    | COND of Expr * Expr * Expr               
    | VARIABLE of string * Expr                    
    | SET of string * Expr                    
    | FUNC_DEF of string * Expr * Expr * env * int 
    | CALL of string * Expr * int
    | PRINT of Expr
    | SIMPLE of string
    | SIMPLEOP of string
    | SIMPLELIST of Expr list
    | SIMPLEARGLIST of Expr list
and env = Map<string, Expr>