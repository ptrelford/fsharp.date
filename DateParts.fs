module FSharp.Date.DateParts

open System
open System.Globalization
open ProviderImplementation.ProvidedTypes

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
      let days =
        [for day = 1 to DateTime.DaysInMonth(year,month) do
            if DateTime(year,month,day).DayOfWeek = dayOfWeek then yield day]
      let doc () = dayOfWeekName + String.Join(" or ", days)
      let dayType = ProvidedTypeDefinition(dayOfWeekName, baseType=Some typeof<DateTime*DayOfWeek>, HideObjectMethods=true)
      let ctor = ProvidedConstructor([],InvokeCode=fun _ -> <@@ (DateTime(year,month,1),dayOfWeek) @@>)      
      ctor.AddXmlDocDelayed(doc)
      dayType.AddMember(ctor)
      dayType.AddXmlDocDelayed(doc)
      for day in days do      
         dayType.AddMemberDelayed(fun () -> createDay year month day)
      yield dayType
   ]

let toCalendar (year,month) =
   let date = System.DateTime(year,month,1)
   let mondayDelta = (int date.DayOfWeek + 6) % 7
   let mondayDelta = if mondayDelta = 0 then 7 else mondayDelta
   [date.ToString("MMMM yyyy");"Mo Tu We Th Fr Sa Su"]
   @
   [for week in 0..5 ->
      [|for weekDay in 0..6->
         let days = week*7 + weekDay - mondayDelta
         let day = date.AddDays(float days).Day
         sprintf "%02d" day
      |] |> String.concat " "
   ]
   |> List.map (fun s -> "<para>"+s+"</para>") 
   |> String.concat "\r\n"

let createMonth year month monthName =
   let doc () = "<summary>"+toCalendar(year,month)+"</summary>"
   let monthType = ProvidedTypeDefinition(monthName, baseType=Some typeof<DateTime>, HideObjectMethods=true)   
   let ctor = ProvidedConstructor([],InvokeCode=fun _ -> <@@ DateTime(year,month,1) @@>)   
   ctor.AddXmlDocDelayed(doc)
   monthType.AddMember(ctor)
   monthType.AddXmlDocDelayed(doc)
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
      let doc () = "Year " + year.ToString()
      let yearType = ProvidedTypeDefinition(yearName, baseType=Some typeof<DateTime>, HideObjectMethods=true)
      let ctor = ProvidedConstructor([],InvokeCode=fun _ -> <@@ DateTime(year,1,1) @@>)
      ctor.AddXmlDocDelayed(doc)
      yearType.AddMember(ctor)
      yearType.AddXmlDocDelayed(doc)
      yearType.AddMembersDelayed(fun () -> createMonths year)
      yield yearType
   ]
