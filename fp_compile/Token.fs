module Token

type Token =
    | OPERATOR of string
    | NUMBER of float
    | STRING of string
    | ID of string
    | DOT
    | COMMA
    | SEMICOLON
    | LEFT_CURLY
    | RIGHT_CURLY
    | LEFT_PARENTHESIS
    | RIGHT_PARENTHESIS

let tokenize source =
    let literal_tokens = Map [
        ('.', Token.DOT);
        (';', Token.SEMICOLON);
        (',', Token.COMMA);
        ('{', Token.LEFT_CURLY);
        ('}', Token.RIGHT_CURLY);
        ('(', Token.LEFT_PARENTHESIS);
        (')', Token.RIGHT_PARENTHESIS);
    ]

    let arithmetic_tokes = Map [
        ('+', Token.OPERATOR("+"));
        ('-', Token.OPERATOR("-"));
        ('*', Token.OPERATOR("*"));
        ('/', Token.OPERATOR("/"));
        ('=', Token.OPERATOR("="));
        ('>', Token.OPERATOR(">"));
        ('<', Token.OPERATOR("<"));
        ('|', Token.OPERATOR("|"));
        ('&', Token.OPERATOR("&"));
    ]

    let rec read_string_end acc = function
    | '\\'::'"'::t -> (toString (List.rev acc)), t
    | '"'::t -> (toString (List.rev acc)), t
    | h::t -> read_string_end (h::acc) t
    | [] -> failwith "read_string_end ERROR: EOF before closing \" found"

    let rec read_comment = function
    | '$'::t -> t
    | _::t -> read_comment t
    | [] -> failwith "read_comment ERROR: EOF before closing comment"

    let rec read_linecomment = function
    | '\n'::t -> t
    | _::t -> read_linecomment t
    | [] -> []

    let rec read_id acc = function
    | h::t when Char.IsWhiteSpace(h) -> (toString (List.rev acc)), t
    | h::t when Char.IsLetter(h) || Char.IsDigit(h) || h = '_' -> read_id (h::acc) t
    | h::t when h = '(' || h = ')' || h = '{' || h = '}' -> (toString (List.rev acc)), (h::t)
    | [] -> (toString (List.rev acc)), []
    | h::_ -> failwith ("read_id ERROR: Unexpected symbol met: " + (string h))

    // Вспомогательная функция для чтения чисел.
    let rec read_number acc = function
    | h::t when Char.IsWhiteSpace(h) -> (toString (List.rev acc)), t
    | h::t when Char.IsDigit(h) -> read_number (h::acc) t
    | '.'::t -> read_number ('.'::acc) t
    | h::t when h = '(' || h = ')' -> (toString (List.rev acc)), (h::t)
    | [] -> (toString (List.rev acc)), []
    | h::_ -> failwith ("read_number ERROR: Unexpected symbol met while reading digit: " + (string h))

    // Основная функция для разбора и преобразования исходного кода в список токенов.
    let rec tokenize_impl acc = function
    | h::t when Char.IsWhiteSpace(h) -> tokenize_impl acc t
    | h::t when literal_tokens |> Map.containsKey h -> tokenize_impl ((literal_tokens |> Map.find h)::acc) t
    | '"'::t | '\\'::'"'::t -> 
        let read_string, remaining_source = read_string_end [] t
        tokenize_impl (Token.STRING( read_string)::acc) remaining_source
    | '$'::t -> 
        let remaining_source = read_comment t
        tokenize_impl acc remaining_source
    | '#'::t -> 
        let remaining_source = read_linecomment t
        tokenize_impl acc remaining_source

    | h::t when Char.IsLetter(h) ->
        let read_id, remaining_source = read_id [] (h::t)
        tokenize_impl (Token.ID(read_id)::acc) remaining_source

    | h::t when Char.IsDigit(h) ->
        let read_number, remaining_source = read_number [] (h::t)
        try 
            let parsed_number = System.Double.Parse(read_number, System.Globalization.CultureInfo.InvariantCulture)
            tokenize_impl (Token.NUMBER(parsed_number)::acc) remaining_source
        with
            _ -> failwith ("tokenize_impl ERROR: Unrecognizable number met: " + read_number)
    | '-'::h::t when Char.IsDigit(h) ->
        let read_number, remaining_source = read_number [] (h::t)
        try 
            let parsed_number = System.Double.Parse("-" + read_number, System.Globalization.CultureInfo.InvariantCulture)
            tokenize_impl (Token.NUMBER(parsed_number)::acc) remaining_source
        with
            _ -> failwith ("tokenize_impl ERROR: Unrecognizable number met: " + read_number)
    | h::' '::t when (arithmetic_tokes |> Map.tryFind h).IsSome ->
         tokenize_impl ((arithmetic_tokes |> Map.find h)::acc) t

    | h::_ -> failwith ("tokenize_impl ERROR: Unsupported symbol met: " + (string h))
    | [] -> List.rev acc

    tokenize_impl [] source