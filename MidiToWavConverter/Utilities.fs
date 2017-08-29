module Utilities

let inline (^) f x = f x

let isSpacesOrEmpty (s : string) =
    s.Trim().Length = 0

type [<Measure>] percent
