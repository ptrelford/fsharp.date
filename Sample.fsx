#r "bin/debug/FSharp.Date.dll"

open FSharp.Date

type Date = DateProvider<epoch=2010>
type Time = TimeProvider

Date.``2014``.``02``.``28``

Time.``23``.``59``.``50``

Date.``2014``.``02``.``28`` + Time.``23``.``59``.``50``
