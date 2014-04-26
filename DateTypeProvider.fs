module FSharp.Date.TypeProvider

open System.Reflection
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open FSharp.Date.DateParts

[<TypeProvider>]
type DateProvider (config:TypeProviderConfig) as this = 
   inherit TypeProviderForNamespaces ()
   
   let ns = "FSharp.Date"
   let asm = Assembly.GetExecutingAssembly()

   let createDateProvider typeName epoch =     
      let dateType = ProvidedTypeDefinition(asm, ns, typeName, baseType=Some typeof<obj>, HideObjectMethods=true)     
      dateType.AddMembersDelayed(fun () -> createYears epoch)
      dateType

   let dateType = ProvidedTypeDefinition(asm, ns, "DateProvider", Some typeof<obj>)   
   do  dateType.DefineStaticParameters(
         parameters=[ProvidedStaticParameter("epoch", typeof<int>)], 
         instantiationFunction=(fun typeName parameterValues ->
            match parameterValues with 
            | [| :? int as epoch|] -> epoch              
            | _ -> 2010
            |> createDateProvider typeName))
 
   do  this.AddNamespace(ns, [dateType])

[<assembly:TypeProviderAssembly>]   
do ()