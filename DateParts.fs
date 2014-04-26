module FSharp.Date.DateParts

open System
open System.Globalization
open ProviderImplementation.ProvidedTypes

let typeDef name = 
   ProvidedTypeDefinition(name, baseType=Some typeof<obj>, HideObjectMethods=true)

let createDay year month day =
   let date = DateTime(year,month,day)
   let dayName = day.ToString("D2")
   let getter args = <@@ DateTime(year,month,day) @@>
   let property = ProvidedProperty(dayName, typeof<DateTime>, IsStatic=true, GetterCode=getter)
   property.AddXmlDocDelayed(fun () -> date.ToLongDateString())
   property
   
let createDaysInMonth year month =
   [for day in 1..DateTime.DaysInMonth(year,month) -> createDay year month day]

let createDaysOfWeek year month =      
   [for dayOfWeek = 0 to 6 do
      let dayOfWeek = enum<DayOfWeek> dayOfWeek
      let dayOfWeekName = Enum.GetName(typeof<DayOfWeek>, dayOfWeek)
      let dayType = typeDef dayOfWeekName
      for day = 1 to DateTime.DaysInMonth(year,month) do
         if DateTime(year,month,day).DayOfWeek = dayOfWeek then
            dayType.AddMemberDelayed(fun () -> createDay year month day)
      yield dayType
   ]

let createMonth year month monthName =
   let monthType = typeDef monthName
   monthType.AddXmlDocDelayed(fun () -> CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month))
   monthType.AddMembersDelayed(fun () -> createDaysInMonth year month)
   monthType.AddMembersDelayed(fun () -> createDaysOfWeek year month)
   monthType

let createMonths year =
   [for month = 1 to 12 do
      let monthName = month.ToString("D2")              
      yield createMonth year month monthName
      let monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)
      yield createMonth year month monthName]

let createYears epoch =
   let thisYear = DateTime.Now.Year
   [for year = epoch to (thisYear + 10) do
      let yearName = year.ToString("D4")
      let yearType = typeDef yearName
      yearType.AddXmlDocDelayed(fun () -> "Year " + year.ToString())
      yearType.AddMembersDelayed(fun () -> createMonths year)                      
      yield yearType
   ]


