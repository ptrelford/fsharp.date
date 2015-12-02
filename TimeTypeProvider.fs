module FSharp.Date.TimeTypeProvider

open System
open System.Reflection
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open FSharp.Date.DateParts

[<TypeProvider>]
type TimeProvider (config:TypeProviderConfig) as this = 
   inherit TypeProviderForNamespaces ()
   
   let createSeconds hours minutes =
      [for seconds = 0 to 59 do
         let secondsName = seconds.ToString("D2")
         let getter args = <@@ TimeSpan(hours,minutes,seconds) @@>
         let property = ProvidedProperty(secondsName, typeof<TimeSpan>, IsStatic=true, GetterCode=getter)
         property.AddXmlDocDelayed(fun () -> TimeSpan(hours,minutes,seconds).ToString())
         yield property
      ]

   let createMinutes hours =
      [for minutes = 0 to 59 do
         let minutesName = minutes.ToString("D2")
         let doc () = sprintf "%d Minutes" minutes
         let minutesType = ProvidedTypeDefinition(minutesName, baseType=Some typeof<TimeSpan>, HideObjectMethods=true)
         let ctor = ProvidedConstructor([],InvokeCode=fun _ -> <@@ TimeSpan(hours,minutes,0) @@>)
         ctor.AddXmlDocDelayed(doc)
         minutesType.AddMember(ctor)
         minutesType.AddMembersDelayed(fun () -> createSeconds hours minutes)
         minutesType.AddXmlDocDelayed(doc)
         yield minutesType
      ]

   let createHours () =
      [for hours = 0 to 23 do
         let hoursName = hours.ToString("D2")
         let doc () = sprintf "%d Hours" hours
         let hoursType = ProvidedTypeDefinition(hoursName, baseType=Some typeof<TimeSpan>, HideObjectMethods=true)
         let ctor = ProvidedConstructor([],InvokeCode=fun _ -> <@@ TimeSpan(hours,0,0) @@>)
         ctor.AddXmlDocDelayed(doc)
         hoursType.AddMember(ctor)
         hoursType.AddMembersDelayed(fun () -> createMinutes hours)
         hoursType.AddXmlDocDelayed(doc)
         yield hoursType
      ]

   let ns = "FSharp.Date"
   let asm = Assembly.GetExecutingAssembly()

   let timeType = ProvidedTypeDefinition(asm, ns, "TimeProvider", Some typeof<obj>)
   do  timeType.AddMembersDelayed(fun () -> createHours ()) 

   do  this.AddNamespace(ns, [timeType])