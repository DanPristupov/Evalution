module Evalution
    open System
    open System.Globalization

    let decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.[0]

    type Token = 
        | Double of float
        | Integer of int
        | Operator of char
        | None

    let readFormula input = 
        let length = String.length(input)

        let isPartOfNumeric char =
            match char with
            | x when x = decimalSeparator -> true
            | x when x > '0' && x < '9' -> true
            | _ -> false

        let getToken start = 
            let char = input.[start]

            if isPartOfNumeric char then
                let rec getNumberSubString str index result =
                    if index < String.length(str) && isPartOfNumeric input.[index] then
                        getNumberSubString str (index+1) (result + input.[index].ToString())
                    else
                        (result, index)
                let (substring, index) = getNumberSubString input start ""

                let (succ, intValue) = Int32.TryParse substring
                if succ then
                    ((Integer intValue), index)
                else
                    let (succ, doubleValue) = Double.TryParse(substring, NumberStyles.Float ||| NumberStyles.AllowThousands, CultureInfo.CurrentCulture)
                    if (succ) then
                        ((Double doubleValue), index)
                    else
                        failwith "fail"
            else
                match char with
                | ('+' | '-' | '*' | '/' | '(' | ')') -> ((Operator char), start+1)
                | ' ' -> (None, start+1)
                | _ -> failwith "fail"

        let mutable tokens = []
        let mutable i = 0
        while i < length do
            let (token, nextPos) = getToken i
            if (token <> None) then
                tokens <- (token::tokens)
            i <- nextPos
        tokens
 
    [<EntryPoint>]
    let main argv = 
        let result = readFormula("(42+8) *2")
        result |> Seq.iter ( printfn "%A")

        Console.ReadLine()
        0 // return an integer exit code
