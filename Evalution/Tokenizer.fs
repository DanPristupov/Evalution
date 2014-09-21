namespace Evalution
open System
open System.Globalization

type Token = 
    | Double of float
    | Integer of int
    | Operator of char
    | Bracket of char
    | Identifier of string
    | None

type public Tokenizer() =
    let decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.[0]

    let readFormula input = 
        let length = String.length(input)

        let isPartOfNumeric (symbol:char) =
            match symbol with
            | x when x = decimalSeparator -> true
            | _ when Char.IsDigit(symbol) -> true
            | _ -> false

        let isPartOfProperty (symbol:char) =
            match symbol with
            | _ when Char.IsLetterOrDigit(symbol) -> true
            | _ -> false

        let rec getSubString str index result testFunc=
            if index < length && testFunc input.[index] then
                getSubString str (index+1) (result + input.[index].ToString()) testFunc
            else
                (result, index)

        let getToken start = 
            let symbol = input.[start]

            if isPartOfNumeric symbol then
                let (substring, index) = getSubString input start "" isPartOfNumeric

                let (succ, intValue) = Int32.TryParse substring
                if succ then
                    ((Integer intValue), index)
                else
                    let (succ, doubleValue) = Double.TryParse(substring, NumberStyles.Float ||| NumberStyles.AllowThousands, CultureInfo.CurrentCulture)
                    if (succ) then
                        ((Double doubleValue), index)
                    else
                        failwith "fail"
            else if isPartOfProperty symbol then
                let (substring, index) = getSubString input start "" isPartOfProperty
                ((Identifier substring), index)
            else
                match symbol with
                | ('+' | '-' | '*' | '/') -> ((Operator symbol), start+1)
                | ('(' | ')') -> ((Bracket symbol), start+1)
                | ' ' -> (None, start+1)
                | _ -> failwith "fail"

        let mutable tokens = []
        let mutable i = 0
        while i < length do
            let (token, nextPos) = getToken i
            if (token <> None) then
                tokens <- (token::tokens)
            i <- nextPos
        List.rev(tokens)

    member this.Read input = readFormula input