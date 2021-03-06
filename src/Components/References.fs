namespace Ionide.VSCode.FSharp

open System
open FunScript
open FunScript.TypeScript
open FunScript.TypeScript.vscode
open FunScript.TypeScript.vscode.languages

open DTO
open Ionide.VSCode.Helpers

[<ReflectedDefinition>]
module Reference =
    let private createProvider () =
        let provider = createEmpty<ReferenceProvider> ()

        let mapResult (doc : TextDocument) (o : SymbolUseResult) =  
            o.Data.Uses |> Array.map (fun s ->
                let loc = createEmpty<Location> ()
                loc.range <-  Range.Create(float s.StartLine - 1., float s.StartColumn - 1., float s.EndLine - 1., float s.EndColumn - 1.)
                loc.uri <- Uri.file s.FileName
                loc  )

        provider.``provideReferences <-`` (fun doc pos _ _ ->
            LanguageService.symbolUseProject (doc.fileName) (int pos.line + 1) (int pos.character + 1)
            |> Promise.success (mapResult doc)
            |> Promise.toThenable )
        provider

    let activate selector (disposables: Disposable[]) =
        Globals.registerReferenceProvider(selector, createProvider())
        |> ignore
        ()
