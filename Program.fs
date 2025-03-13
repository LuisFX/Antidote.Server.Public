namespace OpenApi

open Falco
open Falco.OpenApi
open Falco.Routing
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text

type FortuneInput =
    { Name : string }

type Fortune =
    { Description : string }
    static member Create age input =
        match age with
        | Some age when age > 0 ->
            { Description = $"{input.Name}, you will experience great success when you are {age + 3}." }
        | _ ->
            { Description = $"{input.Name}, your future is unclear." }

type Greeting =
    { Message : string }


module Program =

    let authScheme = JwtBearerDefaults.AuthenticationScheme

    let secureResourceHandler : HttpHandler =
        let handleAuth : HttpHandler =
            Response.ofPlainText "Hello authenticated user"

        Request.ifAuthenticated authScheme handleAuth

    let testAuth =
        mapPost "/test"
            (fun req ->
                match req.TryGetString "Authorization" with
                | Some auth ->
                    printfn "Authorization: %s" auth
                    let isAuthorized = "Bearer yourtoken" = auth
                    // secureResourceHandler
                    Response.ofPlainText "Authorized"
                | _ -> 
                    printfn "Unauthorized"
                    Response.ofPlainText "Unauthorized"
            )
            Response.ofJson
                |> OpenApi.name "Test Auth"
                |> OpenApi.summary "Test authentication"
                |> OpenApi.description "This endpoint will test authentication."
                |> OpenApi.route [
                    { Type = typeof<string>; Name = "Name"; Required = false } ]
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<string>
                |> OpenApi.returnType typeof<Greeting>

    let greeterHandler =
        mapGet "/hello/{name?}"
                (fun route ->
                    let auth = route.TryGetString("auth")
                    let name = route?name.AsString("world")
                    let age = route?age.AsIntOption()

                    let message =
                        match age with
                        | Some a when a > 0 -> $"Hello {name}, you are {a} years old!"
                        | Some _ -> $"Hello {name}, you are ageless!"
                        | _ -> $"Hello {name}!"

                    { Message = message })
                Response.ofJson
                |> OpenApi.name "Greeting"
                |> OpenApi.summary "A friendly greeter"
                |> OpenApi.description "This endpoint will provide a customized greeting based on the name and age (optional) provided."
                |> OpenApi.route [
                    { Type = typeof<string>; Name = "Name"; Required = false } ]
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<string>
                |> OpenApi.returnType typeof<Greeting>

    
    let fortuneHandler =
        mapPost "/fortune"
                (fun r -> r?age.AsIntOption())
                (fun ageOpt ->
                    Request.mapJson<FortuneInput> (Fortune.Create ageOpt >> Response.ofJson))
                |> OpenApi.name "Fortune"
                |> OpenApi.summary "A mystic fortune teller"
                |> OpenApi.description "Get a glimpse into your future, if you dare."
                |> OpenApi.query [
                    { Type = typeof<int>; Name = "Age"; Required = false } ]
                |> OpenApi.acceptsType typeof<FortuneInput>
                |> OpenApi.returnType typeof<Fortune>

    let endpoints =
        [
            greeterHandler
            fortuneHandler
            testAuth
            // mapGet "/secure" (fun _ -> secureResourceHandler)
        ]

    [<EntryPoint>]
    let main args =
        let bldr = WebApplication.CreateBuilder(args)

        bldr.Services
            .AddAuthorization()
            .AddAntiforgery()
            .AddFalcoOpenApi()
            .AddSwaggerGen()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(fun options ->
                options.TokenValidationParameters <- TokenValidationParameters(
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "yourissuer",
                    ValidAudience = "youraudience",
                    IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes("yoursecret"))
                )
            )
        |> ignore

        let wapp = bldr.Build()

        wapp.UseHttpsRedirection()
            .UseAntiforgery()
            .UseSwagger()
            .UseSwaggerUI()
            .UseAuthentication()
            .UseAuthorization()
        |> ignore

        wapp.UseRouting()
            .UseFalco(endpoints)
            .Run()

        0

// curl -X 'POST' \
//   'http://localhost:5169/test' \
//   -H 'accept: application/json' \
//   -H 'Content-Type: text/plain' \
//   -H 'Authorization: Bearer yourtoken' \
//   -d 'string'
