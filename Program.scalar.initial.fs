open Falco
open Falco.Routing
open Falco.OpenApi
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Scalar.AspNetCore

let rootHandler =
    get "/" (Response.ofPlainText "Hello world")
    |> OpenApi.name "HelloWorld"
    |> OpenApi.summary "This is a summary"
    |> OpenApi.description "This is a test description, which is the long form of the summary."
    |> OpenApi.returnType typeof<string>

let builder = WebApplication.CreateBuilder()


builder.Services.AddOpenApi() |> ignore
builder.Services.AddFalcoOpenApi() |> ignore

let app = builder.Build()

app.MapOpenApi() |> ignore
app.MapScalarApiReference() |> ignore

app.UseRouting().UseFalco(
    [
        rootHandler
         
    ]
).Run()
