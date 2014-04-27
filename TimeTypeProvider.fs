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
         let minutesType = ProvidedTypeDefinition(minutesName, baseType=Some typeof<obj>, HideObjectMethods=true)
         minutesType.AddMembersDelayed(fun () -> createSeconds hours minutes)
         minutesType.AddXmlDocDelayed(fun () -> sprintf "%d Minutes" minutes)
         yield minutesType
      ]

   let createHours () =
      [for hours = 0 to 23 do
         let hoursName = hours.ToString("D2")
         let hoursType = ProvidedTypeDefinition(hoursName, baseType=Some typeof<obj>, HideObjectMethods=true)
         hoursType.AddMembersDelayed(fun () -> createMinutes hours)
         hoursType.AddXmlDocDelayed(fun () -> sprintf "%d Hours" hours)
         yield hoursType
      ]

   let ns = "FSharp.Date"
   let asm = Assembly.GetExecutingAssembly()

   let timeType = ProvidedTypeDefinition(asm, ns, "TimeProvider", Some typeof<obj>)   
   do  timeType.AddMembersDelayed(fun () -> createHours ()) 

   do  this.AddNamespace(ns, [timeType])