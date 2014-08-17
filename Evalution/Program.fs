module Evalution
    open System
    open System.Globalization

    let decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.[0]

    type Token = 
        | Double of float
        | Integer of int
        | Operator of char

    let readFormula input = 
        let length = String.length(input)

        let isPartOfNumeric char =
            match char with
            | x when x = decimalSeparator -> true
            | x when x > '0' && x < '9' -> true
            | _ -> false

        let getToken start = 
            let mutable i = start
            let mutable buffer = ""
            if isPartOfNumeric input.[i] then
                while i< String.length(input) && (isPartOfNumeric input.[i]) do
                    buffer <- buffer + input.[i].ToString()
                    i <- i+1

                let (succ, intValue) = Int32.TryParse buffer
                if succ then
                    ((Integer intValue), i)
                else
                    let (succ, doubleValue) = Double.TryParse(buffer, NumberStyles.Float ||| NumberStyles.AllowThousands, CultureInfo.CurrentCulture)
                    if (succ) then
                        ((Double doubleValue), i)
                    else
                        failwith "fail"
            else
                match input.[i] with
                | ('+' | '-' | '*' | '/' | '(' | ')') -> ((Operator input.[i]), i+1)
                | _ -> failwith "fail"

        let mutable tokens = []
        let mutable i = 0
        while i < String.length(input) do
            let (token, nextPos) = getToken i
            tokens <- (token::tokens)
            i <- nextPos
        tokens
 
    [<EntryPoint>]
    let main argv = 
        let result = readFormula("(42+8)*2")
        result |> Seq.iter ( printfn "%A")

        Console.ReadLine()
        0 // return an integer exit code
