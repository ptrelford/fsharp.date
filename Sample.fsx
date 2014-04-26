#r "bin/debug/FSharp.Date.dll"

open FSharp.DateTypeProvider

type Date = DateProvider<epoch=2010>

Date.``2014``.``02``.``28``
